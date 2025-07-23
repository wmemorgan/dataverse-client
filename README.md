# Dataverse.Client

A comprehensive, high-performance C# .NET client library for Microsoft Dataverse operations, providing unified access to CRUD operations, batch processing, metadata management, and query execution.

## 🚀 Features

### Core Capabilities
- **Individual CRUD Operations** - Create, Read, Update, Delete single records
- **High-Performance Batch Processing** - Execute bulk operations using `ExecuteMultipleRequest` 
- **Metadata Management** - Create tables, columns, and manage schema operations
- **Advanced Querying** - Support for FetchXML and QueryExpression
- **Connection Management** - Automatic connection validation and retry logic
- **Schema Validation** - Validate table access and schema compatibility
- **Configuration Management** - Flexible options pattern with validation
- **Progress Reporting** - Real-time progress tracking for long operations
- **Comprehensive Error Handling** - Detailed error reporting and retry mechanisms

### Architecture Benefits
- ✅ **Single Interface** - `IDataverseClient` handles all operations
- ✅ **Clean Registration** - Simple `services.AddDataverseClient()` setup
- ✅ **Dependency Injection Ready** - Full DI container support
- ✅ **High Performance** - Optimized batch processing for large datasets
- ✅ **Enterprise Ready** - Comprehensive logging, validation, and error handling

## 📦 Installation

```bash
# Install via NuGet Package Manager
Install-Package Dataverse.Client

# Or via .NET CLI
dotnet add package Dataverse.Client
```

## ⚙️ Configuration

### Option 1: Connection String Configuration

```csharp
services.AddDataverseClient(options =>
{
    options.ConnectionString = "AuthType=ClientSecret;Url=https://yourorg.crm.dynamics.com;ClientId=your-client-id;ClientSecret=your-secret";
    options.DefaultBatchSize = 100;
    options.MaxBatchSize = 1000;
    options.RetryAttempts = 3;
    options.RetryDelayMs = 1000;
});
```

### Option 2: Individual Properties Configuration

```csharp
services.AddDataverseClient(options =>
{
    options.Url = "https://yourorg.crm.dynamics.com";
    options.ClientId = "your-client-id";
    options.ClientSecret = "your-client-secret";
    options.DefaultBatchSize = 100;
    options.MaxBatchSize = 1000;
    options.RetryAttempts = 3;
});
```

### Option 3: Configuration Section Binding

```json
// appsettings.json
{
  "Dataverse": {
    "Url": "https://yourorg.crm.dynamics.com",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "DefaultBatchSize": 100,
    "MaxBatchSize": 1000,
    "RetryAttempts": 3,
    "RetryDelayMs": 1000,
    "ConnectionTimeoutSeconds": 300
  }
}
```

```csharp
// Program.cs
services.AddDataverseClient(configuration, "Dataverse");
```

## 🎯 Usage Examples

### Basic Service Registration and Injection

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register Dataverse client services
builder.Services.AddDataverseClient(configuration, "Dataverse");

// Register your business services
builder.Services.AddScoped<CustomerService>();

var app = builder.Build();
```

```csharp
// Your service class
public class CustomerService
{
    private readonly IDataverseClient _dataverseClient;
    
    public CustomerService(IDataverseClient dataverseClient)
    {
        _dataverseClient = dataverseClient;
    }
    
    // Use the client for operations...
}
```

### Individual CRUD Operations

```csharp
public class CustomerService
{
    private readonly IDataverseClient _dataverseClient;
    
    public CustomerService(IDataverseClient dataverseClient)
    {
        _dataverseClient = dataverseClient;
    }
    
    // Create a single record
    public async Task<Guid> CreateCustomerAsync(string name, string email)
    {
        var customer = new Entity("contact");
        customer["fullname"] = name;
        customer["emailaddress1"] = email;
        
        return await _dataverseClient.CreateAsync(customer);
    }
    
