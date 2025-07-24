# Dataverse Client Demo Application

A comprehensive console application that demonstrates all the capabilities of the **Dataverse.Client** library. This demo combines and enhances the functionality from the separate BasicCrud and BatchOperations sample projects into a single, well-structured application.

## 🎯 Purpose

This demo application showcases:
- **CRUD Operations** - Individual create, read, update, delete operations
- **Batch Processing** - High-performance bulk operations  
- **Table Management** - Creating and managing custom tables
- **Query Operations** - QueryExpression and FetchXML demonstrations
- **Validation Operations** - Connection and schema validation
- **Performance Testing** - Comparative performance analysis

## 🏗️ Architecture

The application follows SOLID principles with clean separation of concerns:

```
├── Program.cs                 # Entry point and DI configuration
├── DemoApplication.cs         # Main application orchestration
├── DataverseOperations.cs     # Core Dataverse operations (IDataverseOperations interface)
├── IUserInterface.cs          # UI abstraction interface
├── ConsoleUserInterface.cs    # Spectre.Console implementation
└── Models.cs                  # Configuration and option models (renamed from DemoModels.cs)
```

### Key Design Principles

- **Dependency Injection** - Full DI container support
- **Interface Segregation** - Separate interfaces for different concerns
- **Single Responsibility** - Each class has a focused purpose
- **Open/Closed** - Easy to extend with new demo scenarios
- **DRY Principle** - Shared functionality through common services

## 🚀 Features

### 1. Basic CRUD Operations
- Individual record operations on custom tables or Contact entities
- Configurable operation types (Create, Read, Update, Delete)
- **Enhanced cleanup options** with three levels:
  - **Records Only** - Delete records but keep custom table
  - **Records and Table** - Delete records and custom table
  - **None** - Keep everything for inspection
- Real-time progress feedback

### 2. Batch Operations
- High-performance bulk processing using `ExecuteMultipleRequest`
- Configurable batch sizes and record counts
- Progress reporting with real-time updates
- Comprehensive error handling and reporting
- Support for both custom tables and standard entities
- **Enhanced cleanup options** same as CRUD operations

### 3. Table Management
- Create custom tables with full schema definition using `SampleDataGenerator`
- Retrieve table metadata and structure information
- Check table existence with robust error handling
- Delete custom tables (with confirmation)
- **Improved error handling** for table existence checks

### 4. Query Operations
- **QueryExpression** demonstrations with filtering and sorting
- **FetchXML** query examples with aggregation capabilities
- Side-by-side comparison of both query types
- Configurable result limits and entity selection
- **Smart query generation** based on entity type

### 5. Validation Operations
- Connection validation and health checks
- Table access permission validation
- Schema validation with column existence checks
- **Enhanced validation** with individual column testing
- Comprehensive validation reporting

### 6. Performance Testing
- **Individual vs Batch** performance comparisons
- **Batch Size Optimization** testing with multiple sizes (10, 50, 100, 200)
- **Concurrent Operations** testing with parallel execution
- Detailed performance metrics and recommendations
- **Enhanced cleanup management** for performance tests

## ⚙️ Configuration

### Prerequisites

1. **Azure AD App Registration** with appropriate Dataverse permissions
2. **User Secrets** configured with connection details:

```bash
dotnet user-secrets set "Dataverse:Url" "https://yourorg.crm.dynamics.com"
dotnet user-secrets set "Dataverse:ClientId" "your-client-id-guid"
dotnet user-secrets set "Dataverse:ClientSecret" "your-client-secret"
```

### Alternative Configuration

You can also use `appsettings.json` or environment variables:

```json
{
  "Dataverse": {
    "Url": "https://yourorg.crm.dynamics.com",
    "ClientId": "your-client-id-guid", 
    "ClientSecret": "your-client-secret",
    "DefaultBatchSize": 100,
    "MaxBatchSize": 1000,
    "RetryAttempts": 3
  }
}
```

## 🎮 Usage

### Running the Demo

```bash
cd samples/ConsoleDemo
dotnet run
```

