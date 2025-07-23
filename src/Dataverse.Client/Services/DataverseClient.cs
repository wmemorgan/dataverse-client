using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using ValidationResult = Dataverse.Client.Models.ValidationResult;

namespace Dataverse.Client.Services;

/// <summary>
/// Concrete implementation of IDataverseClient providing comprehensive Dataverse operations
/// including CRUD operations, batch processing, querying, and validation.
/// </summary>
public class DataverseClient : IDataverseClient
{
    #region Private Fields

    private readonly ServiceClient _serviceClient;
    private readonly ILogger<DataverseClient> _logger;
    private readonly DataverseClientOptions _options;
    private readonly IBatchProcessor _batchProcessor;
    private bool _disposed;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the DataverseClient class.
    /// </summary>
    public DataverseClient(
        ServiceClient serviceClient,
        IOptions<DataverseClientOptions> options,
        IBatchProcessor batchProcessor,
        ILogger<DataverseClient> logger)
    {
        ArgumentNullException.ThrowIfNull(serviceClient, nameof(serviceClient));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(batchProcessor, nameof(batchProcessor));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _serviceClient = serviceClient;
        _options = options.Value;
        _batchProcessor = batchProcessor;
        _logger = logger;

        _logger.LogInformation("DataverseClient initialized with connection to {OrganizationUri}",
            _serviceClient.ConnectedOrgUriActual);
    }

    #endregion

    #region Connection Management

    /// <inheritdoc />
    public async Task<bool> ValidateConnectionAsync()
    {
        try
        {
            _logger.LogDebug("Starting connection validation");

            if (_serviceClient?.IsReady != true)
            {
                _logger.LogWarning("ServiceClient is not ready or null");
                return false;
            }

            WhoAmIRequest whoAmIRequest = new();
            WhoAmIResponse? whoAmIResponse = await ExecuteRequestWithRetryAsync<WhoAmIResponse>(whoAmIRequest);

            if (whoAmIResponse?.UserId != null && whoAmIResponse.UserId != Guid.Empty)
            {
                _logger.LogInformation("Connection validation successful. Connected as user {UserId}",
                    whoAmIResponse.UserId);
                return true;
            }

            _logger.LogWarning("Connection validation failed: Invalid user response");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection validation failed with exception");
            return false;
        }
    }

    /// <inheritdoc />
    public ConnectionInfo GetConnectionInfo()
    {
        try
        {
            ConnectionInfo connectionInfo = new()
            {
                Id = Guid.NewGuid(),
                OrganizationUrl = _serviceClient.ConnectedOrgUriActual?.ToString() ?? string.Empty,
                OrganizationName = _serviceClient.ConnectedOrgFriendlyName ?? string.Empty,
                OrganizationId = _serviceClient?.ConnectedOrgId ?? Guid.Empty,
                UserId = Guid.Empty,
                UserName = string.Empty,
                ConnectedAt = DateTime.UtcNow,
                OrganizationVersion = _serviceClient?.ConnectedOrgVersion?.ToString() ?? string.Empty,
                State = _serviceClient?.IsReady == true ? ConnectionState.Connected : ConnectionState.Failed
            };

            try
            {
                if (_serviceClient?.IsReady == true)
                {
                    WhoAmIRequest whoAmIRequest = new();
                    WhoAmIResponse? whoAmIResponse = (WhoAmIResponse)_serviceClient.Execute(whoAmIRequest);

                    if (whoAmIResponse != null)
                    {
                        connectionInfo.UserId = whoAmIResponse.UserId;
                        connectionInfo.BusinessUnitId = whoAmIResponse.BusinessUnitId;
                        connectionInfo.OrganizationId = whoAmIResponse.OrganizationId;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not retrieve user information for connection info");
            }

            return connectionInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get connection information");
            return new ConnectionInfo { State = ConnectionState.Failed, Errors = [ex.Message] };
        }
    }

    #endregion

    #region Individual CRUD Operations

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            _logger.LogDebug("Creating entity {EntityName} with {AttributeCount} attributes",
                entity.LogicalName, entity.Attributes.Count);

            CreateRequest request = new() { Target = entity };
            CreateResponse response = await ExecuteRequestWithRetryAsync<CreateResponse>(request);

            Guid createdId = response.id;
            _logger.LogInformation("Successfully created {EntityName} with ID {EntityId}",
                entity.LogicalName, createdId);

            return createdId;
        }
        catch (Exception ex)
        {
            string errorMessage = $"Failed to create entity {entity.LogicalName}";
            _logger.LogError(ex, "Failed to create entity {EntityName}: {Message}", entity.LogicalName, ex.Message);
            throw new DataverseException(DataverseConstants.ErrorCodes.BatchOperationFailed, errorMessage, ex);
        }
    }

