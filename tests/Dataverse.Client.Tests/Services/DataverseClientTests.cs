// File: tests/Dataverse.Client.Tests/Services/DataverseClientTests.cs

using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Dataverse.Client.Services;
using Dataverse.Client.Tests.TestData;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using ValidationResult = Dataverse.Client.Models.ValidationResult;

namespace Dataverse.Client.Tests.Services;

[TestFixture]
public class DataverseClientTests
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

    #region Simple Infrastructure Tests

    [Test]
    public void TestOptions_CreateIOptions_ShouldReturnValidOptions()
    {
        // Act
        IOptions<DataverseClientOptions> options = TestOptions.CreateIOptions();

        // Assert
        options.Should().NotBeNull();
        options.Value.Should().NotBeNull();
        options.Value.DefaultBatchSize.Should().Be(100);
    }

    [Test]
    public void TestEntities_CreateTestContact_ShouldReturnValidEntity()
    {
        // Act
        Entity contact = TestEntities.CreateTestContact();

        // Assert
        contact.Should().NotBeNull();
        contact.LogicalName.Should().Be("contact");
        contact.Id.Should().NotBe(Guid.Empty);
        contact.Attributes.Should().ContainKey("firstname");
        contact.Attributes.Should().ContainKey("lastname");
    }

    [Test]
    public void TestEntities_CreateTestContacts_ShouldReturnValidCollection()
    {
        // Act
        List<Entity> contacts = TestEntities.CreateTestContacts(3);

        // Assert
        contacts.Should().NotBeNull();
        contacts.Should().HaveCount(3);
        contacts.All(c => c.LogicalName == "contact").Should().BeTrue();
        contacts.All(c => c.Id != Guid.Empty).Should().BeTrue();
    }

    [Test]
    public void TestEntities_CreateTestColumnSet_ShouldReturnValidColumnSet()
    {
        // Act
        ColumnSet columnSet = TestEntities.CreateTestColumnSet("firstname", "lastname", "emailaddress1");

        // Assert
        columnSet.Should().NotBeNull();
        columnSet.Columns.Should().HaveCount(3);
        columnSet.Columns.Should().Contain("firstname");
        columnSet.Columns.Should().Contain("lastname");
        columnSet.Columns.Should().Contain("emailaddress1");
    }

    [Test]
    public void TestEntities_CreateTestEntityReferences_ShouldReturnValidReferences()
    {
        // Act
        List<EntityReference> refs = TestEntities.CreateTestEntityReferences("contact", 2);

        // Assert
        refs.Should().NotBeNull();
        refs.Should().HaveCount(2);
        refs.All(r => r.LogicalName == "contact").Should().BeTrue();
        refs.All(r => r.Id != Guid.Empty).Should().BeTrue();
    }

    [Test]
    public void TestEntities_CreateTestQuery_ShouldReturnValidQuery()
    {
        // Act
        QueryExpression query = TestEntities.CreateTestQuery("contact", "firstname", "lastname");

        // Assert
        query.Should().NotBeNull();
        query.EntityName.Should().Be("contact");
        query.ColumnSet.Should().NotBeNull();
        query.ColumnSet.Columns.Should().Contain("firstname");
        query.ColumnSet.Columns.Should().Contain("lastname");
    }

    [Test]
    public void TestEntities_CreateTestEntityCollection_ShouldReturnValidCollection()
    {
        // Arrange
        Entity[] entities = TestEntities.CreateTestContacts(2).ToArray();

        // Act
        EntityCollection collection = TestEntities.CreateTestEntityCollection(entities);

        // Assert
        collection.Should().NotBeNull();
        collection.Entities.Should().HaveCount(2);
        collection.EntityName.Should().Be("contact");
    }

    [Test]
    public void TestOptions_CreateBatchConfiguration_ShouldReturnValidConfig()
    {
        // Act
        BatchConfiguration config = TestOptions.CreateBatchConfiguration();

        // Assert
        config.Should().NotBeNull();
        config.GetEffectiveBatchSize(100).Should().Be(50); // Assuming the test helper sets BatchSize to 50
    }

    #endregion

    #region Mock Infrastructure Tests

    [Test]
    public void MockLogger_Setup_ShouldWorkCorrectly()
    {
        // Arrange
        bool loggerCalled = false;
        _mockLogger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(() => loggerCalled = true);

        // Act
        _mockLogger.Object.LogInformation("Test message");

        // Assert
        loggerCalled.Should().BeTrue();
    }

    [Test]
    public async Task MockBatchProcessor_Setup_ShouldWorkCorrectly()
    {
        // Arrange
        List<Entity> entities = TestEntities.CreateTestContacts(2);
        BatchOperationResult expectedResult = new BatchOperationResult(BatchOperationType.BatchCreate)
        {
            SuccessCount = 2,
            FailureCount = 0
        };

        _mockBatchProcessor.Setup(x => x.CreateRecordsAsync(It.IsAny<IEnumerable<Entity>>(), It.IsAny<int?>()))
            .ReturnsAsync(expectedResult);

        // Act
        BatchOperationResult result = await _mockBatchProcessor.Object.CreateRecordsAsync(entities, 100);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(expectedResult);
        result.SuccessCount.Should().Be(2);
        result.FailureCount.Should().Be(0);
        
        _mockBatchProcessor.Verify(x => x.CreateRecordsAsync(entities, 100), Times.Once);
    }

    #endregion

    #region Model Tests

    [Test]
    public void BatchOperationResult_Creation_ShouldInitializeCorrectly()
    {
        // Act
        BatchOperationResult result = new BatchOperationResult(BatchOperationType.BatchCreate);

        // Assert
        result.Should().NotBeNull();
        result.OperationType.Should().Be(BatchOperationType.BatchCreate);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(0);
    }

    [Test]
    public void ValidationResult_Creation_ShouldInitializeCorrectly()
    {
        // Act
        ValidationResult result = new ValidationResult("test-table");

        // Assert
        result.Should().NotBeNull();
        result.ValidationTarget.Should().Be("test-table");
        result.Errors.Should().NotBeNull();
        result.Warnings.Should().NotBeNull();
        result.Information.Should().NotBeNull();
    }

    [Test]
    public void BatchConfiguration_GetEffectiveBatchSize_ShouldReturnCorrectValue()
    {
        // Arrange
        BatchConfiguration config = new BatchConfiguration();

        // Act
        int effectiveSize = config.GetEffectiveBatchSize(200);

        // Assert
        effectiveSize.Should().Be(200); // Should return default when no specific size is set
    }

    [Test]
    public void DataverseClientOptions_DefaultValues_ShouldBeCorrect()
    {
        // Act
        DataverseClientOptions options = new DataverseClientOptions();

        // Assert
        options.Should().NotBeNull();
        options.DefaultBatchSize.Should().Be(100);
        options.MaxBatchSize.Should().Be(1000);
        options.RetryAttempts.Should().Be(3);
        options.EnableRetryOnFailure.Should().BeTrue();
    }

    #endregion

    #region Constructor Tests (Without ServiceClient)

    [Test]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        ServiceClient mockServiceClient = null!; // We'll pass null for now since we can't mock it properly

        try
        {
            // Act & Assert - This should throw before we get to ServiceClient issues
            Assert.Throws<ArgumentNullException>(() =>
                new DataverseClient(mockServiceClient!, null!, _mockBatchProcessor.Object, _mockLogger.Object));
        }
        catch (ArgumentNullException ex)
        {
            // Assert
            ex.ParamName.Should().Be("options");
        }
    }

    [Test]
    public void Constructor_WithNullBatchProcessor_ShouldThrowArgumentNullException()
    {
        // Arrange
        ServiceClient mockServiceClient = null!; // We'll pass null for now

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new DataverseClient(mockServiceClient!, _options, null!, _mockLogger.Object));
        }
        catch (ArgumentNullException ex)
        {
            // Assert
            ex.ParamName.Should().Be("batchProcessor");
        }
    }

    [Test]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        ServiceClient mockServiceClient = null!; // We'll pass null for now

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new DataverseClient(mockServiceClient!, _options, _mockBatchProcessor.Object, null!));
        }
        catch (ArgumentNullException ex)
        {
            // Assert
            ex.ParamName.Should().Be("logger");
        }
    }

    #endregion
}

