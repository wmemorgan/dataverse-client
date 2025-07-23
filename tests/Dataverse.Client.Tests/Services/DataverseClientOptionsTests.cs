using Dataverse.Client.Models;
using Dataverse.Client.Tests.TestData;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Dataverse.Client.Tests.Services;

/// <summary>
/// Tests for DataverseClientOptions configuration validation coverage.
/// These tests focus on configuration validation, effective values calculation, and options cloning.
/// </summary>
[TestFixture]
public class DataverseClientOptionsTests
{
    #region Default Values Tests

    [Test]
    public void Constructor_ShouldInitializeWithCorrectDefaults()
    {
        // Act
        DataverseClientOptions options = new();

        // Assert
        options.Should().NotBeNull();
        options.ConnectionString.Should().BeEmpty();
        options.Url.Should().BeEmpty();
        options.ClientId.Should().BeEmpty();
        options.ClientSecret.Should().BeEmpty();
        options.DefaultBatchSize.Should().Be(100);
        options.MaxBatchSize.Should().Be(1000);
        options.RetryAttempts.Should().Be(3);
        options.RetryDelayMs.Should().Be(1000);
        options.ConnectionTimeoutSeconds.Should().Be(300);
        options.EnableRetryOnFailure.Should().BeTrue();
        options.EnablePerformanceLogging.Should().BeFalse();
        options.EnableProgressReporting.Should().BeFalse();
        options.BatchTimeoutMs.Should().Be(300000);
        options.AdditionalConnectionParameters.Should().NotBeNull();
        options.AdditionalConnectionParameters.Should().BeEmpty();
    }

    [Test]
    public void UsesIndividualParameters_WithConnectionString_ShouldReturnFalse()
    {
        // Arrange
        DataverseClientOptions options = new()
        {
            ConnectionString = "AuthType=ClientSecret;Url=https://test.crm.dynamics.com;ClientId=test;ClientSecret=secret"
        };

        // Act & Assert
        options.UsesIndividualParameters.Should().BeFalse();
    }

    [Test]
    public void UsesIndividualParameters_WithoutConnectionString_ShouldReturnTrue()
    {
        // Arrange
        DataverseClientOptions options = new()
        {
            Url = "https://test.crm.dynamics.com",
            ClientId = "test-client-id",
            ClientSecret = "test-secret"
        };

        // Act & Assert
        options.UsesIndividualParameters.Should().BeTrue();
    }

    #endregion

    #region Validation Tests

    [Test]
    public void Validate_WithValidConnectionString_ShouldNotThrow()
    {
        // Arrange
        DataverseClientOptions options = new()
        {
            ConnectionString = "AuthType=ClientSecret;Url=https://test.crm.dynamics.com;ClientId=test;ClientSecret=secret"
        };

        // Act & Assert
        Assert.DoesNotThrow(() => options.Validate());
    }

    [Test]
    public void Validate_WithValidIndividualParameters_ShouldNotThrow()
    {
        // Arrange
        DataverseClientOptions options = new()
        {
            Url = "https://test.crm.dynamics.com",
            ClientId = "test-client-id",
            ClientSecret = "test-secret"
        };

        // Act & Assert
        Assert.DoesNotThrow(() => options.Validate());
    }

    [Test]
    public void Validate_WithEmptyConnectionInfo_ShouldThrowInvalidOperationException()
    {
        // Arrange
        DataverseClientOptions options = new();

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        exception.Message.Should().Contain("ConnectionString or individual settings");
    }

    [Test]
    public void Validate_WithInvalidUrl_ShouldThrowInvalidOperationException()
    {
        // Arrange
        DataverseClientOptions options = new()
        {
            Url = "invalid-url",
            ClientId = "test-client-id",
            ClientSecret = "test-secret"
        };

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        exception.Message.Should().Contain("Url format is invalid");
    }


    [Test]
    public void Validate_WithNegativeBatchSize_ShouldThrowInvalidOperationException()
    {
        // Arrange
        DataverseClientOptions options = TestOptions.CreateValidOptions();
        options.DefaultBatchSize = -1;

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        exception.Message.Should().Contain("DefaultBatchSize");
    }