    // Retrieve a single record
    public async Task<Entity?> GetCustomerAsync(Guid customerId)
    {
        var columnSet = new ColumnSet("fullname", "emailaddress1", "telephone1");
        return await _dataverseClient.RetrieveAsync("contact", customerId, columnSet);
    }
    
    // Update a single record
    public async Task UpdateCustomerAsync(Guid customerId, string newPhone)
    {
        var customer = new Entity("contact", customerId);
        customer["telephone1"] = newPhone;
        
        await _dataverseClient.UpdateAsync(customer);
    }
    
    // Delete a single record
    public async Task DeleteCustomerAsync(Guid customerId)
    {
        await _dataverseClient.DeleteAsync("contact", customerId);
    }
}
```

### Batch Operations

```csharp
public class CustomerBatchService
{
    private readonly IDataverseClient _dataverseClient;
    
    public CustomerBatchService(IDataverseClient dataverseClient)
    {
        _dataverseClient = dataverseClient;
    }
    
    // Create multiple records in batches
    public async Task<BatchOperationResult> CreateCustomersAsync(List<CustomerDto> customers)
    {
        var entities = customers.Select(c => new Entity("contact")
        {
            ["fullname"] = c.Name,
            ["emailaddress1"] = c.Email,
            ["telephone1"] = c.Phone
        }).ToList();
        
        // Use default batch configuration
        return await _dataverseClient.CreateBatchAsync(entities);
    }
    
    // Update multiple records with custom batch configuration
    public async Task<BatchOperationResult> UpdateCustomersAsync(List<Entity> customers)
    {
        var batchConfig = new BatchConfiguration
        {
            BatchSize = 50,               // Custom batch size
            MaxRetries = 5,               // Custom retry attempts
            ContinueOnError = true,       // Continue processing on individual failures
            EnableProgressReporting = true,
            ProgressReporter = new Progress<BatchProgress>(progress =>
                Console.WriteLine($"Progress: {progress.ProcessedRecords}/{progress.TotalRecords}"))
        };
        
        return await _dataverseClient.UpdateBatchAsync(customers, batchConfig);
    }
    
    // Delete multiple records
    public async Task<BatchOperationResult> DeleteCustomersAsync(List<Guid> customerIds)
    {
        var entityRefs = customerIds.Select(id => new EntityReference("contact", id)).ToList();
        return await _dataverseClient.DeleteBatchAsync(entityRefs);
    }
    
    // Retrieve multiple records in batches
    public async Task<BatchRetrieveResult> GetCustomersAsync(List<Guid> customerIds)
    {
        var entityRefs = customerIds.Select(id => new EntityReference("contact", id)).ToList();
        var columnSet = new ColumnSet("fullname", "emailaddress1", "telephone1");
        
        return await _dataverseClient.RetrieveBatchAsync(entityRefs, columnSet);
    }
}
```

### Advanced Querying

```csharp
public class CustomerQueryService
{
    private readonly IDataverseClient _dataverseClient;
    
    public CustomerQueryService(IDataverseClient dataverseClient)
    {
        _dataverseClient = dataverseClient;
    }
    
    // Query using QueryExpression
    public async Task<EntityCollection> GetActiveCustomersAsync()
    {
        var query = new QueryExpression("contact")
        {
            ColumnSet = new ColumnSet("fullname", "emailaddress1", "telephone1"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                }
            },
            Orders =
            {
                new OrderExpression("fullname", OrderType.Ascending)
            }
        };
        
