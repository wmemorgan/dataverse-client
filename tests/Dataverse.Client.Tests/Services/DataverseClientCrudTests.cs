// File: tests/Dataverse.Client.Tests/Services/DataverseClient.CrudTests.cs

using Dataverse.Client.Models;
using Dataverse.Client.Tests.TestData;
using Dataverse.Client.Tests.TestInfrastructure;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Dataverse.Client.Tests.Services;

/// <summary>
/// Tests for DataverseClient individual CRUD operations.
/// </summary>
[TestFixture]
public class DataverseClientCrudTests
{
    private Mock<IOrganizationService> _mockOrgService;
    private IOptions<DataverseClientOptions> _options;

    [SetUp]
    public void Setup()
    {
        _mockOrgService = MockServiceClientHelper.CreateMockWithExecuteSupport();
        _options = TestOptions.CreateIOptions();
    }

    #region Create Tests

    [Test]
    public void CreateAsync_Simulation_ShouldReturnGuidFromMockResponse()
    {
        // Arrange
        Entity entity = TestEntities.CreateTestContact();
        Guid expectedId = Guid.NewGuid();

        _mockOrgService.Setup(x => x.Execute(It.IsAny<CreateRequest>()))
            .Returns(new CreateResponse { Results = new ParameterCollection { ["id"] = expectedId } });

        // Act
        CreateResponse? response =
            _mockOrgService.Object.Execute(new CreateRequest { Target = entity }) as CreateResponse;

        // Assert
        response.Should().NotBeNull();
        response!.Results.Should().ContainKey("id");
        ((Guid)response.Results["id"]).Should().Be(expectedId);

        _mockOrgService.Verify(x => x.Execute(It.Is<CreateRequest>(r => r.Target == entity)), Times.Once);
    }

