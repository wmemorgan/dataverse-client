# Dataverse.Samples.Common

A comprehensive utility library for Microsoft Dataverse sample applications and demonstrations. This library provides reusable components for data generation, console output formatting, and common demo scenarios to accelerate development of Dataverse sample projects.

## 🎯 Purpose

The `Dataverse.Samples.Common` library serves as a foundation for building consistent, professional sample applications that demonstrate Dataverse.Client capabilities. It eliminates repetitive code across sample projects and provides standardized utilities for:

- **Sample Data Generation** - Create realistic test data for demos and testing
- **Console UI Components** - Professional console output using Spectre.Console
- **Table Management** - Complete table lifecycle management for demos
- **Batch Testing** - Optimized data generation for performance testing

## 📦 Installation

### Package Reference
```xml
<PackageReference Include="Dataverse.Samples.Common" Version="1.0.0" />
```

### Project Reference
```xml
<ProjectReference Include="..\..\samples\Dataverse.Samples.Common\Dataverse.Samples.Common.csproj" />
```

## 🚀 Core Features

### Sample Data Generation (`SampleDataGenerator`)

#### Create Custom Test Tables
```csharp
using Dataverse.Samples.Common;

// Create complete table definition
TableDefinition tableDefinition = SampleDataGenerator.CreateTestTableDefinition();

// Create table in Dataverse
string tableName = await SampleDataGenerator.CreateTestTableAsync(metadataClient);

// Check if table exists
bool exists = await SampleDataGenerator.TestTableExistsAsync(metadataClient);

// Get table metadata
TableMetadata metadata = await SampleDataGenerator.GetTestTableMetadataAsync(metadataClient);

// Clean up table
bool deleted = await SampleDataGenerator.DeleteTestTableAsync(metadataClient);
```

#### Generate Realistic Sample Records
```csharp
// Create individual test records
Entity testRecord = SampleDataGenerator.CreateSampleTestRecord(recordNumber: 1);

// Create multiple test records
List<Entity> records = SampleDataGenerator.CreateSampleTestRecords(count: 50, useSequentialNumbering: true);

// Create optimized batch test records
List<Entity> batchRecords = SampleDataGenerator.CreateBatchTestRecords(
    totalRecords: 1000,
    batchSize: 100,
    includeVariations: true);
```

#### Generate Standard Entity Records
```csharp
// Create sample contacts
Entity contact = SampleDataGenerator.CreateSampleContact(contactNumber: 1);
List<Entity> contacts = SampleDataGenerator.CreateSampleContacts(count: 25);

// Create sample accounts
Entity account = SampleDataGenerator.CreateSampleAccount(accountNumber: 1);
List<Entity> accounts = SampleDataGenerator.CreateSampleAccounts(count: 15);
```

#### Complete Test Environment Management
```csharp
// Create complete test environment with data
List<Guid> createdIds = await SampleDataGenerator.CreateCompleteTestEnvironmentAsync(
    metadataClient: metadataClient,
    dataClient: dataClient,
    recordCount: 100,
    useBatchOperations: true,
    batchSize: 50);

// Cleanup test environment
await SampleDataGenerator.CleanupTestEnvironmentAsync(
    metadataClient: metadataClient,
    dataClient: dataClient,
    recordIds: createdIds,
    useBatchOperations: true,
    batchSize: 100);
```

### Console UI Components (`ConsoleHelper`)

#### Connection Testing and Display
```csharp
using Dataverse.Samples.Common;

// Test connection with loading spinner
bool connected = await ConsoleHelper.TestConnectionAsync(dataverseClient);

// Display detailed connection information
ConnectionInfo connectionInfo = dataverseClient.GetConnectionInfo();
ConsoleHelper.DisplayConnectionInfo(connectionInfo);
```

#### Professional Console Output
```csharp
// Display application header
ConsoleHelper.ShowHeader("My Dataverse Demo");

// Show batch operation results
BatchOperationResult result = await dataverseClient.CreateBatchAsync(entities);
ConsoleHelper.DisplayBatchResults(result);
```

