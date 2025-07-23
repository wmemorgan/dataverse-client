// File: tests/Dataverse.Client.Tests/TestData/TestOptions.cs
using Dataverse.Client.Models;
using Microsoft.Extensions.Options;

namespace Dataverse.Client.Tests.TestData;

public static class TestOptions
{
    public static DataverseClientOptions CreateValidOptions()
    {
        return new DataverseClientOptions
        {
            Url = "https://test.crm.dynamics.com",
            ClientId = Guid.NewGuid().ToString(),
            ClientSecret = "test-secret",
            DefaultBatchSize = 100,
            MaxBatchSize = 1000,
            RetryAttempts = 3,
            RetryDelayMs = 1000,
            ConnectionTimeoutSeconds = 300,
            EnableRetryOnFailure = true,
            EnablePerformanceLogging = false,
            BatchTimeoutMs = 300000
        };
    }

    public static DataverseClientOptions CreateOptionsWithConnectionString()
    {
        return new DataverseClientOptions
        {
            ConnectionString = "AuthType=ClientSecret;Url=https://test.crm.dynamics.com;ClientId=" + Guid.NewGuid() + ";ClientSecret=test-secret",
            DefaultBatchSize = 50,
            MaxBatchSize = 500,
            RetryAttempts = 2
        };
    }

    public static IOptions<DataverseClientOptions> CreateIOptions(DataverseClientOptions? options = null)
    {
        return Options.Create(options ?? CreateValidOptions());
    }

    public static BatchConfiguration CreateBatchConfiguration()
    {
        return new BatchConfiguration
        {
            BatchSize = 50,
            MaxRetries = 2,
            RetryDelayMs = 500,
            ContinueOnError = true,
            EnableProgressReporting = false,
            TimeoutMs = 30000
        };
    }
}
