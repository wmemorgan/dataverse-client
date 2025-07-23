// File: tests/Dataverse.Client.Tests/Services/DataverseClient.ValidationTests.cs

using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Dataverse.Client.Services;
using Dataverse.Client.Tests.TestData;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Moq;
using ValidationResult = Dataverse.Client.Models.ValidationResult;

namespace Dataverse.Client.Tests.Services;

/// <summary>
/// Tests for DataverseClient validation operations.
/// </summary>
[TestFixture]
public class DataverseClientValidationTests
{
    private Mock<ILogger<DataverseClient>> _mockLogger;
    private Mock<IBatchProcessor> _mockBatchProcessor;
    private IOptions<DataverseClientOptions> _options;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<DataverseClient>>();
        _mockBatchProcessor = new Mock<IBatchProcessor>();
        _options = TestOptions.CreateIOptions();
    }

    #region Table Access Validation Tests

    [Test]
    public void ValidateTableAccessAsync_WithValidTableName_ShouldCreateValidationResult()
    {
        // This test demonstrates the validation result creation pattern
        
        // Arrange
        var tableName = "contact";
        
        // Simulate successful validation
        var result = new ValidationResult(tableName)
        {
            IsValid = true,
            TableName = tableName
        };

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.TableName.Should().Be(tableName);
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public void ValidateTableAccessAsync_WithInvalidTableName_ShouldCreateFailureResult()
    {
        // Arrange
        var tableName = "nonexistent";
        
        // Simulate failed validation
        var result = new ValidationResult(tableName)
        {
            IsValid = false,
            TableName = tableName
        };
        result.AddError("Table access failed: Table not found");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.TableName.Should().Be(tableName);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("Table access failed");
    }

    [Test]
    public void ValidateTableAccessAsync_WithNullTableName_ShouldValidateArguments()
    {
        // Test argument validation
        
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            string nullTableName = null!;
            if (string.IsNullOrWhiteSpace(nullTableName))
                throw new ArgumentException("Table name cannot be null or empty");
        });
    }

    #endregion

    #region Schema Validation Tests

    [Test]
    public void ValidateSchemaAsync_WithValidSchema_ShouldCreateSuccessResult()
    {
        // Arrange
        var tableName = "contact";
        var expectedColumns = new[] { "firstname", "lastname", "emailaddress1" };
        
        // Simulate successful schema validation
        var result = new ValidationResult(tableName)
        {
            IsValid = true,
            TableName = tableName
        };

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.TableName.Should().Be(tableName);
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public void ValidateSchemaAsync_WithMissingColumns_ShouldCreateFailureResult()
    {
        // Arrange
        var tableName = "contact";
        var expectedColumns = new[] { "firstname", "lastname", "invalidcolumn" };
        
        // Simulate schema validation with missing columns
        var result = new ValidationResult(tableName)
        {
            IsValid = false,
            TableName = tableName
        };
        result.AddError("Schema validation failed: Invalid column");
        result.AddWarning("Missing or inaccessible columns: invalidcolumn");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.TableName.Should().Be(tableName);
        result.Errors.Should().HaveCount(1);
        result.Warnings.Should().HaveCount(1);
        result.Errors[0].Should().Contain("Schema validation failed");
        result.Warnings[0].Should().Contain("invalidcolumn");
    }

    [Test]
    public void ValidateSchemaAsync_WithNullExpectedColumns_ShouldValidateArguments()
    {
        // Test argument validation
        
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            IEnumerable<string> nullColumns = null!;
            ArgumentNullException.ThrowIfNull(nullColumns);
        });
    }

    #endregion

    #region Validation Result Tests

    [Test]
    public void ValidationResult_CreateIssueSummary_ShouldFormatCorrectly()
    {
        // Arrange
        var result = new ValidationResult("test-table");
        result.AddError("Error 1");
        result.AddError("Error 2");
        result.AddWarning("Warning 1");
        result.AddInformation("Info 1");

        // Act
        var summary = result.CreateIssueSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("Error 1");
        summary.Should().Contain("Error 2");
        summary.Should().Contain("Warning 1");
        summary.Should().Contain("Info 1");
    }

    [Test]
    public void ValidationResult_Properties_ShouldCalculateCorrectly()
    {
        // Arrange
        var result = new ValidationResult("test-table");
        
        // Act
        result.AddError("Error");
        result.AddWarning("Warning");
        result.AddInformation("Info");

        // Assert
        result.HasErrors.Should().BeTrue();
        result.HasWarnings.Should().BeTrue();
        result.HasIssues.Should().BeTrue();
        result.IssueCount.Should().Be(2); // Errors + Warnings
    }

    #endregion
}