        return await _dataverseClient.RetrieveMultipleAsync(query);
    }
    
    // Query using FetchXML
    public async Task<EntityCollection> GetCustomersByEmailDomainAsync(string domain)
    {
        var fetchXml = $@"
            <fetch>
                <entity name='contact'>
                    <attribute name='fullname' />
                    <attribute name='emailaddress1' />
                    <attribute name='telephone1' />
                    <filter>
                        <condition attribute='emailaddress1' operator='like' value='%@{domain}' />
                        <condition attribute='statecode' operator='eq' value='0' />
                    </filter>
                    <order attribute='fullname' />
                </entity>
            </fetch>";
        
        return await _dataverseClient.RetrieveMultipleAsync(fetchXml);
    }
}
```

### Metadata Management

```csharp
public class SchemaManagementService
{
    private readonly IDataverseMetadataClient _metadataClient;
    
    public SchemaManagementService(IDataverseMetadataClient metadataClient)
    {
        _metadataClient = metadataClient;
    }
    
    // Create a new table
    public async Task<string> CreateProductTableAsync()
    {
        var tableDefinition = new TableDefinition
        {
            LogicalName = "new_product",
            DisplayName = "Product",
            DisplayCollectionName = "Products",
            Description = "Custom product table",
            OwnershipType = OwnershipTypes.UserOwned,
            HasActivities = true,
            HasNotes = true,
            PrimaryAttribute = new PrimaryAttributeDefinition
            {
                LogicalName = "new_name",
                DisplayName = "Product Name",
                MaxLength = 100,
                RequiredLevel = AttributeRequiredLevel.ApplicationRequired
            },
            Columns = new List<ColumnDefinition>
            {
                new()
                {
                    LogicalName = "new_price",
                    DisplayName = "Price",
                    DataType = AttributeTypeCode.Money,
                    RequiredLevel = AttributeRequiredLevel.Recommended
                },
                new()
                {
                    LogicalName = "new_category",
                    DisplayName = "Category",
                    DataType = AttributeTypeCode.Picklist,
                    RequiredLevel = AttributeRequiredLevel.Optional
                }
            }
        };
        
        return await _metadataClient.CreateTableAsync(tableDefinition);
    }
    
    // Check if a table exists
    public async Task<bool> CheckTableExistsAsync(string tableName)
    {
        return await _metadataClient.TableExistsAsync(tableName);
    }
    
    // Get table metadata
    public async Task<TableMetadata> GetTableInfoAsync(string tableName)
    {
        return await _metadataClient.GetTableMetadataAsync(tableName);
    }
    
    // Add a column to existing table
    public async Task AddColumnAsync(string tableName, string columnName)
    {
        var columnDefinition = new ColumnDefinition
        {
            LogicalName = columnName,
            DisplayName = "New Column",
            DataType = AttributeTypeCode.String,
            MaxLength = 255,
            RequiredLevel = AttributeRequiredLevel.Optional
        };
        
        await _metadataClient.AddColumnAsync(tableName, columnDefinition);
    }
}
```

### Connection and Schema Validation

```csharp
public class ValidationService
{
    private readonly IDataverseClient _dataverseClient;
    
    public ValidationService(IDataverseClient dataverseClient)
    {
        _dataverseClient = dataverseClient;
    }
    
    // Validate connection
    public async Task<bool> ValidateConnectionAsync()
    {
        return await _dataverseClient.ValidateConnectionAsync();
    }
    
    // Validate table access
    public async Task<ValidationResult> ValidateTableAccessAsync(string tableName)
    {
        return await _dataverseClient.ValidateTableAccessAsync(tableName);
    }
    
    // Validate schema compatibility
    public async Task<ValidationResult> ValidateSchemaAsync(string tableName, List<string> expectedColumns)
    {
        return await _dataverseClient.ValidateSchemaAsync(tableName, expectedColumns);
    }
}
```

## 🔧 Configuration Options

### DataverseClientOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ConnectionString` | `string` | `null` | Complete connection string (preferred) |
| `Url` | `string` | `null` | Dataverse environment URL |
| `ClientId` | `string` | `null` | Azure AD application client ID |
| `ClientSecret` | `string` | `null` | Azure AD application client secret |
| `DefaultBatchSize` | `int` | `100` | Default number of records per batch |
| `MaxBatchSize` | `int` | `1000` | Maximum allowed batch size |
| `RetryAttempts` | `int` | `3` | Number of retry attempts for failed operations |
| `RetryDelayMs` | `int` | `1000` | Base delay between retries (ms) |
| `ConnectionTimeoutSeconds` | `int` | `300` | Connection timeout in seconds |
| `AdditionalConnectionParameters` | `Dictionary<string, string>` | `{}` | Additional connection string parameters |

