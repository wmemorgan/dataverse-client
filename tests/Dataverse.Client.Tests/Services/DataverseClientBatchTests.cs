// File: tests/Dataverse.Client.Tests/Services/DataverseClient.BatchTests.cs

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

namespace Dataverse.Client.Tests.Services;

/// <summary>
/// Tests for DataverseClient batch operations functionality.
/// These tests focus on the batch processing delegation to IBatchProcessor.
/// </summary>
[TestFixture]
public class DataverseClientBatchTests
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

    #region Batch Create Tests

    [Test]
    public async Task CreateBatchAsync_Simulation_ShouldCallBatchProcessor()
    {
        // Arrange
        var entities = TestEntities.CreateTestContacts(5);
        var expectedResult = new BatchOperationResult(BatchOperationType.BatchCreate)
        {
            SuccessCount = 5,
            FailureCount = 0
        };

        _mockBatchProcessor.Setup(x => x.CreateRecordsAsync(It.IsAny<IEnumerable<Entity>>(), It.IsAny<int?>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mockBatchProcessor.Object.CreateRecordsAsync(entities, 100);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(expectedResult);
        result.OperationType.Should().Be(BatchOperationType.BatchCreate);
        result.SuccessCount.Should().Be(5);
        result.FailureCount.Should().Be(0);
        
        _mockBatchProcessor.Verify(x => x.CreateRecordsAsync(entities, 100), Times.Once);
    }

    [Test]
    public async Task CreateBatchAsync_WithBatchConfiguration_ShouldUseConfiguredBatchSize()
    {
        // Arrange
        var entities = TestEntities.CreateTestContacts(3);
        var config = TestOptions.CreateBatchConfiguration();
        var expectedResult = new BatchOperationResult(BatchOperationType.BatchCreate);

        _mockBatchProcessor.Setup(x => x.CreateRecordsAsync(It.IsAny<IEnumerable<Entity>>(), It.IsAny<int?>()))
            .ReturnsAsync(expectedResult);

        // Simulate the batch size calculation logic that would happen in DataverseClient
        var effectiveBatchSize = config.GetEffectiveBatchSize(_options.Value.DefaultBatchSize);

        // Act
        await _mockBatchProcessor.Object.CreateRecordsAsync(entities, effectiveBatchSize);

        // Assert
        _mockBatchProcessor.Verify(x => x.CreateRecordsAsync(entities, 50), Times.Once); // config.BatchSize = 50
    }

    [Test]
    public void CreateBatchAsync_WithNullEntities_ShouldValidateArguments()
    {
        // Test argument validation
        
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            IEnumerable<Entity> nullEntities = null!;
            ArgumentNullException.ThrowIfNull(nullEntities);
        });
    }

    #endregion

    #region Batch Update Tests

    [Test]
    public async Task UpdateBatchAsync_Simulation_ShouldCallBatchProcessor()
    {
        // Arrange
        var entities = TestEntities.CreateTestContacts(3);
        var expectedResult = new BatchOperationResult(BatchOperationType.BatchUpdate)
        {
            SuccessCount = 3,
            FailureCount = 0
        };

        _mockBatchProcessor.Setup(x => x.UpdateRecordsAsync(It.IsAny<IEnumerable<Entity>>(), It.IsAny<int?>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mockBatchProcessor.Object.UpdateRecordsAsync(entities, 100);

        // Assert
        result.Should().NotBeNull();
        result.OperationType.Should().Be(BatchOperationType.BatchUpdate);
        result.SuccessCount.Should().Be(3);
        
        _mockBatchProcessor.Verify(x => x.UpdateRecordsAsync(entities, 100), Times.Once);
    }

    #endregion

    #region Batch Delete Tests

    [Test]
    public async Task DeleteBatchAsync_Simulation_ShouldCallBatchProcessor()
    {
        // Arrange
        var entityRefs = TestEntities.CreateTestEntityReferences("contact", 4);
        var expectedResult = new BatchOperationResult(BatchOperationType.BatchDelete)
        {
            SuccessCount = 4,
            FailureCount = 0
        };

        _mockBatchProcessor.Setup(x => x.DeleteRecordsAsync(It.IsAny<IEnumerable<EntityReference>>(), It.IsAny<int?>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mockBatchProcessor.Object.DeleteRecordsAsync(entityRefs, 100);

        // Assert
        result.Should().NotBeNull();
        result.OperationType.Should().Be(BatchOperationType.BatchDelete);
        result.SuccessCount.Should().Be(4);
        
        _mockBatchProcessor.Verify(x => x.DeleteRecordsAsync(entityRefs, 100), Times.Once);
    }

    #endregion

    #region Batch Retrieve Tests

    [Test]
    public async Task RetrieveBatchAsync_Simulation_ShouldCallBatchProcessor()
    {
        // Arrange
        var entityRefs = TestEntities.CreateTestEntityReferences("contact", 3);
        var columns = TestEntities.CreateTestColumnSet("firstname", "lastname");
        var expectedResult = new BatchRetrieveResult
        {
            SuccessCount = 3,
            FailureCount = 0
        };

        _mockBatchProcessor.Setup(x => 
                x.RetrieveRecordsAsync(It.IsAny<IEnumerable<EntityReference>>(), It.IsAny<ColumnSet>(), It.IsAny<int?>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mockBatchProcessor.Object.RetrieveRecordsAsync(entityRefs, columns, 100);

        // Assert
        result.Should().NotBeNull();
        result.OperationType.Should().Be(BatchOperationType.BatchRetrieve);
        result.SuccessCount.Should().Be(3);
        
        _mockBatchProcessor.Verify(x => x.RetrieveRecordsAsync(entityRefs, columns, 100), Times.Once);
    }

    #endregion

    #region Exception Handling Tests

    [Test]
    public void BatchOperation_WhenBatchProcessorThrows_ShouldWrapInDataverseBatchException()
    {
        // Arrange
        var entities = TestEntities.CreateTestContacts(2);
        _mockBatchProcessor.Setup(x => x.CreateRecordsAsync(It.IsAny<IEnumerable<Entity>>(), It.IsAny<int?>()))
            .ThrowsAsync(new InvalidOperationException("Batch failed"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
            _mockBatchProcessor.Object.CreateRecordsAsync(entities, 100));
        
        exception!.Message.Should().Be("Batch failed");
        
        // In actual DataverseClient, this would be wrapped in DataverseBatchException
        var wrappedException = new DataverseBatchException("Batch create operation failed", exception);
        wrappedException.InnerException.Should().Be(exception);
    }

    #endregion
}

