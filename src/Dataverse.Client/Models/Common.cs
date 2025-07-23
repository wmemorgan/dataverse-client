using System.Net;
using System.Text.RegularExpressions;

namespace Dataverse.Client.Models;

#region Exceptions

/// <summary>
/// Base exception class for all Dataverse client-related exceptions.
/// </summary>
public class DataverseException : Exception
{
    /// <summary>
    /// Gets or sets the Dataverse error code if available.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets additional error details from Dataverse.
    /// </summary>
    public Dictionary<string, object> ErrorDetails { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the DataverseException class.
    /// </summary>
    public DataverseException() { }

    /// <summary>
    /// Initializes a new instance of the DataverseException class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    public DataverseException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the DataverseException class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public DataverseException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the DataverseException class with error code and message.
    /// </summary>
    /// <param name="errorCode">The Dataverse error code</param>
    /// <param name="message">The error message</param>
    public DataverseException(string errorCode, string message) : base(message) => ErrorCode = errorCode;

    /// <summary>
    /// Initializes a new instance of the DataverseException class with error code, message, and inner exception.
    /// </summary>
    /// <param name="errorCode">The Dataverse error code</param>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public DataverseException(string errorCode, string message, Exception innerException) : base(message,
        innerException) => ErrorCode = errorCode;

    /// <summary>
    /// Returns a string representation of the exception including error code and details.
    /// </summary>
    /// <returns>A formatted string containing exception information</returns>
    public override string ToString()
    {
        string result = base.ToString();

        if (!string.IsNullOrWhiteSpace(ErrorCode)) result = $"ErrorCode: {ErrorCode}\n{result}";

        if (ErrorDetails.Count > 0)
        {
            string details = string.Join(", ", ErrorDetails.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            result = $"{result}\nErrorDetails: {details}";
        }

        return result;
    }
}

/// <summary>
/// Exception thrown when Dataverse connection operations fail.
/// </summary>
public class DataverseConnectionException : DataverseException
{
    /// <summary>
    /// Gets or sets the connection string that failed (with sensitive data masked).
    /// </summary>
    public string? MaskedConnectionString { get; set; }

    /// <summary>
    /// Initializes a new instance of the DataverseConnectionException class.
    /// </summary>
    public DataverseConnectionException() { }

    /// <summary>
    /// Initializes a new instance of the DataverseConnectionException class with a specified error message.
    /// </summary>
    /// <param name="message">The error message</param>
    public DataverseConnectionException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the DataverseConnectionException class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public DataverseConnectionException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when batch operations fail due to validation or processing errors.
/// </summary>
public class DataverseBatchException : DataverseException
{
    /// <summary>
    /// Gets or sets the batch number where the error occurred.
    /// </summary>
    public int BatchNumber { get; set; }

    /// <summary>
    /// Gets or sets the total number of records that were being processed.
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Gets or sets the number of records processed successfully before the error.
    /// </summary>
    public int ProcessedRecords { get; set; }

    /// <summary>
    /// Initializes a new instance of the DataverseBatchException class.
    /// </summary>
    public DataverseBatchException() { }

    /// <summary>
    /// Initializes a new instance of the DataverseBatchException class with a specified error message.
    /// </summary>
    /// <param name="message">The error message</param>
    public DataverseBatchException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the DataverseBatchException class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public DataverseBatchException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when validation operations fail.
/// </summary>
public class DataverseValidationException : DataverseException
{
    /// <summary>
    /// Gets or sets the validation errors that occurred.
    /// </summary>
    public List<string> ValidationErrors { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the DataverseValidationException class.
    /// </summary>
    public DataverseValidationException() { }

    /// <summary>
    /// Initializes a new instance of the DataverseValidationException class with validation errors.
    /// </summary>
    /// <param name="validationErrors">The validation errors</param>
    public DataverseValidationException(List<string> validationErrors)
        : base($"Validation failed with {validationErrors.Count} error(s): {string.Join("; ", validationErrors)}") =>
        ValidationErrors = validationErrors ?? [];

    /// <summary>
    /// Initializes a new instance of the DataverseValidationException class with a single validation error.
    /// </summary>
    /// <param name="validationError">The validation error</param>
    public DataverseValidationException(string validationError) : base(validationError) =>
        ValidationErrors = [validationError];
}

#endregion

#region Enumerations

/// <summary>
/// Enumeration of Dataverse operation types.
/// </summary>
public enum OperationType
{
    /// <summary>Create operation</summary>
    Create,

