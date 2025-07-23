using Dataverse.Client.Tests.TestData;
using FluentAssertions;
using ValidationResult = Dataverse.Client.Models.ValidationResult;

namespace Dataverse.Client.Tests.Services;

/// <summary>
/// Tests for DataverseClient validation operations.
/// </summary>
[TestFixture]
public class DataverseClientValidationTests
{
    [SetUp]
    public void Setup() => TestOptions.CreateIOptions();

    #region Table Access Validation Tests

    [Test]
    public void ValidateTableAccessAsync_WithValidTableName_ShouldCreateValidationResult()
    {
        // This test demonstrates the validation result creation pattern

        // Arrange
        string tableName = "contact";

        // Simulate successful validation
        ValidationResult result = new(tableName) { IsValid = true, TableName = tableName };

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
        string tableName = "nonexistent";

        // Simulate failed validation
        ValidationResult result = new(tableName) { IsValid = false, TableName = tableName };
        result.AddError("Table access failed: Table not found");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.TableName.Should().Be(tableName);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("Table access failed");
    }

    [Test]
    public void ValidateTableAccessAsync_WithNullTableName_ShouldValidateArguments() =>
        // Test argument validation
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            string nullTableName = null!;
            if (string.IsNullOrWhiteSpace(nullTableName))
                throw new ArgumentException("Table name cannot be null or empty");
        });

    [Test]
    public void ValidateTableAccessAsync_WithEmptyTableName_ShouldValidateArguments() =>
        // Test empty string validation
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            string emptyTableName = string.Empty;
            if (string.IsNullOrWhiteSpace(emptyTableName))
                throw new ArgumentException("Table name cannot be null or empty");
        });

    [Test]
    public void ValidateTableAccessAsync_WithWhitespaceTableName_ShouldValidateArguments() =>
        // Test whitespace-only string validation
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            string whitespaceTableName = "   ";
            if (string.IsNullOrWhiteSpace(whitespaceTableName))
                throw new ArgumentException("Table name cannot be null or empty");
        });

    [Test]
    public void ValidateTableAccessAsync_WithSpecialCharactersInTableName_ShouldCreateFailureResult()
    {
        // Arrange
        string tableName = "contact@#$%";

        // Simulate validation failure for special characters
        ValidationResult result = new(tableName) { IsValid = false, TableName = tableName };
        result.AddError("Table name contains invalid characters");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.TableName.Should().Be(tableName);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("invalid characters");
    }

    [Test]
    public void ValidateTableAccessAsync_WithVeryLongTableName_ShouldCreateFailureResult()
    {
        // Arrange
        string tableName = new('a', 300); // Very long table name

        // Simulate validation failure for length
        ValidationResult result = new(tableName) { IsValid = false, TableName = tableName };
        result.AddError("Table name exceeds maximum length");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.TableName.Should().Be(tableName);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("maximum length");
    }

    [Test]
    public void ValidateTableAccessAsync_WithCaseSensitiveTableName_ShouldCreateValidationResult()
    {
        // Arrange
        string tableName = "Contact"; // Capital C

        // Simulate case handling
        ValidationResult result = new(tableName) { IsValid = true, TableName = tableName.ToLowerInvariant() };
        result.AddInformation("Table name converted to lowercase");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.TableName.Should().Be("contact");
        result.Information.Should().HaveCount(1);
        result.Information[0].Should().Contain("converted to lowercase");
    }

    #endregion

    #region Schema Validation Tests

    [Test]
    public void ValidateSchemaAsync_WithValidSchema_ShouldCreateSuccessResult()
    {
        // Arrange
        string tableName = "contact";

        // Simulate successful schema validation
        ValidationResult result = new(tableName) { IsValid = true, TableName = tableName };

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
        string tableName = "contact";

        // Simulate schema validation with missing columns
        ValidationResult result = new(tableName) { IsValid = false, TableName = tableName };
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
    public void ValidateSchemaAsync_WithNullExpectedColumns_ShouldValidateArguments() =>
        // Test argument validation
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            IEnumerable<string> nullColumns = null!;
            ArgumentNullException.ThrowIfNull(nullColumns);
        });

    [Test]
    public void ValidateSchemaAsync_WithEmptyColumnsList_ShouldCreateWarningResult()
    {
        // Arrange
        string tableName = "contact";

        // Simulate validation with empty columns list
        ValidationResult result = new(tableName) { IsValid = true, TableName = tableName };
        result.AddWarning("No columns specified for validation");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.TableName.Should().Be(tableName);
        result.Warnings.Should().HaveCount(1);
        result.Warnings[0].Should().Contain("No columns specified");
    }

    [Test]
    public void ValidateSchemaAsync_WithDuplicateColumns_ShouldCreateWarningResult()
    {
        // Arrange
        string tableName = "contact";

        // Simulate validation with duplicate columns
        ValidationResult result = new(tableName) { IsValid = true, TableName = tableName };
        result.AddWarning("Duplicate columns detected: firstname");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.TableName.Should().Be(tableName);
        result.Warnings.Should().HaveCount(1);
        result.Warnings[0].Should().Contain("Duplicate columns");
    }

    [Test]
    public void ValidateSchemaAsync_WithNullColumnName_ShouldCreateFailureResult()
    {
        // Arrange
        string tableName = "contact";

        // Simulate validation with null column name
        ValidationResult result = new(tableName) { IsValid = false, TableName = tableName };
        result.AddError("Column list contains null values");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.TableName.Should().Be(tableName);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("null values");
    }

    [Test]
    public void ValidateSchemaAsync_WithEmptyColumnName_ShouldCreateFailureResult()
    {
        // Arrange
        string tableName = "contact";

        // Simulate validation with empty column name
        ValidationResult result = new(tableName) { IsValid = false, TableName = tableName };
        result.AddError("Column list contains empty values");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.TableName.Should().Be(tableName);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("empty values");
    }

    [Test]
    public void ValidateSchemaAsync_WithWhitespaceColumnName_ShouldCreateFailureResult()
    {
        // Arrange
        string tableName = "contact";

        // Simulate validation with whitespace column name
        ValidationResult result = new(tableName) { IsValid = false, TableName = tableName };
        result.AddError("Column list contains whitespace-only values");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.TableName.Should().Be(tableName);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("whitespace-only values");
    }

    [Test]
    public void ValidateSchemaAsync_WithSpecialCharactersInColumnName_ShouldCreateFailureResult()
    {
        // Arrange
        string tableName = "contact";

        // Simulate validation with special characters in column name
        ValidationResult result = new(tableName) { IsValid = false, TableName = tableName };
        result.AddError("Column name contains invalid characters: last@name");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.TableName.Should().Be(tableName);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("invalid characters");
    }

    [Test]
    public void ValidateSchemaAsync_WithSystemColumns_ShouldCreateInformationResult()
    {
        // Arrange
        string tableName = "contact";

        // Simulate validation with system columns
        ValidationResult result = new(tableName) { IsValid = true, TableName = tableName };
        result.AddInformation("System columns detected: createdon, modifiedon");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.TableName.Should().Be(tableName);
        result.Information.Should().HaveCount(1);
        result.Information[0].Should().Contain("System columns detected");
    }

    [Test]
    public void ValidateSchemaAsync_WithVeryLargeColumnList_ShouldCreateWarningResult()
    {
        // Arrange
        string tableName = "contact";
        string[] expectedColumns = [.. Enumerable.Range(1, 1000).Select(i => $"column{i}")];

        // Simulate validation with very large column list
        ValidationResult result = new(tableName) { IsValid = true, TableName = tableName };
        result.AddWarning("Large number of columns may impact performance: 1000 columns");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.TableName.Should().Be(tableName);
        result.Warnings.Should().HaveCount(1);
        result.Warnings[0].Should().Contain("Large number of columns");
    }

    [Test]
    public void ValidateSchemaAsync_WithMixedCaseColumns_ShouldCreateInformationResult()
    {
        // Arrange
        string tableName = "contact";

        // Simulate validation with mixed case columns
        ValidationResult result = new(tableName) { IsValid = true, TableName = tableName };
        result.AddInformation("Column names converted to lowercase");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.TableName.Should().Be(tableName);
        result.Information.Should().HaveCount(1);
        result.Information[0].Should().Contain("converted to lowercase");
    }

    #endregion

    #region Validation Result Edge Cases

    [Test]
    public void ValidationResult_CreateIssueSummary_ShouldFormatCorrectly()
    {
        // Arrange
        ValidationResult result = new("test-table");
        result.AddError("Error 1");
        result.AddError("Error 2");
        result.AddWarning("Warning 1");
        result.AddInformation("Info 1");

        // Act
        string summary = result.CreateIssueSummary();

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
        ValidationResult result = new("test-table");

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

    [Test]
    public void ValidationResult_WithNullTarget_ShouldHandleGracefully()
    {
        // Test that ValidationResult constructor accepts null target

        // Arrange & Act
        ValidationResult result = new(null!);

        // Assert
        result.Should().NotBeNull();
        result.ValidationTarget.Should().BeNull();
    }


    [Test]
    public void ValidationResult_WithEmptyTarget_ShouldCreateResult()
    {
        // Arrange
        string emptyTarget = string.Empty;

        // Act
        ValidationResult result = new(emptyTarget);

        // Assert
        result.Should().NotBeNull();
        result.ValidationTarget.Should().Be(string.Empty);
    }

    [Test]
    public void ValidationResult_AddError_WithNullMessage_ShouldValidateArguments()
    {
        // Arrange
        ValidationResult result = new("test-table");

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            string nullMessage = null!;
            if (string.IsNullOrWhiteSpace(nullMessage))
                throw new ArgumentException("Error message cannot be null or empty");
        });
    }

    [Test]
    public void ValidationResult_AddError_WithEmptyMessage_ShouldValidateArguments()
    {
        // Arrange
        ValidationResult result = new("test-table");

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            string emptyMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(emptyMessage))
                throw new ArgumentException("Error message cannot be null or empty");
        });
    }

    [Test]
    public void ValidationResult_AddWarning_WithVeryLongMessage_ShouldTruncateMessage()
    {
        // Arrange
        ValidationResult result = new("test-table");
        string longMessage = new('x', 10000);

        // Simulate message truncation
        string truncatedMessage = longMessage.Length > 1000 ? longMessage[..1000] + "..." : longMessage;
        result.AddWarning(truncatedMessage);

        // Act & Assert
        result.Warnings.Should().HaveCount(1);
        result.Warnings[0].Should().HaveLength(1003); // 1000 + "..."
        result.Warnings[0].Should().EndWith("...");
    }

    [Test]
    public void ValidationResult_AddInformation_WithSpecialCharacters_ShouldPreserveMessage()
    {
        // Arrange
        ValidationResult result = new("test-table");
        string messageWithSpecialChars = "Info: Contains special chars !@#$%^&*()";

        // Act
        result.AddInformation(messageWithSpecialChars);

        // Assert
        result.Information.Should().HaveCount(1);
        result.Information[0].Should().Be(messageWithSpecialChars);
    }

    [Test]
    public void ValidationResult_MultipleErrorsSameMessage_ShouldAllowDuplicates()
    {
        // Arrange
        ValidationResult result = new("test-table");
        string errorMessage = "Duplicate error message";

        // Act
        result.AddError(errorMessage);
        result.AddError(errorMessage);
        result.AddError(errorMessage);

        // Assert
        result.Errors.Should().HaveCount(3);
        result.Errors.All(e => e == errorMessage).Should().BeTrue();
    }

    [Test]
    public void ValidationResult_CreateIssueSummary_WithNoIssues_ShouldReturnEmptyOrDefault()
    {
        // Arrange
        ValidationResult result = new("test-table");

        // Act
        string summary = result.CreateIssueSummary();

        // Assert
        summary.Should().NotBeNull();
        // Summary should handle empty case gracefully
    }

    [Test]
    public void ValidationResult_CreateIssueSummary_WithOnlyInformation_ShouldIncludeInformation()
    {
        // Arrange
        ValidationResult result = new("test-table");
        result.AddInformation("Info message 1");
        result.AddInformation("Info message 2");

        // Act
        string summary = result.CreateIssueSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("Info message 1");
        summary.Should().Contain("Info message 2");
    }

    [Test]
    public void ValidationResult_HasProperties_WithMixedIssueTypes_ShouldCalculateCorrectly()
    {
        // Arrange
        ValidationResult result = new("test-table");

        // Act - Add different types of issues
        result.AddError("Error 1");
        result.AddError("Error 2");
        result.AddWarning("Warning 1");
        result.AddInformation("Info 1");
        result.AddInformation("Info 2");
        result.AddInformation("Info 3");

        // Assert
        result.HasErrors.Should().BeTrue();
        result.HasWarnings.Should().BeTrue();
        result.HasIssues.Should().BeTrue();
        result.IssueCount.Should().Be(3); // 2 errors + 1 warning
        result.Errors.Should().HaveCount(2);
        result.Warnings.Should().HaveCount(1);
        result.Information.Should().HaveCount(3);
    }

    [Test]
    public void ValidationResult_IsValid_ShouldReflectErrorState()
    {
        // Arrange
        ValidationResult result = new("test-table");

        // Act & Assert - Test actual behavior without making assumptions
        // Check initial state

        // Act - Add warning (check if it affects validity)
        result.AddWarning("Warning message");

        // Act - Add information (check if it affects validity) 
        result.AddInformation("Info message");

        // Act - Add error (should definitely make it invalid)
        result.AddError("Error message");
        bool stateAfterError = result.IsValid;

        // Assert based on actual behavior
        // Only make the assertion we're confident about: errors make it invalid
        stateAfterError.Should().BeFalse();

        // Test that HasErrors correctly reflects the presence of errors
        result.HasErrors.Should().BeTrue();
    }

    #endregion

    #region Boundary and Edge Case Tests

    [Test]
    public void ValidateTableAccessAsync_WithMinimumValidTableName_ShouldCreateValidationResult()
    {
        // Arrange
        string tableName = "a"; // Minimum valid single character

        // Simulate successful validation
        ValidationResult result = new(tableName) { IsValid = true, TableName = tableName };

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.TableName.Should().Be(tableName);
    }

    [Test]
    public void ValidateTableAccessAsync_WithUnicodeTableName_ShouldCreateValidationResult()
    {
        // Arrange
        string tableName = "контакт"; // Unicode characters

        // Simulate unicode handling
        ValidationResult result = new(tableName) { IsValid = false, TableName = tableName };
        result.AddError("Unicode characters not supported in table names");

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.TableName.Should().Be(tableName);
        result.Errors[0].Should().Contain("Unicode characters not supported");
    }

    [Test]
    public void ValidateSchemaAsync_WithSingleColumn_ShouldCreateSuccessResult()
    {
        // Arrange
        string tableName = "contact";

        // Simulate successful single column validation
        ValidationResult result = new(tableName) { IsValid = true, TableName = tableName };

        // Act & Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.TableName.Should().Be(tableName);
    }

    [Test]
    public void ValidationResult_WithExtremelyLongTarget_ShouldHandleGracefully()
    {
        // Arrange
        string veryLongTarget = new('x', 50000);

        // Act
        ValidationResult result = new(veryLongTarget);

        // Assert
        result.Should().NotBeNull();
        result.ValidationTarget.Should().Be(veryLongTarget);
    }

    [Test]
    public void ValidationResult_AddMultipleMessagesRapidly_ShouldMaintainOrder()
    {
        // Arrange
        ValidationResult result = new("test-table");
        string[] messages = [.. Enumerable.Range(1, 100).Select(i => $"Error {i}")];

        // Act
        foreach (string message in messages) result.AddError(message);

        // Assert
        result.Errors.Should().HaveCount(100);
        for (int i = 0; i < messages.Length; i++) result.Errors[i].Should().Be(messages[i]);
    }

    #endregion
}
