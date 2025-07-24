namespace ConsoleDemo;

#region Enumerations

/// <summary>
/// Available demo options in the main menu.
/// </summary>
public enum DemoOption
{
    BasicCrudOperations,
    BatchOperations,
    TableManagement,
    QueryOperations,
    ValidationOperations,
    PerformanceTesting,
    Exit
}

/// <summary>
/// Entity types available for operations.
/// </summary>
public enum EntityType
{
    CustomTable,
    Contact
}

/// <summary>
/// Available table management operations.
/// </summary>
public enum TableOperation
{
    Create,
    GetMetadata,
    CheckExists,
    Delete
}

/// <summary>
/// Available query types for demonstration.
/// </summary>
public enum QueryType
{
    QueryExpression,
    FetchXml,
    Both
}

/// <summary>
/// Available performance test types.
/// </summary>
public enum PerformanceTestType
{
    BatchVsIndividual,
    DifferentBatchSizes,
    ConcurrentOperations
}

#endregion

#region Configuration Classes

/// <summary>
/// Configuration options for CRUD operations demonstration.
/// </summary>
public class CrudOptions
{
    /// <summary>
    /// Gets or sets the entity type to perform CRUD operations on.
    /// </summary>
    public EntityType EntityType { get; set; } = EntityType.CustomTable;