    /// <summary>Retrieve/Read operation</summary>
    Retrieve,

    /// <summary>Update operation</summary>
    Update,

    /// <summary>Delete operation</summary>
    Delete,

    /// <summary>Query operation (RetrieveMultiple)</summary>
    Query,

    /// <summary>Validation operation</summary>
    Validation,

    /// <summary>Connection test operation</summary>
    ConnectionTest
}

/// <summary>
/// Enumeration of batch operation types for tracking and logging.
/// </summary>
public enum BatchOperationType
{
    /// <summary>Batch create operation</summary>
    BatchCreate,

    /// <summary>Batch update operation</summary>
    BatchUpdate,

    /// <summary>Batch delete operation</summary>
    BatchDelete,

    /// <summary>Batch retrieve operation</summary>
    BatchRetrieve
}

/// <summary>
/// Enumeration of error severity levels.
/// </summary>
public enum ErrorSeverity
{
    /// <summary>Informational message</summary>
    Info,

    /// <summary>Warning that doesn't prevent operation completion</summary>
    Warning,

    /// <summary>Error that affects individual records but allows batch continuation</summary>
    Error,

    /// <summary>Critical error that stops the entire operation</summary>
    Critical
}

/// <summary>
/// Enumeration of connection states.
/// </summary>
public enum ConnectionState
{
    /// <summary>Connection has not been tested</summary>
    Unknown,

    /// <summary>Connection is healthy and ready</summary>
    Connected,

    /// <summary>Connection failed</summary>
    Failed,

    /// <summary>Connection is being tested</summary>
    Testing,

    /// <summary>Connection timed out</summary>
    Timeout
}

/// <summary>
/// Enumeration of validation result types.
/// </summary>
public enum ValidationResultType
{
    /// <summary>Validation passed successfully</summary>
    Success,

    /// <summary>Validation passed with warnings</summary>
    Warning,

    /// <summary>Validation failed with errors</summary>
    Error,

    /// <summary>Validation could not be completed</summary>
    Incomplete
}

#endregion

#region Constants

/// <summary>
/// Constants used throughout the Dataverse client library.
/// </summary>
public static class DataverseConstants
{
    /// <summary>
    /// Default values for various operations.
    /// </summary>
    public static class Defaults
    {
        /// <summary>Default batch size for operations</summary>
        public const int BatchSize = 100;

        /// <summary>Maximum batch size allowed by Microsoft Dataverse</summary>
        public const int MaxBatchSize = 1000;

        /// <summary>Default retry attempts</summary>
        public const int RetryAttempts = 3;

        /// <summary>Default retry delay in milliseconds</summary>
        public const int RetryDelayMs = 1000;

        /// <summary>Default connection timeout in seconds</summary>
        public const int ConnectionTimeoutSeconds = 300;

        /// <summary>Default batch timeout in milliseconds</summary>
        public const int BatchTimeoutMs = 300000;

        /// <summary>Default validation timeout in milliseconds</summary>
        public const int ValidationTimeoutMs = 30000;
    }

    /// <summary>
    /// Common error codes returned by Dataverse operations.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>Authentication failed</summary>
        public const string AuthenticationFailed = "0x80040220";

        /// <summary>Insufficient privileges</summary>
        public const string InsufficientPrivileges = "0x80040220";

        /// <summary>Record not found</summary>
        public const string RecordNotFound = "0x80040217";

        /// <summary>Record not found error code (decimal value)</summary>
        public const int RecordNotFoundCode = -2147220969;

        /// <summary>Record already exists</summary>
        public const string RecordAlreadyExists = "0x80040237";

