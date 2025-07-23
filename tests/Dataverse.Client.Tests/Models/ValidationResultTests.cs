using FluentAssertions;
using ValidationResult = Dataverse.Client.Models.ValidationResult;

namespace Dataverse.Client.Tests.Models;

/// <summary>
/// Tests for ValidationResult model with complete coverage.
/// </summary>
[TestFixture]
public class ValidationResultTests
{
    #region Constructor Tests

    [Test]
    public void Constructor_WithValidTarget_ShouldCreateInstance()
    {
        // Arrange
        string validationTarget = "test-table";

        // Act
        ValidationResult result = new(validationTarget);

        // Assert
        result.Should().NotBeNull();
        result.ValidationTarget.Should().Be(validationTarget);
        result.IsValid.Should().BeFalse(); // Initial state is false
        result.TableName.Should().Be(string.Empty); // Initial state is empty string
        result.Errors.Should().NotBeNull().And.BeEmpty();
        result.Warnings.Should().NotBeNull().And.BeEmpty();
        result.Information.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void Constructor_WithNullTarget_ShouldCreateInstance()
    {
        // Arrange & Act
        ValidationResult result = new(null!);

        // Assert
        result.Should().NotBeNull();
        result.ValidationTarget.Should().BeNull();
        result.IsValid.Should().BeFalse(); // Initial state is false
        result.Errors.Should().NotBeNull().And.BeEmpty();
        result.Warnings.Should().NotBeNull().And.BeEmpty();
        result.Information.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void Constructor_WithEmptyTarget_ShouldCreateInstance()
    {
        // Arrange
        string emptyTarget = string.Empty;

        // Act
        ValidationResult result = new(emptyTarget);

        // Assert
        result.Should().NotBeNull();
        result.ValidationTarget.Should().Be(string.Empty);
        result.IsValid.Should().BeFalse(); // Initial state is false
    }

    [Test]
    public void Constructor_WithWhitespaceTarget_ShouldCreateInstance()
    {
        // Arrange
        string whitespaceTarget = "   ";

        // Act
        ValidationResult result = new(whitespaceTarget);

        // Assert
        result.Should().NotBeNull();
        result.ValidationTarget.Should().Be(whitespaceTarget);
        result.IsValid.Should().BeFalse(); // Initial state is false
    }

    [Test]
    public void Constructor_WithSpecialCharactersTarget_ShouldCreateInstance()
    {
        // Arrange
        string specialTarget = "table@#$%^&*()";

        // Act
        ValidationResult result = new(specialTarget);

        // Assert
        result.Should().NotBeNull();
        result.ValidationTarget.Should().Be(specialTarget);
        result.IsValid.Should().BeFalse(); // Initial state is false
    }

    [Test]
    public void Constructor_WithUnicodeTarget_ShouldCreateInstance()
    {
        // Arrange
        string unicodeTarget = "таблица测试";

        // Act
        ValidationResult result = new(unicodeTarget);

        // Assert
        result.Should().NotBeNull();
        result.ValidationTarget.Should().Be(unicodeTarget);
        result.IsValid.Should().BeFalse(); // Initial state is false
    }

    [Test]
    public void Constructor_WithVeryLongTarget_ShouldCreateInstance()
    {
        // Arrange
        string longTarget = new('x', 10000);

        // Act
        ValidationResult result = new(longTarget);

        // Assert
        result.Should().NotBeNull();
        result.ValidationTarget.Should().Be(longTarget);
        result.IsValid.Should().BeFalse(); // Initial state is false
    }

    #endregion

    #region IsValid Property Tests

    [Test]
    public void IsValid_InitialState_ShouldBeFalse()
    {
        // Arrange & Act
        ValidationResult result = new("test-target");

        // Assert
        result.IsValid.Should().BeFalse(); // Initial state is false
    }

    [Test]
    public void IsValid_SetToFalse_ShouldBeFalse()
    {
        // Arrange
        ValidationResult result = new("test-target")
        {
            // Act
            IsValid = false
        };

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Test]
    public void IsValid_SetToTrue_ShouldBeTrue()
    {
        // Arrange
        ValidationResult result = new("test-target") { IsValid = false };

        // Act
        result.IsValid = true;

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region TableName Property Tests

    [Test]
    public void TableName_InitialState_ShouldBeEmpty()
    {
        // Arrange & Act
        ValidationResult result = new("test-target");

        // Assert
        result.TableName.Should().Be(string.Empty); // Initial state is empty string
    }

    [Test]
    public void TableName_SetToValidName_ShouldReturnName()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string tableName = "contact";

        // Act
        result.TableName = tableName;

        // Assert
        result.TableName.Should().Be(tableName);
    }

    [Test]
    public void TableName_SetToEmpty_ShouldBeEmpty()
    {
        // Arrange
        ValidationResult result = new("test-target")
        {
            // Act
            TableName = string.Empty
        };

        // Assert
        result.TableName.Should().Be(string.Empty);
    }

    [Test]
    public void TableName_SetToWhitespace_ShouldPreserveWhitespace()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string whitespaceTableName = "   ";

        // Act
        result.TableName = whitespaceTableName;

        // Assert
        result.TableName.Should().Be(whitespaceTableName);
    }

    #endregion

    #region AddError Method Tests

    [Test]
    public void AddError_WithValidMessage_ShouldAddToErrors()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string errorMessage = "Test error message";

        // Act
        result.AddError(errorMessage);

        // Assert
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be(errorMessage);
    }