## 🔧 Configuration

### Project Dependencies
The library includes essential packages for sample applications:

```xml
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.7" />
<PackageReference Include="Microsoft.Extensions.Configuration.UserSecrets" Version="9.0.7" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.7" />
<PackageReference Include="Spectre.Console" Version="0.50.0" />
```

### User Secrets Configuration
The library is configured for user secrets support:

```bash
# Set user secrets for your sample project
dotnet user-secrets set "Dataverse:Url" "https://yourorg.crm.dynamics.com"
dotnet user-secrets set "Dataverse:ClientId" "your-client-id"
dotnet user-secrets set "Dataverse:ClientSecret" "your-client-secret"
```

## 📊 Sample Data Characteristics

### Test Table Schema
The generated test table includes these columns:
- **Name** (Primary) - Person's full name with optional numbering
- **Email** - Realistic email addresses across multiple domains
- **Phone** - Various phone number formats
- **Age** - Random ages between 18-80
- **Description** - Timestamped descriptions with metadata

### Data Variety Features
- **Names**: 200+ diverse first and last names from multiple cultures
- **Companies**: 22 realistic company names for account generation
- **Job Titles**: 32 modern job titles across various industries
- **Email Domains**: 5 different domains with realistic patterns
- **Phone Formats**: 5 different phone number formats
- **Variations**: Edge cases and boundary testing data

### Performance Optimizations
- **Sequential Numbering** for easy identification and tracking
- **Batch Boundary Markers** for testing batch operations
- **Edge Case Variations** for robustness testing
- **Progress Reporting** for large dataset generation

## 🎮 Usage Examples

### Basic Sample Application
```csharp
using Dataverse.Client;
using Dataverse.Samples.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();

// Add Dataverse client
builder.Services.AddDataverseClient(builder.Configuration, "Dataverse");

IHost host = builder.Build();
IDataverseClient dataverseClient = host.Services.GetRequiredService<IDataverseClient>();
IDataverseMetadataClient metadataClient = host.Services.GetRequiredService<IDataverseMetadataClient>();

// Show application header
ConsoleHelper.ShowHeader("Dataverse Sample App");

// Test connection
if (await ConsoleHelper.TestConnectionAsync(dataverseClient))
{
    // Create test environment
    await SampleDataGenerator.CreateTestTableAsync(metadataClient);

    // Generate and import sample data
    List<Entity> sampleRecords = SampleDataGenerator.CreateSampleTestRecords(50);
    BatchOperationResult result = await dataverseClient.CreateBatchAsync(sampleRecords);

    // Display results
    ConsoleHelper.DisplayBatchResults(result);

    // Cleanup
    await SampleDataGenerator.DeleteTestTableAsync(metadataClient);
}
```

### Performance Testing Scenario
```csharp
// Create large dataset for performance testing
List<Entity> performanceTestData = SampleDataGenerator.CreateBatchTestRecords(
    totalRecords: 5000,
    batchSize: 200,
    includeVariations: true);

// Test batch operations with progress reporting
BatchConfiguration batchConfig = new()
{
    BatchSize = 200,
    EnableProgressReporting = true,
    ProgressReporter = new Progress<BatchProgress>(progress =>
        Console.WriteLine($"Progress: {progress.ProcessedRecords}/{progress.TotalRecords}"))
};

BatchOperationResult result = await dataverseClient.CreateBatchAsync(performanceTestData, batchConfig);
ConsoleHelper.DisplayBatchResults(result);
```

### Table Management Demo
```csharp
// Complete table lifecycle demonstration
string tableName = await SampleDataGenerator.CreateTestTableAsync(metadataClient);
Console.WriteLine($"✅ Created table: {tableName}");

// Get table information
TableMetadata metadata = await SampleDataGenerator.GetTestTableMetadataAsync(metadataClient);
Console.WriteLine($"📊 Table has {metadata.ColumnNames.Count} columns");

// Add sample data
List<Entity> records = SampleDataGenerator.CreateSampleTestRecords(25);
await dataverseClient.CreateBatchAsync(records);
Console.WriteLine($"📝 Added {records.Count} sample records");

// Cleanup everything
await SampleDataGenerator.DeleteTestTableAsync(metadataClient);
Console.WriteLine("🧹 Cleaned up test table");
```