        /// <summary>Invalid argument</summary>
        public const string InvalidArgument = "0x80040203";

        /// <summary>Request timeout</summary>
        public const string Timeout = "0x80040204";

        /// <summary>Service unavailable</summary>
        public const string ServiceUnavailable = "0x80040205";

        /// <summary>Rate limit exceeded</summary>
        public const string RateLimitExceeded = "0x80040206";

        /// <summary>Batch operation failed</summary>
        public const string BatchOperationFailed = "BATCH_FAILED";

        /// <summary>Connection failed</summary>
        public const string ConnectionFailed = "CONNECTION_FAILED";

        /// <summary>Validation failed</summary>
        public const string ValidationFailed = "VALIDATION_FAILED";
    }

    /// <summary>
    /// Common attribute names used in Dataverse.
    /// </summary>
    public static class AttributeNames
    {
        /// <summary>Primary key attribute name</summary>
        public const string PrimaryKey = "Id";

        /// <summary>Primary name attribute suffix</summary>
        public const string PrimaryNameSuffix = "name";

        /// <summary>Created on attribute name</summary>
        public const string CreatedOn = "createdon";

        /// <summary>Modified on attribute name</summary>
        public const string ModifiedOn = "modifiedon";

        /// <summary>Created by attribute name</summary>
        public const string CreatedBy = "createdby";

        /// <summary>Modified by attribute name</summary>
        public const string ModifiedBy = "modifiedby";

        /// <summary>State code attribute name</summary>
        public const string StateCode = "statecode";

        /// <summary>Status code attribute name</summary>
        public const string StatusCode = "statuscode";

        /// <summary>Owner attribute name</summary>
        public const string Owner = "ownerid";
    }

    /// <summary>
    /// Regular expressions for validation.
    /// </summary>
    public static class RegexPatterns
    {
        /// <summary>Pattern for validating Dataverse URLs</summary>
        public const string DataverseUrl = @"^https://[\w\-]+\.crm\d*\.dynamics\.com/?$";

        /// <summary>Pattern for validating Azure AD Client IDs (GUIDs)</summary>
        public const string ClientId = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";

        /// <summary>Pattern for validating table logical names</summary>
        public const string TableLogicalName = @"^[a-z][a-z0-9_]*[a-z0-9]$";

        /// <summary>Pattern for validating field logical names</summary>
        public const string FieldLogicalName = @"^[a-z][a-z0-9_]*[a-z0-9]$";
    }
}

#endregion

#region Utilities

/// <summary>
/// Utility methods for common operations.
/// </summary>
public static partial class DataverseUtilities
{
    [GeneratedRegex(@"(ClientSecret\s*=\s*)[^;]+", RegexOptions.IgnoreCase)]
    private static partial Regex MaskClientSecretRegex();

    [GeneratedRegex(@"(Password\s*=\s*)[^;]+", RegexOptions.IgnoreCase)]
    private static partial Regex MaskPasswordRegex();

    [GeneratedRegex(@"^https://[\w\-]+\.crm\d*\.dynamics\.com/?$", RegexOptions.None)]
    private static partial Regex DataverseUrlRegex();

    [GeneratedRegex(@"[^a-z0-9_]", RegexOptions.None)]
    private static partial Regex InvalidTableCharactersRegex();

    [GeneratedRegex(@"_{2,}", RegexOptions.None)]
    private static partial Regex ConsecutiveUnderscoresRegex();

    /// <summary>
    /// Masks sensitive information in connection strings for logging.
    /// </summary>
    /// <param name="connectionString">The connection string to mask</param>
    /// <returns>A masked connection string safe for logging</returns>
    public static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return string.Empty;

        string masked = connectionString;
        masked = MaskClientSecretRegex().Replace(masked, "$1***");
        masked = MaskPasswordRegex().Replace(masked, "$1***");
        return masked;
    }