    [Test]
    public void Validate_WithExcessiveMaxBatchSize_ShouldThrowInvalidOperationException()
    {
        // Arrange
        DataverseClientOptions options = TestOptions.CreateValidOptions();
        options.MaxBatchSize = 5000; // Exceeds Microsoft's limit

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        exception.Message.Should().Contain("MaxBatchSize");
    }

    [Test]
    public void Validate_WithNegativeRetryAttempts_ShouldThrowInvalidOperationException()
    {
        // Arrange
        DataverseClientOptions options = TestOptions.CreateValidOptions();
        options.RetryAttempts = -1;

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        exception.Message.Should().Contain("RetryAttempts");
    }

    [Test]
    public void Validate_WithNegativeConnectionTimeout_ShouldThrowInvalidOperationException()
    {
        // Arrange
        DataverseClientOptions options = TestOptions.CreateValidOptions();
        options.ConnectionTimeoutSeconds = -1;

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        exception.Message.Should().Contain("ConnectionTimeoutSeconds");
    }

    [Test]
    public void Validate_WithDefaultLargerThanMax_ShouldThrowInvalidOperationException()
    {
        // Arrange
        DataverseClientOptions options = TestOptions.CreateValidOptions();
        options.DefaultBatchSize = 500;
        options.MaxBatchSize = 100; // Max smaller than default

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        exception.Message.Should().Contain("DefaultBatchSize");
    }

    #endregion

    #region GetValidationErrors Tests

    [Test]
    public void GetValidationErrors_WithValidConfiguration_ShouldReturnEmptyList()
    {
        // Arrange
        DataverseClientOptions options = TestOptions.CreateValidOptions();

        // Act
        List<string> errors = options.GetValidationErrors();

        // Assert
        errors.Should().NotBeNull();
        errors.Should().BeEmpty();
    }

    [Test]
    public void GetValidationErrors_WithMultipleIssues_ShouldReturnAllErrors()
    {
        // Arrange
        DataverseClientOptions options = new()
        {
            DefaultBatchSize = -1,
            RetryAttempts = -1,
            ConnectionTimeoutSeconds = -1
        };

        // Act
        List<string> errors = options.GetValidationErrors();

        // Assert
        errors.Should().NotBeNull();
        errors.Should().HaveCountGreaterThan(0);
        errors.Should().Contain(e => e.Contains("DefaultBatchSize"));
        errors.Should().Contain(e => e.Contains("RetryAttempts"));
        errors.Should().Contain(e => e.Contains("ConnectionTimeoutSeconds"));
    }

    [Test]
    public void GetValidationErrors_WithMissingConnectionInfo_ShouldReturnConnectionError()
    {
        // Arrange
        DataverseClientOptions options = new();

        // Act
        List<string> errors = options.GetValidationErrors();

        // Assert
        errors.Should().NotBeNull();
        errors.Should().HaveCountGreaterThan(0);
        errors.Should().Contain(e => e.Contains("ConnectionString or individual settings"));
    }

    [Test]
    public void IsValid_WithValidConfiguration_ShouldReturnTrue()
    {
        // Arrange
        DataverseClientOptions options = TestOptions.CreateValidOptions();

        // Act & Assert
        options.IsValid.Should().BeTrue();
    }

    [Test]
    public void IsValid_WithInvalidConfiguration_ShouldReturnFalse()
    {
        // Arrange
        DataverseClientOptions options = new()
        {
            DefaultBatchSize = -1
        };

        // Act & Assert
        options.IsValid.Should().BeFalse();
    }

    #endregion

    #region GetEffectiveConnectionString Tests

    [Test]
    public void GetEffectiveConnectionString_WithConnectionString_ShouldReturnOriginal()
    {
        // Arrange
        string expectedConnectionString = "AuthType=ClientSecret;Url=https://test.crm.dynamics.com;ClientId=test;ClientSecret=secret";
        DataverseClientOptions options = new()
        {
            ConnectionString = expectedConnectionString
        };

        // Act
        string effectiveConnectionString = options.GetEffectiveConnectionString();

        // Assert
        effectiveConnectionString.Should().Be(expectedConnectionString);
    }

