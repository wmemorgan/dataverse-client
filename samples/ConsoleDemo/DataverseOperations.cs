using System.Diagnostics;
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
public class DataverseOperations(
    IDataverseClient dataverseClient,
    IDataverseMetadataClient metadataClient,
    IUserInterface userInterface,
    ILogger<DataverseOperations> logger)
    : IDataverseOperations
{
    private string? _testTableName;
    private readonly List<Guid> _createdRecordIds = [];

    #region CRUD Operations

    public async Task DemonstrateCustomTableCrudAsync(CrudOptions options)
    {
        userInterface.ShowInfo("Demonstrating Custom Table CRUD Operations");

        try
        {
            // Create test table if not exists
            _testTableName = await SampleDataGenerator.CreateTestTableAsync(metadataClient);
            userInterface.ShowSuccess($"Test table '{_testTableName}' is ready");

            // Create records
            if (options.IncludeCreate) await CreateTestRecordsAsync(options.RecordCount);

            // Retrieve records
            if (options.IncludeRetrieve && _createdRecordIds.Count > 0) await RetrieveTestRecordsAsync();

            // Update records
            if (options.IncludeUpdate && _createdRecordIds.Count > 0) await UpdateTestRecordsAsync();

            // Delete records and optionally table
            if (options.IncludeDelete ||
                (options.CleanupAfter && options.TableCleanupOption != TableCleanupOption.None))
            {
                await HandleCustomTableCleanupAsync(options.TableCleanupOption, options.CleanupAfter);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Custom table CRUD demonstration failed");
            userInterface.ShowError($"Custom table CRUD operations failed: {ex.Message}");
        }
    }

    public async Task DemonstrateContactCrudAsync(CrudOptions options)
    {
        userInterface.ShowInfo("Demonstrating Contact Entity CRUD Operations");

        // First validate access to Contact entity
        DataverseValidationResult validation = await dataverseClient.ValidateTableAccessAsync("contact");
        if (!validation.IsValid)
        {
            userInterface.ShowError("Contact entity is not accessible in this environment");
            userInterface.DisplayValidationResult(validation);
            return;
        }

        try
        {
            List<Guid> contactIds = [];

            // Create contacts
            if (options.IncludeCreate) contactIds = await CreateTestContactsAsync(options.RecordCount);

            // Retrieve contacts
            if (options.IncludeRetrieve && contactIds.Count > 0) await RetrieveTestContactsAsync(contactIds);

            // Update contacts
            if (options.IncludeUpdate && contactIds.Count > 0) await UpdateTestContactsAsync(contactIds);

            // Delete contacts
            if (options.IncludeDelete && options.CleanupAfter && contactIds.Count > 0)
                await DeleteTestContactsAsync(contactIds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Contact CRUD demonstration failed");
            userInterface.ShowError($"Contact CRUD operations failed: {ex.Message}");
        }
    }

    #endregion

    #region Batch Operations

    public async Task DemonstrateBatchOperationsAsync(BatchOptions options)
    {
        userInterface.ShowInfo($"Demonstrating Batch Operations with {options.RecordCount} records");

        try
        {
            // Create test table
            _testTableName = await SampleDataGenerator.CreateTestTableAsync(metadataClient);

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
                    userInterface.DisplayBatchProgress(progress));
            }

            // Test batch create
            await TestBatchCreateAsync(options.RecordCount, batchConfig);

            // Test batch retrieve
            if (_createdRecordIds.Count > 0) await TestBatchRetrieveAsync(batchConfig);

            // Test batch update
            if (_createdRecordIds.Count > 0) await TestBatchUpdateAsync(batchConfig);

            // Handle cleanup based on options
            if (options.CleanupAfter && options.TableCleanupOption != TableCleanupOption.None)
                await HandleCustomTableCleanupAsync(options.TableCleanupOption, true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Batch operations demonstration failed");
            userInterface.ShowError($"Batch operations failed: {ex.Message}");
        }
    }

    public async Task DemonstrateContactBatchOperationsAsync(BatchOptions options)
    {
        userInterface.ShowInfo($"Demonstrating Contact Batch Operations with {options.RecordCount} records");

        // Validate access to Contact entity
        DataverseValidationResult validation = await dataverseClient.ValidateTableAccessAsync("contact");
        if (!validation.IsValid)
        {
            userInterface.ShowError("Contact entity is not accessible");
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
                    userInterface.DisplayBatchProgress(progress));
            }

            await TestContactBatchOperationsAsync(options.RecordCount, batchConfig, options.CleanupAfter);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Contact batch operations demonstration failed");
            userInterface.ShowError($"Contact batch operations failed: {ex.Message}");
        }
    }

    #endregion

    #region Table Management

    public async Task DemonstrateTableManagementAsync(TableManagementOptions options)
    {
        userInterface.ShowInfo("Demonstrating Table Management Operations");

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
                    if (!string.IsNullOrEmpty(options.TableName)) await DeleteCustomTableAsync(options.TableName);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Table management demonstration failed");
            userInterface.ShowError($"Table management operations failed: {ex.Message}");
        }
    }

    #endregion

    #region Query Operations

    public async Task DemonstrateQueryOperationsAsync(QueryOptions options)
    {
        userInterface.ShowInfo($"Demonstrating Query Operations on {options.EntityName}");

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
            logger.LogError(ex, "Query operations demonstration failed");
            userInterface.ShowError($"Query operations failed: {ex.Message}");
        }
    }

    #endregion

    #region Validation Operations

    public async Task DemonstrateValidationOperationsAsync(ValidationOptions options)
    {
        userInterface.ShowInfo($"Demonstrating Validation Operations for {options.TableName}");

        try
        {
            // Test table access validation
            if (options.ValidateTableAccess)
            {
                DataverseValidationResult accessResult =
                    await dataverseClient.ValidateTableAccessAsync(options.TableName);
                userInterface.DisplayValidationResult(accessResult);
            }

            // Test schema validation
            if (options.ValidateSchema && options.ExpectedColumns?.Length > 0)
            {
                DataverseValidationResult schemaResult =
                    await dataverseClient.ValidateSchemaAsync(options.TableName, options.ExpectedColumns);
                userInterface.DisplayValidationResult(schemaResult);
            }

            // Test connection validation
            if (options.ValidateConnection)
            {
                bool connectionValid = await dataverseClient.ValidateConnectionAsync();
                userInterface.ShowInfo($"Connection validation result: {(connectionValid ? "Valid" : "Invalid")}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Validation operations demonstration failed");
            userInterface.ShowError($"Validation operations failed: {ex.Message}");
        }
    }

    #endregion

    #region Performance Testing

    public async Task DemonstratePerformanceTestingAsync(PerformanceOptions options)
    {
        userInterface.ShowInfo($"Running Performance Test: {options.TestType} with {options.RecordCount} records");

        try
        {
            switch (options.TestType)
            {
                case PerformanceTestType.BatchVsIndividual:
                    await ComparePerformanceAsync(options.RecordCount, options.BatchSize, options.TableCleanupOption);
                    break;
                case PerformanceTestType.DifferentBatchSizes:
                    await TestDifferentBatchSizesAsync(options.RecordCount, options.TableCleanupOption);
                    break;
                case PerformanceTestType.ConcurrentOperations:
                    await TestConcurrentOperationsAsync(options.RecordCount, options.BatchSize,
                        options.TableCleanupOption);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Performance testing demonstration failed");
            userInterface.ShowError($"Performance testing failed: {ex.Message}");
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task CreateTestRecordsAsync(int count)
    {
        userInterface.ShowInfo($"Creating {count} test records...");

        for (int i = 0; i < count; i++)
        {
            Entity record = SampleDataGenerator.CreateSampleTestRecord(i + 1);
            Guid id = await dataverseClient.CreateAsync(record);
            _createdRecordIds.Add(id);
        }

        userInterface.ShowSuccess($"Created {count} test records");
    }

    private async Task RetrieveTestRecordsAsync()
    {
        userInterface.ShowInfo("Retrieving test records...");

        string[] columnNames = SampleDataGenerator.GetTestTableColumnNames();
        ColumnSet columns = new(columnNames);

        foreach (Guid recordId in _createdRecordIds.Take(3))
        {
            Entity record = await dataverseClient.RetrieveAsync(_testTableName!, recordId, columns);
            userInterface.DisplayEntityRecord(record, _testTableName!);
        }

        userInterface.ShowSuccess($"Retrieved {Math.Min(3, _createdRecordIds.Count)} sample records");
    }

    private async Task UpdateTestRecordsAsync()
    {
        userInterface.ShowInfo("Updating test records...");

        foreach (Guid recordId in _createdRecordIds.Take(2))
        {
            Entity updateRecord = new(_testTableName!, recordId)
            {
                [$"{_testTableName}_phone"] = "+1-555-UPDATED",
                [$"{_testTableName}_description"] = $"Updated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
            };

            await dataverseClient.UpdateAsync(updateRecord);
        }

        userInterface.ShowSuccess($"Updated {Math.Min(2, _createdRecordIds.Count)} test records");
    }

    private async Task DeleteTestRecordsAsync()
    {
        userInterface.ShowInfo("Deleting test records...");

        int deletedCount = 0;
        foreach (Guid recordId in _createdRecordIds.ToList())
        {
            try
            {
                await dataverseClient.DeleteAsync(_testTableName!, recordId);
                _createdRecordIds.Remove(recordId);
                deletedCount++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to delete record {RecordId}", recordId);
            }
        }

        userInterface.ShowSuccess($"Deleted {deletedCount} test records");
    }

    private async Task<List<Guid>> CreateTestContactsAsync(int count)
    {
        userInterface.ShowInfo($"Creating {count} test contacts...");

        List<Guid> contactIds = [];
        for (int i = 0; i < count; i++)
        {
            Entity contact = SampleDataGenerator.CreateSampleContact(i + 1);
            Guid id = await dataverseClient.CreateAsync(contact);
            contactIds.Add(id);
        }

        userInterface.ShowSuccess($"Created {count} test contacts");
        return contactIds;
    }

    private async Task RetrieveTestContactsAsync(List<Guid> contactIds)
    {
        userInterface.ShowInfo("Retrieving test contacts...");

        ColumnSet columns = new("firstname", "lastname", "emailaddress1", "telephone1", "jobtitle");

        foreach (Guid contactId in contactIds.Take(3))
        {
            Entity contact = await dataverseClient.RetrieveAsync("contact", contactId, columns);
            userInterface.DisplayEntityRecord(contact, "contact");
        }

        userInterface.ShowSuccess($"Retrieved {Math.Min(3, contactIds.Count)} sample contacts");
    }

    private async Task UpdateTestContactsAsync(List<Guid> contactIds)
    {
        userInterface.ShowInfo("Updating test contacts...");

        foreach (Guid contactId in contactIds.Take(2))
        {
            Entity updateContact = new("contact", contactId)
            {
                ["telephone1"] = "+1-555-UPDATED",
                ["jobtitle"] = "Updated Job Title",
                ["description"] = $"Updated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
            };

            await dataverseClient.UpdateAsync(updateContact);
        }

        userInterface.ShowSuccess($"Updated {Math.Min(2, contactIds.Count)} test contacts");
    }

    private async Task DeleteTestContactsAsync(List<Guid> contactIds)
    {
        userInterface.ShowInfo("Deleting test contacts...");

        int deletedCount = 0;
        foreach (Guid contactId in contactIds)
        {
            try
            {
                await dataverseClient.DeleteAsync("contact", contactId);
                deletedCount++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to delete contact {ContactId}", contactId);
            }
        }

        userInterface.ShowSuccess($"Deleted {deletedCount} test contacts");
    }

    private async Task TestBatchCreateAsync(int recordCount, BatchConfiguration config)
    {
        userInterface.ShowInfo($"Testing batch create with {recordCount} records...");

        List<Entity> records = SampleDataGenerator.CreateBatchTestRecords(recordCount, config.BatchSize ?? 100, true);

        Stopwatch stopwatch = Stopwatch.StartNew();
        BatchOperationResult result = await dataverseClient.CreateBatchAsync(records, config);
        stopwatch.Stop();

        _createdRecordIds.AddRange(result.CreatedRecords.Select(er => er.Id));
        userInterface.DisplayBatchResult(result, stopwatch.Elapsed);
    }

    private async Task TestBatchRetrieveAsync(BatchConfiguration config)
    {
        userInterface.ShowInfo("Testing batch retrieve...");

        List<EntityReference> entityRefs =
            [.. _createdRecordIds.Select(id => new EntityReference(_testTableName!, id))];
        ColumnSet columns = new(SampleDataGenerator.GetTestTableColumnNames());

        Stopwatch stopwatch = Stopwatch.StartNew();
        BatchRetrieveResult result = await dataverseClient.RetrieveBatchAsync(entityRefs, columns, config);
        stopwatch.Stop();

        userInterface.DisplayBatchRetrieveResult(result, stopwatch.Elapsed);
    }

    private async Task TestBatchUpdateAsync(BatchConfiguration config)
    {
        userInterface.ShowInfo("Testing batch update...");

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

        Stopwatch stopwatch = Stopwatch.StartNew();
        BatchOperationResult result = await dataverseClient.UpdateBatchAsync(updateEntities, config);
        stopwatch.Stop();

        userInterface.DisplayBatchResult(result, stopwatch.Elapsed);
    }

    private async Task TestContactBatchOperationsAsync(int recordCount, BatchConfiguration config, bool cleanup)
    {
        List<Guid> contactIds = [];

        try
        {
            // Create batch of contacts
            userInterface.ShowInfo($"Creating {recordCount} contacts via batch...");
            List<Entity> contacts = SampleDataGenerator.CreateSampleContacts(recordCount, true);

            Stopwatch stopwatch = Stopwatch.StartNew();
            BatchOperationResult createResult = await dataverseClient.CreateBatchAsync(contacts, config);
            stopwatch.Stop();

            contactIds.AddRange(createResult.CreatedRecords.Select(er => er.Id));
            userInterface.DisplayBatchResult(createResult, stopwatch.Elapsed);

            // Retrieve batch of contacts
            if (contactIds.Count > 0)
            {
                userInterface.ShowInfo("Retrieving contacts via batch...");
                List<EntityReference> entityRefs = [.. contactIds.Select(id => new EntityReference("contact", id))];
                ColumnSet columns = new("firstname", "lastname", "emailaddress1", "telephone1", "jobtitle");

                stopwatch.Restart();
                BatchRetrieveResult retrieveResult =
                    await dataverseClient.RetrieveBatchAsync(entityRefs, columns, config);
                stopwatch.Stop();

                userInterface.DisplayBatchRetrieveResult(retrieveResult, stopwatch.Elapsed);
            }

            // Update batch of contacts
            if (contactIds.Count > 0)
            {
                userInterface.ShowInfo("Updating contacts via batch...");
                List<Entity> updateContacts = [.. contactIds.Take(contactIds.Count / 2).Select(id =>
                    new Entity("contact", id)
                    {
                        ["jobtitle"] = "BATCH UPDATED - Senior Developer",
                        ["description"] =
                            $"Contact updated via batch operation at {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                    })];

                stopwatch.Restart();
                BatchOperationResult updateResult = await dataverseClient.UpdateBatchAsync(updateContacts, config);
                stopwatch.Stop();

                userInterface.DisplayBatchResult(updateResult, stopwatch.Elapsed);
            }

            // Delete batch of contacts
            if (cleanup && contactIds.Count > 0)
            {
                userInterface.ShowInfo("Deleting contacts via batch...");
                List<EntityReference> entityRefs = [.. contactIds.Select(id => new EntityReference("contact", id))];

                stopwatch.Restart();
                BatchOperationResult deleteResult = await dataverseClient.DeleteBatchAsync(entityRefs, config);
                stopwatch.Stop();

                userInterface.DisplayBatchResult(deleteResult, stopwatch.Elapsed);
                contactIds.Clear();
            }
        }
        finally
        {
            // Cleanup any remaining contacts
            if (contactIds.Count > 0)
            {
                userInterface.ShowWarning($"Cleaning up {contactIds.Count} remaining test contacts...");
                foreach (Guid contactId in contactIds)
                {
                    try
                    {
                        await dataverseClient.DeleteAsync("contact", contactId);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to delete contact {ContactId}", contactId);
                    }
                }
            }
        }
    }

    private async Task CreateCustomTableAsync()
    {
        userInterface.ShowInfo("Creating custom table...");

        TableDefinition tableDefinition = SampleDataGenerator.CreateTestTableDefinition();
        string tableName = await metadataClient.CreateTableAsync(tableDefinition);

        userInterface.ShowSuccess($"Created custom table: {tableName}");
        _testTableName = tableName;
    }

    private async Task GetTableMetadataAsync(string tableName)
    {
        userInterface.ShowInfo($"Retrieving metadata for table: {tableName}");

        TableMetadata metadata = await metadataClient.GetTableMetadataAsync(tableName);
        userInterface.DisplayTableMetadata(metadata);
    }

    private async Task CheckTableExistsAsync(string tableName)
    {
        userInterface.ShowInfo($"Checking if table exists: {tableName}");

        bool exists = await metadataClient.TableExistsAsync(tableName);
        userInterface.ShowInfo($"Table '{tableName}' exists: {exists}");
    }

    /// <summary>
    /// Deletes a custom table and displays appropriate user feedback.
    /// </summary>
    private async Task DeleteCustomTableAsync(string tableName)
    {
        try
        {
            userInterface.ShowInfo($"Deleting custom table '{tableName}'...");

            // Check if table exists before attempting deletion
            if (await metadataClient.TableExistsAsync(tableName))
            {
                await metadataClient.DeleteTableAsync(tableName);
                userInterface.ShowSuccess($"Custom table '{tableName}' deleted successfully");
            }
            else
            {
                userInterface.ShowWarning($"Custom table '{tableName}' no longer exists");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete custom table {TableName}", tableName);
            userInterface.ShowError($"Failed to delete custom table '{tableName}': {ex.Message}");
            throw;
        }
    }

    private async Task DemonstrateQueryExpressionAsync(string entityName)
    {
        userInterface.ShowInfo($"Executing QueryExpression on {entityName}");

        QueryExpression query = new(entityName) { ColumnSet = new ColumnSet(true), TopCount = 5 };

        if (entityName == "contact")
        {
            query.Criteria = new FilterExpression
            {
                Conditions = { new ConditionExpression("statecode", ConditionOperator.Equal, 0) }
            };
        }

        EntityCollection results = await dataverseClient.RetrieveMultipleAsync(query);
        userInterface.DisplayQueryResults(results, "QueryExpression");
    }

    private async Task DemonstrateFetchXmlAsync(string entityName)
    {
        userInterface.ShowInfo($"Executing FetchXML on {entityName}");

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

        EntityCollection results = await dataverseClient.RetrieveMultipleAsync(fetchXml);
        userInterface.DisplayQueryResults(results, "FetchXML");
    }

    private async Task ComparePerformanceAsync(int recordCount, int batchSize, TableCleanupOption cleanupOption)
    {
        userInterface.ShowInfo($"Comparing Individual vs Batch Performance ({recordCount} records)");

        // Ensure test table exists
        _testTableName = await SampleDataGenerator.CreateTestTableAsync(metadataClient);

        try
        {
            // Test individual operations
            userInterface.ShowInfo("Testing individual operations...");
            Stopwatch individualStopwatch = Stopwatch.StartNew();

            List<Guid> individualIds = [];
            for (int i = 0; i < recordCount; i++)
            {
                Entity record = SampleDataGenerator.CreateSampleTestRecord(i + 1);
                Guid id = await dataverseClient.CreateAsync(record);
                individualIds.Add(id);
            }

            individualStopwatch.Stop();

            // Test batch operations
            userInterface.ShowInfo("Testing batch operations...");
            Stopwatch batchStopwatch = Stopwatch.StartNew();

            List<Entity> batchRecords = SampleDataGenerator.CreateBatchTestRecords(recordCount, batchSize, false);
            BatchOperationResult batchResult =
                await dataverseClient.CreateBatchAsync(batchRecords, new BatchConfiguration { BatchSize = batchSize });

            batchStopwatch.Stop();

            userInterface.DisplayPerformanceComparison(recordCount, individualStopwatch.Elapsed,
                batchStopwatch.Elapsed);

            // Cleanup based on option
            List<Guid> allRecordIds = [.. individualIds, .. batchResult.CreatedRecords.Select(er => er.Id)];
            await CleanupRecordsAsync(allRecordIds);

            if (cleanupOption == TableCleanupOption.RecordsAndTable)
            {
                await DeleteCustomTableAsync(_testTableName!);
                _testTableName = null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Performance comparison failed");
            throw;
        }
    }

    private async Task TestDifferentBatchSizesAsync(int recordCount, TableCleanupOption cleanupOption)
    {
        userInterface.ShowInfo($"Testing Different Batch Sizes ({recordCount} records)");

        // Ensure test table exists
        _testTableName = await SampleDataGenerator.CreateTestTableAsync(metadataClient);

        try
        {
            int[] batchSizes = [10, 50, 100, 200];
            List<(int BatchSize, TimeSpan Duration, int SuccessCount)> results = [];

            foreach (int batchSize in batchSizes)
            {
                userInterface.ShowInfo($"Testing batch size: {batchSize}");

                List<Entity> records = SampleDataGenerator.CreateBatchTestRecords(recordCount, batchSize, false);

                Stopwatch stopwatch = Stopwatch.StartNew();
                BatchOperationResult result =
                    await dataverseClient.CreateBatchAsync(records, new BatchConfiguration { BatchSize = batchSize });
                stopwatch.Stop();

                results.Add((batchSize, stopwatch.Elapsed, result.SuccessCount));

                // Cleanup records immediately after each test
                await CleanupRecordsAsync([.. result.CreatedRecords.Select(er => er.Id)]);
            }

            userInterface.DisplayBatchSizeComparison(results);

            // Handle table cleanup
            if (cleanupOption == TableCleanupOption.RecordsAndTable)
            {
                await DeleteCustomTableAsync(_testTableName!);
                _testTableName = null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Batch size testing failed");
            throw;
        }
    }

    private async Task TestConcurrentOperationsAsync(int recordCount, int batchSize, TableCleanupOption cleanupOption)
    {
        userInterface.ShowInfo($"Testing Concurrent Operations ({recordCount} records, {batchSize} batch size)");

        // Ensure test table exists
        _testTableName = await SampleDataGenerator.CreateTestTableAsync(metadataClient);

        try
        {
            int concurrentBatches = 3;
            int recordsPerBatch = recordCount / concurrentBatches;

            userInterface.ShowInfo(
                $"Running {concurrentBatches} concurrent batches of {recordsPerBatch} records each");

            List<Task<BatchOperationResult>> tasks = [];
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < concurrentBatches; i++)
            {
                List<Entity> batchRecords =
                    SampleDataGenerator.CreateBatchTestRecords(recordsPerBatch, batchSize, false);
                tasks.Add(dataverseClient.CreateBatchAsync(batchRecords,
                    new BatchConfiguration { BatchSize = batchSize }));
            }

            BatchOperationResult[] results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            userInterface.DisplayConcurrentOperationResults(results, stopwatch.Elapsed);

            // Cleanup records
            List<Guid> allCreatedIds = [.. results.SelectMany(r => r.CreatedRecords.Select(er => er.Id))];
            await CleanupRecordsAsync(allCreatedIds);

            // Handle table cleanup
            if (cleanupOption == TableCleanupOption.RecordsAndTable)
            {
                await DeleteCustomTableAsync(_testTableName!);
                _testTableName = null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Concurrent operations testing failed");
            throw;
        }
    }

    private async Task CleanupRecordsAsync(List<Guid> recordIds)
    {
        if (recordIds.Count == 0 || string.IsNullOrEmpty(_testTableName)) return;

        try
        {
            List<EntityReference> entityRefs = [.. recordIds.Select(id => new EntityReference(_testTableName, id))];
            await dataverseClient.DeleteBatchAsync(entityRefs, new BatchConfiguration { BatchSize = 100 });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to cleanup {RecordCount} records", recordIds.Count);
        }
    }

    /// <summary>
    /// Handles cleanup of custom table records and optionally the table itself.
    /// </summary>
    private async Task HandleCustomTableCleanupAsync(TableCleanupOption cleanupOption, bool shouldCleanup)
    {
        if (!shouldCleanup || cleanupOption == TableCleanupOption.None)
        {
            userInterface.ShowInfo("Skipping cleanup as requested");
            return;
        }

        try
        {
            switch (cleanupOption)
            {
                case TableCleanupOption.RecordsOnly:
                    await DeleteTestRecordsAsync();
                    userInterface.ShowInfo($"Custom table '{_testTableName}' preserved for future use");
                    break;

                case TableCleanupOption.RecordsAndTable:
                    await DeleteTestRecordsAsync();
                    if (!string.IsNullOrEmpty(_testTableName))
                    {
                        await DeleteCustomTableAsync(_testTableName);
                        _testTableName = null; // Reset to indicate table is gone
                    }

                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to perform cleanup with option {CleanupOption}", cleanupOption);
            userInterface.ShowError($"Cleanup failed: {ex.Message}");
        }
    }

    #endregion
}
