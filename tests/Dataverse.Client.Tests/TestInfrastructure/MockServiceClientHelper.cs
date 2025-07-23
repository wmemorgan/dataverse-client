// File: tests/Dataverse.Client.Tests/TestInfrastructure/MockServiceClientHelper.cs

using Dataverse.Client.Tests.TestData;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Crm.Sdk.Messages;
using Moq;

namespace Dataverse.Client.Tests.TestInfrastructure;

/// <summary>
/// Helper class to create mock ServiceClient instances that can mimic basic SDK behavior
/// without requiring actual ServiceClient functionality to be tested.
/// </summary>
public static class MockServiceClientHelper
{
    /// <summary>
    /// Creates a basic mock ServiceClient that returns predictable responses.
    /// This avoids the complexity of mocking non-virtual ServiceClient members.
    /// </summary>
    public static Mock<IOrganizationService> CreateMockOrganizationService()
    {
        var mockOrgService = new Mock<IOrganizationService>();
        
        // Setup basic CRUD operations
        mockOrgService.Setup(x => x.Create(It.IsAny<Entity>()))
            .Returns(() => Guid.NewGuid());
            
        mockOrgService.Setup(x => x.Retrieve(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Microsoft.Xrm.Sdk.Query.ColumnSet>()))
            .Returns((string entityName, Guid id, Microsoft.Xrm.Sdk.Query.ColumnSet columns) => 
                TestEntities.CreateTestContact(id: id));
                
        mockOrgService.Setup(x => x.RetrieveMultiple(It.IsAny<Microsoft.Xrm.Sdk.Query.QueryBase>()))
            .Returns(() => TestEntities.CreateTestEntityCollection(TestEntities.CreateTestContact()));
            
        return mockOrgService;
    }
    
    /// <summary>
    /// Creates a mock that simulates ServiceClient execute patterns for testing purposes.
    /// This provides predictable responses without requiring actual Dataverse connectivity.
    /// </summary>
    public static Mock<IOrganizationService> CreateMockWithExecuteSupport()
    {
        var mockOrgService = CreateMockOrganizationService();
        
        // Setup Execute method for different request types
        mockOrgService.Setup(x => x.Execute(It.IsAny<WhoAmIRequest>()))
            .Returns(() => new WhoAmIResponse
            {
                Results = new ParameterCollection
                {
                    ["UserId"] = Guid.NewGuid(),
                    ["BusinessUnitId"] = Guid.NewGuid(),
                    ["OrganizationId"] = Guid.NewGuid()
                }
            });
            
        mockOrgService.Setup(x => x.Execute(It.IsAny<CreateRequest>()))
            .Returns((OrganizationRequest request) =>
            {
                var createRequest = (CreateRequest)request;
                return new CreateResponse
                {
                    Results = new ParameterCollection { ["id"] = Guid.NewGuid() }
                };
            });
            
        mockOrgService.Setup(x => x.Execute(It.IsAny<UpdateRequest>()))
            .Returns(() => new UpdateResponse());
            
        mockOrgService.Setup(x => x.Execute(It.IsAny<DeleteRequest>()))
            .Returns(() => new DeleteResponse());
            
        mockOrgService.Setup(x => x.Execute(It.IsAny<RetrieveRequest>()))
            .Returns((OrganizationRequest request) =>
            {
                var retrieveRequest = (RetrieveRequest)request;
                return new RetrieveResponse
                {
                    Results = new ParameterCollection 
                    { 
                        ["Entity"] = TestEntities.CreateTestContact(id: retrieveRequest.Target.Id) 
                    }
                };
            });
            
        mockOrgService.Setup(x => x.Execute(It.IsAny<RetrieveMultipleRequest>()))
            .Returns(() => new RetrieveMultipleResponse
            {
                Results = new ParameterCollection 
                { 
                    ["EntityCollection"] = TestEntities.CreateTestEntityCollection(TestEntities.CreateTestContact()) 
                }
            });
            
        return mockOrgService;
    }
}

