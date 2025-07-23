using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Dataverse.Client.Services;
using Dataverse.Client.Tests.TestData;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Dataverse.Client.Tests.Services;

/// <summary>
/// Comprehensive tests for ServiceInjection extension methods and DataverseClientOptionsValidator.
/// Tests dependency injection registration, configuration binding, and options validation.
/// </summary>
[TestFixture]
public class ServiceInjectionTests
{
    private IServiceCollection _services = null!;
    private IConfiguration _configuration = null!;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();
        
        // Create test configuration
        Dictionary<string, string?> configData = new()
        {
            ["Dataverse:Url"] = "https://test.crm.dynamics.com",
            ["Dataverse:ClientId"] = "test-client-id",
            ["Dataverse:ClientSecret"] = "test-secret",
            ["Dataverse:DefaultBatchSize"] = "150",
            ["Dataverse:MaxBatchSize"] = "500",
            ["Dataverse:RetryAttempts"] = "5",
            ["Dataverse:EnablePerformanceLogging"] = "true",
            ["CustomSection:Url"] = "https://custom.crm.dynamics.com",
            ["CustomSection:ClientId"] = "custom-client-id",
            ["CustomSection:ClientSecret"] = "custom-secret"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    #region AddDataverseClient(Action<DataverseClientOptions>) Tests

    [Test]
    public void AddDataverseClient_WithValidConfigureAction_ShouldConfigureOptionsCorrectly()
    {
        // Arrange
        const string expectedUrl = "https://test.crm.dynamics.com";
        const string expectedClientId = "test-client-id";
        const string expectedClientSecret = "test-secret";
        const int expectedBatchSize = 200;

        // Act
        _services.AddDataverseClient(options =>
        {
            options.Url = expectedUrl;
            options.ClientId = expectedClientId;
            options.ClientSecret = expectedClientSecret;
            options.DefaultBatchSize = expectedBatchSize;
            options.EnablePerformanceLogging = true;
        });

        using ServiceProvider provider = _services.BuildServiceProvider();
        IOptions<DataverseClientOptions> optionsWrapper = provider.GetRequiredService<IOptions<DataverseClientOptions>>();

        // Assert
        DataverseClientOptions options = optionsWrapper.Value;
        options.Url.Should().Be(expectedUrl);
        options.ClientId.Should().Be(expectedClientId);
        options.ClientSecret.Should().Be(expectedClientSecret);
        options.DefaultBatchSize.Should().Be(expectedBatchSize);
        options.EnablePerformanceLogging.Should().BeTrue();
    }

    [Test]
    public void AddDataverseClient_WithNullConfigureAction_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Action act = () => _services.AddDataverseClient(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configureOptions");
    }

    [Test]
    public void AddDataverseClient_WithValidConfiguration_ShouldReturnServiceCollection()
    {
        // Act
        IServiceCollection result = _services.AddDataverseClient(options =>
        {
            options.Url = "https://test.crm.dynamics.com";
            options.ClientId = "test-client-id";
            options.ClientSecret = "test-secret";
        });

        // Assert
        result.Should().BeSameAs(_services);
    }

    [Test]
    public void AddDataverseClient_WithComplexConfiguration_ShouldConfigureAllOptions()
    {
        // Act
        _services.AddDataverseClient(options =>
        {
            options.Url = "https://test.crm.dynamics.com";
            options.ClientId = "test-client-id";
            options.ClientSecret = "test-secret";
            options.DefaultBatchSize = 250;
            options.MaxBatchSize = 800;
            options.RetryAttempts = 4;
            options.RetryDelayMs = 2000;
            options.ConnectionTimeoutSeconds = 600;
            options.EnableRetryOnFailure = false;
            options.EnablePerformanceLogging = true;
            options.EnableProgressReporting = true;
            options.BatchTimeoutMs = 600000;
            options.AdditionalConnectionParameters["RequireNewInstance"] = "true";
            options.AdditionalConnectionParameters["LoginPrompt"] = "Never";
        });

        using ServiceProvider provider = _services.BuildServiceProvider();
        DataverseClientOptions options = provider.GetRequiredService<IOptions<DataverseClientOptions>>().Value;

        // Assert
        options.Url.Should().Be("https://test.crm.dynamics.com");
        options.ClientId.Should().Be("test-client-id");
        options.ClientSecret.Should().Be("test-secret");
        options.DefaultBatchSize.Should().Be(250);
        options.MaxBatchSize.Should().Be(800);
        options.RetryAttempts.Should().Be(4);
        options.RetryDelayMs.Should().Be(2000);
        options.ConnectionTimeoutSeconds.Should().Be(600);
        options.EnableRetryOnFailure.Should().BeFalse();
        options.EnablePerformanceLogging.Should().BeTrue();
        options.EnableProgressReporting.Should().BeTrue();
        options.BatchTimeoutMs.Should().Be(600000);
        options.AdditionalConnectionParameters.Should().ContainKey("RequireNewInstance");
        options.AdditionalConnectionParameters["RequireNewInstance"].Should().Be("true");
        options.AdditionalConnectionParameters.Should().ContainKey("LoginPrompt");
        options.AdditionalConnectionParameters["LoginPrompt"].Should().Be("Never");
    }

    [Test]
    public void AddDataverseClient_CalledMultipleTimes_ShouldReplaceRegistrations()
    {
        // Arrange & Act
        _services.AddDataverseClient(options =>
        {
            options.Url = "https://first.crm.dynamics.com";
            options.ClientId = "first-client-id";
            options.ClientSecret = "first-secret";
            options.DefaultBatchSize = 100;
        });

        _services.AddDataverseClient(options =>
        {
            options.Url = "https://second.crm.dynamics.com";
            options.ClientId = "second-client-id";
            options.ClientSecret = "second-secret";
            options.DefaultBatchSize = 200;
        });

        using ServiceProvider provider = _services.BuildServiceProvider();
        DataverseClientOptions options = provider.GetRequiredService<IOptions<DataverseClientOptions>>().Value;

        // Assert - Should use the last configuration
        options.Url.Should().Be("https://second.crm.dynamics.com");
        options.ClientId.Should().Be("second-client-id");
        options.ClientSecret.Should().Be("second-secret");
        options.DefaultBatchSize.Should().Be(200);
    }

    #endregion

    #region AddDataverseClient(IConfiguration, string) Tests

    [Test]
    public void AddDataverseClient_WithConfiguration_ShouldBindConfigurationCorrectly()
    {
        // Act
        _services.AddDataverseClient(_configuration);

        using ServiceProvider provider = _services.BuildServiceProvider();
        DataverseClientOptions options = provider.GetRequiredService<IOptions<DataverseClientOptions>>().Value;

        // Assert
        options.Url.Should().Be("https://test.crm.dynamics.com");
        options.ClientId.Should().Be("test-client-id");
        options.ClientSecret.Should().Be("test-secret");
        options.DefaultBatchSize.Should().Be(150);
        options.MaxBatchSize.Should().Be(500);
        options.RetryAttempts.Should().Be(5);
        options.EnablePerformanceLogging.Should().BeTrue();
    }

    [Test]
    public void AddDataverseClient_WithConfigurationAndDefaultSection_ShouldUseDataverseSection()
    {
        // Act
        _services.AddDataverseClient(_configuration);

        using ServiceProvider provider = _services.BuildServiceProvider();
        DataverseClientOptions options = provider.GetRequiredService<IOptions<DataverseClientOptions>>().Value;

        // Assert
        options.Url.Should().Be("https://test.crm.dynamics.com");
        options.ClientId.Should().Be("test-client-id");
        options.ClientSecret.Should().Be("test-secret");
    }

    [Test]
    public void AddDataverseClient_WithConfigurationAndCustomSection_ShouldUseCustomSection()
    {
        // Act
        _services.AddDataverseClient(_configuration, "CustomSection");

        using ServiceProvider provider = _services.BuildServiceProvider();
        DataverseClientOptions options = provider.GetRequiredService<IOptions<DataverseClientOptions>>().Value;

        // Assert
        options.Url.Should().Be("https://custom.crm.dynamics.com");
        options.ClientId.Should().Be("custom-client-id");
        options.ClientSecret.Should().Be("custom-secret");
    }

    [Test]
    public void AddDataverseClient_WithConfiguration_ShouldReturnServiceCollection()
    {
        // Act
        IServiceCollection result = _services.AddDataverseClient(_configuration);

        // Assert
        result.Should().BeSameAs(_services);
    }

    #endregion

    #region Service Registration Tests

    [Test]
    public void AddDataverseClient_ShouldRegisterServicesWithCorrectLifetimes()
    {
        // Act
        _services.AddDataverseClient(options =>
        {
            options.Url = "https://test.crm.dynamics.com";
            options.ClientId = "test-client-id";
            options.ClientSecret = "test-secret";
        });

        // Assert - Check service descriptors for correct lifetimes
        ServiceDescriptor? optionsDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IConfigureOptions<DataverseClientOptions>));
        optionsDescriptor.Should().NotBeNull();
        optionsDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

