// File: tests/Dataverse.Client.Tests/TestData/TestEntities.cs
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Dataverse.Client.Tests.TestData;

public static class TestEntities
{
    public static Entity CreateTestContact(string? firstName = null, string? lastName = null, Guid? id = null)
    {
        var contact = new Entity("contact", id ?? Guid.NewGuid());
        contact["firstname"] = firstName ?? "John";
        contact["lastname"] = lastName ?? "Doe";
        contact["emailaddress1"] = $"{firstName?.ToLower() ?? "john"}.{lastName?.ToLower() ?? "doe"}@test.com";
        contact["telephone1"] = "+1-555-0123";
        contact["createdon"] = DateTime.UtcNow;
        contact["modifiedon"] = DateTime.UtcNow;
        return contact;
    }

    public static Entity CreateTestAccount(string? name = null, Guid? id = null)
    {
        var account = new Entity("account", id ?? Guid.NewGuid());
        account["name"] = name ?? "Test Company";
        account["telephone1"] = "+1-555-0456";
        account["websiteurl"] = "https://test.com";
        account["createdon"] = DateTime.UtcNow;
        account["modifiedon"] = DateTime.UtcNow;
        return account;
    }

    public static List<Entity> CreateTestContacts(int count)
    {
        var contacts = new List<Entity>();
        for (int i = 0; i < count; i++)
        {
            contacts.Add(CreateTestContact($"First{i}", $"Last{i}"));
        }
        return contacts;
    }

    public static EntityReference CreateTestEntityReference(string entityName = "contact", Guid? id = null)
    {
        return new EntityReference(entityName, id ?? Guid.NewGuid());
    }

    public static List<EntityReference> CreateTestEntityReferences(string entityName, int count)
    {
        var refs = new List<EntityReference>();
        for (int i = 0; i < count; i++)
        {
            refs.Add(CreateTestEntityReference(entityName));
        }
        return refs;
    }

    public static ColumnSet CreateTestColumnSet(params string[] columns)
    {
        return columns.Length == 0 ? new ColumnSet(true) : new ColumnSet(columns);
    }

    public static QueryExpression CreateTestQuery(string entityName = "contact", params string[] columns)
    {
        return new QueryExpression(entityName)
        {
            ColumnSet = CreateTestColumnSet(columns),
            TopCount = 10
        };
    }

    public static EntityCollection CreateTestEntityCollection(params Entity[] entities)
    {
        var collection = new EntityCollection();
        collection.Entities.AddRange(entities);
        collection.TotalRecordCount = entities.Length;
        collection.MoreRecords = false;
        
        // Set the EntityName based on the first entity's logical name if entities are provided
        if (entities.Length > 0)
        {
            collection.EntityName = entities[0].LogicalName;
        }
        
        return collection;
    }
}