### BatchConfiguration (Per-Operation)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BatchSize` | `int?` | `null` | Override default batch size |
| `MaxRetries` | `int?` | `null` | Override default retry attempts |
| `RetryDelayMs` | `int?` | `null` | Override default retry delay |
| `ContinueOnError` | `bool` | `true` | Continue processing on individual failures |
| `EnableProgressReporting` | `bool` | `false` | Enable progress tracking |
| `ProgressReporter` | `IProgress<BatchProgress>?` | `null` | Progress callback handler |
| `TimeoutMs` | `int?` | `null` | Operation timeout override |
| `CancellationToken` | `CancellationToken` | `default` | Cancellation token |
| `Metadata` | `Dictionary<string, object>` | `{}` | Custom operation metadata |

## 📊 Result Models

### BatchOperationResult

```csharp
public class BatchOperationResult
{
    public int TotalRecords { get; set; }           // Total records processed
    public int SuccessCount { get; set; }           // Successfully processed records
    public int FailureCount { get; set; }           // Failed records
    public List<BatchError> Errors { get; set; }    // Detailed error information
    public TimeSpan Duration { get; set; }          // Total operation time
    public BatchOperationType OperationType { get; set; } // Create/Update/Delete
    public bool HasErrors => FailureCount > 0;      // Convenience property
}
```

### BatchRetrieveResult

```csharp
public class BatchRetrieveResult : BatchOperationResult
{
    public EntityCollection Entities { get; set; }     // Successfully retrieved entities
    public List<EntityReference> MissingRecords { get; set; } // Records not found
}
```

### ValidationResult

```csharp
public class ValidationResult
{
    public bool IsValid { get; set; }               // Overall validation result
    public List<string> Errors { get; set; }        // Validation error messages
    public List<string> Warnings { get; set; }      // Non-critical warnings
    public Dictionary<string, object> Metadata { get; set; } // Additional validation info
}
```

## 🏗️ Architecture

### Key Interfaces

- **`IDataverseClient`** - Main client interface for all operations
- **`IDataverseMetadataClient`** - Metadata and schema management
- **`IBatchProcessor`** - Internal batch processing operations

### Key Services

- **`DataverseClient`** - Main implementation with unified operations
- **`DataverseMetadataClient`** - Metadata operations implementation  
- **`BatchProcessor`** - High-performance batch processing engine
- **`ServiceInjection`** - Dependency injection registration and configuration

### Internal Components

- **Connection Management** - Automatic connection validation and retry logic
- **Validation Engine** - Schema and access validation
- **Error Handling** - Comprehensive exception management
- **Performance Monitoring** - Operation timing and metrics
- **Progress Reporting** - Real-time operation progress tracking

## 🚦 Error Handling

### Exception Types

- **`DataverseException`** - Base exception for all Dataverse operations
- **`DataverseBatchException`** - Batch operation specific errors
- **`DataverseValidationException`** - Validation failures

### Error Recovery

- **Automatic Retry Logic** - Configurable retry attempts with exponential backoff
- **Batch Error Isolation** - Individual record failures don't stop batch processing
- **Detailed Error Reporting** - Complete error context and resolution guidance

## 📈 Performance Considerations

### Batch Processing Optimization

- **ExecuteMultipleRequest** - Native Dataverse batch processing
- **Configurable Batch Sizes** - Optimize for your data and network conditions
- **Memory Efficient** - Streaming processing for large datasets
- **Progress Tracking** - Monitor long-running operations

