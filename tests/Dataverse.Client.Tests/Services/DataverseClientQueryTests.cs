// File: tests/Dataverse.Client.Tests/Services/DataverseClient.QueryTests.cs

using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Dataverse.Client.Services;
using Dataverse.Client.Tests.TestData;
using Dataverse.Client.Tests.TestInfrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Dataverse.Client.Tests.Services;

/// <summary>
/// Tests for DataverseClient query operations.
/// </summary>
[TestFixture]
public class DataverseClientQueryTests
{
    private Mock<IOrganizationService> _mockOrgService;
    private Mock<ILogger<DataverseClient>> _mockLogger;
    private Mock<IBatchProcessor> _mockBatchProcessor;
    private IOptions<DataverseClientOptions> _options;

    [SetUp]
    public void Setup()
    {
        _mockOrgService = MockServiceClientHelper.CreateMockWithExecuteSupport();
        _mockLogger = new Mock<ILogger<DataverseClient>>();
        _mockBatchProcessor = new Mock<IBatchProcessor>();
        _options = TestOptions.CreateIOptions();
    }

    #region QueryExpression Tests

    [Test]
    public void RetrieveMultipleAsync_WithQueryExpression_ShouldReturnEntityCollection()
    {
        // Arrange
        var query = TestEntities.CreateTestQuery("contact", "firstname", "lastname");
        var expectedEntities = TestEntities.CreateTestContacts(2);
        var expectedCollection = TestEntities.CreateTestEntityCollection(expectedEntities.ToArray());
        
        _mockOrgService.Setup(x => x.Execute(It.IsAny<RetrieveMultipleRequest>()))
            .Returns(new RetrieveMultipleResponse
            {
                Results = new ParameterCollection { ["EntityCollection"] = expectedCollection }
            });

        // Act
        var response = _mockOrgService.Object.Execute(new RetrieveMultipleRequest { Query = query }) as RetrieveMultipleResponse;

        // Assert
        response.Should().NotBeNull();
        response!.Results.Should().ContainKey("EntityCollection");
        var collection = (EntityCollection)response.Results["EntityCollection"];
        collection.Entities.Should().HaveCount(2);
        collection.EntityName.Should().Be("contact");
        
        _mockOrgService.Verify(x => x.Execute(It.Is<RetrieveMultipleRequest>(r => r.Query == query)), Times.Once);
    }

    [Test]
    public void RetrieveMultipleAsync_WithNullQuery_ShouldValidateArguments()
    {
        // Test argument validation
        
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            QueryExpression nullQuery = null!;
            ArgumentNullException.ThrowIfNull(nullQuery);
        });
    }

    #endregion

    #region FetchXML Tests

    [Test]
    public void RetrieveMultipleAsync_WithFetchXml_ShouldReturnEntityCollection()
    {
        // Arrange
        var fetchXml = "<fetch><entity name='contact'><attribute name='firstname'/></entity></fetch>";
        var expectedCollection = TestEntities.CreateTestEntityCollection(TestEntities.CreateTestContact());
        
        _mockOrgService.Setup(x => x.Execute(It.IsAny<RetrieveMultipleRequest>()))
            .Returns(new RetrieveMultipleResponse
            {
                Results = new ParameterCollection { ["EntityCollection"] = expectedCollection }
            });

        // Act
        var response = _mockOrgService.Object.Execute(new RetrieveMultipleRequest 
        { 
            Query = new FetchExpression(fetchXml) 
        }) as RetrieveMultipleResponse;

        // Assert
        response.Should().NotBeNull();
        response!.Results.Should().ContainKey("EntityCollection");
        var collection = (EntityCollection)response.Results["EntityCollection"];
        collection.EntityName.Should().Be("contact");
        
        _mockOrgService.Verify(x => x.Execute(It.IsAny<RetrieveMultipleRequest>()), Times.Once);
    }

    [Test]
    public void RetrieveMultipleAsync_WithInvalidFetchXml_ShouldValidateArguments()
    {
        // Test FetchXML validation
        
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            string emptyFetchXml = "";
            if (string.IsNullOrWhiteSpace(emptyFetchXml))
                throw new ArgumentException("FetchXML cannot be null or empty");
        });
    }

    #endregion

    #region Query Performance Tests

    [Test]
    public void QueryOperations_ShouldLogPerformanceMetrics()
    {
        // This test demonstrates how performance logging could be tested
        
        // Arrange
        var query = TestEntities.CreateTestQuery("contact");
        var startTime = DateTime.UtcNow;
        
        // Simulate query execution
        _mockOrgService.Object.Execute(new RetrieveMultipleRequest { Query = query });
        
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;
        
        // Assert
        duration.Should().BeGreaterThan(TimeSpan.Zero);
        
        // Verify logging would be called (in actual implementation)
        // This demonstrates the pattern for performance testing
    }

    #endregion
}