    [Test]
    public void AddError_WithMultipleMessages_ShouldAddAllErrors()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string[] errorMessages = ["Error 1", "Error 2", "Error 3"];

        // Act
        foreach (string message in errorMessages)
            result.AddError(message);

        // Assert
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Equal(errorMessages);
    }

    [Test]
    public void AddError_WithDuplicateMessages_ShouldAllowDuplicates()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string errorMessage = "Duplicate error";

        // Act
        result.AddError(errorMessage);
        result.AddError(errorMessage);
        result.AddError(errorMessage);

        // Assert
        result.Errors.Should().HaveCount(3);
        result.Errors.All(e => e == errorMessage).Should().BeTrue();
    }

    [Test]
    public void AddError_WithSpecialCharacters_ShouldPreserveMessage()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string specialMessage = "Error with special chars: !@#$%^&*()";

        // Act
        result.AddError(specialMessage);

        // Assert
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be(specialMessage);
    }

    [Test]
    public void AddError_WithUnicodeCharacters_ShouldPreserveMessage()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string unicodeMessage = "Ошибка с unicode символами 测试";

        // Act
        result.AddError(unicodeMessage);

        // Assert
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be(unicodeMessage);
    }

    [Test]
    public void AddError_WithVeryLongMessage_ShouldAddCompleteMessage()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string longMessage = new('x', 5000);

        // Act
        result.AddError(longMessage);

        // Assert
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be(longMessage);
    }

    [Test]
    public void AddError_WithNewlineCharacters_ShouldPreserveFormatting()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string messageWithNewlines = "Line 1\nLine 2\r\nLine 3";

        // Act
        result.AddError(messageWithNewlines);

        // Assert
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be(messageWithNewlines);
    }

    [Test]
    public void AddError_MaintainOrder_ShouldPreserveInsertionOrder()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string[] messages = [.. Enumerable.Range(1, 50).Select(i => $"Error {i}")];

        // Act
        foreach (string message in messages)
            result.AddError(message);

        // Assert
        result.Errors.Should().HaveCount(50);
        for (int i = 0; i < messages.Length; i++)
            result.Errors[i].Should().Be(messages[i]);
    }

    #endregion

    #region AddWarning Method Tests

    [Test]
    public void AddWarning_WithValidMessage_ShouldAddToWarnings()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string warningMessage = "Test warning message";

        // Act
        result.AddWarning(warningMessage);

        // Assert
        result.Warnings.Should().HaveCount(1);
        result.Warnings[0].Should().Be(warningMessage);
    }

    [Test]
    public void AddWarning_WithMultipleMessages_ShouldAddAllWarnings()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string[] warningMessages = ["Warning 1", "Warning 2", "Warning 3"];

        // Act
        foreach (string message in warningMessages)
            result.AddWarning(message);

        // Assert
        result.Warnings.Should().HaveCount(3);
        result.Warnings.Should().Equal(warningMessages);
    }

    [Test]
    public void AddWarning_WithDuplicateMessages_ShouldAllowDuplicates()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string warningMessage = "Duplicate warning";

        // Act
        result.AddWarning(warningMessage);
        result.AddWarning(warningMessage);

        // Assert
        result.Warnings.Should().HaveCount(2);
        result.Warnings.All(w => w == warningMessage).Should().BeTrue();
    }

    [Test]
    public void AddWarning_WithSpecialCharacters_ShouldPreserveMessage()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string specialMessage = "Warning: Special chars !@#$%^&*()";

        // Act
        result.AddWarning(specialMessage);

        // Assert
        result.Warnings.Should().HaveCount(1);
        result.Warnings[0].Should().Be(specialMessage);
    }

    [Test]
    public void AddWarning_WithVeryLongMessage_ShouldAddCompleteMessage()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string longMessage = new('w', 2000);

        // Act
        result.AddWarning(longMessage);

        // Assert
        result.Warnings.Should().HaveCount(1);
        result.Warnings[0].Should().Be(longMessage);
    }

    [Test]
    public void AddWarning_MaintainOrder_ShouldPreserveInsertionOrder()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string[] messages = [.. Enumerable.Range(1, 25).Select(i => $"Warning {i}")];

        // Act
        foreach (string message in messages)
            result.AddWarning(message);

        // Assert
        result.Warnings.Should().HaveCount(25);
        for (int i = 0; i < messages.Length; i++)
            result.Warnings[i].Should().Be(messages[i]);
    }

    #endregion

    #region AddInformation Method Tests

    [Test]
    public void AddInformation_WithValidMessage_ShouldAddToInformation()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string infoMessage = "Test information message";

        // Act
        result.AddInformation(infoMessage);

        // Assert
        result.Information.Should().HaveCount(1);
        result.Information[0].Should().Be(infoMessage);
    }

    [Test]
    public void AddInformation_WithMultipleMessages_ShouldAddAllInformation()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string[] infoMessages = ["Info 1", "Info 2", "Info 3", "Info 4"];

        // Act
        foreach (string message in infoMessages)
            result.AddInformation(message);

        // Assert
        result.Information.Should().HaveCount(4);
        result.Information.Should().Equal(infoMessages);
    }

    [Test]
    public void AddInformation_WithDuplicateMessages_ShouldAllowDuplicates()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string infoMessage = "Duplicate information";

        // Act
        result.AddInformation(infoMessage);
        result.AddInformation(infoMessage);
        result.AddInformation(infoMessage);

        // Assert
        result.Information.Should().HaveCount(3);
        result.Information.All(i => i == infoMessage).Should().BeTrue();
    }

    [Test]
    public void AddInformation_WithSpecialCharacters_ShouldPreserveMessage()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string specialMessage = "Info: Contains special chars !@#$%^&*()_+-=[]{}|;:,.<>?";

        // Act
        result.AddInformation(specialMessage);

        // Assert
        result.Information.Should().HaveCount(1);
        result.Information[0].Should().Be(specialMessage);
    }

    [Test]
    public void AddInformation_WithJsonContent_ShouldPreserveMessage()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string jsonMessage = @"{""key"": ""value"", ""number"": 123, ""array"": [1, 2, 3]}";

        // Act
        result.AddInformation(jsonMessage);

        // Assert
        result.Information.Should().HaveCount(1);
        result.Information[0].Should().Be(jsonMessage);
    }

    [Test]
    public void AddInformation_MaintainOrder_ShouldPreserveInsertionOrder()
    {
        // Arrange
        ValidationResult result = new("test-target");
        string[] messages = [.. Enumerable.Range(1, 30).Select(i => $"Information {i}")];

        // Act
        foreach (string message in messages)
            result.AddInformation(message);

        // Assert
        result.Information.Should().HaveCount(30);
        for (int i = 0; i < messages.Length; i++)
            result.Information[i].Should().Be(messages[i]);
    }

    #endregion

    #region HasErrors Property Tests

    [Test]
    public void HasErrors_WithNoErrors_ShouldBeFalse()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Assert
        result.HasErrors.Should().BeFalse();
    }

    [Test]
    public void HasErrors_WithOneError_ShouldBeTrue()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddError("Test error");

        // Assert
        result.HasErrors.Should().BeTrue();
    }

    [Test]
    public void HasErrors_WithMultipleErrors_ShouldBeTrue()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddError("Error 1");
        result.AddError("Error 2");

        // Assert
        result.HasErrors.Should().BeTrue();
    }

    [Test]
    public void HasErrors_WithOnlyWarningsAndInformation_ShouldBeFalse()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddWarning("Warning");
        result.AddInformation("Information");

        // Assert
        result.HasErrors.Should().BeFalse();
    }

    #endregion

    #region HasWarnings Property Tests

    [Test]
    public void HasWarnings_WithNoWarnings_ShouldBeFalse()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Assert
        result.HasWarnings.Should().BeFalse();
    }

    [Test]
    public void HasWarnings_WithOneWarning_ShouldBeTrue()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddWarning("Test warning");

        // Assert
        result.HasWarnings.Should().BeTrue();
    }

    [Test]
    public void HasWarnings_WithMultipleWarnings_ShouldBeTrue()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddWarning("Warning 1");
        result.AddWarning("Warning 2");

        // Assert
        result.HasWarnings.Should().BeTrue();
    }

    [Test]
    public void HasWarnings_WithOnlyErrorsAndInformation_ShouldBeFalse()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddError("Error");
        result.AddInformation("Information");

        // Assert
        result.HasWarnings.Should().BeFalse();
    }

    #endregion

    #region HasIssues Property Tests

    [Test]
    public void HasIssues_WithNoIssues_ShouldBeFalse()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddInformation("Just information");

        // Assert
        result.HasIssues.Should().BeFalse();
    }

    [Test]
    public void HasIssues_WithOnlyErrors_ShouldBeTrue()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddError("Error");

        // Assert
        result.HasIssues.Should().BeTrue();
    }

    [Test]
    public void HasIssues_WithOnlyWarnings_ShouldBeTrue()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddWarning("Warning");

        // Assert
        result.HasIssues.Should().BeTrue();
    }

    [Test]
    public void HasIssues_WithErrorsAndWarnings_ShouldBeTrue()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddError("Error");
        result.AddWarning("Warning");

        // Assert
        result.HasIssues.Should().BeTrue();
    }

    [Test]
    public void HasIssues_WithOnlyInformation_ShouldBeFalse()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddInformation("Info 1");
        result.AddInformation("Info 2");

        // Assert
        result.HasIssues.Should().BeFalse();
    }

    #endregion

    #region IssueCount Property Tests

    [Test]
    public void IssueCount_WithNoIssues_ShouldBeZero()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Assert
        result.IssueCount.Should().Be(0);
    }

    [Test]
    public void IssueCount_WithOnlyInformation_ShouldBeZero()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddInformation("Info 1");
        result.AddInformation("Info 2");

        // Assert
        result.IssueCount.Should().Be(0);
    }

    [Test]
    public void IssueCount_WithOnlyErrors_ShouldCountErrors()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddError("Error 1");
        result.AddError("Error 2");
        result.AddError("Error 3");

        // Assert
        result.IssueCount.Should().Be(3);
    }

    [Test]
    public void IssueCount_WithOnlyWarnings_ShouldCountWarnings()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddWarning("Warning 1");
        result.AddWarning("Warning 2");

        // Assert
        result.IssueCount.Should().Be(2);
    }

    [Test]
    public void IssueCount_WithErrorsAndWarnings_ShouldCountBoth()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddError("Error 1");
        result.AddError("Error 2");
        result.AddWarning("Warning 1");
        result.AddWarning("Warning 2");
        result.AddWarning("Warning 3");

        // Assert
        result.IssueCount.Should().Be(5); // 2 errors + 3 warnings
    }

    [Test]
    public void IssueCount_WithMixedMessagesIncludingInformation_ShouldNotCountInformation()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        result.AddError("Error");
        result.AddWarning("Warning");
        result.AddInformation("Info 1");
        result.AddInformation("Info 2");
        result.AddInformation("Info 3");

        // Assert
        result.IssueCount.Should().Be(2); // Only errors + warnings, not information
    }

    #endregion

    #region CreateIssueSummary Method Tests

    [Test]
    public void CreateIssueSummary_WithNoIssues_ShouldReturnValidString()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Act
        string summary = result.CreateIssueSummary();

        // Assert
        summary.Should().NotBeNull();
    }

    [Test]
    public void CreateIssueSummary_WithOnlyInformation_ShouldIncludeInformation()
    {
        // Arrange
        ValidationResult result = new("test-target");
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
    public void CreateIssueSummary_WithOnlyErrors_ShouldIncludeErrors()
    {
        // Arrange
        ValidationResult result = new("test-target");
        result.AddError("Error message 1");
        result.AddError("Error message 2");

        // Act
        string summary = result.CreateIssueSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("Error message 1");
        summary.Should().Contain("Error message 2");
    }

    [Test]
    public void CreateIssueSummary_WithOnlyWarnings_ShouldIncludeWarnings()
    {
        // Arrange
        ValidationResult result = new("test-target");
        result.AddWarning("Warning message 1");
        result.AddWarning("Warning message 2");

        // Act
        string summary = result.CreateIssueSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("Warning message 1");
        summary.Should().Contain("Warning message 2");
    }

    [Test]
    public void CreateIssueSummary_WithMixedMessages_ShouldIncludeAll()
    {
        // Arrange
        ValidationResult result = new("test-target");
        result.AddError("Error message");
        result.AddWarning("Warning message");
        result.AddInformation("Info message");

        // Act
        string summary = result.CreateIssueSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("Error message");
        summary.Should().Contain("Warning message");
        summary.Should().Contain("Info message");
    }

    [Test]
    public void CreateIssueSummary_WithSpecialCharacters_ShouldPreserveCharacters()
    {
        // Arrange
        ValidationResult result = new("test-target");
        result.AddError("Error with special chars: !@#$%");
        result.AddWarning("Warning with unicode: тест测试");

        // Act
        string summary = result.CreateIssueSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("!@#$%");
        summary.Should().Contain("тест测试");
    }

    [Test]
    public void CreateIssueSummary_WithManyMessages_ShouldIncludeAllMessages()
    {
        // Arrange
        ValidationResult result = new("test-target");

        // Add many messages
        for (int i = 1; i <= 10; i++)
        {
            result.AddError($"Error {i}");
            result.AddWarning($"Warning {i}");
            result.AddInformation($"Info {i}");
        }

        // Act
        string summary = result.CreateIssueSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();

        // Verify all errors are included
        for (int i = 1; i <= 10; i++)
        {
            summary.Should().Contain($"Error {i}");
            summary.Should().Contain($"Warning {i}");
            summary.Should().Contain($"Info {i}");
        }
    }

    [Test]
    public void CreateIssueSummary_CalledMultipleTimes_ShouldReturnConsistentResults()
    {
        // Arrange
        ValidationResult result = new("test-target");
        result.AddError("Error message");
        result.AddWarning("Warning message");

        // Act
        string summary1 = result.CreateIssueSummary();
        string summary2 = result.CreateIssueSummary();
        string summary3 = result.CreateIssueSummary();

        // Assert
        summary1.Should().Be(summary2);
        summary2.Should().Be(summary3);
    }

    #endregion

    #region Integration Tests

    [Test]
    public void ValidationResult_CompleteWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        ValidationResult result = new("contact")
        {
            // Act - Simulate a complete validation workflow
            TableName = "contact",
            IsValid = false
        };

        result.AddError("Primary key constraint violation");
        result.AddError("Invalid data type for field 'age'");

        result.AddWarning("Field 'phone' format may be invalid");
        result.AddWarning("Missing recommended field 'email'");

        result.AddInformation("Validation completed successfully");
        result.AddInformation("Total records processed: 1");

        // Assert - Verify all properties work together
        result.ValidationTarget.Should().Be("contact");
        result.TableName.Should().Be("contact");
        result.IsValid.Should().BeFalse();

        result.HasErrors.Should().BeTrue();
        result.HasWarnings.Should().BeTrue();
        result.HasIssues.Should().BeTrue();

        result.Errors.Should().HaveCount(2);
        result.Warnings.Should().HaveCount(2);
        result.Information.Should().HaveCount(2);
        result.IssueCount.Should().Be(4); // 2 errors + 2 warnings

        string summary = result.CreateIssueSummary();
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("Primary key constraint violation");
        summary.Should().Contain("phone");
        summary.Should().Contain("Validation completed successfully");
    }

    [Test]
    public void ValidationResult_SuccessfulValidation_ShouldWorkCorrectly()
    {
        // Arrange & Act
        ValidationResult result = new("account")
        {
            TableName = "account",
            IsValid = true
        };
        result.AddInformation("All validations passed");
        result.AddInformation("Schema is up to date");

        // Assert
        result.ValidationTarget.Should().Be("account");
        result.TableName.Should().Be("account");
        result.IsValid.Should().BeTrue();

        result.HasErrors.Should().BeFalse();
        result.HasWarnings.Should().BeFalse();
        result.HasIssues.Should().BeFalse();

        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
        result.Information.Should().HaveCount(2);
        result.IssueCount.Should().Be(0);
    }

    [Test]
    public void ValidationResult_EmptyCollections_ShouldBehaveProperly()
    {
        // Arrange
        ValidationResult result = new("empty-test");

        // Assert - Verify initial state of collections
        result.Errors.Should().NotBeNull().And.BeEmpty();
        result.Warnings.Should().NotBeNull().And.BeEmpty();
        result.Information.Should().NotBeNull().And.BeEmpty();

        // Verify count properties with empty collections
        result.HasErrors.Should().BeFalse();
        result.HasWarnings.Should().BeFalse();
        result.HasIssues.Should().BeFalse();
        result.IssueCount.Should().Be(0);

        // Verify summary with empty collections
        string summary = result.CreateIssueSummary();
        summary.Should().NotBeNull();
    }

    [Test]
    public void ValidationResult_ModifyPropertiesAfterCreation_ShouldAllowModification()
    {
        // Arrange
        ValidationResult result = new("modifiable-test")
        {
            // Act - Modify properties after creation
            ValidationTarget = "modified-target",
            TableName = "modified_table",
            IsValid = false
        };

        // Assert
        result.ValidationTarget.Should().Be("modified-target");
        result.TableName.Should().Be("modified_table");
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Test]
    public void ValidationResult_ExtremelyLargeNumberOfMessages_ShouldHandleGracefully()
    {
        // Arrange
        ValidationResult result = new("stress-test");
        int messageCount = 1000;

        // Act - Add many messages of each type
        for (int i = 0; i < messageCount; i++)
        {
            result.AddError($"Error {i}");
            result.AddWarning($"Warning {i}");
            result.AddInformation($"Info {i}");
        }

        // Assert
        result.Errors.Should().HaveCount(messageCount);
        result.Warnings.Should().HaveCount(messageCount);
        result.Information.Should().HaveCount(messageCount);
        result.IssueCount.Should().Be(messageCount * 2); // Errors + Warnings

        // Verify order is maintained
        result.Errors[0].Should().Be("Error 0");
        result.Errors[messageCount - 1].Should().Be($"Error {messageCount - 1}");
    }

    [Test]
    public void ValidationResult_NullValidationTargetWithMessages_ShouldWorkCorrectly()
    {
        // Arrange
        ValidationResult result = new(null!);

        // Act
        result.AddError("Error with null target");
        result.AddWarning("Warning with null target");
        result.AddInformation("Info with null target");

        // Assert
        result.ValidationTarget.Should().BeNull();
        result.HasErrors.Should().BeTrue();
        result.HasWarnings.Should().BeTrue();
        result.IssueCount.Should().Be(2);

        string summary = result.CreateIssueSummary();
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("Error with null target");
    }

    [Test]
    public void ValidationResult_PropertyAccessPatterns_ShouldBeConsistent()
    {
        // Arrange
        ValidationResult result = new("consistency-test");

        // Act & Assert - Test property access patterns
        // Multiple accesses should return same values
        string target1 = result.ValidationTarget;
        string target2 = result.ValidationTarget;
        target1.Should().Be(target2);

        bool hasErrors1 = result.HasErrors;
        bool hasErrors2 = result.HasErrors;
        hasErrors1.Should().Be(hasErrors2);

        int count1 = result.IssueCount;
        int count2 = result.IssueCount;
        count1.Should().Be(count2);
    }

    #endregion
}