    /// <inheritdoc />
    public async Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columns)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentException("Entity name cannot be null or empty", nameof(entityName));
        if (id == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty", nameof(id));
        ArgumentNullException.ThrowIfNull(columns);

        try
        {
            _logger.LogDebug("Retrieving {EntityName} with ID {EntityId}, columns: {ColumnCount}",
                entityName, id, columns.AllColumns ? "All" : columns.Columns.Count.ToString());

            RetrieveRequest request = new() { Target = new EntityReference(entityName, id), ColumnSet = columns };

            RetrieveResponse response = await ExecuteRequestWithRetryAsync<RetrieveResponse>(request);

            _logger.LogInformation("Successfully retrieved {EntityName} with ID {EntityId}",
                entityName, id);

            return response.Entity;
        }
        catch (Exception ex)
        {
            string errorMessage = $"Failed to retrieve {entityName} with ID {id}";
            _logger.LogError(ex, "Failed to retrieve {EntityName} with ID {EntityId}: {Message}", entityName, id,
                ex.Message);
            throw new DataverseException(DataverseConstants.ErrorCodes.RecordNotFound, errorMessage, ex);
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (entity.Id == Guid.Empty)
            throw new ArgumentException("Entity ID must be set for update operations", nameof(entity));

        try
        {
            _logger.LogDebug("Updating entity {EntityName} with ID {EntityId}, {AttributeCount} attributes",
                entity.LogicalName, entity.Id, entity.Attributes.Count);

            UpdateRequest request = new() { Target = entity };
            await ExecuteRequestWithRetryAsync<UpdateResponse>(request);

            _logger.LogInformation("Successfully updated {EntityName} with ID {EntityId}",
                entity.LogicalName, entity.Id);
        }
        catch (Exception ex)
        {
            string errorMessage = $"Failed to update entity {entity.LogicalName} with ID {entity.Id}";
            _logger.LogError(ex, "Failed to update entity {EntityName} with ID {EntityId}: {Message}",
                entity.LogicalName, entity.Id, ex.Message);
            throw new DataverseException(DataverseConstants.ErrorCodes.InvalidArgument, errorMessage, ex);
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string entityName, Guid id)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentException("Entity name cannot be null or empty", nameof(entityName));
        if (id == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty", nameof(id));

        try
        {
            _logger.LogDebug("Deleting {EntityName} with ID {EntityId}", entityName, id);

            DeleteRequest request = new() { Target = new EntityReference(entityName, id) };

            await ExecuteRequestWithRetryAsync<DeleteResponse>(request);

            _logger.LogInformation("Successfully deleted {EntityName} with ID {EntityId}",
                entityName, id);
        }
        catch (Exception ex)
        {
            string errorMessage = $"Failed to delete {entityName} with ID {id}";
            _logger.LogError(ex, "Failed to delete {EntityName} with ID {EntityId}: {Message}", entityName, id,
                ex.Message);
            throw new DataverseException(DataverseConstants.ErrorCodes.RecordNotFound, errorMessage, ex);
        }
    }

    #endregion

    #region Batch CRUD Operations

    /// <inheritdoc />
    public async Task<BatchOperationResult> CreateBatchAsync(IEnumerable<Entity> entities,
        BatchConfiguration? config = null)
    {
        ArgumentNullException.ThrowIfNull(entities);

        try
        {
            _logger.LogInformation("Starting batch create operation with {EntityCount} entities",
                entities.Count());

            int batchSize = config?.GetEffectiveBatchSize(_options.DefaultBatchSize) ?? _options.DefaultBatchSize;
            BatchOperationResult result = await _batchProcessor.CreateRecordsAsync(entities, batchSize);

            _logger.LogInformation("Batch create operation completed: {SuccessCount} succeeded, {FailureCount} failed",
                result.SuccessCount, result.FailureCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch create operation failed");
            throw new DataverseBatchException("Batch create operation failed", ex);
        }
    }

    /// <inheritdoc />
    public async Task<BatchOperationResult> UpdateBatchAsync(IEnumerable<Entity> entities,
        BatchConfiguration? config = null)
    {
        ArgumentNullException.ThrowIfNull(entities);

        try
        {
            _logger.LogInformation("Starting batch update operation with {EntityCount} entities",
                entities.Count());

            int batchSize = config?.GetEffectiveBatchSize(_options.DefaultBatchSize) ?? _options.DefaultBatchSize;
            BatchOperationResult result = await _batchProcessor.UpdateRecordsAsync(entities, batchSize);

            _logger.LogInformation("Batch update operation completed: {SuccessCount} succeeded, {FailureCount} failed",
                result.SuccessCount, result.FailureCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch update operation failed");
            throw new DataverseBatchException("Batch update operation failed", ex);
        }
    }

    /// <inheritdoc />
    public async Task<BatchOperationResult> DeleteBatchAsync(IEnumerable<EntityReference> entityRefs,
        BatchConfiguration? config = null)
    {
        ArgumentNullException.ThrowIfNull(entityRefs);

        try
        {
            _logger.LogInformation("Starting batch delete operation with {EntityRefCount} entity references",
                entityRefs.Count());

            int batchSize = config?.GetEffectiveBatchSize(_options.DefaultBatchSize) ?? _options.DefaultBatchSize;
            BatchOperationResult result = await _batchProcessor.DeleteRecordsAsync(entityRefs, batchSize);

            _logger.LogInformation("Batch delete operation completed: {SuccessCount} succeeded, {FailureCount} failed",
                result.SuccessCount, result.FailureCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch delete operation failed");
            throw new DataverseBatchException("Batch delete operation failed", ex);
        }
    }

    /// <inheritdoc />
    public async Task<BatchRetrieveResult> RetrieveBatchAsync(IEnumerable<EntityReference> entityRefs,
        ColumnSet columns, BatchConfiguration? config = null)
    {
        ArgumentNullException.ThrowIfNull(entityRefs);
        ArgumentNullException.ThrowIfNull(columns);

        try
        {
            _logger.LogInformation("Starting batch retrieve operation with {EntityRefCount} entity references",
                entityRefs.Count());

            int batchSize = config?.GetEffectiveBatchSize(_options.DefaultBatchSize) ?? _options.DefaultBatchSize;
            BatchRetrieveResult result = await _batchProcessor.RetrieveRecordsAsync(entityRefs, columns, batchSize);

            _logger.LogInformation(
                "Batch retrieve operation completed: {SuccessCount} retrieved, {NotFoundCount} not found",
                result.SuccessCount, result.NotFoundReferences.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch retrieve operation failed");
            throw new DataverseBatchException("Batch retrieve operation failed", ex);
        }
    }

    #endregion

    #region Query Operations

    /// <inheritdoc />
    public async Task<EntityCollection> RetrieveMultipleAsync(QueryExpression query)
    {
        ArgumentNullException.ThrowIfNull(query);

        try
        {
            _logger.LogDebug("Executing QueryExpression for entity {EntityName} with {ConditionCount} conditions",
                query.EntityName, query.Criteria?.Conditions?.Count ?? 0);

            RetrieveMultipleRequest request = new() { Query = query };
            RetrieveMultipleResponse response = await ExecuteRequestWithRetryAsync<RetrieveMultipleResponse>(request);

            _logger.LogInformation("Query executed successfully, returned {EntityCount} records",
                response.EntityCollection.Entities.Count);

            return response.EntityCollection;
        }
        catch (Exception ex)
        {
            string errorMessage = $"Failed to execute QueryExpression for entity {query.EntityName}";
            _logger.LogError(ex, "Failed to execute QueryExpression for entity {EntityName}: {Message}",
                query.EntityName, ex.Message);
            throw new DataverseException(DataverseConstants.ErrorCodes.InvalidArgument, errorMessage, ex);
        }
    }

    /// <inheritdoc />
    public async Task<EntityCollection> RetrieveMultipleAsync(string fetchXml)
    {
        if (string.IsNullOrWhiteSpace(fetchXml))
            throw new ArgumentException("FetchXML cannot be null or empty", nameof(fetchXml));

        try
        {
            _logger.LogDebug("Executing FetchXML query");

            RetrieveMultipleRequest request = new() { Query = new FetchExpression(fetchXml) };
            RetrieveMultipleResponse response = await ExecuteRequestWithRetryAsync<RetrieveMultipleResponse>(request);

            _logger.LogInformation("FetchXML query executed successfully, returned {EntityCount} records",
                response.EntityCollection.Entities.Count);

            return response.EntityCollection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute FetchXML query");
            throw new DataverseException(DataverseConstants.ErrorCodes.InvalidArgument,
                "Failed to execute FetchXML query", ex);
        }
    }

    #endregion

    #region Validation Operations

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateTableAccessAsync(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

        try
        {
            _logger.LogDebug("Validating table access for {TableName}", tableName);

            ValidationResult result = new() { IsValid = true, TableName = tableName, Errors = [], Warnings = [] };

            try
            {
                // Test table access with a simple query
                QueryExpression testQuery = new(tableName)
                {
                    ColumnSet = new ColumnSet(false), // No columns needed, just test access
                    TopCount = 1
                };

                await RetrieveMultipleAsync(testQuery);
                _logger.LogInformation("Table access validation for {TableName} successful", tableName);
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Table access failed: {ex.Message}");
                _logger.LogWarning("Table access validation failed for {TableName}: {Error}", tableName, ex.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Table access validation failed for {TableName}", tableName);
            throw new DataverseValidationException($"Table access validation failed for {tableName}: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateSchemaAsync(string tableName, IEnumerable<string> expectedColumns)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
        ArgumentNullException.ThrowIfNull(expectedColumns);

        try
        {
            _logger.LogDebug("Validating schema for {TableName} with {ColumnCount} expected columns",
                tableName, expectedColumns.Count());

            List<string> expectedColumnsList = [.. expectedColumns];
            ValidationResult result = new() { IsValid = true, TableName = tableName, Errors = [], Warnings = [] };

            try
            {
                // Test column access by attempting to query with the expected columns
                QueryExpression schemaQuery = new(tableName)
                {
                    ColumnSet = new ColumnSet([.. expectedColumnsList]),
                    TopCount = 1
                };

                await RetrieveMultipleAsync(schemaQuery);
                _logger.LogInformation(
                    "Schema validation for {TableName} successful - all {ColumnCount} columns accessible",
                    tableName, expectedColumnsList.Count);
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Schema validation failed: {ex.Message}");

                // Try to identify which specific columns might be missing
                await ValidateIndividualColumnsAsync(tableName, expectedColumnsList, result);

                _logger.LogWarning("Schema validation failed for {TableName}: {Error}", tableName, ex.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schema validation failed for {TableName}", tableName);
            throw new DataverseValidationException($"Schema validation failed for {tableName}: {ex.Message}");
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Executes a request with retry logic for handling transient failures.
    /// </summary>
    private async Task<TResponse> ExecuteRequestWithRetryAsync<TResponse>(OrganizationRequest request)
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
                    "Request failed with transient error on attempt {Attempt}/{MaxAttempts}. Retrying in {Delay}ms",
                    currentAttempt, maxRetries + 1, retryDelay);

                await Task.Delay(retryDelay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request failed on attempt {Attempt}/{MaxAttempts} with non-transient error",
                    currentAttempt + 1, maxRetries + 1);
                throw;
            }
        }

        throw new DataverseException(
            DataverseConstants.ErrorCodes.Timeout,
            $"Request failed after {maxRetries + 1} attempts");
    }

    /// <summary>
    /// Validates individual columns to identify specific missing columns.
    /// </summary>
    private async Task ValidateIndividualColumnsAsync(string tableName, List<string> expectedColumns,
        ValidationResult result)
    {
        List<string> missingColumns = [];

        foreach (string column in expectedColumns)
        {
            try
            {
                QueryExpression singleColumnQuery = new(tableName) { ColumnSet = new ColumnSet(column), TopCount = 1 };

                await RetrieveMultipleAsync(singleColumnQuery);
            }
            catch (Exception)
            {
                missingColumns.Add(column);
            }
        }

        if (missingColumns.Count > 0)
            result.Warnings.Add($"Missing or inaccessible columns: {string.Join(", ", missingColumns)}");
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Releases all resources used by the DataverseClient.
    /// </summary>
    public void Dispose() => Dispose(true);

    /// <summary>
    /// Releases the unmanaged resources used by the DataverseClient and optionally releases the managed resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                _serviceClient?.Dispose();
                _logger.LogInformation("DataverseClient disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occurred during DataverseClient disposal");
            }

            _disposed = true;
        }
    }

    #endregion
}
