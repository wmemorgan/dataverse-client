namespace Dataverse.Client.Models;

/// <summary>
/// Main configuration options for the Dataverse client library.
/// Supports both connection string and individual parameter configuration.
/// </summary>
public class DataverseClientOptions
{
    /// <summary>
    /// Gets or sets the complete Dataverse connection string.
    /// If provided, this takes precedence over individual connection parameters.
    /// </summary>
    /// <example>
    /// "AuthType=ClientSecret;Url=https://myorg.crm.dynamics.com;ClientId=...;ClientSecret=..."
    /// </example>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Dataverse environment URL.
    /// Used when ConnectionString is not provided.
    /// </summary>
    /// <example>
    /// "https://myorg.crm.dynamics.com"
    /// </example>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Azure AD application (client) ID for authentication.
    /// Used when ConnectionString is not provided.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Azure AD application client secret for authentication.
    /// Should be stored securely (user secrets, key vault, environment variables).
    /// Used when ConnectionString is not provided.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default batch size for batch operations.
    /// This value is used when no specific batch size is provided in operation calls.
    /// </summary>
    /// <value>Default: 100, Range: 1-1000</value>
    public int DefaultBatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum allowed batch size for any operation.
    /// This prevents operations from exceeding Microsoft's documented limits.
    /// </summary>
    /// <value>Default: 1000 (Microsoft's maximum for ExecuteMultipleRequest)</value>
    public int MaxBatchSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the number of retry attempts for failed operations.
    /// Applies to both individual and batch operations.
    /// </summary>
    /// <value>Default: 3, Range: 0-10</value>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay in milliseconds between retry attempts.
    /// Actual delay uses exponential backoff: RetryDelayMs * 2^(attempt-1).
    /// </summary>
    /// <value>Default: 1000ms (1 second)</value>
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the connection timeout in seconds for establishing connections.
    /// </summary>
    /// <value>Default: 300 seconds (5 minutes)</value>
    public int ConnectionTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets a value indicating whether to enable retry logic on failures.
    /// When false, operations fail immediately without retry attempts.
    /// </summary>
    /// <value>Default: true</value>
    public bool EnableRetryOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable detailed performance logging.
    /// When enabled, provides detailed timing and performance metrics.
    /// </summary>
    /// <value>Default: false</value>
    public bool EnablePerformanceLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to enable progress reporting by default.
    /// Individual operations can override this setting.
    /// </summary>
    /// <value>Default: false</value>
    public bool EnableProgressReporting { get; set; } = false;

    /// <summary>
    /// Gets or sets the timeout in milliseconds for individual batch operations.
    /// This prevents individual batches from hanging indefinitely.
    /// </summary>
    /// <value>Default: 300000ms (5 minutes)</value>
    public int BatchTimeoutMs { get; set; } = 300000;

    /// <summary>
    /// Gets or sets additional connection parameters for advanced scenarios.
    /// These are appended to the connection string if provided.
    /// </summary>
    public Dictionary<string, string> AdditionalConnectionParameters { get; set; } = [];

