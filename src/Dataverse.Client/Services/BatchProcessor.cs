using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Dataverse.Client.Services;

/// <summary>
/// Concrete implementation of IBatchProcessor for handling batch operations against Microsoft Dataverse.
/// Provides optimized batch processing for CRUD operations with error handling and retry logic.
/// </summary>
/// <remarks>
/// Initializes a new instance of the BatchProcessor class.
/// </remarks>
/// <param name="serviceClient">The Dataverse service client</param>
/// <param name="options">Configuration options for the Dataverse client</param>
/// <param name="logger">Logger instance</param>
public class BatchProcessor(
    ServiceClient serviceClient,
    IOptions<DataverseClientOptions> options,
    ILogger<BatchProcessor> logger) : IBatchProcessor
{
    #region Private Fields

    private readonly ServiceClient _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
    private readonly ILogger<BatchProcessor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly DataverseClientOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    #endregion

    #region Batch Create Operations

    /// <inheritdoc />
    public async Task<BatchOperationResult> CreateRecordsAsync(IEnumerable<Entity> entities, int? batchSize = null)
    {
        ArgumentNullException.ThrowIfNull(entities);

        List<Entity> entityList = [.. entities];
        if (entityList.Count == 0)
            return new BatchOperationResult { OperationType = BatchOperationType.BatchCreate };

        int effectiveBatchSize = batchSize ?? _options.DefaultBatchSize;
        _logger.LogInformation("Starting batch create operation for {EntityCount} entities with batch size {BatchSize}",
            entityList.Count, effectiveBatchSize);

        BatchOperationResult result = new()
        {
            OperationType = BatchOperationType.BatchCreate,
            StartTime = DateTime.UtcNow
        };

        try
        {
            List<Task<BatchExecutionResult>> batchTasks = [];

            for (int i = 0; i < entityList.Count; i += effectiveBatchSize)
            {
                List<Entity> batch = [.. entityList.Skip(i).Take(effectiveBatchSize)];
                int batchNumber = (i / effectiveBatchSize) + 1;

                Task<BatchExecutionResult> batchTask = ExecuteCreateBatchAsync(batch, batchNumber);
                batchTasks.Add(batchTask);
            }

            BatchExecutionResult[] batchResults = await Task.WhenAll(batchTasks);

            // Aggregate results
            foreach (BatchExecutionResult batchResult in batchResults)
            {
                result.SuccessCount += batchResult.SuccessCount;
                result.FailureCount += batchResult.FailureCount;
                result.Errors.AddRange(batchResult.Errors);
                result.CreatedRecords.AddRange(batchResult.CreatedRecords);
            }

            result.MarkCompleted();

            _logger.LogInformation(
                "Batch create operation completed: {SuccessCount} succeeded, {FailureCount} failed in {Duration}ms",
                result.SuccessCount, result.FailureCount, result.Duration?.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            result.MarkCompleted();
            result.Errors.Add(new BatchError
            {
                ErrorMessage = $"Batch create operation failed: {ex.Message}",
                Exception = ex,
                Severity = ErrorSeverity.Critical
            });

            _logger.LogError(ex, "Batch create operation failed");
            throw new DataverseBatchException("Batch create operation failed", ex);
        }
    }

    #endregion

    #region Batch Update Operations

    /// <inheritdoc />
    public async Task<BatchOperationResult> UpdateRecordsAsync(IEnumerable<Entity> entities, int? batchSize = null)
    {
        ArgumentNullException.ThrowIfNull(entities);

        List<Entity> entityList = [.. entities];
        if (entityList.Count == 0)
            return new BatchOperationResult { OperationType = BatchOperationType.BatchUpdate };

        // Validate that all entities have IDs
        List<Entity> entitiesWithoutIds = [.. entityList.Where(e => e.Id == Guid.Empty)];
        if (entitiesWithoutIds.Count != 0)
        {
            throw new ArgumentException(
                $"Found {entitiesWithoutIds.Count} entities without IDs. All entities must have valid IDs for update operations.");
        }

        int effectiveBatchSize = batchSize ?? _options.DefaultBatchSize;
        _logger.LogInformation("Starting batch update operation for {EntityCount} entities with batch size {BatchSize}",
            entityList.Count, effectiveBatchSize);

        BatchOperationResult result = new()
        {
            OperationType = BatchOperationType.BatchUpdate,
            StartTime = DateTime.UtcNow
        };

        try
        {
            List<Task<BatchExecutionResult>> batchTasks = [];

            for (int i = 0; i < entityList.Count; i += effectiveBatchSize)
            {
                List<Entity> batch = [.. entityList.Skip(i).Take(effectiveBatchSize)];
                int batchNumber = (i / effectiveBatchSize) + 1;

                Task<BatchExecutionResult> batchTask = ExecuteUpdateBatchAsync(batch, batchNumber);
                batchTasks.Add(batchTask);
            }

            BatchExecutionResult[] batchResults = await Task.WhenAll(batchTasks);

            // Aggregate results
            foreach (BatchExecutionResult batchResult in batchResults)
            {
                result.SuccessCount += batchResult.SuccessCount;
                result.FailureCount += batchResult.FailureCount;
                result.Errors.AddRange(batchResult.Errors);
                result.UpdatedRecords.AddRange(batchResult.UpdatedRecords);
            }

            result.MarkCompleted();

            _logger.LogInformation(
                "Batch update operation completed: {SuccessCount} succeeded, {FailureCount} failed in {Duration}ms",
                result.SuccessCount, result.FailureCount, result.Duration?.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            result.MarkCompleted();
            result.Errors.Add(new BatchError
            {
                ErrorMessage = $"Batch update operation failed: {ex.Message}",
                Exception = ex,
                Severity = ErrorSeverity.Critical
            });

            _logger.LogError(ex, "Batch update operation failed");
            throw new DataverseBatchException("Batch update operation failed", ex);
        }
    }

    #endregion

    #region Batch Delete Operations

    /// <inheritdoc />
    public async Task<BatchOperationResult> DeleteRecordsAsync(IEnumerable<EntityReference> entityReferences,
        int? batchSize = null)
    {
        ArgumentNullException.ThrowIfNull(entityReferences);

        List<EntityReference> entityRefList = [.. entityReferences];
        if (entityRefList.Count == 0)
            return new BatchOperationResult { OperationType = BatchOperationType.BatchDelete };

        int effectiveBatchSize = batchSize ?? _options.DefaultBatchSize;
        _logger.LogInformation(
            "Starting batch delete operation for {EntityCount} entity references with batch size {BatchSize}",
            entityRefList.Count, effectiveBatchSize);

        BatchOperationResult result = new()
        {
            OperationType = BatchOperationType.BatchDelete,
            StartTime = DateTime.UtcNow
        };

        try
        {
            List<Task<BatchExecutionResult>> batchTasks = [];

            for (int i = 0; i < entityRefList.Count; i += effectiveBatchSize)
            {
                List<EntityReference> batch = [.. entityRefList.Skip(i).Take(effectiveBatchSize)];
                int batchNumber = (i / effectiveBatchSize) + 1;

                Task<BatchExecutionResult> batchTask = ExecuteDeleteBatchAsync(batch, batchNumber);
                batchTasks.Add(batchTask);
            }

            BatchExecutionResult[] batchResults = await Task.WhenAll(batchTasks);

            // Aggregate results
            foreach (BatchExecutionResult batchResult in batchResults)
            {
                result.SuccessCount += batchResult.SuccessCount;
                result.FailureCount += batchResult.FailureCount;
                result.Errors.AddRange(batchResult.Errors);
                result.DeletedRecords.AddRange(batchResult.DeletedRecords);
            }

            result.MarkCompleted();

            _logger.LogInformation(
                "Batch delete operation completed: {SuccessCount} succeeded, {FailureCount} failed in {Duration}ms",
                result.SuccessCount, result.FailureCount, result.Duration?.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            result.MarkCompleted();
            result.Errors.Add(new BatchError
            {
                ErrorMessage = $"Batch delete operation failed: {ex.Message}",
                Exception = ex,
                Severity = ErrorSeverity.Critical
            });

            _logger.LogError(ex, "Batch delete operation failed");
            throw new DataverseBatchException("Batch delete operation failed", ex);
        }
    }

    #endregion

    #region Batch Retrieve Operations

    /// <inheritdoc />
    public async Task<BatchRetrieveResult> RetrieveRecordsAsync(IEnumerable<EntityReference> entityReferences,
        ColumnSet columns, int? batchSize = null)
    {
        ArgumentNullException.ThrowIfNull(entityReferences);
        ArgumentNullException.ThrowIfNull(columns);

        List<EntityReference> entityRefList = [.. entityReferences];
        if (entityRefList.Count == 0)
            return new BatchRetrieveResult { OperationType = BatchOperationType.BatchRetrieve };

        int effectiveBatchSize = batchSize ?? _options.DefaultBatchSize;
        _logger.LogInformation(
            "Starting batch retrieve operation for {EntityCount} entity references with batch size {BatchSize}",
            entityRefList.Count, effectiveBatchSize);

        BatchRetrieveResult result = new()
        {
            OperationType = BatchOperationType.BatchRetrieve,
            StartTime = DateTime.UtcNow
        };

        try
        {
            List<Task<BatchRetrieveExecutionResult>> batchTasks = [];

            for (int i = 0; i < entityRefList.Count; i += effectiveBatchSize)
            {
                List<EntityReference> batch = [.. entityRefList.Skip(i).Take(effectiveBatchSize)];
                int batchNumber = (i / effectiveBatchSize) + 1;

                Task<BatchRetrieveExecutionResult> batchTask = ExecuteRetrieveBatchAsync(batch, columns, batchNumber);
                batchTasks.Add(batchTask);
            }

            BatchRetrieveExecutionResult[] batchResults = await Task.WhenAll(batchTasks);

            // Aggregate results
            foreach (BatchRetrieveExecutionResult batchResult in batchResults)
            {
                result.SuccessCount += batchResult.SuccessCount;
                result.FailureCount += batchResult.FailureCount;
                result.Errors.AddRange(batchResult.Errors);
                result.RetrievedEntities.AddRange(batchResult.RetrievedEntities);
                result.NotFoundReferences.AddRange(batchResult.NotFoundReferences);
            }

            result.MarkCompleted();

            _logger.LogInformation(
                "Batch retrieve operation completed: {SuccessCount} retrieved, {NotFoundCount} not found, {FailureCount} failed in {Duration}ms",
                result.SuccessCount, result.NotFoundReferences.Count, result.FailureCount,
                result.Duration?.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            result.MarkCompleted();
            result.Errors.Add(new BatchError
            {
                ErrorMessage = $"Batch retrieve operation failed: {ex.Message}",
                Exception = ex,
                Severity = ErrorSeverity.Critical
            });

            _logger.LogError(ex, "Batch retrieve operation failed");
            throw new DataverseBatchException("Batch retrieve operation failed", ex);
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Executes a create batch using ExecuteMultiple request.
    /// </summary>
    private async Task<BatchExecutionResult> ExecuteCreateBatchAsync(List<Entity> entities, int batchNumber)
    {
        BatchExecutionResult result = new() { BatchNumber = batchNumber };

        try
        {
            _logger.LogDebug("Executing create batch {BatchNumber} with {EntityCount} entities", batchNumber,
                entities.Count);

            ExecuteMultipleRequest executeMultipleRequest = new()
            {
                Settings = new ExecuteMultipleSettings { ContinueOnError = true, ReturnResponses = true },
                Requests = []
            };

            foreach (Entity entity in entities)
                executeMultipleRequest.Requests.Add(new CreateRequest { Target = entity });

            ExecuteMultipleResponse executeMultipleResponse =
                await ExecuteWithRetryAsync<ExecuteMultipleResponse>(executeMultipleRequest);

            // Process responses
            for (int i = 0; i < executeMultipleResponse.Responses.Count; i++)
            {
                ExecuteMultipleResponseItem? responseItem = executeMultipleResponse.Responses[i];

                if (responseItem.Fault == null)
                {
                    result.SuccessCount++;
                    if (responseItem.Response is CreateResponse createResponse)
                        result.CreatedRecords.Add(new EntityReference(entities[i].LogicalName, createResponse.id));
                }
                else
                {
                    result.FailureCount++;
                    result.Errors.Add(new BatchError
                    {
                        BatchNumber = batchNumber,
                        RequestIndex = i,
                        ErrorMessage = responseItem.Fault.Message,
                        ErrorCode = responseItem.Fault.ErrorCode.ToString(),
                        EntityReference = new EntityReference(entities[i].LogicalName, entities[i].Id),
                        Severity = ErrorSeverity.Error
                    });
                }
            }

            _logger.LogDebug("Create batch {BatchNumber} completed: {SuccessCount} succeeded, {FailureCount} failed",
                batchNumber, result.SuccessCount, result.FailureCount);
        }
        catch (Exception ex)
        {
            result.FailureCount = entities.Count;
            result.Errors.Add(new BatchError
            {
                BatchNumber = batchNumber,
                ErrorMessage = $"Batch execution failed: {ex.Message}",
                Exception = ex,
                Severity = ErrorSeverity.Critical
            });

            _logger.LogError(ex, "Create batch {BatchNumber} failed", batchNumber);
        }

        return result;
    }

    /// <summary>
    /// Executes an update batch using ExecuteMultiple request.
    /// </summary>
    private async Task<BatchExecutionResult> ExecuteUpdateBatchAsync(List<Entity> entities, int batchNumber)
    {
        BatchExecutionResult result = new() { BatchNumber = batchNumber };

        try
        {
            _logger.LogDebug("Executing update batch {BatchNumber} with {EntityCount} entities", batchNumber,
                entities.Count);

            ExecuteMultipleRequest executeMultipleRequest = new()
            {
                Settings = new ExecuteMultipleSettings { ContinueOnError = true, ReturnResponses = true },
                Requests = []
            };

            foreach (Entity entity in entities)
                executeMultipleRequest.Requests.Add(new UpdateRequest { Target = entity });

            ExecuteMultipleResponse executeMultipleResponse =
                await ExecuteWithRetryAsync<ExecuteMultipleResponse>(executeMultipleRequest);

            // Process responses
            for (int i = 0; i < executeMultipleResponse.Responses.Count; i++)
            {
                ExecuteMultipleResponseItem? responseItem = executeMultipleResponse.Responses[i];

                if (responseItem.Fault == null)
                {
                    result.SuccessCount++;
                    result.UpdatedRecords.Add(new EntityReference(entities[i].LogicalName, entities[i].Id));
                }
                else
                {
                    result.FailureCount++;
                    result.Errors.Add(new BatchError
                    {
                        BatchNumber = batchNumber,
                        RequestIndex = i,
                        ErrorMessage = responseItem.Fault.Message,
                        ErrorCode = responseItem.Fault.ErrorCode.ToString(),
                        EntityReference = new EntityReference(entities[i].LogicalName, entities[i].Id),
                        Severity = ErrorSeverity.Error
                    });
                }
            }

            _logger.LogDebug("Update batch {BatchNumber} completed: {SuccessCount} succeeded, {FailureCount} failed",
                batchNumber, result.SuccessCount, result.FailureCount);
        }
        catch (Exception ex)
        {
            result.FailureCount = entities.Count;
            result.Errors.Add(new BatchError
            {
                BatchNumber = batchNumber,
                ErrorMessage = $"Batch execution failed: {ex.Message}",
                Exception = ex,
                Severity = ErrorSeverity.Critical
            });

            _logger.LogError(ex, "Update batch {BatchNumber} failed", batchNumber);
        }

        return result;
    }

    /// <summary>
    /// Executes a delete batch using ExecuteMultiple request.
    /// </summary>
    private async Task<BatchExecutionResult> ExecuteDeleteBatchAsync(List<EntityReference> entityReferences,
        int batchNumber)
    {
        BatchExecutionResult result = new() { BatchNumber = batchNumber };

        try
        {
            _logger.LogDebug("Executing delete batch {BatchNumber} with {EntityCount} entity references", batchNumber,
                entityReferences.Count);

            ExecuteMultipleRequest executeMultipleRequest = new()
            {
                Settings = new ExecuteMultipleSettings { ContinueOnError = true, ReturnResponses = true },
                Requests = []
            };

            foreach (EntityReference entityRef in entityReferences)
                executeMultipleRequest.Requests.Add(new DeleteRequest { Target = entityRef });

            ExecuteMultipleResponse executeMultipleResponse =
                await ExecuteWithRetryAsync<ExecuteMultipleResponse>(executeMultipleRequest);

            // Process responses
            for (int i = 0; i < executeMultipleResponse.Responses.Count; i++)
            {
                ExecuteMultipleResponseItem? responseItem = executeMultipleResponse.Responses[i];

                if (responseItem.Fault == null)
                {
                    result.SuccessCount++;
                    result.DeletedRecords.Add(entityReferences[i]);
                }
                else
                {
                    result.FailureCount++;
                    result.Errors.Add(new BatchError
                    {
                        BatchNumber = batchNumber,
                        RequestIndex = i,
                        ErrorMessage = responseItem.Fault.Message,
                        ErrorCode = responseItem.Fault.ErrorCode.ToString(),
                        EntityReference = entityReferences[i],
                        Severity = ErrorSeverity.Error
                    });
                }
            }

            _logger.LogDebug("Delete batch {BatchNumber} completed: {SuccessCount} succeeded, {FailureCount} failed",
                batchNumber, result.SuccessCount, result.FailureCount);
        }
        catch (Exception ex)
        {
            result.FailureCount = entityReferences.Count;
            result.Errors.Add(new BatchError
            {
                BatchNumber = batchNumber,
                ErrorMessage = $"Batch execution failed: {ex.Message}",
                Exception = ex,
                Severity = ErrorSeverity.Critical
            });

            _logger.LogError(ex, "Delete batch {BatchNumber} failed", batchNumber);
        }

        return result;
    }

    /// <summary>
    /// Executes a retrieve batch using ExecuteMultiple request.
    /// </summary>
    private async Task<BatchRetrieveExecutionResult> ExecuteRetrieveBatchAsync(List<EntityReference> entityReferences,
        ColumnSet columns, int batchNumber)
    {
        BatchRetrieveExecutionResult result = new() { BatchNumber = batchNumber };

        try
        {
            _logger.LogDebug("Executing retrieve batch {BatchNumber} with {EntityCount} entity references", batchNumber,
                entityReferences.Count);

            ExecuteMultipleRequest executeMultipleRequest = new()
            {
                Settings = new ExecuteMultipleSettings { ContinueOnError = true, ReturnResponses = true },
                Requests = []
            };

            foreach (EntityReference entityRef in entityReferences)
                executeMultipleRequest.Requests.Add(new RetrieveRequest { Target = entityRef, ColumnSet = columns });

            ExecuteMultipleResponse executeMultipleResponse =
                await ExecuteWithRetryAsync<ExecuteMultipleResponse>(executeMultipleRequest);

            // Process responses
            for (int i = 0; i < executeMultipleResponse.Responses.Count; i++)
            {
                ExecuteMultipleResponseItem? responseItem = executeMultipleResponse.Responses[i];

                if (responseItem.Fault == null)
                {
                    result.SuccessCount++;
                    if (responseItem.Response is RetrieveResponse retrieveResponse)
                        result.RetrievedEntities.Add(retrieveResponse.Entity);
                }
                else
                {
                    if (responseItem.Fault.ErrorCode == DataverseConstants.ErrorCodes.RecordNotFoundCode)
                    {
                        result.NotFoundReferences.Add(entityReferences[i]);
                    }
                    else
                    {
                        result.FailureCount++;
                        result.Errors.Add(new BatchError
                        {
                            BatchNumber = batchNumber,
                            RequestIndex = i,
                            ErrorMessage = responseItem.Fault.Message,
                            ErrorCode = responseItem.Fault.ErrorCode.ToString(),
                            EntityReference = entityReferences[i],
                            Severity = ErrorSeverity.Error
                        });
                    }
                }
            }

            _logger.LogDebug(
                "Retrieve batch {BatchNumber} completed: {SuccessCount} retrieved, {NotFoundCount} not found, {FailureCount} failed",
                batchNumber, result.SuccessCount, result.NotFoundReferences.Count, result.FailureCount);
        }
        catch (Exception ex)
        {
            result.FailureCount = entityReferences.Count;
            result.Errors.Add(new BatchError
            {
                BatchNumber = batchNumber,
                ErrorMessage = $"Batch execution failed: {ex.Message}",
                Exception = ex,
                Severity = ErrorSeverity.Critical
            });

            _logger.LogError(ex, "Retrieve batch {BatchNumber} failed", batchNumber);
        }

        return result;
    }

    /// <summary>
    /// Executes a request with retry logic for handling transient failures.
    /// </summary>
    private async Task<TResponse> ExecuteWithRetryAsync<TResponse>(OrganizationRequest request)
        where TResponse : OrganizationResponse
    {
        int maxRetries = _options.RetryAttempts;
        int currentAttempt = 0;

        while (currentAttempt <= maxRetries)
        {
            try
            {
                OrganizationResponse? response = await Task.Run(() => _serviceClient.Execute(request));
                return (TResponse)response;
            }
            catch (Exception ex) when (currentAttempt < maxRetries && DataverseUtilities.IsTransientException(ex))
            {
                currentAttempt++;
                int retryDelay = DataverseUtilities.CalculateRetryDelay(currentAttempt, _options.RetryDelayMs);

                _logger.LogWarning(ex,
                    "Batch request failed with transient error on attempt {Attempt}/{MaxAttempts}. Retrying in {Delay}ms",
                    currentAttempt, maxRetries + 1, retryDelay);

                await Task.Delay(retryDelay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch request failed on attempt {Attempt}/{MaxAttempts} with non-transient error",
                    currentAttempt + 1, maxRetries + 1);
                throw;
            }
        }

        throw new DataverseException(DataverseConstants.ErrorCodes.Timeout,
            $"Batch request failed after {maxRetries + 1} attempts");
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Internal class for tracking batch execution results.
    /// </summary>
    private class BatchExecutionResult
    {
        public int BatchNumber { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<BatchError> Errors { get; set; } = [];
        public List<EntityReference> CreatedRecords { get; set; } = [];
        public List<EntityReference> UpdatedRecords { get; set; } = [];
        public List<EntityReference> DeletedRecords { get; set; } = [];
    }

    /// <summary>
    /// Internal class for tracking batch retrieve execution results.
    /// </summary>
    private class BatchRetrieveExecutionResult : BatchExecutionResult
    {
        public List<Entity> RetrievedEntities { get; set; } = [];
        public List<EntityReference> NotFoundReferences { get; set; } = [];
    }

    #endregion
}