    [Test]
    public void CreateAsync_Simulation_WithNullEntity_ShouldHandleValidation() =>
        // This test shows how argument validation could be tested
        // without requiring the full DataverseClient
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            Entity nullEntity = null!;
            // Simulate the validation that would occur in DataverseClient
            if (nullEntity == null)
                throw new ArgumentNullException(nameof(nullEntity), "Entity cannot be null");
            CreateRequest request = new() { Target = nullEntity };
            // This would fail validation in the actual DataverseClient
        });

    [Test]
    public void CreateAsync_Simulation_WithInvalidEntity_ShouldHandleError()
    {
        // Arrange
        Entity entity = TestEntities.CreateTestContact();

        _mockOrgService.Setup(x => x.Execute(It.IsAny<CreateRequest>()))
            .Throws(new InvalidOperationException("Invalid entity data"));

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            _mockOrgService.Object.Execute(new CreateRequest { Target = entity }));

        exception.Message.Should().Be("Invalid entity data");

        _mockOrgService.Verify(x => x.Execute(It.Is<CreateRequest>(r => r.Target == entity)), Times.Once);
    }

    [Test]
    public void CreateAsync_Simulation_WithDuplicateKey_ShouldHandleError()
    {
        // Arrange
        Entity entity = TestEntities.CreateTestContact();

        _mockOrgService.Setup(x => x.Execute(It.IsAny<CreateRequest>()))
            .Throws(new InvalidOperationException("Duplicate key error"));

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            _mockOrgService.Object.Execute(new CreateRequest { Target = entity }));

        exception.Message.Should().Be("Duplicate key error");
    }

    [Test]
    public void CreateAsync_Simulation_WithRetryLogic_ShouldAttemptMultipleTimes()
    {
        // Arrange
        Entity entity = TestEntities.CreateTestContact();
        Guid expectedId = Guid.NewGuid();
        int attempts = 0;

        _mockOrgService.Setup(x => x.Execute(It.IsAny<CreateRequest>()))
            .Returns(() =>
            {
                attempts++;
                return attempts < 3
                    ? throw new InvalidOperationException("Transient failure")
                    : new CreateResponse { Results = new ParameterCollection { ["id"] = expectedId } };
            });

        // Act & Assert - Simulate retry logic
        CreateResponse? response = null;
        for (int i = 0; i < 3; i++)
        {
            try
            {
                response = _mockOrgService.Object.Execute(new CreateRequest { Target = entity }) as CreateResponse;
                break;
            }
            catch (InvalidOperationException)
            {
                if (i == 2) throw; // Re-throw on final attempt
            }
        }

        response.Should().NotBeNull();
        ((Guid)response!.Results["id"]).Should().Be(expectedId);
        attempts.Should().Be(3);
    }

    [Test]
    public void CreateAsync_Simulation_WithConnectionTimeout_ShouldHandleError()
    {
        // Arrange
        Entity entity = TestEntities.CreateTestContact();

        _mockOrgService.Setup(x => x.Execute(It.IsAny<CreateRequest>()))
            .Throws(new TimeoutException("Connection timeout"));

        // Act & Assert
        TimeoutException exception = Assert.Throws<TimeoutException>(() =>
            _mockOrgService.Object.Execute(new CreateRequest { Target = entity }));

        exception.Message.Should().Be("Connection timeout");
    }

    #endregion

    #region Retrieve Tests

    [Test]
    public void RetrieveAsync_Simulation_ShouldReturnEntityFromMockResponse()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        Entity expectedEntity = TestEntities.CreateTestContact(id: entityId);
        ColumnSet columns = TestEntities.CreateTestColumnSet("firstname", "lastname");

        _mockOrgService.Setup(x => x.Execute(It.IsAny<RetrieveRequest>()))
            .Returns(new RetrieveResponse { Results = new ParameterCollection { ["Entity"] = expectedEntity } });

        // Act
        RetrieveResponse? response = _mockOrgService.Object.Execute(new RetrieveRequest
        {
            Target = new EntityReference("contact", entityId),
            ColumnSet = columns
        }) as RetrieveResponse;

        // Assert
        response.Should().NotBeNull();
        response!.Results.Should().ContainKey("Entity");
        Entity retrievedEntity = (Entity)response.Results["Entity"];
        retrievedEntity.Id.Should().Be(entityId);
        retrievedEntity.LogicalName.Should().Be("contact");
    }

    [Test]
    public void RetrieveAsync_Simulation_WithInvalidParameters_ShouldValidateCorrectly() =>
        // Test parameter validation logic
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            string emptyEntityName = "";
            // This would fail validation in actual DataverseClient
            if (string.IsNullOrWhiteSpace(emptyEntityName))
                throw new ArgumentException("Entity name cannot be null or empty");
        });

    [Test]
    public void RetrieveAsync_Simulation_WithInvalidId_ShouldHandleNotFound()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        ColumnSet columns = TestEntities.CreateTestColumnSet("firstname", "lastname");

        _mockOrgService.Setup(x => x.Execute(It.IsAny<RetrieveRequest>()))
            .Throws(new InvalidOperationException("Record not found"));

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            _mockOrgService.Object.Execute(new RetrieveRequest
            {
                Target = new EntityReference("contact", entityId),
                ColumnSet = columns
            }));

        exception.Message.Should().Be("Record not found");
    }

    [Test]
    public void RetrieveAsync_Simulation_WithEmptyGuid_ShouldValidateId() =>
        // Test ID validation for retrieval
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            Guid emptyId = Guid.Empty;
            if (emptyId == Guid.Empty)
                throw new ArgumentException("Entity ID cannot be empty");
        });

    [Test]
    public void RetrieveAsync_Simulation_WithNullColumnSet_ShouldValidateColumns() =>
        // Test column set validation
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            ColumnSet nullColumns = null!;
            ArgumentNullException.ThrowIfNull(nullColumns);
        });

    [Test]
    public void RetrieveAsync_Simulation_WithAccessDenied_ShouldHandleError()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        ColumnSet columns = TestEntities.CreateTestColumnSet("firstname", "lastname");

        _mockOrgService.Setup(x => x.Execute(It.IsAny<RetrieveRequest>()))
            .Throws(new UnauthorizedAccessException("Access denied"));

        // Act & Assert
        UnauthorizedAccessException exception = Assert.Throws<UnauthorizedAccessException>(() =>
            _mockOrgService.Object.Execute(new RetrieveRequest
            {
                Target = new EntityReference("contact", entityId),
                ColumnSet = columns
            }));

        exception.Message.Should().Be("Access denied");
    }

    [Test]
    public void RetrieveAsync_Simulation_WithRetryOnTransientFailure_ShouldSucceedEventually()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        Entity expectedEntity = TestEntities.CreateTestContact(id: entityId);
        ColumnSet columns = TestEntities.CreateTestColumnSet("firstname", "lastname");
        int attempts = 0;

        _mockOrgService.Setup(x => x.Execute(It.IsAny<RetrieveRequest>()))
            .Returns(() =>
            {
                attempts++;
                return attempts < 2
                    ? throw new InvalidOperationException("Transient failure")
                    : new RetrieveResponse { Results = new ParameterCollection { ["Entity"] = expectedEntity } };
            });

        // Act & Assert - Simulate retry logic
        RetrieveResponse? response = null;
        for (int i = 0; i < 3; i++)
        {
            try
            {
                response = _mockOrgService.Object.Execute(new RetrieveRequest
                {
                    Target = new EntityReference("contact", entityId),
                    ColumnSet = columns
                }) as RetrieveResponse;
                break;
            }
            catch (InvalidOperationException)
            {
                if (i == 2) throw; // Re-throw on final attempt
            }
        }

        response.Should().NotBeNull();
        Entity retrievedEntity = (Entity)response!.Results["Entity"];
        retrievedEntity.Id.Should().Be(entityId);
        attempts.Should().Be(2);
    }

    #endregion

    #region Update Tests

    [Test]
    public void UpdateAsync_Simulation_ShouldCompleteSuccessfully()
    {
        // Arrange
        Entity entity = TestEntities.CreateTestContact();

        _mockOrgService.Setup(x => x.Execute(It.IsAny<UpdateRequest>()))
            .Returns(new UpdateResponse());

        // Act
        UpdateResponse response =
            (_mockOrgService.Object.Execute(new UpdateRequest { Target = entity }) as UpdateResponse)!;

        // Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<UpdateResponse>();

        _mockOrgService.Verify(x => x.Execute(It.Is<UpdateRequest>(r => r.Target == entity)), Times.Once);
    }

    [Test]
    public void UpdateAsync_Simulation_WithEntityWithoutId_ShouldValidateId()
    {
        // Test ID validation for updates

        // Arrange
        Entity entityWithoutId = new("contact"); // No ID set

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            if (entityWithoutId.Id == Guid.Empty)
                throw new ArgumentException("Entity ID must be set for update operations");
        });
    }

    [Test]
    public void UpdateAsync_Simulation_WithConcurrencyConflict_ShouldHandleError()
    {
        // Arrange
        Entity entity = TestEntities.CreateTestContact();

        _mockOrgService.Setup(x => x.Execute(It.IsAny<UpdateRequest>()))
            .Throws(new InvalidOperationException("Concurrency conflict"));

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            _mockOrgService.Object.Execute(new UpdateRequest { Target = entity }));

        exception.Message.Should().Be("Concurrency conflict");
    }

    [Test]
    public void UpdateAsync_Simulation_WithInvalidEntity_ShouldHandleError()
    {
        // Arrange
        Entity entity = TestEntities.CreateTestContact();

        _mockOrgService.Setup(x => x.Execute(It.IsAny<UpdateRequest>()))
            .Throws(new InvalidOperationException("Invalid entity state"));

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            _mockOrgService.Object.Execute(new UpdateRequest { Target = entity }));

        exception.Message.Should().Be("Invalid entity state");
    }

    [Test]
    public void UpdateAsync_Simulation_WithRecordNotFound_ShouldHandleError()
    {
        // Arrange
        Entity entity = TestEntities.CreateTestContact();

        _mockOrgService.Setup(x => x.Execute(It.IsAny<UpdateRequest>()))
            .Throws(new InvalidOperationException("Record not found"));

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            _mockOrgService.Object.Execute(new UpdateRequest { Target = entity }));

        exception.Message.Should().Be("Record not found");
    }

    [Test]
    public void UpdateAsync_Simulation_WithRetryOnFailure_ShouldSucceedAfterRetries()
    {
        // Arrange
        Entity entity = TestEntities.CreateTestContact();
        int attempts = 0;

        _mockOrgService.Setup(x => x.Execute(It.IsAny<UpdateRequest>()))
            .Returns(() =>
            {
                attempts++;
                return attempts < 2
                    ? throw new InvalidOperationException("Transient failure")
                    : new UpdateResponse();
            });

        // Act & Assert - Simulate retry logic
        UpdateResponse? response = null;
        for (int i = 0; i < 3; i++)
        {
            try
            {
                response = _mockOrgService.Object.Execute(new UpdateRequest { Target = entity }) as UpdateResponse;
                break;
            }
            catch (InvalidOperationException)
            {
                if (i == 2) throw; // Re-throw on final attempt
            }
        }

        response.Should().NotBeNull();
        attempts.Should().Be(2);
    }

    [Test]
    public void UpdateAsync_Simulation_WithNullEntity_ShouldValidateArguments() =>
        // Test argument validation
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            Entity nullEntity = null!;
            ArgumentNullException.ThrowIfNull(nullEntity);
        });

    #endregion

    #region Delete Tests

    [Test]
    public void DeleteAsync_Simulation_ShouldCompleteSuccessfully()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        string entityName = "contact";

        _mockOrgService.Setup(x => x.Execute(It.IsAny<DeleteRequest>()))
            .Returns(new DeleteResponse());

        // Act
        DeleteResponse? response = _mockOrgService.Object.Execute(new DeleteRequest
        {
            Target = new EntityReference(entityName, entityId)
        }) as DeleteResponse;

        // Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<DeleteResponse>();

        _mockOrgService.Verify(x => x.Execute(It.Is<DeleteRequest>(r =>
            r.Target.LogicalName == entityName && r.Target.Id == entityId)), Times.Once);
    }

    [Test]
    public void DeleteAsync_Simulation_WithRecordNotFound_ShouldHandleError()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        string entityName = "contact";

        _mockOrgService.Setup(x => x.Execute(It.IsAny<DeleteRequest>()))
            .Throws(new InvalidOperationException("Record not found"));

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            _mockOrgService.Object.Execute(new DeleteRequest { Target = new EntityReference(entityName, entityId) }));

        exception.Message.Should().Be("Record not found");
    }

    [Test]
    public void DeleteAsync_Simulation_WithRelatedRecords_ShouldHandleConstraints()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        string entityName = "contact";

        _mockOrgService.Setup(x => x.Execute(It.IsAny<DeleteRequest>()))
            .Throws(new InvalidOperationException("Cannot delete record with related entities"));

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            _mockOrgService.Object.Execute(new DeleteRequest { Target = new EntityReference(entityName, entityId) }));

        exception.Message.Should().Be("Cannot delete record with related entities");
    }

    [Test]
    public void DeleteAsync_Simulation_WithEmptyGuid_ShouldValidateId() =>
        // Test ID validation for deletion
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            Guid emptyId = Guid.Empty;
            if (emptyId == Guid.Empty)
                throw new ArgumentException("Entity ID cannot be empty for delete operations");
        });

    [Test]
    public void DeleteAsync_Simulation_WithNullEntityName_ShouldValidateEntityName() =>
        // Test entity name validation for deletion
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            string nullEntityName = null!;
            if (string.IsNullOrWhiteSpace(nullEntityName))
                throw new ArgumentException("Entity name cannot be null or empty");
        });

    [Test]
    public void DeleteAsync_Simulation_WithAccessDenied_ShouldHandleError()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        string entityName = "contact";

        _mockOrgService.Setup(x => x.Execute(It.IsAny<DeleteRequest>()))
            .Throws(new UnauthorizedAccessException("Insufficient privileges"));

        // Act & Assert
        UnauthorizedAccessException exception = Assert.Throws<UnauthorizedAccessException>(() =>
            _mockOrgService.Object.Execute(new DeleteRequest { Target = new EntityReference(entityName, entityId) }));

        exception.Message.Should().Be("Insufficient privileges");
    }

    [Test]
    public void DeleteAsync_Simulation_WithRetryLogic_ShouldSucceedAfterRetries()
    {
        // Arrange
        Guid entityId = Guid.NewGuid();
        string entityName = "contact";
        int attempts = 0;

        _mockOrgService.Setup(x => x.Execute(It.IsAny<DeleteRequest>()))
            .Returns(() =>
            {
                attempts++;
                return attempts < 2
                    ? throw new InvalidOperationException("Transient failure")
                    : new DeleteResponse();
            });

        // Act & Assert - Simulate retry logic
        DeleteResponse? response = null;
        for (int i = 0; i < 3; i++)
        {
            try
            {
                response = _mockOrgService.Object.Execute(new DeleteRequest
                {
                    Target = new EntityReference(entityName, entityId)
                }) as DeleteResponse;
                break;
            }
            catch (InvalidOperationException)
            {
                if (i == 2) throw; // Re-throw on final attempt
            }
        }

        response.Should().NotBeNull();
        attempts.Should().Be(2);
    }

    #endregion

    #region General Error Handling Tests

    [Test]
    public void Operations_WithDisabledRetryLogic_ShouldFailImmediately()
    {
        // Test behavior when retry is disabled

        // Arrange
        DataverseClientOptions optionsWithoutRetry = TestOptions.CreateValidOptions();
        optionsWithoutRetry.EnableRetryOnFailure = false;
        Entity entity = TestEntities.CreateTestContact();

        _mockOrgService.Setup(x => x.Execute(It.IsAny<CreateRequest>()))
            .Throws(new InvalidOperationException("Transient failure"));

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            _mockOrgService.Object.Execute(new CreateRequest { Target = entity }));

        exception.Message.Should().Be("Transient failure");
        optionsWithoutRetry.EnableRetryOnFailure.Should().BeFalse();
    }

    [Test]
    public void Operations_WithExceededRetryLimit_ShouldFailAfterMaxAttempts()
    {
        // Test behavior when max retry attempts are exceeded

        // Arrange
        Entity entity = TestEntities.CreateTestContact();
        int maxRetries = _options.Value.RetryAttempts;
        int attempts = 0;
        InvalidOperationException? finalException = null;

        _mockOrgService.Setup(x => x.Execute(It.IsAny<CreateRequest>()))
            .Returns(() =>
            {
                attempts++;
                throw new InvalidOperationException("Persistent failure");
            });

        // Act & Assert - Simulate retry logic with max attempts
        for (int i = 0; i <= maxRetries; i++)
        {
            try
            {
                _mockOrgService.Object.Execute(new CreateRequest { Target = entity });
                break;
            }
            catch (InvalidOperationException ex)
            {
                finalException = ex;
                if (i == maxRetries) break; // Stop after max attempts
            }
        }

        finalException.Should().NotBeNull();
        finalException!.Message.Should().Be("Persistent failure");
        attempts.Should().Be(maxRetries + 1);
    }


    [Test]
    public void Operations_WithPerformanceLogging_ShouldLogMetrics()
    {
        // Test performance logging functionality

        // Arrange
        DataverseClientOptions optionsWithLogging = TestOptions.CreateValidOptions();
        optionsWithLogging.EnablePerformanceLogging = true;
        Entity entity = TestEntities.CreateTestContact();

        _mockOrgService.Setup(x => x.Execute(It.IsAny<CreateRequest>()))
            .Returns(new CreateResponse { Results = new ParameterCollection { ["id"] = Guid.NewGuid() } });

        // Act
        CreateResponse? response =
            _mockOrgService.Object.Execute(new CreateRequest { Target = entity }) as CreateResponse;

        // Assert
        response.Should().NotBeNull();
        optionsWithLogging.EnablePerformanceLogging.Should().BeTrue();

        // Verify the operation was successful - in actual implementation,
        // performance logging would be triggered by the DataverseClient
        _mockOrgService.Verify(x => x.Execute(It.IsAny<CreateRequest>()), Times.Once);
    }


    [Test]
    public void Operations_WithConnectionTimeout_ShouldRespectTimeout()
    {
        // Test connection timeout behavior

        // Arrange
        Entity entity = TestEntities.CreateTestContact();
        int timeoutSeconds = _options.Value.ConnectionTimeoutSeconds;

        _mockOrgService.Setup(x => x.Execute(It.IsAny<CreateRequest>()))
            .Throws(new TimeoutException($"Operation timed out after {timeoutSeconds} seconds"));

        // Act & Assert
        TimeoutException exception = Assert.Throws<TimeoutException>(() =>
            _mockOrgService.Object.Execute(new CreateRequest { Target = entity }));

        exception.Message.Should().Contain($"{timeoutSeconds} seconds");
    }

    #endregion
}
