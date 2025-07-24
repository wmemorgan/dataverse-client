using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Dataverse.Samples.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using DataverseValidationResult = Dataverse.Client.Models.ValidationResult;

namespace ConsoleDemo;

/// <summary>
/// Interface for Dataverse operations to support dependency injection and testing.
/// </summary>
public interface IDataverseOperations
{
    Task DemonstrateCustomTableCrudAsync(CrudOptions options);
    Task DemonstrateContactCrudAsync(CrudOptions options);
    Task DemonstrateBatchOperationsAsync(BatchOptions options);
    Task DemonstrateContactBatchOperationsAsync(BatchOptions options);
    Task DemonstrateTableManagementAsync(TableManagementOptions options);
    Task DemonstrateQueryOperationsAsync(QueryOptions options);
    Task DemonstrateValidationOperationsAsync(ValidationOptions options);
    Task DemonstratePerformanceTestingAsync(PerformanceOptions options);
}

/// <summary>
/// Implements core Dataverse operations for the demo application.
/// </summary>
public class DataverseOperations : IDataverseOperations
{
    private readonly IDataverseClient _dataverseClient;
    private readonly IDataverseMetadataClient _metadataClient;
    private readonly IUserInterface _userInterface;
    private readonly ILogger<DataverseOperations> _logger;

    private string? _testTableName;
    private readonly List<Guid> _createdRecordIds = [];

    public DataverseOperations(
        IDataverseClient dataverseClient,
        IDataverseMetadataClient metadataClient,
        IUserInterface userInterface,
        ILogger<DataverseOperations> logger)
    {
        _dataverseClient = dataverseClient;
        _metadataClient = metadataClient;
        _userInterface = userInterface;
        _logger = logger;
    }

    #region CRUD Operations

