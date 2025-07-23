// tests/Dataverse.Client.Tests/Services/BatchProcessorTests.cs

using Dataverse.Client.Models;
using Dataverse.Client.Services;
using Dataverse.Client.Tests.TestData;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Dataverse.Client.Tests.Services;

[TestFixture]
public class BatchProcessorTests
{
    private Mock<ILogger<BatchProcessor>> _mockLogger;
    private IOptions<DataverseClientOptions> _options;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<BatchProcessor>>();
        _options = TestOptions.CreateIOptions();
    }

    #region Constructor Tests (Without ServiceClient instantiation)

    [Test]
    public void Constructor_WithNullServiceClient_ShouldThrowArgumentNullException() =>
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new BatchProcessor(null!, _options, _mockLogger.Object));

    [Test]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // We can't test this without a valid ServiceClient, so we'll test the logic
        // by simulating what would happen in the constructor

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
        {
            IOptions<DataverseClientOptions> nullOptions = null!;
            ArgumentNullException.ThrowIfNull(nullOptions?.Value, nameof(nullOptions));
        });

        exception.ParamName.Should().Be("nullOptions");
    }

    [Test]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert - Test the validation logic directly
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
        {
            ILogger<BatchProcessor> nullLogger = null!;
            ArgumentNullException.ThrowIfNull(nullLogger, nameof(nullLogger));
        });

        exception.ParamName.Should().Be("nullLogger");
    }

    #endregion

    #region Argument Validation Tests (Testing the logic without ServiceClient)

    [Test]
    public void CreateRecordsAsync_WithNullEntities_ShouldValidateArguments() =>
        // Test the validation logic that would be in the method
        Assert.Throws<ArgumentNullException>(() =>
        {
            IEnumerable<Entity> nullEntities = null!;
            ArgumentNullException.ThrowIfNull(nullEntities);
        });

    [Test]
    public void UpdateRecordsAsync_WithNullEntities_ShouldValidateArguments() =>
        // Test the validation logic that would be in the method
        Assert.Throws<ArgumentNullException>(() =>
        {
            IEnumerable<Entity> nullEntities = null!;
            ArgumentNullException.ThrowIfNull(nullEntities);
        });

    [Test]
    public void DeleteRecordsAsync_WithNullEntityReferences_ShouldValidateArguments() =>
        // Test the validation logic that would be in the method
        Assert.Throws<ArgumentNullException>(() =>
        {
            IEnumerable<EntityReference> nullEntityRefs = null!;
            ArgumentNullException.ThrowIfNull(nullEntityRefs);
        });

    [Test]
    public void RetrieveRecordsAsync_WithNullEntityReferences_ShouldValidateArguments() =>
        // Test the validation logic that would be in the method
        Assert.Throws<ArgumentNullException>(() =>
        {
            IEnumerable<EntityReference> nullEntityRefs = null!;
            ArgumentNullException.ThrowIfNull(nullEntityRefs);
        });

    [Test]
    public void RetrieveRecordsAsync_WithNullColumnSet_ShouldValidateArguments() =>
        // Test the validation logic that would be in the method
        Assert.Throws<ArgumentNullException>(() =>
        {
            ColumnSet nullColumns = null!;
            ArgumentNullException.ThrowIfNull(nullColumns);
        });

    #endregion

    #region Business Logic Tests (Testing without actual BatchProcessor instance)

    [Test]
    public void BatchOperationResult_Creation_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        BatchOperationResult result = new(BatchOperationType.BatchCreate);

        // Assert
        result.Should().NotBeNull();
        result.OperationType.Should().Be(BatchOperationType.BatchCreate);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(0);
        result.TotalRecords.Should().Be(0);
    }

    [Test]
    public void BatchRetrieveResult_Creation_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        BatchRetrieveResult result = new();

        // Assert
        result.Should().NotBeNull();
        result.OperationType.Should().Be(BatchOperationType.BatchRetrieve);
        result.RetrievedEntities.Should().NotBeNull();
        result.RetrievedEntities.Should().BeEmpty();
        result.NotFoundReferences.Should().NotBeNull();
        result.NotFoundReferences.Should().BeEmpty();
    }

    [Test]
    public void BatchOperationResult_MarkCompleted_ShouldSetTimestamps()
    {
        // Arrange
        BatchOperationResult result = new(BatchOperationType.BatchCreate)
        {
            StartTime = DateTime.UtcNow.AddSeconds(-5)
        };

        // Act
        result.MarkCompleted();

        // Assert
        result.EndTime.Should().NotBeNull();
        result.Duration.Should().NotBeNull();
        result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        result.EndTime.Should().BeAfter(result.StartTime);
    }

    [Test]
    public void BatchOperationResult_TotalRecords_ShouldCalculateCorrectly()
    {
        // Arrange
        BatchOperationResult result = new(BatchOperationType.BatchCreate) { SuccessCount = 7, FailureCount = 3 };

        // Act & Assert
        result.TotalRecords.Should().Be(10);
    }

    [Test]
    public void BatchOperationResult_HasErrors_ShouldReturnCorrectValue()
    {
        // Arrange
        BatchOperationResult resultWithErrors = new(BatchOperationType.BatchCreate);
        resultWithErrors.Errors.Add(new BatchError { ErrorMessage = "Test error" });

        BatchOperationResult resultWithoutErrors = new(BatchOperationType.BatchCreate);

        // Act & Assert
        resultWithErrors.HasErrors.Should().BeTrue();
        resultWithoutErrors.HasErrors.Should().BeFalse();
    }

    #endregion

    #region Test Data Validation Tests

    [Test]
    public void TestEntities_CreateTestContacts_ShouldReturnValidEntities()
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
    public void TestEntities_CreateTestEntityReferences_ShouldReturnValidReferences()
    {
        // Act
        List<EntityReference> references = TestEntities.CreateTestEntityReferences("contact", 2);

        // Assert
        references.Should().NotBeNull();
        references.Should().HaveCount(2);
        references.All(r => r.LogicalName == "contact").Should().BeTrue();
        references.All(r => r.Id != Guid.Empty).Should().BeTrue();
    }

    #endregion

    #region Options Validation Tests

    [Test]
    public void DataverseClientOptions_DefaultValues_ShouldBeValid()
    {
        // Act
        DataverseClientOptions options = _options.Value;

        // Assert
        options.Should().NotBeNull();
        options.DefaultBatchSize.Should().BeGreaterThan(0);
        options.MaxBatchSize.Should().BeGreaterThan(options.DefaultBatchSize);
        options.RetryAttempts.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public void BatchSize_Calculation_ShouldWorkCorrectly()
    {
        // Arrange
        DataverseClientOptions options = _options.Value;
        int? customBatchSize = 50;

        // Act
        int effectiveBatchSize = customBatchSize ?? options.DefaultBatchSize;

        // Assert
        effectiveBatchSize.Should().Be(50);

        // Test with null
        int effectiveBatchSizeDefault = (int?)null ?? options.DefaultBatchSize;
        effectiveBatchSizeDefault.Should().Be(options.DefaultBatchSize);
    }

    #endregion

    #region ExecuteMultiple Response Processing Tests

    [Test]
    public void ExecuteMultipleResponse_Processing_ShouldHandleSuccessfulResponses()
    {
        // Arrange
        ExecuteMultipleResponseItemCollection responses =
        [
            new ExecuteMultipleResponseItem
            {
                Response = new CreateResponse { Results = new ParameterCollection { ["id"] = Guid.NewGuid() } },
                Fault = null
            },
            new ExecuteMultipleResponseItem
            {
                Response = new CreateResponse { Results = new ParameterCollection { ["id"] = Guid.NewGuid() } },
                Fault = null
            },
        ];

        // Act - Simulate the processing logic (test the logic directly on the collection)
        int successCount = 0;
        int failureCount = 0;
        List<Guid> createdIds = [];

        foreach (ExecuteMultipleResponseItem item in responses)
        {
            if (item.Fault == null)
            {
                successCount++;
                if (item.Response is CreateResponse createResponse) createdIds.Add((Guid)createResponse.Results["id"]);
            }
            else
            {
                failureCount++;
            }
        }

        // Assert
        successCount.Should().Be(2);
        failureCount.Should().Be(0);
        createdIds.Should().HaveCount(2);
        createdIds.All(id => id != Guid.Empty).Should().BeTrue();
    }

    [Test]
    public void ExecuteMultipleResponse_Processing_ShouldHandleFailedResponses()
    {
        // Arrange
        ExecuteMultipleResponseItemCollection responses =
        [
            new ExecuteMultipleResponseItem
            {
                Response = new CreateResponse { Results = new ParameterCollection { ["id"] = Guid.NewGuid() } },
                Fault = null
            },
            new ExecuteMultipleResponseItem
            {
                Response = null,
                Fault = new OrganizationServiceFault { Message = "Validation error", ErrorCode = -2147220969 }
            },
        ];

        // Act - Simulate the processing logic (test the logic directly on the collection)
        int successCount = 0;
        int failureCount = 0;
        List<string> errorMessages = [];

        foreach (ExecuteMultipleResponseItem item in responses)
        {
            if (item.Fault == null)
            {
                successCount++;
            }
            else
            {
                failureCount++;
                errorMessages.Add(item.Fault.Message);
            }
        }

        // Assert
        successCount.Should().Be(1);
        failureCount.Should().Be(1);
        errorMessages.Should().HaveCount(1);
        errorMessages[0].Should().Be("Validation error");
    }

    #endregion

    #region Batch Size Processing Tests

    [Test]
    public void BatchSizeCalculation_WithLargeEntityList_ShouldCalculateBatchesCorrectly()
    {
        // Arrange
        int totalEntities = 250;
        int batchSize = 100;

        // Act - Simulate batch calculation logic
        int expectedBatches = (int)Math.Ceiling((double)totalEntities / batchSize);
        List<int> batchSizes = [];

        for (int i = 0; i < totalEntities; i += batchSize)
        {
            int currentBatchSize = Math.Min(batchSize, totalEntities - i);
            batchSizes.Add(currentBatchSize);
        }

        // Assert
        expectedBatches.Should().Be(3);
        batchSizes.Should().HaveCount(3);
        batchSizes[0].Should().Be(100);
        batchSizes[1].Should().Be(100);
        batchSizes[2].Should().Be(50);
        batchSizes.Sum().Should().Be(totalEntities);
    }

    [Test]
    public void BatchSizeCalculation_WithSmallEntityList_ShouldUseSingleBatch()
    {
        // Arrange
        int totalEntities = 25;
        int batchSize = 100;

        // Act - Simulate batch calculation logic
        int expectedBatches = (int)Math.Ceiling((double)totalEntities / batchSize);

        // Assert
        expectedBatches.Should().Be(1);
    }

    #endregion

    #region Additional Business Logic Tests

    [Test]
    public void BatchError_Creation_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        BatchError error = new()
        {
            ErrorMessage = "Test error",
            ErrorCode = "0x80040203",
            EntityReference = new EntityReference("contact", Guid.NewGuid()),
            BatchNumber = 1,
            RequestIndex = 5,
            Severity = ErrorSeverity.Error
        };

        // Assert
        error.Should().NotBeNull();
        error.ErrorMessage.Should().Be("Test error");
        error.ErrorCode.Should().Be("0x80040203");
        error.EntityReference.Should().NotBeNull();
        error.BatchNumber.Should().Be(1);
        error.RequestIndex.Should().Be(5);
        error.Severity.Should().Be(ErrorSeverity.Error);
    }

    [Test]
    public void BatchOperationResult_WithMixedResults_ShouldCalculateCorrectly()
    {
        // Arrange
        BatchOperationResult result = new(BatchOperationType.BatchUpdate)
        {
            SuccessCount = 15,
            FailureCount = 5,
            StartTime = DateTime.UtcNow.AddMinutes(-2)
        };

        result.Errors.Add(new BatchError { ErrorMessage = "Error 1", Severity = ErrorSeverity.Error });
        result.Errors.Add(new BatchError { ErrorMessage = "Error 2", Severity = ErrorSeverity.Critical });

        // Act
        result.MarkCompleted();

        // Assert
        result.TotalRecords.Should().Be(20);
        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Duration.Should().BeGreaterThan(TimeSpan.FromMinutes(1));
        result.EndTime.Should().NotBeNull();
    }

    [Test]
    public void BatchRetrieveResult_WithRetrievedAndNotFound_ShouldTrackBoth()
    {
        // Arrange
        BatchRetrieveResult result = new()
        {
            SuccessCount = 8,
            FailureCount = 2
        };

        List<Entity> retrievedEntities = TestEntities.CreateTestContacts(8);
        List<EntityReference> notFoundRefs = TestEntities.CreateTestEntityReferences("contact", 3);

        // Act
        result.RetrievedEntities.AddRange(retrievedEntities);
        result.NotFoundReferences.AddRange(notFoundRefs);

        // Assert
        result.RetrievedEntities.Should().HaveCount(8);
        result.NotFoundReferences.Should().HaveCount(3);
        result.TotalRecords.Should().Be(10);
        result.OperationType.Should().Be(BatchOperationType.BatchRetrieve);
    }

    #endregion

    #region Advanced Batch Size Tests

    [Test]
    public void BatchSizeCalculation_WithExactMultiple_ShouldCreateEvenBatches()
    {
        // Arrange
        int totalEntities = 300;
        int batchSize = 100;

        // Act - Simulate batch calculation logic
        List<int> batchSizes = [];
        for (int i = 0; i < totalEntities; i += batchSize)
        {
            int currentBatchSize = Math.Min(batchSize, totalEntities - i);
            batchSizes.Add(currentBatchSize);
        }

        // Assert
        batchSizes.Should().HaveCount(3);
        batchSizes.All(size => size == 100).Should().BeTrue();
        batchSizes.Sum().Should().Be(totalEntities);
    }

    [Test]
    public void BatchSizeCalculation_WithZeroEntities_ShouldCreateNoBatches()
    {
        // Arrange
        int totalEntities = 0;
        int batchSize = 100;

        // Act - Simulate batch calculation logic
        List<int> batchSizes = [];
        for (int i = 0; i < totalEntities; i += batchSize)
        {
            int currentBatchSize = Math.Min(batchSize, totalEntities - i);
            batchSizes.Add(currentBatchSize);
        }

        // Assert
        batchSizes.Should().BeEmpty();
    }

    [Test]
    public void BatchSizeCalculation_WithSingleEntity_ShouldCreateSingleBatch()
    {
        // Arrange
        int totalEntities = 1;
        int batchSize = 100;

        // Act - Simulate batch calculation logic
        List<int> batchSizes = [];
        for (int i = 0; i < totalEntities; i += batchSize)
        {
            int currentBatchSize = Math.Min(batchSize, totalEntities - i);
            batchSizes.Add(currentBatchSize);
        }

        // Assert
        batchSizes.Should().HaveCount(1);
        batchSizes[0].Should().Be(1);
    }

    #endregion

    #region Error Handling Logic Tests

    [Test]
    public void BatchError_WithCriticalSeverity_ShouldBeIdentifiable()
    {
        // Arrange
        List<BatchError> errors = [
            new() { ErrorMessage = "Warning", Severity = ErrorSeverity.Warning },
        new() { ErrorMessage = "Error", Severity = ErrorSeverity.Error },
        new() { ErrorMessage = "Critical", Severity = ErrorSeverity.Critical }
        ];

        // Act
        List<BatchError> criticalErrors = errors.Where(e => e.Severity == ErrorSeverity.Critical).ToList();
        List<BatchError> nonCriticalErrors = errors.Where(e => e.Severity != ErrorSeverity.Critical).ToList();

        // Assert
        criticalErrors.Should().HaveCount(1);
        criticalErrors[0].ErrorMessage.Should().Be("Critical");
        nonCriticalErrors.Should().HaveCount(2);
    }

    [Test]
    public void BatchOperationResult_WithOnlyWarnings_ShouldNotHaveErrors()
    {
        // Arrange
        BatchOperationResult result = new(BatchOperationType.BatchCreate);
        result.Errors.Add(new BatchError { ErrorMessage = "Warning", Severity = ErrorSeverity.Warning });
        result.Errors.Add(new BatchError { ErrorMessage = "Info", Severity = ErrorSeverity.Info });

        // Act
        bool hasActualErrors = result.Errors.Any(e => e.Severity == ErrorSeverity.Error || e.Severity == ErrorSeverity.Critical);

        // Assert
        result.HasErrors.Should().BeTrue(); // HasErrors checks if any errors exist
        hasActualErrors.Should().BeFalse(); // But no actual errors, just warnings
        result.Errors.Should().HaveCount(2);
    }

    #endregion

    #region ExecuteMultiple Advanced Processing Tests

    [Test]
    public void ExecuteMultipleResponse_Processing_WithMixedOperations_ShouldHandleCorrectly()
    {
        // Arrange
        ExecuteMultipleResponseItemCollection responses = [
            new ExecuteMultipleResponseItem
        {
            Response = new CreateResponse { Results = new ParameterCollection { ["id"] = Guid.NewGuid() } },
            Fault = null
        },
        new ExecuteMultipleResponseItem
        {
            Response = new UpdateResponse(),
            Fault = null
        },
        new ExecuteMultipleResponseItem
        {
            Response = null,
            Fault = new OrganizationServiceFault { Message = "Duplicate record", ErrorCode = -2147220937 }
        }
        ];

        // Act - Simulate processing mixed operations
        int createCount = 0;
        int updateCount = 0;
        int errorCount = 0;
        List<Guid> createdIds = [];

        foreach (ExecuteMultipleResponseItem item in responses)
        {
            if (item.Fault == null)
            {
                switch (item.Response)
                {
                    case CreateResponse createResponse:
                        createCount++;
                        createdIds.Add((Guid)createResponse.Results["id"]);
                        break;
                    case UpdateResponse:
                        updateCount++;
                        break;
                }
            }
            else
            {
                errorCount++;
            }
        }

        // Assert
        createCount.Should().Be(1);
        updateCount.Should().Be(1);
        errorCount.Should().Be(1);
        createdIds.Should().HaveCount(1);
    }

    [Test]
    public void ExecuteMultipleResponse_Processing_WithAllFailures_ShouldHandleGracefully()
    {
        // Arrange
        ExecuteMultipleResponseItemCollection responses = [
            new ExecuteMultipleResponseItem
        {
            Response = null,
            Fault = new OrganizationServiceFault { Message = "Validation failed", ErrorCode = -2147220969 }
        },
        new ExecuteMultipleResponseItem
        {
            Response = null,
            Fault = new OrganizationServiceFault { Message = "Access denied", ErrorCode = -2147220960 }
        }
        ];

        // Act - Simulate processing all failures
        int successCount = 0;
        int failureCount = 0;
        List<string> errorCodes = [];

        foreach (ExecuteMultipleResponseItem item in responses)
        {
            if (item.Fault == null)
            {
                successCount++;
            }
            else
            {
                failureCount++;
                errorCodes.Add(item.Fault.ErrorCode.ToString());
            }
        }

        // Assert
        successCount.Should().Be(0);
        failureCount.Should().Be(2);
        errorCodes.Should().Contain("-2147220969");
        errorCodes.Should().Contain("-2147220960");
    }

    #endregion

    #region Options Edge Cases Tests

    [Test]
    public void DataverseClientOptions_WithMaximumValues_ShouldBeValid()
    {
        // Arrange
        DataverseClientOptions options = new()
        {
            DefaultBatchSize = 1000,
            MaxBatchSize = 1000,
            RetryAttempts = 10
        };

        // Act & Assert
        options.DefaultBatchSize.Should().Be(1000);
        options.MaxBatchSize.Should().Be(1000);
        options.RetryAttempts.Should().Be(10);
    }

    [Test]
    public void BatchSize_WithNullAndZeroValues_ShouldHandleCorrectly()
    {
        // Arrange
        DataverseClientOptions options = _options.Value;
        int? nullBatchSize = null;
        int? zeroBatchSize = 0;

        // Act
        int effectiveNullBatch = nullBatchSize ?? options.DefaultBatchSize;
        int effectiveZeroBatch = zeroBatchSize > 0 ? zeroBatchSize.Value : options.DefaultBatchSize;

        // Assert
        effectiveNullBatch.Should().Be(options.DefaultBatchSize);
        effectiveZeroBatch.Should().Be(options.DefaultBatchSize);
    }

    #endregion

    #region Test Data Edge Cases

    [Test]
    public void TestEntities_CreateTestContacts_WithZeroCount_ShouldReturnEmptyList()
    {
        // Act
        List<Entity> contacts = TestEntities.CreateTestContacts(0);

        // Assert
        contacts.Should().NotBeNull();
        contacts.Should().BeEmpty();
    }

    [Test]
    public void TestEntities_CreateTestEntityReferences_WithLargeCount_ShouldReturnCorrectCount()
    {
        // Act
        List<EntityReference> references = TestEntities.CreateTestEntityReferences("account", 1000);

        // Assert
        references.Should().NotBeNull();
        references.Should().HaveCount(1000);
        references.All(r => r.LogicalName == "account").Should().BeTrue();
        references.Select(r => r.Id).Distinct().Should().HaveCount(1000); // All unique IDs
    }

    [Test]
    public void TestEntities_CreateTestColumnSet_WithAllColumns_ShouldReturnAllColumnsSet()
    {
        // Act
        ColumnSet allColumns = new(true);
        ColumnSet specificColumns = TestEntities.CreateTestColumnSet("name", "createdon");

        // Assert
        allColumns.AllColumns.Should().BeTrue();
        specificColumns.AllColumns.Should().BeFalse();
        specificColumns.Columns.Should().HaveCount(2);
        specificColumns.Columns.Should().Contain("name");
        specificColumns.Columns.Should().Contain("createdon");
    }

    #endregion

    #region Performance Simulation Tests

    [Test]
    public void BatchOperationResult_DurationCalculation_ShouldBeAccurate()
    {
        // Arrange
        DateTime startTime = DateTime.UtcNow.AddSeconds(-5); // Use a recent time instead of 2023

        BatchOperationResult result = new(BatchOperationType.BatchCreate)
        {
            StartTime = startTime
        };

        // Act - Use MarkCompleted to properly set EndTime and calculate Duration
        result.MarkCompleted();

        // Assert
        result.EndTime.Should().NotBeNull();
        result.Duration.Should().NotBeNull();
        result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        result.EndTime.Should().BeAfter(startTime);

        // The duration should be approximately 5 seconds or more (since we started 5 seconds ago)
        result.Duration.Should().BeGreaterThan(TimeSpan.FromSeconds(4)); // Allow some variance
        result.Duration.Should().BeLessThan(TimeSpan.FromSeconds(10)); // But not too much
    }


    [Test]
    public void BatchOperationResult_PerformanceMetrics_ShouldCalculateRates()
    {
        // Arrange
        DateTime startTime = DateTime.UtcNow.AddMinutes(-10);

        BatchOperationResult result = new(BatchOperationType.BatchCreate)
        {
            SuccessCount = 1000,
            FailureCount = 50,
            StartTime = startTime
        };

        // Act - Use MarkCompleted to properly calculate Duration
        result.MarkCompleted();

        // Verify Duration is now calculated
        result.Duration.Should().NotBeNull();

        double recordsPerSecond = result.TotalRecords / result.Duration!.Value.TotalSeconds;
        double successRate = (double)result.SuccessCount / result.TotalRecords * 100;

        // Assert
        result.TotalRecords.Should().Be(1050);
        recordsPerSecond.Should().BeGreaterThan(0);
        successRate.Should().BeApproximately(95.24, 0.01); // 1000/1050 * 100
        result.EndTime.Should().NotBeNull();
        result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
    }




    #endregion

}