### Best Practices

- Use batch operations for > 10 records
- Configure appropriate batch sizes (50-200 records typical)
- Enable progress reporting for long operations
- Use column sets to retrieve only needed data
- Implement proper retry logic for production scenarios

## 🔍 Logging and Monitoring

The library provides comprehensive logging using Microsoft.Extensions.Logging:

```csharp
// Configure logging in Program.cs
builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
    builder.SetMinimumLevel(LogLevel.Information);
});
```

### Log Categories

- **Connection Events** - Connection establishment, validation, errors
- **Operation Tracking** - CRUD operation start/completion/errors  
- **Batch Processing** - Batch progress, performance metrics, error details
- **Validation Results** - Schema validation, access checks
- **Performance Metrics** - Operation timing, throughput statistics

## 🧪 Testing

### Unit Testing

```csharp
// Mock the interface for unit testing
var mockClient = new Mock<IDataverseClient>();
mockClient.Setup(x => x.CreateAsync(It.IsAny<Entity>()))
         .ReturnsAsync(Guid.NewGuid());

var service = new CustomerService(mockClient.Object);
```

### Integration Testing

```csharp
// Use real client for integration tests
services.AddDataverseClient(options =>
{
    options.Url = testConfiguration.DataverseUrl;
    options.ClientId = testConfiguration.ClientId;
    options.ClientSecret = testConfiguration.ClientSecret;
});
```

## 📚 Advanced Scenarios

### Custom Progress Reporting

```csharp
var progressReporter = new Progress<BatchProgress>(progress =>
{
    Console.WriteLine($"Batch {progress.CurrentBatch}/{progress.TotalBatches}: " +
                     $"{progress.ProcessedRecords}/{progress.TotalRecords} records " +
                     $"({progress.ProgressPercentage:F1}%)");
    
    if (progress.EstimatedTimeRemaining.HasValue)
    {
        Console.WriteLine($"Estimated time remaining: {progress.EstimatedTimeRemaining.Value:hh\\:mm\\:ss}");
    }
});

var config = new BatchConfiguration
{
    EnableProgressReporting = true,
    ProgressReporter = progressReporter
};

await dataverseClient.CreateBatchAsync(entities, config);
```

### Transaction Handling

```csharp
// Batch operations are automatically transactional within each batch
// For cross-batch transactions, handle manually:

var allSuccessful = true;
var createdIds = new List<Guid>();

try 
{
    var result = await dataverseClient.CreateBatchAsync(entities);
    if (result.HasErrors)
    {
        allSuccessful = false;
        // Handle partial success scenario
    }
    else 
    {
        createdIds.AddRange(result.CreatedEntities.Select(e => e.Id));
    }
}
catch (Exception ex)
{
    allSuccessful = false;
    // Handle complete failure
}

if (!allSuccessful && createdIds.Any())
{
    // Cleanup: delete successfully created records
    var refsToDelete = createdIds.Select(id => new EntityReference("contact", id));
    await dataverseClient.DeleteBatchAsync(refsToDelete);
}
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## 🆘 Support

- **Documentation**: [Full API Documentation](docs/api.md)
- **Issues**: [GitHub Issues](https://github.com/yourorg/dataverse.client/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourorg/dataverse.client/discussions)
- **Email**: support@yourorg.com

## 🎯 Roadmap

- [ ] **Connection String Builder** - Fluent API for connection string creation
- [ ] **Caching Layer** - Metadata and query result caching
- [ ] **OData Query Support** - Native OData query integration
- [ ] **Audit Trail Integration** - Built-in audit logging
- [ ] **Real-time Sync** - Change tracking and synchronization
- [ ] **Advanced Retry Policies** - Circuit breaker and custom retry strategies
- [ ] **Performance Profiler** - Built-in performance analysis tools

---

**Made with ❤️ for the Microsoft Dataverse community**