### Main Menu Options

1. **Basic CRUD Operations**
   - Choose entity type (Custom Table or Contact)
   - Configure record count and operations to include
   - **New**: Choose cleanup option (Records Only, Records + Table, or None)

2. **Batch Operations**
   - Select entity type and batch configuration
   - Configure progress reporting for large datasets
   - Performance monitoring with real-time metrics
   - **New**: Enhanced cleanup options

3. **Table Management**
   - Create new custom tables with full schema using `SampleDataGenerator.CreateTestTableDefinition()`
   - Inspect existing table metadata
   - Check table existence with improved error handling
   - Delete custom tables (with safety confirmations)

4. **Query Operations**
   - Choose entity to query
   - Select query type (QueryExpression, FetchXML, or both)
   - **New**: Smart query generation with entity-specific filters
   - View structured query results

5. **Validation Operations**
   - Test connection health and validity
   - Validate table access permissions
   - Check schema compatibility with expected columns
   - **New**: Individual column validation for detailed diagnostics

6. **Performance Testing**
   - Compare individual vs batch operation performance
   - Test optimal batch sizes for your environment (10, 50, 100, 200)
   - Run concurrent operation stress tests (3 parallel batches)
   - **New**: Enhanced cleanup management for performance tests

### Sample Interaction Flow

```
╭─────────────────────────────────────────────╮
│       Dataverse Client Demo                │
╰─────────────────────────────────────────────╯

✓ Connection successful!
✓ Connected to: YourOrg (https://yourorg.crm.dynamics.com)

Select a demonstration to run:
❯ Basic CRUD Operations
  Batch Operations  
  Table Management
  Query Operations
  Validation Operations
  Performance Testing
  Exit

Choose entity type for CRUD operations:
❯ Custom Table
  Contact

Number of records to create: 5
✓ Include Create operations? Yes
✓ Include Retrieve operations? Yes  
✓ Include Update operations? Yes
✗ Include Delete operations? No
✓ Cleanup records after demo? Yes

Choose cleanup option for custom table:
❯ Delete records only (keep table)
  Delete records and table

[Creating custom table...]
✅ Test table 'cr123_testtable' is ready
[Creating 5 test records...]
✅ Created 5 test records
[Retrieving test records...]
✅ Retrieved 3 sample records
[Updating test records...]  
✅ Updated 2 test records
[Cleaning up...]
✅ Deleted 5 test records
ℹ Custom table 'cr123_testtable' preserved for future use

Demo completed successfully!
```

## 📊 Performance Insights

The demo provides detailed performance analysis:

### Batch vs Individual Comparison
```
Performance Comparison (200 records)
Method                 Duration    Records/Second    Improvement
Individual Operations  01:23.456   2.42             Baseline
Batch Operations       00:08.123   24.61            901.5% faster
```

### Batch Size Optimization
```
Batch Size Performance Comparison
Batch Size    Duration    Records/Second    Success Count
10           00:45.123    4.43             200
50           00:12.456    16.05            200  
100          00:08.789    22.75            200
200          00:09.234    21.67            200

Optimal Batch Size: 100 (22.75 records/second)
```

### Concurrent Operations Testing
```
Concurrent Operation Results
Batch #    Success Count    Failure Count    Success Rate
1          67               0                100.0%
2          67               0                100.0%
3          66               0                100.0%

Overall Results:
• Total Records: 200 successful, 0 failed
• Success Rate: 100.0%
• Total Duration: 00:08.456
• Overall Rate: 23.64 records/second
• Concurrent Batches: 3
```

## 🔧 Extending the Demo

### Adding New Demo Scenarios

1. **Add new enum value** to `DemoOption` in `Models.cs`
2. **Create configuration class** following existing patterns (e.g., `CrudOptions`, `BatchOptions`)
3. **Add menu option** in `ConsoleUserInterface.GetXxxOptions()`
4. **Implement operation** in `DataverseOperations.cs` (implements `IDataverseOperations`)
5. **Add menu handler** in `DemoApplication.HandleMenuSelectionAsync()`