    /// <summary>
    /// Validates the configuration options and throws an exception if invalid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid</exception>
    public void Validate()
    {
        List<string> errors = GetValidationErrors();
        
        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"Invalid Dataverse client configuration: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Gets a list of validation errors for the current configuration.
    /// </summary>
    /// <returns>List of validation error messages, empty if configuration is valid</returns>
    public List<string> GetValidationErrors()
    {
        List<string> errors = [];

        // Check if we have either a connection string or individual parameters
        bool hasConnectionString = !string.IsNullOrWhiteSpace(ConnectionString);
        bool hasIndividualSettings = !string.IsNullOrWhiteSpace(Url) && 
                                   !string.IsNullOrWhiteSpace(ClientId) && 
                                   !string.IsNullOrWhiteSpace(ClientSecret);

        if (!hasConnectionString && !hasIndividualSettings)
        {
            errors.Add("Either ConnectionString or individual settings (Url, ClientId, ClientSecret) must be provided");
        }

        // Validate connection string format if provided
        if (hasConnectionString && !IsValidConnectionStringFormat(ConnectionString))
        {
            errors.Add("ConnectionString format is invalid");
        }

        // Validate URL format if provided
        if (!string.IsNullOrWhiteSpace(Url) && !IsValidUrlFormat(Url))
        {
            errors.Add("Url format is invalid");
        }

        // Validate batch size settings
        if (DefaultBatchSize <= 0 || DefaultBatchSize > MaxBatchSize)
        {
            errors.Add($"DefaultBatchSize ({DefaultBatchSize}) must be between 1 and MaxBatchSize ({MaxBatchSize})");
        }

        if (MaxBatchSize is <= 0 or > 1000)
        {
            errors.Add($"MaxBatchSize ({MaxBatchSize}) must be between 1 and 1000");
        }

        // Validate retry settings
        if (RetryAttempts is < 0 or > 10)
        {
            errors.Add($"RetryAttempts ({RetryAttempts}) must be between 0 and 10");
        }

        if (RetryDelayMs < 0)
        {
            errors.Add($"RetryDelayMs ({RetryDelayMs}) must be non-negative");
        }

        // Validate timeout settings
        if (ConnectionTimeoutSeconds <= 0)
        {
            errors.Add($"ConnectionTimeoutSeconds ({ConnectionTimeoutSeconds}) must be positive");
        }

        if (BatchTimeoutMs <= 0)
        {
            errors.Add($"BatchTimeoutMs ({BatchTimeoutMs}) must be positive");
        }

        return errors;
    }

    /// <summary>
    /// Gets the effective connection string to use for Dataverse connections.
    /// Builds connection string from individual parameters if ConnectionString is not provided.
    /// </summary>
    /// <returns>A properly formatted connection string</returns>
    public string GetEffectiveConnectionString()
    {
        if (!string.IsNullOrWhiteSpace(ConnectionString))
        {
            return EnhanceConnectionString(ConnectionString);
        }

        if (string.IsNullOrWhiteSpace(Url) || string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(ClientSecret))
        {
            throw new InvalidOperationException("Either ConnectionString or individual connection parameters must be provided");
        }

        string baseConnectionString = $"AuthType=ClientSecret;Url={Url};ClientId={ClientId};ClientSecret={ClientSecret}";
        return EnhanceConnectionString(baseConnectionString);
    }

    /// <summary>
    /// Gets a value indicating whether the configuration uses individual connection parameters.
    /// </summary>
    public bool UsesIndividualParameters => string.IsNullOrWhiteSpace(ConnectionString) &&
                                          !string.IsNullOrWhiteSpace(Url) &&
                                          !string.IsNullOrWhiteSpace(ClientId) &&
                                          !string.IsNullOrWhiteSpace(ClientSecret);

    /// <summary>
    /// Gets a value indicating whether the configuration is valid.
    /// </summary>
    public bool IsValid => GetValidationErrors().Count == 0;

    /// <summary>
    /// Creates a copy of the current options with the specified modifications.
    /// </summary>
    /// <param name="modifier">Action to modify the copied options</param>
    /// <returns>A new DataverseClientOptions instance with modifications applied</returns>
    public DataverseClientOptions Clone(Action<DataverseClientOptions>? modifier = null)
    {
        DataverseClientOptions clone = new()
        {
            ConnectionString = ConnectionString,
            Url = Url,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            DefaultBatchSize = DefaultBatchSize,
            MaxBatchSize = MaxBatchSize,
            RetryAttempts = RetryAttempts,
            RetryDelayMs = RetryDelayMs,
            ConnectionTimeoutSeconds = ConnectionTimeoutSeconds,
            EnableRetryOnFailure = EnableRetryOnFailure,
            EnablePerformanceLogging = EnablePerformanceLogging,
            EnableProgressReporting = EnableProgressReporting,
            BatchTimeoutMs = BatchTimeoutMs,
            AdditionalConnectionParameters = new Dictionary<string, string>(AdditionalConnectionParameters)
        };

        modifier?.Invoke(clone);
        return clone;
    }

    /// <summary>
    /// Returns a string representation of the configuration (with sensitive data masked).
    /// </summary>
    /// <returns>A formatted string containing configuration information</returns>
    public override string ToString()
    {
        string connectionInfo = !string.IsNullOrWhiteSpace(ConnectionString) 
            ? "ConnectionString=***" 
            : $"Url={Url}, ClientId={ClientId}, ClientSecret=***";

        return $"DataverseClientOptions [{connectionInfo}, DefaultBatchSize={DefaultBatchSize}, " +
               $"MaxBatchSize={MaxBatchSize}, RetryAttempts={RetryAttempts}]";
    }

    #region Private Helper Methods

    private static bool IsValidConnectionStringFormat(string connectionString)
    {
        try
        {
            // Basic validation - check for required components
            return connectionString.Contains("AuthType=") && 
                   connectionString.Contains("Url=") &&
                   (connectionString.Contains("ClientId=") || connectionString.Contains("Username="));
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidUrlFormat(string url)
    {
        try
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) && 
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
                   uri.Host.Contains(".crm") && uri.Host.Contains(".dynamics.com");
        }
        catch
        {
            return false;
        }
    }

    private string EnhanceConnectionString(string baseConnectionString)
    {
        List<string> connectionParts = [baseConnectionString];

        // Add connection timeout if not already specified
        if (!baseConnectionString.Contains("Timeout=") && ConnectionTimeoutSeconds != 300)
        {
            connectionParts.Add($"Timeout={ConnectionTimeoutSeconds}");
        }

        // Add additional connection parameters
        foreach (KeyValuePair<string, string> kvp in AdditionalConnectionParameters)
        {
            if (!string.IsNullOrWhiteSpace(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value))
            {
                connectionParts.Add($"{kvp.Key}={kvp.Value}");
            }
        }

        return string.Join(";", connectionParts);
    }

    #endregion
}

/// <summary>
/// Configuration options for individual batch operations.
/// This can be provided per operation to override default settings.
/// </summary>
public class BatchConfiguration
{
    /// <summary>
    /// Gets or sets the number of records to include in each batch.
    /// If not specified, uses the default from DataverseClientOptions.
    /// </summary>
    /// <value>Range: 1-1000, Default: 100</value>
    public int? BatchSize { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for this operation.
    /// If not specified, uses the default from DataverseClientOptions.
    /// </summary>
    /// <value>Range: 0-10, Default: 3</value>
    public int? MaxRetries { get; set; }

    /// <summary>
    /// Gets or sets the base delay in milliseconds between retry attempts.
    /// If not specified, uses the default from DataverseClientOptions.
    /// </summary>
    /// <value>Default: 1000ms</value>
    public int? RetryDelayMs { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to continue processing remaining batches
    /// when individual records fail within a batch.
    /// </summary>
    /// <value>Default: true</value>
    public bool ContinueOnError { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable progress reporting for this operation.
    /// </summary>
    /// <value>Default: false</value>
    public bool EnableProgressReporting { get; set; } = false;

    /// <summary>
    /// Gets or sets the progress reporter for tracking operation progress.
    /// Only used when EnableProgressReporting is true.
    /// </summary>
    public IProgress<BatchProgress>? ProgressReporter { get; set; }

    /// <summary>
    /// Gets or sets the timeout in milliseconds for this specific operation.
    /// If not specified, uses the default from DataverseClientOptions.
    /// </summary>
    public int? TimeoutMs { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for this operation.
    /// Can be used for tracking, logging, or custom business logic.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];

    /// <summary>
    /// Gets or sets a cancellation token for this operation.
    /// Allows for graceful cancellation of long-running batch operations.
    /// </summary>
    public CancellationToken CancellationToken { get; set; } = default;

    /// <summary>
    /// Gets the effective batch size, using provided value or default.
    /// </summary>
    /// <param name="defaultBatchSize">The default batch size to use if not specified</param>
    /// <returns>The effective batch size to use</returns>
    public int GetEffectiveBatchSize(int defaultBatchSize) => BatchSize ?? defaultBatchSize;

    /// <summary>
    /// Gets the effective retry attempts, using provided value or default.
    /// </summary>
    /// <param name="defaultRetryAttempts">The default retry attempts to use if not specified</param>
    /// <returns>The effective retry attempts to use</returns>
    public int GetEffectiveRetryAttempts(int defaultRetryAttempts) => MaxRetries ?? defaultRetryAttempts;

    /// <summary>
    /// Gets the effective retry delay, using provided value or default.
    /// </summary>
    /// <param name="defaultRetryDelayMs">The default retry delay to use if not specified</param>
    /// <returns>The effective retry delay to use</returns>
    public int GetEffectiveRetryDelayMs(int defaultRetryDelayMs) => RetryDelayMs ?? defaultRetryDelayMs;

    /// <summary>
    /// Gets the effective timeout, using provided value or default.
    /// </summary>
    /// <param name="defaultTimeoutMs">The default timeout to use if not specified</param>
    /// <returns>The effective timeout to use</returns>
    public int GetEffectiveTimeoutMs(int defaultTimeoutMs) => TimeoutMs ?? defaultTimeoutMs;

    /// <summary>
    /// Creates a copy of the current configuration.
    /// </summary>
    /// <returns>A new BatchConfiguration instance with copied values</returns>
    public BatchConfiguration Clone()
    {
        return new BatchConfiguration
        {
            BatchSize = BatchSize,
            MaxRetries = MaxRetries,
            RetryDelayMs = RetryDelayMs,
            ContinueOnError = ContinueOnError,
            EnableProgressReporting = EnableProgressReporting,
            ProgressReporter = ProgressReporter,
            TimeoutMs = TimeoutMs,
            Metadata = new Dictionary<string, object>(Metadata),
            CancellationToken = CancellationToken
        };
    }

    /// <summary>
    /// Returns a string representation of the batch configuration.
    /// </summary>
    /// <returns>A formatted string containing configuration information</returns>
    public override string ToString()
    {
        return $"BatchConfiguration [BatchSize={BatchSize}, MaxRetries={MaxRetries}, " +
               $"RetryDelayMs={RetryDelayMs}, ContinueOnError={ContinueOnError}, " +
               $"EnableProgressReporting={EnableProgressReporting}]";
    }
}

/// <summary>
/// Configuration options for validation operations.
/// </summary>
public class ValidationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to perform deep validation checks.
    /// When true, performs comprehensive validation including schema checks.
    /// </summary>
    /// <value>Default: false</value>
    public bool EnableDeepValidation { get; set; } = false;

    /// <summary>
    /// Gets or sets the timeout in milliseconds for validation operations.
    /// </summary>
    /// <value>Default: 30000ms (30 seconds)</value>
    public int ValidationTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Gets or sets a value indicating whether to validate table permissions.
    /// </summary>
    /// <value>Default: true</value>
    public bool ValidateTablePermissions { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to validate field access permissions.
    /// </summary>
    /// <value>Default: false</value>
    public bool ValidateFieldPermissions { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to cache validation results.
    /// When true, repeated validation calls return cached results for better performance.
    /// </summary>
    /// <value>Default: true</value>
    public bool EnableValidationCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache duration in minutes for validation results.
    /// </summary>
    /// <value>Default: 15 minutes</value>
    public int ValidationCacheDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Returns a string representation of the validation options.
    /// </summary>
    /// <returns>A formatted string containing validation options information</returns>
    public override string ToString()
    {
        return $"ValidationOptions [DeepValidation={EnableDeepValidation}, " +
               $"Timeout={ValidationTimeoutMs}ms, TablePermissions={ValidateTablePermissions}, " +
               $"FieldPermissions={ValidateFieldPermissions}]";
    }
}