    /// <summary>
    /// Determines if an exception represents a transient error that can be retried.
    /// </summary>
    /// <param name="exception">The exception to evaluate</param>
    /// <returns>True if the exception indicates a transient failure</returns>
    public static bool IsTransientException(Exception? exception)
    {
        if (exception == null)
            return false;

        string message = exception.Message?.ToLowerInvariant() ?? string.Empty;

        return message.Contains("timeout") ||
               message.Contains("connection") ||
               message.Contains("network") ||
               message.Contains("throttle") ||
               message.Contains("rate limit") ||
               message.Contains("service unavailable") ||
               message.Contains("503") ||
               message.Contains("502") ||
               message.Contains("429") ||
               exception is TimeoutException ||
               (exception is HttpRequestException httpEx && IsTransientHttpException(httpEx));
    }

    /// <summary>
    /// Calculates the delay for retry attempts using exponential backoff.
    /// </summary>
    /// <param name="attemptNumber">The current attempt number (1-based)</param>
    /// <param name="baseDelayMs">The base delay in milliseconds</param>
    /// <param name="maxDelayMs">The maximum delay in milliseconds</param>
    /// <returns>The delay in milliseconds for the retry attempt</returns>
    public static int CalculateRetryDelay(int attemptNumber, int baseDelayMs, int maxDelayMs = 30000)
    {
        if (attemptNumber <= 0)
            return 0;

        // Exponential backoff: baseDelay * 2^(attempt-1)
        int delay = baseDelayMs * (int)Math.Pow(2, attemptNumber - 1);
        return Math.Min(delay, maxDelayMs);
    }

    /// <summary>
    /// Validates that a string is a properly formatted Dataverse URL.
    /// </summary>
    /// <param name="url">The URL to validate</param>
    /// <returns>True if the URL is a valid Dataverse URL</returns>
    public static bool IsValidDataverseUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        try
        {
            return DataverseUrlRegex().IsMatch(url);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Sanitizes a table name for safe usage in queries and operations.
    /// </summary>
    /// <param name="tableName">The table name to sanitize</param>
    /// <returns>A sanitized table name</returns>
    public static string SanitizeTableName(string? tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return string.Empty;

        string sanitized = tableName.ToLowerInvariant().Trim();

        // Replace spaces and invalid characters with underscores
        sanitized = InvalidTableCharactersRegex().Replace(sanitized, "_");

        // Ensure it starts with a letter
        if (sanitized.Length > 0 && char.IsDigit(sanitized[0]))
            sanitized = $"tbl_{sanitized}";

        // Remove consecutive underscores and trim
        sanitized = ConsecutiveUnderscoresRegex().Replace(sanitized, "_").Trim('_');

        return sanitized;
    }


    /// <summary>
    /// Creates a unique operation ID for tracking and correlation.
    /// </summary>
    /// <param name="operationType">The type of operation</param>
    /// <returns>A unique operation ID</returns>
    public static string GenerateOperationId(OperationType operationType)
    {
        string prefix = operationType switch
        {
            OperationType.Create => "CRT",
            OperationType.Retrieve => "RTV",
            OperationType.Update => "UPD",
            OperationType.Delete => "DEL",
            OperationType.Query => "QRY",
            OperationType.Validation => "VAL",
            OperationType.ConnectionTest => "CNT",
            _ => "OPR"
        };

        return $"{prefix}-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}"[..24];
    }

    /// <summary>
    /// Gets the HttpStatusCode enum value for common HTTP status codes.
    /// Use this instead of hardcoded integers: GetHttpStatusCode(HttpStatusCode.OK)
    /// </summary>
    /// <param name="statusCode">The HttpStatusCode enum value</param>
    /// <returns>The integer value of the status code</returns>
    public static int GetHttpStatusCode(HttpStatusCode statusCode) => (int)statusCode;

    #region Private Helper Methods

    private static bool IsTransientHttpException(HttpRequestException httpEx)
    {
        // Check for specific HTTP status codes that indicate transient errors
        string message = httpEx.Message.ToLowerInvariant();
        return message.Contains("502") || // Bad Gateway
               message.Contains("503") || // Service Unavailable
               message.Contains("504") || // Gateway Timeout
               message.Contains("429"); // Too Many Requests
    }

    #endregion
}

#endregion