    [Test]
    public void GetEffectiveConnectionString_WithIndividualParameters_ShouldBuildConnectionString()
    {
        // Arrange
        DataverseClientOptions options = new()
        {
            Url = "https://test.crm.dynamics.com",
            ClientId = "test-client-id",
            ClientSecret = "test-secret"
        };

        // Act
        string effectiveConnectionString = options.GetEffectiveConnectionString();

        // Assert
        effectiveConnectionString.Should().NotBeNullOrEmpty();
        effectiveConnectionString.Should().Contain("AuthType=ClientSecret");
        effectiveConnectionString.Should().Contain("Url=https://test.crm.dynamics.com");
        effectiveConnectionString.Should().Contain("ClientId=test-client-id");
        effectiveConnectionString.Should().Contain("ClientSecret=test-secret");
    }

    [Test]
    public void GetEffectiveConnectionString_WithAdditionalParameters_ShouldIncludeAll()
    {
        // Arrange
        DataverseClientOptions options = new()
        {
            ConnectionString = "AuthType=ClientSecret;Url=https://test.crm.dynamics.com;ClientId=test;ClientSecret=secret",
            AdditionalConnectionParameters = new Dictionary<string, string>
            {
                ["RequireNewInstance"] = "true",
                ["LoginPrompt"] = "Never"
            }
        };

        // Act
        string effectiveConnectionString = options.GetEffectiveConnectionString();

        // Assert
        effectiveConnectionString.Should().Contain("RequireNewInstance=true");
        effectiveConnectionString.Should().Contain("LoginPrompt=Never");
    }

    #endregion

    #region Clone Tests

    [Test]
    public void Clone_WithoutModifier_ShouldCreateExactCopy()
    {
        // Arrange
        DataverseClientOptions original = TestOptions.CreateValidOptions();
        original.DefaultBatchSize = 150;
        original.EnablePerformanceLogging = true;

        // Act
        DataverseClientOptions clone = original.Clone();

        // Assert
        clone.Should().NotBeSameAs(original);
        clone.Url.Should().Be(original.Url);
        clone.ClientId.Should().Be(original.ClientId);
        clone.ClientSecret.Should().Be(original.ClientSecret);
        clone.DefaultBatchSize.Should().Be(150);
        clone.EnablePerformanceLogging.Should().BeTrue();
        clone.AdditionalConnectionParameters.Should().NotBeSameAs(original.AdditionalConnectionParameters);
    }

    [Test]
    public void Clone_WithModifier_ShouldApplyModifications()
    {
        // Arrange
        DataverseClientOptions original = TestOptions.CreateValidOptions();

        // Act
        DataverseClientOptions clone = original.Clone(options =>
        {
            options.DefaultBatchSize = 200;
            options.EnablePerformanceLogging = true;
            options.RetryAttempts = 5;
        });

        // Assert
        clone.Should().NotBeSameAs(original);
        clone.Url.Should().Be(original.Url); // Unchanged
        clone.DefaultBatchSize.Should().Be(200); // Modified
        clone.EnablePerformanceLogging.Should().BeTrue(); // Modified
        clone.RetryAttempts.Should().Be(5); // Modified
    }

    [Test]
    public void Clone_ShouldDeepCopyCollections()
    {
        // Arrange
        DataverseClientOptions original = TestOptions.CreateValidOptions();
        original.AdditionalConnectionParameters["test"] = "value";

        // Act
        DataverseClientOptions clone = original.Clone();
        clone.AdditionalConnectionParameters["new"] = "newvalue";

        // Assert
        original.AdditionalConnectionParameters.Should().ContainKey("test");
        original.AdditionalConnectionParameters.Should().NotContainKey("new");
        clone.AdditionalConnectionParameters.Should().ContainKey("test");
        clone.AdditionalConnectionParameters.Should().ContainKey("new");
    }

    #endregion

    #region ToString Tests

    [Test]
    public void ToString_ShouldMaskSensitiveData()
    {
        // Arrange
        DataverseClientOptions options = new()
        {
            Url = "https://test.crm.dynamics.com",
            ClientId = "test-client-id",
            ClientSecret = "very-secret-value"
        };

        // Act
        string result = options.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("https://test.crm.dynamics.com");
        result.Should().Contain("test-client-id");
        result.Should().NotContain("very-secret-value"); // Should be masked
        result.Should().Contain("***"); // Should contain masking characters
    }