## 🔍 API Reference

### SampleDataGenerator Methods

#### Table Management
| Method | Description | Returns |
|--------|-------------|---------|
| `CreateTestTableDefinition()` | Creates a complete table definition | `TableDefinition` |
| `CreateTestTableAsync(metadataClient)` | Creates test table in Dataverse | `Task<string>` |
| `DeleteTestTableAsync(metadataClient)` | Deletes the test table | `Task<bool>` |
| `TestTableExistsAsync(metadataClient)` | Checks if test table exists | `Task<bool>` |
| `GetTestTableMetadataAsync(metadataClient)` | Gets test table metadata | `Task<TableMetadata>` |

#### Data Generation
| Method | Description | Returns |
|--------|-------------|---------|
| `CreateSampleTestRecord(recordNumber?)` | Creates single test record | `Entity` |
| `CreateSampleTestRecords(count, sequential?)` | Creates multiple test records | `List<Entity>` |
| `CreateBatchTestRecords(total, batchSize, variations?)` | Creates optimized batch records | `List<Entity>` |
| `CreateSampleContact(contactNumber?)` | Creates sample contact | `Entity` |
| `CreateSampleContacts(count, sequential?)` | Creates multiple contacts | `List<Entity>` |
| `CreateSampleAccount(accountNumber?)` | Creates sample account | `Entity` |
| `CreateSampleAccounts(count, sequential?)` | Creates multiple accounts | `List<Entity>` |

#### Environment Management
| Method | Description | Returns |
|--------|-------------|---------|
| `CreateCompleteTestEnvironmentAsync(...)` | Creates table and sample data | `Task<List<Guid>>` |
| `CleanupTestEnvironmentAsync(...)` | Cleans up test environment | `Task` |

### ConsoleHelper Methods

#### Connection Operations
| Method | Description | Returns |
|--------|-------------|---------|
| `TestConnectionAsync(client)` | Tests connection with UI feedback | `Task<bool>` |
| `DisplayConnectionInfo(connectionInfo)` | Shows connection details table | `void` |

#### Display Operations
| Method | Description | Returns |
|--------|-------------|---------|
| `ShowHeader(title)` | Shows application header | `void` |
| `DisplayBatchResults(result)` | Shows batch operation results | `void` |

## 🏗️ Architecture

### Design Principles
- **Reusability** - Components designed for use across multiple sample projects
- **Consistency** - Standardized data generation and UI patterns
- **Performance** - Optimized for large dataset generation and batch operations
- **Flexibility** - Configurable options for different demo scenarios
- **Maintainability** - Clean separation of concerns and clear APIs

### Dependencies
- **Dataverse.Client** - Core Dataverse operations and models
- **Spectre.Console** - Professional console UI components
- **Microsoft.Extensions.*** - Configuration, DI, and hosting support

## 🧪 Testing Support

### Data Variations
The library automatically includes test data variations:
- **Edge Cases** - Maximum lengths, special characters, boundary values
- **International Data** - Names and formats from multiple cultures
- **Sequential Markers** - Easy identification of first/last records
- **Batch Boundaries** - Special markers for batch operation testing

### Performance Testing
- **Large Dataset Generation** - Efficiently create thousands of records
- **Progress Reporting** - Built-in progress tracking for long operations
- **Memory Management** - Optimized memory usage for large datasets
- **Batch Optimization** - Pre-configured for optimal batch sizes

## 🤝 Contributing

This library is designed to grow with the needs of Dataverse sample applications. To contribute:

1. Follow existing patterns for consistency
2. Add comprehensive XML documentation
3. Include both individual and batch operation examples
4. Test with various data sizes and scenarios
5. Update this README with new functionality

## 📄 License

This library is part of the Dataverse.Client ecosystem and follows the same licensing terms.

---

**Made with ❤️ for the Microsoft Dataverse developer community**