    public async Task DemonstrateCustomTableCrudAsync(CrudOptions options)
    {
        _userInterface.ShowInfo("Demonstrating Custom Table CRUD Operations");

        try
        {
            // Create test table if not exists
            _testTableName = await SampleDataGenerator.CreateTestTableAsync(_metadataClient);
            _userInterface.ShowSuccess($"Test table '{_testTableName}' is ready");

            // Create records
            if (options.IncludeCreate)
            {
                await CreateTestRecordsAsync(options.RecordCount);
            }

            // Retrieve records
            if (options.IncludeRetrieve && _createdRecordIds.Count > 0)
            {
                await RetrieveTestRecordsAsync();
            }

            // Update records
            if (options.IncludeUpdate && _createdRecordIds.Count > 0)
            {
                await UpdateTestRecordsAsync();
            }

            // Delete records
            if (options.IncludeDelete && options.CleanupAfter)
            {
                await DeleteTestRecordsAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Custom table CRUD demonstration failed");
            _userInterface.ShowError($"Custom table CRUD operations failed: {ex.Message}");
        }
    }

    public async Task DemonstrateContactCrudAsync(CrudOptions options)
    {
        _userInterface.ShowInfo("Demonstrating Contact Entity CRUD Operations");

        // First validate access to Contact entity
        DataverseValidationResult validation = await _dataverseClient.ValidateTableAccessAsync("contact");
        if (!validation.IsValid)
        {
            _userInterface.ShowError("Contact entity is not accessible in this environment");
            _userInterface.DisplayValidationResult(validation);
            return;
        }

        try
        {
            List<Guid> contactIds = [];

            // Create contacts
            if (options.IncludeCreate)
            {
                contactIds = await CreateTestContactsAsync(options.RecordCount);
            }

            // Retrieve contacts
            if (options.IncludeRetrieve && contactIds.Count > 0)
            {
                await RetrieveTestContactsAsync(contactIds);
            }

            // Update contacts
            if (options.IncludeUpdate && contactIds.Count > 0)
            {
                await UpdateTestContactsAsync(contactIds);
            }

            // Delete contacts
            if (options.IncludeDelete && options.CleanupAfter && contactIds.Count > 0)
            {
                await DeleteTestContactsAsync(contactIds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Contact CRUD demonstration failed");
            _userInterface.ShowError($"Contact CRUD operations failed: {ex.Message}");
        }
    }

    #endregion

    #region Batch Operations

    public async Task DemonstrateBatchOperationsAsync(BatchOptions options)
    {
        _userInterface.ShowInfo($"Demonstrating Batch Operations with {options.RecordCount} records");

        try
        {
            // Create test table
            _testTableName = await SampleDataGenerator.CreateTestTableAsync(_metadataClient);

            // Create batch configuration
            BatchConfiguration batchConfig = new()
            {
                BatchSize = options.BatchSize,
                EnableProgressReporting = options.EnableProgressReporting,
                ContinueOnError = true
            };

            if (options.EnableProgressReporting)
            {
                batchConfig.ProgressReporter = new Progress<BatchProgress>(progress =>
                    _userInterface.DisplayBatchProgress(progress));
            }

            // Test batch create
            await TestBatchCreateAsync(options.RecordCount, batchConfig);

            // Test batch retrieve
            if (_createdRecordIds.Count > 0)
            {
                await TestBatchRetrieveAsync(batchConfig);
            }

            // Test batch update
            if (_createdRecordIds.Count > 0)
            {
                await TestBatchUpdateAsync(batchConfig);
            }

            // Test batch delete
            if (options.CleanupAfter && _createdRecordIds.Count > 0)
            {
                await TestBatchDeleteAsync(batchConfig);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch operations demonstration failed");
            _userInterface.ShowError($"Batch operations failed: {ex.Message}");
        }
    }

    public async Task DemonstrateContactBatchOperationsAsync(BatchOptions options)
    {
        _userInterface.ShowInfo($"Demonstrating Contact Batch Operations with {options.RecordCount} records");

        // Validate access to Contact entity
        DataverseValidationResult validation = await _dataverseClient.ValidateTableAccessAsync("contact");
        if (!validation.IsValid)
        {
            _userInterface.ShowError("Contact entity is not accessible");
            return;
        }

        try
        {
            BatchConfiguration batchConfig = new()
            {
                BatchSize = options.BatchSize,
                EnableProgressReporting = options.EnableProgressReporting,
                ContinueOnError = true
            };

            if (options.EnableProgressReporting)
            {
                batchConfig.ProgressReporter = new Progress<BatchProgress>(progress =>
                    _userInterface.DisplayBatchProgress(progress));
            }

            await TestContactBatchOperationsAsync(options.RecordCount, batchConfig, options.CleanupAfter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Contact batch operations demonstration failed");
            _userInterface.ShowError($"Contact batch operations failed: {ex.Message}");
        }
    }

    #endregion

    #region Table Management

    public async Task DemonstrateTableManagementAsync(TableManagementOptions options)
    {
        _userInterface.ShowInfo("Demonstrating Table Management Operations");

        try
        {
            switch (options.Operation)
            {
                case TableOperation.Create:
                    await CreateCustomTableAsync();
                    break;
                case TableOperation.GetMetadata:
                    await GetTableMetadataAsync(options.TableName ?? "contact");
                    break;
                case TableOperation.CheckExists:
                    await CheckTableExistsAsync(options.TableName ?? "contact");
                    break;
                case TableOperation.Delete:
                    if (!string.IsNullOrEmpty(options.TableName))
                    {
                        await DeleteCustomTableAsync(options.TableName);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Table management demonstration failed");
            _userInterface.ShowError($"Table management operations failed: {ex.Message}");
        }
    }

    #endregion

    #region Query Operations

    public async Task DemonstrateQueryOperationsAsync(QueryOptions options)
    {
        _userInterface.ShowInfo($"Demonstrating Query Operations on {options.EntityName}");

        try
        {
            switch (options.QueryType)
            {
                case QueryType.QueryExpression:
                    await DemonstrateQueryExpressionAsync(options.EntityName);
                    break;
                case QueryType.FetchXml:
                    await DemonstrateFetchXmlAsync(options.EntityName);
                    break;
                case QueryType.Both:
                    await DemonstrateQueryExpressionAsync(options.EntityName);
                    await DemonstrateFetchXmlAsync(options.EntityName);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Query operations demonstration failed");
            _userInterface.ShowError($"Query operations failed: {ex.Message}");
        }
    }

    #endregion

    #region Validation Operations

    public async Task DemonstrateValidationOperationsAsync(ValidationOptions options)
    {
        _userInterface.ShowInfo($"Demonstrating Validation Operations for {options.TableName}");

        try
        {
            // Test table access validation
            if (options.ValidateTableAccess)
            {
                DataverseValidationResult accessResult = await _dataverseClient.ValidateTableAccessAsync(options.TableName);
                _userInterface.DisplayValidationResult(accessResult);
            }

            // Test schema validation
            if (options.ValidateSchema && options.ExpectedColumns?.Length > 0)
            {
                DataverseValidationResult schemaResult = await _dataverseClient.ValidateSchemaAsync(options.TableName, options.ExpectedColumns);
                _userInterface.DisplayValidationResult(schemaResult);
            }

            // Test connection validation
            if (options.ValidateConnection)
            {
                bool connectionValid = await _dataverseClient.ValidateConnectionAsync();
                _userInterface.ShowInfo($"Connection validation result: {(connectionValid ? "Valid" : "Invalid")}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validation operations demonstration failed");
            _userInterface.ShowError($"Validation operations failed: {ex.Message}");
        }
    }

    #endregion

    #region Performance Testing

    public async Task DemonstratePerformanceTestingAsync(PerformanceOptions options)
    {
        _userInterface.ShowInfo($"Running Performance Test: {options.TestType} with {options.RecordCount} records");

        try
        {
            switch (options.TestType)
            {
                case PerformanceTestType.BatchVsIndividual:
                    await ComparePerformanceAsync(options.RecordCount, options.BatchSize);
                    break;
                case PerformanceTestType.DifferentBatchSizes:
                    await TestDifferentBatchSizesAsync(options.RecordCount);
                    break;
                case PerformanceTestType.ConcurrentOperations:
                    await TestConcurrentOperationsAsync(options.RecordCount, options.BatchSize);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Performance testing demonstration failed");
            _userInterface.ShowError($"Performance testing failed: {ex.Message}");
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task CreateTestRecordsAsync(int count)
    {
        _userInterface.ShowInfo($"Creating {count} test records...");

        for (int i = 0; i < count; i++)
        {
            Entity record = SampleDataGenerator.CreateSampleTestRecord(i + 1);
            Guid id = await _dataverseClient.CreateAsync(record);
            _createdRecordIds.Add(id);
        }

        _userInterface.ShowSuccess($"Created {count} test records");
    }

    private async Task RetrieveTestRecordsAsync()
    {
        _userInterface.ShowInfo("Retrieving test records...");

        string[] columnNames = SampleDataGenerator.GetTestTableColumnNames();
        ColumnSet columns = new(columnNames);

        foreach (Guid recordId in _createdRecordIds.Take(3))
        {
            Entity record = await _dataverseClient.RetrieveAsync(_testTableName!, recordId, columns);
            _userInterface.DisplayEntityRecord(record, _testTableName!);
        }

        _userInterface.ShowSuccess($"Retrieved {Math.Min(3, _createdRecordIds.Count)} sample records");
    }

    private async Task UpdateTestRecordsAsync()
    {
        _userInterface.ShowInfo("Updating test records...");

        foreach (Guid recordId in _createdRecordIds.Take(2))
        {
            Entity updateRecord = new(_testTableName!, recordId)
            {
                [$"{_testTableName}_phone"] = "+1-555-UPDATED",
                [$"{_testTableName}_description"] = $"Updated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
            };

            await _dataverseClient.UpdateAsync(updateRecord);
        }

        _userInterface.ShowSuccess($"Updated {Math.Min(2, _createdRecordIds.Count)} test records");
    }

    private async Task DeleteTestRecordsAsync()
    {
        _userInterface.ShowInfo("Deleting test records...");

        int deletedCount = 0;
        foreach (Guid recordId in _createdRecordIds.ToList())
        {
            try
            {
                await _dataverseClient.DeleteAsync(_testTableName!, recordId);
                _createdRecordIds.Remove(recordId);
                deletedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete record {RecordId}", recordId);
            }
        }

        _userInterface.ShowSuccess($"Deleted {deletedCount} test records");
    }

    private async Task<List<Guid>> CreateTestContactsAsync(int count)
    {
        _userInterface.ShowInfo($"Creating {count} test contacts...");

        List<Guid> contactIds = [];
        for (int i = 0; i < count; i++)
        {
            Entity contact = SampleDataGenerator.CreateSampleContact(i + 1);
            Guid id = await _dataverseClient.CreateAsync(contact);
            contactIds.Add(id);
        }

        _userInterface.ShowSuccess($"Created {count} test contacts");
        return contactIds;
    }

    private async Task RetrieveTestContactsAsync(List<Guid> contactIds)
    {
        _userInterface.ShowInfo("Retrieving test contacts...");

        ColumnSet columns = new("firstname", "lastname", "emailaddress1", "telephone1", "jobtitle");

        foreach (Guid contactId in contactIds.Take(3))
        {
            Entity contact = await _dataverseClient.RetrieveAsync("contact", contactId, columns);
            _userInterface.DisplayEntityRecord(contact, "contact");
        }

        _userInterface.ShowSuccess($"Retrieved {Math.Min(3, contactIds.Count)} sample contacts");
    }

    private async Task UpdateTestContactsAsync(List<Guid> contactIds)
    {
        _userInterface.ShowInfo("Updating test contacts...");

        foreach (Guid contactId in contactIds.Take(2))
        {
            Entity updateContact = new("contact", contactId)
            {
                ["telephone1"] = "+1-555-UPDATED",
                ["jobtitle"] = "Updated Job Title",
                ["description"] = $"Updated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
            };

            await _dataverseClient.UpdateAsync(updateContact);
        }

        _userInterface.ShowSuccess($"Updated {Math.Min(2, contactIds.Count)} test contacts");
    }

    private async Task DeleteTestContactsAsync(List<Guid> contactIds)
    {
        _userInterface.ShowInfo("Deleting test contacts...");

        int deletedCount = 0;
        foreach (Guid contactId in contactIds)
        {
            try
            {
                await _dataverseClient.DeleteAsync("contact", contactId);
                deletedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete contact {ContactId}", contactId);
            }
        }

        _userInterface.ShowSuccess($"Deleted {deletedCount} test contacts");
    }

    private async Task TestBatchCreateAsync(int recordCount, BatchConfiguration config)
    {
        _userInterface.ShowInfo($"Testing batch create with {recordCount} records...");

        List<Entity> records = SampleDataGenerator.CreateBatchTestRecords(recordCount, config.BatchSize ?? 100, true);
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        BatchOperationResult result = await _dataverseClient.CreateBatchAsync(records, config);
        stopwatch.Stop();

        _createdRecordIds.AddRange(result.CreatedRecords.Select(er => er.Id));
        _userInterface.DisplayBatchResult(result, stopwatch.Elapsed);
    }

    private async Task TestBatchRetrieveAsync(BatchConfiguration config)
    {
        _userInterface.ShowInfo("Testing batch retrieve...");

        List<EntityReference> entityRefs = _createdRecordIds.Select(id => new EntityReference(_testTableName!, id)).ToList();
        ColumnSet columns = new(SampleDataGenerator.GetTestTableColumnNames());

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        BatchRetrieveResult result = await _dataverseClient.RetrieveBatchAsync(entityRefs, columns, config);
        stopwatch.Stop();

        _userInterface.DisplayBatchRetrieveResult(result, stopwatch.Elapsed);
    }

    private async Task TestBatchUpdateAsync(BatchConfiguration config)
    {
        _userInterface.ShowInfo("Testing batch update...");

        List<Entity> updateEntities = [];
        foreach (Guid recordId in _createdRecordIds.Take(_createdRecordIds.Count / 2))
        {
            Entity updateEntity = new(_testTableName!, recordId)
            {
                [$"{_testTableName}_phone"] = "+1-555-BATCH-UPD",
                [$"{_testTableName}_description"] = $"BATCH UPDATED: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
            };
            updateEntities.Add(updateEntity);
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        BatchOperationResult result = await _dataverseClient.UpdateBatchAsync(updateEntities, config);
        stopwatch.Stop();

        _userInterface.DisplayBatchResult(result, stopwatch.Elapsed);
    }

    private async Task TestBatchDeleteAsync(BatchConfiguration config)
    {
        _userInterface.ShowInfo("Testing batch delete...");

        List<EntityReference> entityRefs = _createdRecordIds.Select(id => new EntityReference(_testTableName!, id)).ToList();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        BatchOperationResult result = await _dataverseClient.DeleteBatchAsync(entityRefs, config);
        stopwatch.Stop();

        _createdRecordIds.Clear();
        _userInterface.DisplayBatchResult(result, stopwatch.Elapsed);
    }

    private async Task TestContactBatchOperationsAsync(int recordCount, BatchConfiguration config, bool cleanup)
    {
        List<Guid> contactIds = [];

        try
        {
            // Create batch of contacts
            _userInterface.ShowInfo($"Creating {recordCount} contacts via batch...");
            List<Entity> contacts = SampleDataGenerator.CreateSampleContacts(recordCount, true);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            BatchOperationResult createResult = await _dataverseClient.CreateBatchAsync(contacts, config);
            stopwatch.Stop();

            contactIds.AddRange(createResult.CreatedRecords.Select(er => er.Id));
            _userInterface.DisplayBatchResult(createResult, stopwatch.Elapsed);

            // Retrieve batch of contacts
            if (contactIds.Count > 0)
            {
                _userInterface.ShowInfo("Retrieving contacts via batch...");
                List<EntityReference> entityRefs = contactIds.Select(id => new EntityReference("contact", id)).ToList();
                ColumnSet columns = new("firstname", "lastname", "emailaddress1", "telephone1", "jobtitle");

                stopwatch.Restart();
                BatchRetrieveResult retrieveResult = await _dataverseClient.RetrieveBatchAsync(entityRefs, columns, config);
                stopwatch.Stop();

                _userInterface.DisplayBatchRetrieveResult(retrieveResult, stopwatch.Elapsed);
            }

            // Update batch of contacts
            if (contactIds.Count > 0)
            {
                _userInterface.ShowInfo("Updating contacts via batch...");
                List<Entity> updateContacts = contactIds.Take(contactIds.Count / 2).Select(id => new Entity("contact", id)
                {
                    ["jobtitle"] = "BATCH UPDATED - Senior Developer",
                    ["description"] = $"Contact updated via batch operation at {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                }).ToList();

                stopwatch.Restart();
                BatchOperationResult updateResult = await _dataverseClient.UpdateBatchAsync(updateContacts, config);
                stopwatch.Stop();

                _userInterface.DisplayBatchResult(updateResult, stopwatch.Elapsed);
            }

            // Delete batch of contacts
            if (cleanup && contactIds.Count > 0)
            {
                _userInterface.ShowInfo("Deleting contacts via batch...");
                List<EntityReference> entityRefs = contactIds.Select(id => new EntityReference("contact", id)).ToList();

                stopwatch.Restart();
                BatchOperationResult deleteResult = await _dataverseClient.DeleteBatchAsync(entityRefs, config);
                stopwatch.Stop();

                _userInterface.DisplayBatchResult(deleteResult, stopwatch.Elapsed);
                contactIds.Clear();
            }
        }
        finally
        {
            // Cleanup any remaining contacts
            if (contactIds.Count > 0)
            {
                _userInterface.ShowWarning($"Cleaning up {contactIds.Count} remaining test contacts...");
                foreach (Guid contactId in contactIds)
                {
                    try
                    {
                        await _dataverseClient.DeleteAsync("contact", contactId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete contact {ContactId}", contactId);
                    }
                }
            }
        }
    }

    private async Task CreateCustomTableAsync()
    {
        _userInterface.ShowInfo("Creating custom table...");

        TableDefinition tableDefinition = SampleDataGenerator.CreateTestTableDefinition();
        string tableName = await _metadataClient.CreateTableAsync(tableDefinition);

        _userInterface.ShowSuccess($"Created custom table: {tableName}");
        _testTableName = tableName;
    }

    private async Task GetTableMetadataAsync(string tableName)
    {
        _userInterface.ShowInfo($"Retrieving metadata for table: {tableName}");

        TableMetadata metadata = await _metadataClient.GetTableMetadataAsync(tableName);
        _userInterface.DisplayTableMetadata(metadata);
    }

    private async Task CheckTableExistsAsync(string tableName)
    {
        _userInterface.ShowInfo($"Checking if table exists: {tableName}");

        bool exists = await _metadataClient.TableExistsAsync(tableName);
        _userInterface.ShowInfo($"Table '{tableName}' exists: {exists}");
    }

    private async Task DeleteCustomTableAsync(string tableName)
    {
        _userInterface.ShowInfo($"Deleting table: {tableName}");

        await _metadataClient.DeleteTableAsync(tableName);
        _userInterface.ShowSuccess($"Deleted table: {tableName}");
    }

    private async Task DemonstrateQueryExpressionAsync(string entityName)
    {
        _userInterface.ShowInfo($"Executing QueryExpression on {entityName}");

        QueryExpression query = new(entityName)
        {
            ColumnSet = new ColumnSet(true),
            TopCount = 5
        };

        if (entityName == "contact")
        {
            query.Criteria = new FilterExpression
            {
                Conditions = { new ConditionExpression("statecode", ConditionOperator.Equal, 0) }
            };
        }

        EntityCollection results = await _dataverseClient.RetrieveMultipleAsync(query);
        _userInterface.DisplayQueryResults(results, "QueryExpression");
    }

    private async Task DemonstrateFetchXmlAsync(string entityName)
    {
        _userInterface.ShowInfo($"Executing FetchXML on {entityName}");

        string fetchXml = entityName == "contact"
            ? @"<fetch top='5'>
                <entity name='contact'>
                    <attribute name='fullname' />
                    <attribute name='emailaddress1' />
                    <attribute name='telephone1' />
                    <filter>
                        <condition attribute='statecode' operator='eq' value='0' />
                    </filter>
                </entity>
               </fetch>"
            : $@"<fetch top='5'>
                <entity name='{entityName}'>
                    <all-attributes />
                </entity>
               </fetch>";

        EntityCollection results = await _dataverseClient.RetrieveMultipleAsync(fetchXml);
        _userInterface.DisplayQueryResults(results, "FetchXML");
    }

    private async Task ComparePerformanceAsync(int recordCount, int batchSize)
    {
        _userInterface.ShowInfo($"Comparing Individual vs Batch Performance ({recordCount} records)");

        // Ensure test table exists
        _testTableName = await SampleDataGenerator.CreateTestTableAsync(_metadataClient);

        // Test individual operations
        _userInterface.ShowInfo("Testing individual operations...");
        var individualStopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        List<Guid> individualIds = [];
        for (int i = 0; i < recordCount; i++)
        {
            Entity record = SampleDataGenerator.CreateSampleTestRecord(i + 1);
            Guid id = await _dataverseClient.CreateAsync(record);
            individualIds.Add(id);
        }
        
        individualStopwatch.Stop();

        // Test batch operations
        _userInterface.ShowInfo("Testing batch operations...");
        var batchStopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        List<Entity> batchRecords = SampleDataGenerator.CreateBatchTestRecords(recordCount, batchSize, false);
        BatchOperationResult batchResult = await _dataverseClient.CreateBatchAsync(batchRecords, new BatchConfiguration { BatchSize = batchSize });
        
        batchStopwatch.Stop();

        _userInterface.DisplayPerformanceComparison(recordCount, individualStopwatch.Elapsed, batchStopwatch.Elapsed);

        // Cleanup
        await CleanupRecordsAsync([..individualIds, ..batchResult.CreatedRecords.Select(er => er.Id)]);
    }

    private async Task TestDifferentBatchSizesAsync(int recordCount)
    {
        _userInterface.ShowInfo($"Testing Different Batch Sizes ({recordCount} records)");

        // Ensure test table exists
        _testTableName = await SampleDataGenerator.CreateTestTableAsync(_metadataClient);

        int[] batchSizes = [10, 50, 100, 200];
        var results = new List<(int BatchSize, TimeSpan Duration, int SuccessCount)>();

        foreach (int batchSize in batchSizes)
        {
            _userInterface.ShowInfo($"Testing batch size: {batchSize}");

            List<Entity> records = SampleDataGenerator.CreateBatchTestRecords(recordCount, batchSize, false);
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            BatchOperationResult result = await _dataverseClient.CreateBatchAsync(records, new BatchConfiguration { BatchSize = batchSize });
            stopwatch.Stop();

            results.Add((batchSize, stopwatch.Elapsed, result.SuccessCount));

            // Cleanup
            await CleanupRecordsAsync(result.CreatedRecords.Select(er => er.Id).ToList());
        }

        _userInterface.DisplayBatchSizeComparison(results);
    }

    private async Task TestConcurrentOperationsAsync(int recordCount, int batchSize)
    {
        _userInterface.ShowInfo($"Testing Concurrent Operations ({recordCount} records, {batchSize} batch size)");

        // Ensure test table exists
        _testTableName = await SampleDataGenerator.CreateTestTableAsync(_metadataClient);

        int concurrentBatches = 3;
        int recordsPerBatch = recordCount / concurrentBatches;

        _userInterface.ShowInfo($"Running {concurrentBatches} concurrent batches of {recordsPerBatch} records each");

        List<Task<BatchOperationResult>> tasks = [];
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < concurrentBatches; i++)
        {
            List<Entity> batchRecords = SampleDataGenerator.CreateBatchTestRecords(recordsPerBatch, batchSize, false);
            tasks.Add(_dataverseClient.CreateBatchAsync(batchRecords, new BatchConfiguration { BatchSize = batchSize }));
        }

        BatchOperationResult[] results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        _userInterface.DisplayConcurrentOperationResults(results, stopwatch.Elapsed);

        // Cleanup
        List<Guid> allCreatedIds = results.SelectMany(r => r.CreatedRecords.Select(er => er.Id)).ToList();
        await CleanupRecordsAsync(allCreatedIds);
    }

    private async Task CleanupRecordsAsync(List<Guid> recordIds)
    {
        if (recordIds.Count == 0 || string.IsNullOrEmpty(_testTableName)) return;

        try
        {
            List<EntityReference> entityRefs = recordIds.Select(id => new EntityReference(_testTableName, id)).ToList();
            await _dataverseClient.DeleteBatchAsync(entityRefs, new BatchConfiguration { BatchSize = 100 });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup {RecordCount} records", recordIds.Count);
        }
    }

    #endregion
}