        ServiceDescriptor? validatorDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IValidateOptions<DataverseClientOptions>));
        validatorDescriptor.Should().NotBeNull();
        validatorDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

        ServiceDescriptor? serviceClientDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(ServiceClient));
        serviceClientDescriptor.Should().NotBeNull();
        serviceClientDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

        ServiceDescriptor? batchProcessorDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IBatchProcessor));
        batchProcessorDescriptor.Should().NotBeNull();
        batchProcessorDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

        ServiceDescriptor? dataverseClientDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IDataverseClient));
        dataverseClientDescriptor.Should().NotBeNull();
        dataverseClientDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

        ServiceDescriptor? metadataClientDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IDataverseMetadataClient));
        metadataClientDescriptor.Should().NotBeNull();
        metadataClientDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Test]
    public void AddDataverseClient_ShouldRegisterCorrectImplementations()
    {
        // Act
        _services.AddDataverseClient(options =>
        {
            options.Url = "https://test.crm.dynamics.com";
            options.ClientId = "test-client-id";
            options.ClientSecret = "test-secret";
        });

        // Assert - Check service descriptors for correct implementations
        ServiceDescriptor? validatorDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IValidateOptions<DataverseClientOptions>));
        validatorDescriptor.Should().NotBeNull();
        validatorDescriptor!.ImplementationType.Should().Be(typeof(DataverseClientOptionsValidator));

        ServiceDescriptor? batchProcessorDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IBatchProcessor));
        batchProcessorDescriptor.Should().NotBeNull();
        batchProcessorDescriptor!.ImplementationType.Should().Be(typeof(BatchProcessor));

        ServiceDescriptor? dataverseClientDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IDataverseClient));
        dataverseClientDescriptor.Should().NotBeNull();
        dataverseClientDescriptor!.ImplementationType.Should().Be(typeof(DataverseClient));

        ServiceDescriptor? metadataClientDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IDataverseMetadataClient));
        metadataClientDescriptor.Should().NotBeNull();
        metadataClientDescriptor!.ImplementationType.Should().Be(typeof(DataverseMetadataClient));
    }

    #endregion

    #region DataverseClientOptionsValidator Tests

    [Test]
    public void DataverseClientOptionsValidator_WithInvalidOptions_ShouldReturnFailure()
    {
        // Arrange
        DataverseClientOptionsValidator validator = new();
        DataverseClientOptions invalidOptions = new()
        {
            // Missing connection information
            DefaultBatchSize = -1, // Invalid batch size
            RetryAttempts = -1 // Invalid retry attempts
        };

        // Act
        ValidateOptionsResult result = validator.Validate("DataverseClientOptions", invalidOptions);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Failed.Should().BeTrue();
        result.Failures.Should().NotBeEmpty();
        result.Failures.Should().HaveCountGreaterThan(0);
    }

    [Test]
    public void DataverseClientOptionsValidator_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        DataverseClientOptionsValidator validator = new();
        DataverseClientOptions invalidOptions = new()
        {
            // Multiple validation errors
            DefaultBatchSize = -1,
            MaxBatchSize = -1,
            RetryAttempts = -1,
            ConnectionTimeoutSeconds = -1
        };

        // Act
        ValidateOptionsResult result = validator.Validate("DataverseClientOptions", invalidOptions);

        // Assert
        result.Should().NotBeNull();
        result.Failed.Should().BeTrue();
        result.Failures.Should().NotBeEmpty();
        
        // Should contain multiple error messages
        string allFailures = string.Join(", ", result.Failures);
        allFailures.Should().Contain("DefaultBatchSize");
        allFailures.Should().Contain("MaxBatchSize");
        allFailures.Should().Contain("RetryAttempts");
        allFailures.Should().Contain("ConnectionTimeoutSeconds");
    }

    [Test]
    public void DataverseClientOptionsValidator_WithConnectionStringOptions_ShouldReturnSuccess()
    {
        // Arrange
        DataverseClientOptionsValidator validator = new();
        DataverseClientOptions options = TestOptions.CreateOptionsWithConnectionString();

        // Act
        ValidateOptionsResult result = validator.Validate("DataverseClientOptions", options);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Failed.Should().BeFalse();
    }

    [Test]
    public void DataverseClientOptionsValidator_WithNullName_ShouldValidateOptions()
    {
        // Arrange
        DataverseClientOptionsValidator validator = new();
        DataverseClientOptions validOptions = TestOptions.CreateValidOptions();

        // Act
        ValidateOptionsResult result = validator.Validate(null, validOptions);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
    }

    [Test]
    public void DataverseClientOptionsValidator_WithEmptyName_ShouldValidateOptions()
    {
        // Arrange
        DataverseClientOptionsValidator validator = new();
        DataverseClientOptions validOptions = TestOptions.CreateValidOptions();

        // Act
        ValidateOptionsResult result = validator.Validate(string.Empty, validOptions);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
    }

    #endregion
}

