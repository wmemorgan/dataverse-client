using Microsoft.Xrm.Sdk;

namespace Dataverse.Client.Models;

/// <summary>
/// Represents the result of a batch operation in Microsoft Dataverse.
/// Contains comprehensive information about the operation including success/failure counts,
/// timing metrics, and error details.
/// </summary>
public class BatchOperationResult
{
    /// <summary>
    /// Gets or sets the type of batch operation that was performed.
    /// </summary>
    public BatchOperationType OperationType { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for this batch operation.
    /// </summary>
    public string OperationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the batch operation started.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the batch operation ended.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the total duration of the batch operation.
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Gets or sets the number of records processed successfully.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the number of records that failed to process.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Gets the total number of records processed.
    /// </summary>
    public int TotalRecords => SuccessCount + FailureCount;

    /// <summary>
    /// Gets or sets detailed information about errors that occurred during processing.
    /// </summary>
    public List<BatchError> Errors { get; set; } = [];

    /// <summary>
    /// Gets or sets the entity references of successfully created records.
    /// </summary>
    public List<EntityReference> CreatedRecords { get; set; } = [];

    /// <summary>
    /// Gets or sets the entity references of successfully updated records.
    /// </summary>
    public List<EntityReference> UpdatedRecords { get; set; } = [];

    /// <summary>
    /// Gets or sets the entity references of successfully deleted records.
    /// </summary>
    public List<EntityReference> DeletedRecords { get; set; } = [];

    /// <summary>
    /// Gets the success rate as a percentage.
    /// </summary>
    public double SuccessRate => TotalRecords > 0 ? (double)SuccessCount / TotalRecords * 100 : 0;

    /// <summary>
    /// Gets a value indicating whether the batch operation had any errors.
    /// </summary>
    public bool HasErrors => Errors.Count > 0;

    /// <summary>
    /// Initializes a new instance of the BatchOperationResult class.
    /// </summary>
    public BatchOperationResult()
    {
        OperationId = $"BATCH-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..8]}";
        StartTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the BatchOperationResult class with the specified operation type.
    /// </summary>
    public BatchOperationResult(BatchOperationType operationType) : this() => OperationType = operationType;

    /// <summary>
    /// Marks the batch operation as completed and calculates final metrics.
    /// </summary>
    public void MarkCompleted()
    {
        EndTime = DateTime.UtcNow;
        Duration = EndTime - StartTime;
    }

    public override string ToString() =>
        $"BatchOperationResult [{OperationType}] - Total: {TotalRecords:N0}, Success: {SuccessCount:N0}, Failed: {FailureCount:N0}";
}

/// <summary>
/// Represents the result of a batch retrieve operation.
/// </summary>
public class BatchRetrieveResult : BatchOperationResult
{
    /// <summary>
    /// Gets or sets the entities that were successfully retrieved from Dataverse.
    /// </summary>
    public List<Entity> RetrievedEntities { get; set; } = [];

    /// <summary>
    /// Gets or sets the entity references that were requested but not found.
    /// </summary>
    public List<EntityReference> NotFoundReferences { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the BatchRetrieveResult class.
    /// </summary>
    public BatchRetrieveResult() : base(BatchOperationType.BatchRetrieve) { }
}

/// <summary>
/// Represents an error that occurred during a batch operation in Dataverse.
/// </summary>
public class BatchError
{
    /// <summary>
    /// Gets or sets the batch number where this error occurred (1-based).
    /// </summary>
    public int BatchNumber { get; set; }

    /// <summary>
    /// Gets or sets the index of the record within the batch where the error occurred (0-based).
    /// </summary>
    public int RecordIndex { get; set; }

    /// <summary>
    /// Gets or sets the index of the request within the batch where the error occurred (0-based).
    /// </summary>
    public int RequestIndex { get; set; }

    /// <summary>
    /// Gets or sets the Dataverse-specific error code if available.
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable error message.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the severity level of the error.
    /// </summary>
    public ErrorSeverity Severity { get; set; } = ErrorSeverity.Error;

    /// <summary>
    /// Gets or sets the entity reference of the failed record if available.
    /// </summary>
    public EntityReference? EntityReference { get; set; }

    /// <summary>
    /// Gets or sets the exception that caused this error.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this error occurred.
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a formatted error summary for logging and display purposes.
    /// </summary>
    public string Summary => $"[{Severity}] {ErrorCode}: {ErrorMessage}";

    public override string ToString() => Summary;
}

/// <summary>
/// Represents progress information for batch operations.
/// Used for real-time progress reporting during long-running operations.
/// </summary>
public class BatchProgress
{
    /// <summary>
    /// Gets or sets the number of records processed so far.
    /// </summary>
    public int ProcessedRecords { get; set; }

    /// <summary>
    /// Gets or sets the total number of records to be processed.
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Gets or sets the current batch number being processed (1-based).
    /// </summary>
    public int CurrentBatch { get; set; }

    /// <summary>
    /// Gets or sets the total number of batches to be processed.
    /// </summary>
    public int TotalBatches { get; set; }

    /// <summary>
    /// Gets or sets the number of records processed successfully so far.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the number of records that have failed so far.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Gets or sets the current operation being performed.
    /// </summary>
    public string CurrentOperation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the estimated time remaining for the operation.
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; set; }

    /// <summary>
    /// Gets or sets the elapsed time since the operation started.
    /// </summary>
    public TimeSpan ElapsedTime { get; set; }

    /// <summary>
    /// Gets or sets the current processing rate (records per second).
    /// </summary>
    public double CurrentRate { get; set; }

    /// <summary>
    /// Gets or sets additional progress metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];

    /// <summary>
    /// Gets the completion percentage (0-100).
    /// </summary>
    public double PercentComplete => TotalRecords > 0 ? (double)ProcessedRecords / TotalRecords * 100 : 0;

    /// <summary>
    /// Gets the batch completion percentage (0-100).
    /// </summary>
    public double BatchPercentComplete => TotalBatches > 0 ? (double)CurrentBatch / TotalBatches * 100 : 0;

    /// <summary>
    /// Gets the current success rate as a percentage.
    /// </summary>
    public double SuccessRate => ProcessedRecords > 0 ? (double)SuccessCount / ProcessedRecords * 100 : 0;

    /// <summary>
    /// Gets a formatted string showing the current progress.
    /// </summary>
    public string FormattedProgress => $"{ProcessedRecords:N0}/{TotalRecords:N0} ({PercentComplete:F1}%)";

    /// <summary>
    /// Gets a formatted string showing the current batch progress.
    /// </summary>
    public string FormattedBatchProgress => $"Batch {CurrentBatch}/{TotalBatches} ({BatchPercentComplete:F1}%)";

    /// <summary>
    /// Gets a formatted estimated time remaining string.
    /// </summary>
    public string FormattedTimeRemaining => EstimatedTimeRemaining?.ToString(@"hh\:mm\:ss") ?? "Unknown";

    /// <summary>
    /// Initializes a new instance of the BatchProgress class.
    /// </summary>
    public BatchProgress() { }

    /// <summary>
    /// Initializes a new instance of the BatchProgress class with basic progress information.
    /// </summary>
    /// <param name="processedRecords">The number of records processed</param>
    /// <param name="totalRecords">The total number of records</param>
    /// <param name="currentBatch">The current batch number</param>
    /// <param name="totalBatches">The total number of batches</param>
    public BatchProgress(int processedRecords, int totalRecords, int currentBatch, int totalBatches)
    {
        ProcessedRecords = processedRecords;
        TotalRecords = totalRecords;
        CurrentBatch = currentBatch;
        TotalBatches = totalBatches;
    }

    /// <summary>
    /// Updates the progress with new values.
    /// </summary>
    /// <param name="processedRecords">The number of records processed</param>
    /// <param name="successCount">The number of successful records</param>
    /// <param name="failureCount">The number of failed records</param>
    /// <param name="currentBatch">The current batch number</param>
    /// <param name="elapsedTime">The elapsed time</param>
    public void Update(int processedRecords, int successCount, int failureCount, int currentBatch, TimeSpan elapsedTime)
    {
        ProcessedRecords = processedRecords;
        SuccessCount = successCount;
        FailureCount = failureCount;
        CurrentBatch = currentBatch;
        ElapsedTime = elapsedTime;

        // Calculate current rate
        CurrentRate = elapsedTime.TotalSeconds > 0 ? processedRecords / elapsedTime.TotalSeconds : 0;

        // Estimate time remaining
        if (CurrentRate > 0 && TotalRecords > processedRecords)
        {
            double remainingSeconds = (TotalRecords - processedRecords) / CurrentRate;
            EstimatedTimeRemaining = TimeSpan.FromSeconds(remainingSeconds);
        }
    }

    /// <summary>
    /// Returns a string representation of the batch progress.
    /// </summary>
    /// <returns>A formatted string containing progress information</returns>
    public override string ToString() => $"BatchProgress [{FormattedProgress}] {FormattedBatchProgress} - " +
                                         $"Success: {SuccessCount:N0}, Failed: {FailureCount:N0}, " +
                                         $"Rate: {CurrentRate:F1}/sec, ETA: {FormattedTimeRemaining}";
}

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the validation was successful.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the type of validation result.
    /// </summary>
    public ValidationResultType ResultType { get; set; }

    /// <summary>
    /// Gets or sets the name or identifier of what was validated.
    /// </summary>
    public string ValidationTarget { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the table that was validated.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets validation errors that were found.
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Gets or sets validation warnings that were found.
    /// </summary>
    public List<string> Warnings { get; set; } = [];

    /// <summary>
    /// Gets or sets informational messages from the validation process.
    /// </summary>
    public List<string> Information { get; set; } = [];

    /// <summary>
    /// Gets or sets the time taken to perform the validation.
    /// </summary>
    public TimeSpan ValidationTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the validation was performed.
    /// </summary>
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets additional validation metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];

    /// <summary>
    /// Gets a value indicating whether the validation found any errors.
    /// </summary>
    public bool HasErrors => Errors.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the validation found any warnings.
    /// </summary>
    public bool HasWarnings => Warnings.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the validation found any issues (errors or warnings).
    /// </summary>
    public bool HasIssues => HasErrors || HasWarnings;

    /// <summary>
    /// Gets the total number of issues found.
    /// </summary>
    public int IssueCount => Errors.Count + Warnings.Count;

    /// <summary>
    /// Initializes a new instance of the ValidationResult class.
    /// </summary>
    public ValidationResult() { }

    /// <summary>
    /// Initializes a new instance of the ValidationResult class with the specified target.
    /// </summary>
    /// <param name="validationTarget">The target of the validation</param>
    public ValidationResult(string validationTarget) => ValidationTarget = validationTarget;

    /// <summary>
    /// Adds an error to the validation result.
    /// </summary>
    /// <param name="error">The error message to add</param>
    public void AddError(string error)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            Errors.Add(error);
            IsValid = false;
            ResultType = ValidationResultType.Error;
        }
    }

    /// <summary>
    /// Adds a warning to the validation result.
    /// </summary>
    /// <param name="warning">The warning message to add</param>
    public void AddWarning(string warning)
    {
        if (!string.IsNullOrWhiteSpace(warning))
        {
            Warnings.Add(warning);
            if (ResultType != ValidationResultType.Error) ResultType = ValidationResultType.Warning;
        }
    }

    /// <summary>
    /// Adds an informational message to the validation result.
    /// </summary>
    /// <param name="info">The informational message to add</param>
    public void AddInformation(string info)
    {
        if (!string.IsNullOrWhiteSpace(info)) Information.Add(info);
    }

    /// <summary>
    /// Marks the validation as successful if no errors were found.
    /// </summary>
    public void MarkAsSuccessful()
    {
        if (!HasErrors)
        {
            IsValid = true;
            ResultType = HasWarnings ? ValidationResultType.Warning : ValidationResultType.Success;
        }
    }

    /// <summary>
    /// Creates a summary of all issues found during validation.
    /// </summary>
    /// <returns>A formatted summary string</returns>
    public string CreateIssueSummary()
    {
        if (!HasIssues && (Information.Count == 0))
            return "No issues found";

        System.Text.StringBuilder sb = new();

        if (HasErrors)
        {
            sb.AppendLine($"Errors ({Errors.Count}):");
            foreach (string error in Errors) sb.AppendLine($"  - {error}");
        }

        if (HasWarnings)
        {
            if (HasErrors) sb.AppendLine();
            sb.AppendLine($"Warnings ({Warnings.Count}):");
            foreach (string warning in Warnings) sb.AppendLine($"  - {warning}");
        }

        if (Information is { Count: > 0 })
        {
            if (HasErrors || HasWarnings) sb.AppendLine();
            sb.AppendLine($"Information ({Information.Count}):");
            foreach (string info in Information) sb.AppendLine($"  - {info}");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Returns a string representation of the validation result.
    /// </summary>
    /// <returns>A formatted string containing validation information</returns>
    public override string ToString() => $"ValidationResult [{ValidationTarget}] - " +
                                         $"Valid: {IsValid}, Type: {ResultType}, " +
                                         $"Errors: {Errors.Count}, Warnings: {Warnings.Count}, " +
                                         $"Time: {ValidationTime.TotalMilliseconds:F0}ms";
}

/// <summary>
/// Represents information about a Dataverse connection.
/// </summary>
public class ConnectionInfo
{
    /// <summary>
    /// Gets or sets the unique identifier for this connection information instance.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the current state of the connection.
    /// </summary>
    public ConnectionState State { get; set; } = ConnectionState.Unknown;

    /// <summary>
    /// Gets or sets the Dataverse organization name.
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the friendly name of the organization.
    /// </summary>
    public string OrganizationFriendlyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the organization.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the URL of the Dataverse environment.
    /// </summary>
    public string OrganizationUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the Dataverse service.
    /// </summary>
    public string OrganizationVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current user's ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the current user's name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the business unit ID of the current user.
    /// </summary>
    public Guid BusinessUnitId { get; set; }

    /// <summary>
    /// Gets or sets the business unit name of the current user.
    /// </summary>
    public string BusinessUnitName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp of the last successful connection activity.
    /// </summary>
    public DateTime? ConnectedAt { get; set; }

    /// <summary>
    /// Gets or sets the time taken for the last connection.
    /// </summary>
    public TimeSpan? ConnectionDuration { get; set; }

    public List<string> Errors = [];

    /// <summary>
    /// Gets or sets additional connection metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];

    /// <summary>
    /// Gets a value indicating whether the connection is currently healthy.
    /// </summary>
    public bool IsConnected => State == ConnectionState.Connected;

    /// <summary>
    /// Gets a value indicating whether connection information is complete.
    /// </summary>
    public bool IsComplete => !string.IsNullOrEmpty(OrganizationName) &&
                              OrganizationId != Guid.Empty &&
                              UserId != Guid.Empty;

    /// <summary>
    /// Gets a formatted connection test duration string.
    /// </summary>
    public string FormattedConnectionTestDuration => ConnectionDuration?.ToString(@"ss\.fff") + "s" ?? "Unknown";

    /// <summary>
    /// Initializes a new instance of the ConnectionInfo class.
    /// </summary>
    public ConnectionInfo() { }

    /// <summary>
    /// Initializes a new instance of the ConnectionInfo class with basic information.
    /// </summary>
    /// <param name="organizationName">The organization name</param>
    /// <param name="organizationUrl">The organization URL</param>
    /// <param name="state">The connection state</param>
    public ConnectionInfo(string organizationName, string organizationUrl, ConnectionState state)
    {
        OrganizationName = organizationName ?? string.Empty;
        OrganizationUrl = organizationUrl ?? string.Empty;
        State = state;
    }

    /// <summary>
    /// Updates the connection information with successful connection details.
    /// </summary>
    /// <param name="organizationId">The organization ID</param>
    /// <param name="userId">The user ID</param>
    /// <param name="userName">The user name</param>
    /// <param name="businessUnitId">The business unit ID</param>
    public void UpdateConnectionDetails(Guid organizationId, Guid userId, string userName, Guid businessUnitId)
    {
        OrganizationId = organizationId;
        UserId = userId;
        UserName = userName ?? string.Empty;
        BusinessUnitId = businessUnitId;
        State = ConnectionState.Connected;
        ConnectedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the connection as failed with the specified duration.
    /// </summary>
    /// <param name="testDuration">The duration of the failed connection test</param>
    public void MarkAsFailed(TimeSpan testDuration)
    {
        State = ConnectionState.Failed;
        ConnectedAt = DateTime.UtcNow;
        ConnectionDuration = testDuration;
    }

    /// <summary>
    /// Returns a string representation of the connection information.
    /// </summary>
    /// <returns>A formatted string containing connection details</returns>
    public override string ToString() => $"ConnectionInfo [{State}] - " +
                                         $"Org: {OrganizationFriendlyName ?? OrganizationName}, " +
                                         $"User: {UserName}, " +
                                         $"URL: {OrganizationUrl}, " +
                                         $"LastTest: {ConnectedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}";
}