    [Test]
    public void ToString_WithConnectionString_ShouldMaskSensitiveData()
    {
        // Arrange
        DataverseClientOptions options = new()
        {
            ConnectionString = "AuthType=ClientSecret;Url=https://test.crm.dynamics.com;ClientId=test;ClientSecret=secret"
        };

        // Act
        string result = options.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("***"); // Should mask the entire connection string
        result.Should().NotContain("secret"); // Should be masked
    }

    #endregion

    #region Boundary Value Tests

    [Test]
    public void Validate_WithMinimumValidBatchSize_ShouldNotThrow()
    {
        // Arrange
        DataverseClientOptions options = TestOptions.CreateValidOptions();
        options.DefaultBatchSize = 1;
        options.MaxBatchSize = 1;

        // Act & Assert
        Assert.DoesNotThrow(() => options.Validate());
    }

    [Test]
    public void Validate_WithMaximumValidBatchSize_ShouldNotThrow()
    {
        // Arrange
        DataverseClientOptions options = TestOptions.CreateValidOptions();
        options.DefaultBatchSize = 1000;
        options.MaxBatchSize = 1000;

        // Act & Assert
        Assert.DoesNotThrow(() => options.Validate());
    }

    [Test]
    public void Validate_WithZeroRetryAttempts_ShouldNotThrow()
    {
        // Arrange
        DataverseClientOptions options = TestOptions.CreateValidOptions();
        options.RetryAttempts = 0;

        // Act & Assert
        Assert.DoesNotThrow(() => options.Validate());
    }

    [Test]
    public void Validate_WithExcessiveRetryAttempts_ShouldThrowInvalidOperationException()
    {
        // Arrange
        DataverseClientOptions options = TestOptions.CreateValidOptions();
        options.RetryAttempts = 100; // Excessive

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        exception.Message.Should().Contain("RetryAttempts");
    }

    #endregion

    #region Connection String Format Tests

    [Test]
    public void Validate_WithMalformedConnectionString_ShouldThrowInvalidOperationException()
    {
        // Arrange
        DataverseClientOptions options = new()
        {
            ConnectionString = "invalid-connection-string"
        };

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        exception.Message.Should().Contain("ConnectionString format is invalid");
    }


    [Test]
    public void GetEffectiveConnectionString_WithEmptyIndividualParameters_ShouldThrowInvalidOperationException()
    {
        // Arrange
        DataverseClientOptions options = new()
        {
            Url = "",
            ClientId = "",
            ClientSecret = ""
        };

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            options.GetEffectiveConnectionString());
        exception.Message.Should().Contain("connection");
    }

    #endregion

    #region Integration with IOptions Tests

    [Test]
    public void IOptions_Integration_ShouldWorkCorrectly()
    {
        // Arrange
        DataverseClientOptions originalOptions = TestOptions.CreateValidOptions();
        IOptions<DataverseClientOptions> optionsWrapper = Options.Create(originalOptions);

        // Act & Assert
        optionsWrapper.Should().NotBeNull();
        optionsWrapper.Value.Should().BeSameAs(originalOptions);
        optionsWrapper.Value.IsValid.Should().BeTrue();
    }

    [Test]
    public void TestOptions_CreateValidOptions_ShouldReturnValidConfiguration()
    {
        // Act
        DataverseClientOptions options = TestOptions.CreateValidOptions();

        // Assert
        options.Should().NotBeNull();
        options.IsValid.Should().BeTrue();
        options.GetValidationErrors().Should().BeEmpty();
        Assert.DoesNotThrow(() => options.Validate());
    }

    [Test]
    public void TestOptions_CreateOptionsWithConnectionString_ShouldReturnValidConfiguration()
    {
        // Act
        DataverseClientOptions options = TestOptions.CreateOptionsWithConnectionString();

        // Assert
        options.Should().NotBeNull();
        options.ConnectionString.Should().NotBeEmpty();
        options.IsValid.Should().BeTrue();
        options.UsesIndividualParameters.Should().BeFalse();
    }

    #endregion
}