### Example: Adding a Custom Scenario

```csharp
// 1. Add to DemoOption enum in Models.cs
public enum DemoOption
{
    // ... existing options
    CustomScenario,
    Exit
}

// 2. Create configuration class in Models.cs
public class CustomScenarioOptions
{
    public string TargetEntity { get; set; } = "account";
    public bool EnableAdvancedFeatures { get; set; } = false;
    public TableCleanupOption TableCleanupOption { get; set; } = TableCleanupOption.RecordsOnly;
}

// 3. Add to IDataverseOperations interface and implementation
Task DemonstrateCustomScenarioAsync(CustomScenarioOptions options);
```

## 🔍 Key Implementation Details

### Enhanced Error Handling
- **Table Existence Checks**: Robust handling of FaultExceptions with specific error code detection
- **Individual Column Validation**: When schema validation fails, individual columns are tested to identify specific missing fields
- **Batch Error Reporting**: Comprehensive error tracking with batch numbers, request indices, and severity levels

### Improved Cleanup Management
- **Three-tier cleanup system**: None, Records Only, Records + Table
- **Smart cleanup logic**: Different behavior for custom tables vs standard entities
- **Error resilience**: Cleanup continues even if individual operations fail

### Performance Optimizations
- **Concurrent batch processing**: Multiple batches processed in parallel for performance testing
- **Progress reporting**: Real-time progress updates with ETA calculations
- **Memory efficient**: Proper disposal and resource management

### User Experience Enhancements
- **Rich console output**: Using Spectre.Console for tables, progress bars, and formatted output
- **Input validation**: Comprehensive validation for all user inputs
- **Clear feedback**: Success/warning/error messages with appropriate icons and colors

## 🐛 Troubleshooting

### Common Issues

1. **Connection Failed**
   - Verify user secrets are configured correctly
   - Check Azure AD app permissions for Dataverse
   - Ensure the Dataverse URL is correct

2. **Table Access Denied**
   - Verify user has appropriate security roles
   - Check table-level permissions in Dataverse
   - Try with a different entity (e.g., Contact)

3. **Batch Operations Timeout**
   - Reduce batch size in configuration
   - Check network connectivity
   - Verify Dataverse service health

4. **Custom Table Creation Fails**
   - Check if table name conflicts with existing tables
   - Verify user has table creation permissions
   - Ensure table name follows Dataverse naming conventions

### Debug Mode

Enable detailed logging by modifying the logging configuration in `Program.cs`:

```csharp
builder.Services.AddLogging(logging =>
    logging.AddConsole().SetMinimumLevel(LogLevel.Debug));
```

### Enhanced Diagnostics

The application now includes:
- **Connection info display**: Detailed connection information including user ID, organization details
- **Validation result details**: Comprehensive validation results with errors, warnings, and information
- **Performance metrics**: Detailed timing and throughput measurements
- **Error context**: Full error context with batch numbers and entity references

## 📚 Learning Resources

This demo is designed to teach:

- **Dataverse.Client Usage Patterns** - Real-world implementation examples
- **Batch Processing Best Practices** - Performance optimization techniques with `ExecuteMultipleRequest`
- **Error Handling Strategies** - Robust error management patterns including retry logic
- **Progress Reporting** - User experience considerations for long operations
- **Performance Analysis** - Comparative testing methodologies
- **Table Management** - Creating and managing custom Dataverse tables
- **Validation Techniques** - Comprehensive validation approaches for Dataverse operations

## 🤝 Contributing

To contribute new demo scenarios:

1. Follow the existing architectural patterns
2. Maintain separation of concerns between UI, operations, and models
3. Add appropriate error handling with detailed logging
4. Include progress reporting for long operations
5. Update this README with new functionality
6. Add appropriate cleanup options for any resources created

## 📄 License

This demo application is part of the Dataverse.Client library and follows the same licensing terms.

---

**Built with ❤️ to demonstrate the full power of the Dataverse.Client library**