    /// <summary>
    /// Gets or sets the number of records to create for the demonstration.
    /// </summary>
    public int RecordCount { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether to include Create operations in the demonstration.
    /// </summary>
    public bool IncludeCreate { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include Retrieve operations in the demonstration.
    /// </summary>
    public bool IncludeRetrieve { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include Update operations in the demonstration.
    /// </summary>
    public bool IncludeUpdate { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include Delete operations in the demonstration.
    /// </summary>
    public bool IncludeDelete { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to cleanup records after the demonstration.
    /// </summary>
    public bool CleanupAfter { get; set; } = true;

    /// <summary>
    /// Returns a string representation of the CRUD options.
    /// </summary>
    public override string ToString() =>
        $"CrudOptions [Entity: {EntityType}, Records: {RecordCount}, " +
        $"Operations: {(IncludeCreate ? "C" : "")}{(IncludeRetrieve ? "R" : "")}" +
        $"{(IncludeUpdate ? "U" : "")}{(IncludeDelete ? "D" : "")}, Cleanup: {CleanupAfter}]";
}

/// <summary>
/// Configuration options for batch operations demonstration.
/// </summary>
public class BatchOptions
{
    /// <summary>
    /// Gets or sets the entity type to perform batch operations on.
    /// </summary>
    public EntityType EntityType { get; set; } = EntityType.CustomTable;

    /// <summary>
    /// Gets or sets the total number of records to process in batches.
    /// </summary>
    public int RecordCount { get; set; } = 100;

    /// <summary>
    /// Gets or sets the size of each batch for processing.
    /// </summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets whether to enable progress reporting during batch operations.
    /// </summary>
    public bool EnableProgressReporting { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to cleanup records after the demonstration.
    /// </summary>
    public bool CleanupAfter { get; set; } = true;

    /// <summary>
    /// Gets the number of batches that will be created based on record count and batch size.
    /// </summary>
    public int ExpectedBatchCount => (int)Math.Ceiling((double)RecordCount / BatchSize);

    /// <summary>
    /// Returns a string representation of the batch options.
    /// </summary>
    public override string ToString() =>
        $"BatchOptions [Entity: {EntityType}, Records: {RecordCount}, " +
        $"BatchSize: {BatchSize}, Batches: {ExpectedBatchCount}, " +
        $"Progress: {EnableProgressReporting}, Cleanup: {CleanupAfter}]";
}

/// <summary>
/// Configuration options for table management operations demonstration.
/// </summary>
public class TableManagementOptions
{
    /// <summary>
    /// Gets or sets the table management operation to perform.
    /// </summary>
    public TableOperation Operation { get; set; } = TableOperation.Create;

    /// <summary>
    /// Gets or sets the name of the table to operate on (for operations other than Create).
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets whether to include column management operations.
    /// </summary>
    public bool IncludeColumnOperations { get; set; } = false;

    /// <summary>
    /// Returns a string representation of the table management options.
    /// </summary>
    public override string ToString() =>
        $"TableManagementOptions [Operation: {Operation}, Table: {TableName ?? "N/A"}, " +
        $"ColumnOps: {IncludeColumnOperations}]";
}

/// <summary>
/// Configuration options for query operations demonstration.
/// </summary>
public class QueryOptions
{
    /// <summary>
    /// Gets or sets the name of the entity to query.
    /// </summary>
    public string EntityName { get; set; } = "contact";

    /// <summary>
    /// Gets or sets the type of query to demonstrate.
    /// </summary>
    public QueryType QueryType { get; set; } = QueryType.Both;

    /// <summary>
    /// Gets or sets the maximum number of records to retrieve in queries.
    /// </summary>
    public int MaxRecords { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to include advanced query scenarios.
    /// </summary>
    public bool IncludeAdvancedQueries { get; set; } = false;

    /// <summary>
    /// Returns a string representation of the query options.
    /// </summary>
    public override string ToString() =>
        $"QueryOptions [Entity: {EntityName}, Type: {QueryType}, " +
        $"MaxRecords: {MaxRecords}, Advanced: {IncludeAdvancedQueries}]";
}

/// <summary>
/// Configuration options for validation operations demonstration.
/// </summary>
public class ValidationOptions
{
    /// <summary>
    /// Gets or sets the name of the table to validate.
    /// </summary>
    public string TableName { get; set; } = "contact";

    /// <summary>
    /// Gets or sets whether to validate table access permissions.
    /// </summary>
    public bool ValidateTableAccess { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate the connection.
    /// </summary>
    public bool ValidateConnection { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate schema (column existence).
    /// </summary>
    public bool ValidateSchema { get; set; } = false;

    /// <summary>
    /// Gets or sets the expected columns to validate (when ValidateSchema is true).
    /// </summary>
    public string[]? ExpectedColumns { get; set; }

    /// <summary>
    /// Gets or sets whether to perform deep validation checks.
    /// </summary>
    public bool PerformDeepValidation { get; set; } = false;

    /// <summary>
    /// Returns a string representation of the validation options.
    /// </summary>
    public override string ToString() =>
        $"ValidationOptions [Table: {TableName}, Access: {ValidateTableAccess}, " +
        $"Connection: {ValidateConnection}, Schema: {ValidateSchema}, " +
        $"Columns: {ExpectedColumns?.Length ?? 0}, Deep: {PerformDeepValidation}]";
}

/// <summary>
/// Configuration options for performance testing demonstration.
/// </summary>
public class PerformanceOptions
{
    /// <summary>
    /// Gets or sets the type of performance test to run.
    /// </summary>
    public PerformanceTestType TestType { get; set; } = PerformanceTestType.BatchVsIndividual;

    /// <summary>
    /// Gets or sets the number of records to use in performance testing.
    /// </summary>
    public int RecordCount { get; set; } = 200;

    /// <summary>
    /// Gets or sets the batch size for batch operations testing.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the number of concurrent operations for concurrent testing.
    /// </summary>
    public int ConcurrentOperations { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to include cleanup in performance measurements.
    /// </summary>
    public bool IncludeCleanupInMeasurement { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to run multiple iterations for averaging.
    /// </summary>
    public bool RunMultipleIterations { get; set; } = false;

    /// <summary>
    /// Gets or sets the number of iterations to run (when RunMultipleIterations is true).
    /// </summary>
    public int IterationCount { get; set; } = 3;

    /// <summary>
    /// Returns a string representation of the performance options.
    /// </summary>
    public override string ToString() =>
        $"PerformanceOptions [Type: {TestType}, Records: {RecordCount}, " +
        $"BatchSize: {BatchSize}, Concurrent: {ConcurrentOperations}, " +
        $"Iterations: {(RunMultipleIterations ? IterationCount.ToString() : "1")}]";
}

#endregion