using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Microsoft.Xrm.Sdk;
using Spectre.Console;
using DataverseValidationResult = Dataverse.Client.Models.ValidationResult;
using SpectreValidationResult = Spectre.Console.ValidationResult;

namespace ConsoleDemo;

/// <summary>
/// Console-based implementation of the user interface using Spectre.Console.
/// </summary>
public class ConsoleUserInterface : IUserInterface
{
    public void DisplayValidationResult(DataverseValidationResult result)
    {
        Table table = new Table()
            .Title($"Validation Result: {result.ValidationTarget}")
            .AddColumn("Property")
            .AddColumn("Value");

        table.AddRow("Is Valid", result.IsValid ? "[green]✅ Valid[/]" : "[red]❌ Invalid[/]");
        table.AddRow("Result Type", result.ResultType.ToString());
        table.AddRow("Table Name", result.TableName);
        table.AddRow("Validation Time", $"{result.ValidationTime.TotalMilliseconds:F0}ms");
        table.AddRow("Validated At", result.ValidatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        table.AddRow("Error Count", result.Errors.Count.ToString());
        table.AddRow("Warning Count", result.Warnings.Count.ToString());

        AnsiConsole.Write(table);

        if (result.HasErrors)
        {
            AnsiConsole.MarkupLine("\n[red]Errors:[/]");
            foreach (string error in result.Errors)
                AnsiConsole.MarkupLine($"[red]• {error}[/]");
        }

        if (result.HasWarnings)
        {
            AnsiConsole.MarkupLine("\n[yellow]Warnings:[/]");
            foreach (string warning in result.Warnings)
                AnsiConsole.MarkupLine($"[yellow]• {warning}[/]");
        }

        if (result.Information.Count > 0)
        {
            AnsiConsole.MarkupLine("\n[cyan]Information:[/]");
            foreach (string info in result.Information)
                AnsiConsole.MarkupLine($"[cyan]• {info}[/]");
        }
    }

    public void DisplayPerformanceComparison(int recordCount, TimeSpan individualTime, TimeSpan batchTime)
    {
        Table table = new Table()
            .Title($"Performance Comparison ({recordCount} records)")
            .AddColumn("Method")
            .AddColumn("Duration")
            .AddColumn("Records/Second")
            .AddColumn("Improvement");

        double individualRate = recordCount / individualTime.TotalSeconds;
        double batchRate = recordCount / batchTime.TotalSeconds;
        double improvement = (individualTime.TotalMilliseconds / batchTime.TotalMilliseconds - 1) * 100;

        table.AddRow("Individual Operations", 
                    individualTime.ToString(@"mm\:ss\.fff"), 
                    $"{individualRate:F2}",
                    "Baseline");
        
        table.AddRow("Batch Operations", 
                    batchTime.ToString(@"mm\:ss\.fff"), 
                    $"{batchRate:F2}",
                    $"[green]{improvement:F1}% faster[/]");

        AnsiConsole.Write(table);

        AnsiConsole.MarkupLine($"\n[bold]Summary:[/] Batch operations were [green]{improvement:F1}% faster[/] " +
                              $"than individual operations for {recordCount} records.");
    }

    public void DisplayBatchSizeComparison(List<(int BatchSize, TimeSpan Duration, int SuccessCount)> results)
    {
        Table table = new Table()
            .Title("Batch Size Performance Comparison")
            .AddColumn("Batch Size")
            .AddColumn("Duration")
            .AddColumn("Records/Second")
            .AddColumn("Success Count");

        foreach (var result in results)
        {
            double rate = result.SuccessCount / result.Duration.TotalSeconds;
            
            table.AddRow(result.BatchSize.ToString(),
                        result.Duration.ToString(@"mm\:ss\.fff"),
                        $"{rate:F2}",
                        result.SuccessCount.ToString("N0"));
        }

        AnsiConsole.Write(table);

        // Find the best performing batch size
        var bestResult = results.OrderByDescending(r => r.SuccessCount / r.Duration.TotalSeconds).First();
        AnsiConsole.MarkupLine($"\n[bold]Optimal Batch Size:[/] [green]{bestResult.BatchSize}[/] " +
                              $"({bestResult.SuccessCount / bestResult.Duration.TotalSeconds:F2} records/second)");
    }

    public void DisplayConcurrentOperationResults(BatchOperationResult[] results, TimeSpan totalTime)
    {
        Table table = new Table()
            .Title("Concurrent Operation Results")
            .AddColumn("Batch #")
            .AddColumn("Success Count")
            .AddColumn("Failure Count")
            .AddColumn("Success Rate");

        int totalSuccessful = 0;
        int totalFailed = 0;

        for (int i = 0; i < results.Length; i++)
        {
            var result = results[i];
            totalSuccessful += result.SuccessCount;
            totalFailed += result.FailureCount;

            table.AddRow((i + 1).ToString(),
                        result.SuccessCount.ToString("N0"),
                        result.FailureCount > 0 ? $"[red]{result.FailureCount:N0}[/]" : "0",
                        $"{result.SuccessRate:F1}%");
        }

        AnsiConsole.Write(table);

        double overallRate = totalSuccessful / totalTime.TotalSeconds;
        double overallSuccessRate = totalSuccessful / (double)(totalSuccessful + totalFailed) * 100;

        AnsiConsole.MarkupLine($"\n[bold]Overall Results:[/]");
        AnsiConsole.MarkupLine($"• Total Records: [green]{totalSuccessful:N0}[/] successful, [red]{totalFailed:N0}[/] failed");
        AnsiConsole.MarkupLine($"• Success Rate: [green]{overallSuccessRate:F1}%[/]");
        AnsiConsole.MarkupLine($"• Total Duration: {totalTime:mm\\:ss\\.fff}");
        AnsiConsole.MarkupLine($"• Overall Rate: [green]{overallRate:F2} records/second[/]");
        AnsiConsole.MarkupLine($"• Concurrent Batches: [cyan]{results.Length}[/]");
    }

    public void ShowWelcome()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Dataverse Client Demo")
            .Centered()
            .Color(Color.Blue));

        AnsiConsole.Write(new Rule("[yellow]Comprehensive Dataverse Operations Demonstration[/]")
            .RuleStyle("grey")
            .Centered());

        AnsiConsole.WriteLine();
    }

    public void ShowSectionHeader(string title)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[bold blue]{title}[/]")
            .RuleStyle("blue")
            .LeftJustified());
        AnsiConsole.WriteLine();
    }

    public void ShowInfo(string message)
    {
        AnsiConsole.MarkupLine($"[cyan]ℹ {message}[/]");
    }

    public void ShowSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]✅ {message}[/]");
    }

    public void ShowWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]⚠ {message}[/]");
    }

    public void ShowError(string message)
    {
        AnsiConsole.MarkupLine($"[red]❌ {message}[/]");
    }

    public void PauseForUser()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    public async Task<bool> TestConnectionAsync(IDataverseClient client)
    {
        return await AnsiConsole.Status()
            .StartAsync("Testing Dataverse connection...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                bool isConnected = await client.ValidateConnectionAsync();

                if (isConnected)
                {
                    ShowSuccess("Connection successful!");
                    return true;
                }
                else
                {
                    ShowError("Connection failed!");
                    return false;
                }
            });
    }

    public void DisplayConnectionInfo(ConnectionInfo connectionInfo)
    {
        Table table = new Table()
            .Title("Dataverse Connection Information")
            .AddColumn(new TableColumn("Property").LeftAligned())
            .AddColumn(new TableColumn("Value").LeftAligned());

        table.AddRow("Organization Name", connectionInfo.OrganizationName);
        table.AddRow("Organization URL", connectionInfo.OrganizationUrl);
        table.AddRow("User Name", connectionInfo.UserName);
        table.AddRow("Organization ID", connectionInfo.OrganizationId.ToString());
        table.AddRow("User ID", connectionInfo.UserId.ToString());
        table.AddRow("Connection State", connectionInfo.State.ToString());
        table.AddRow("Connected At", connectionInfo.ConnectedAt?.ToString("yyyy-MM-dd HH:mm:ss UTC") ?? "Unknown");

        AnsiConsole.Write(table);
    }

    public DemoOption ShowMainMenu()
    {
        AnsiConsole.WriteLine();
        return AnsiConsole.Prompt(
            new SelectionPrompt<DemoOption>()
                .Title("[bold]Select a demonstration to run:[/]")
                .PageSize(10)
                .AddChoices([
                    DemoOption.BasicCrudOperations,
                    DemoOption.BatchOperations,
                    DemoOption.TableManagement,
                    DemoOption.QueryOperations,
                    DemoOption.ValidationOperations,
                    DemoOption.PerformanceTesting,
                    DemoOption.Exit
                ]));
    }

    public CrudOptions GetCrudOptions()
    {
        var entityType = AnsiConsole.Prompt(
            new SelectionPrompt<EntityType>()
                .Title("Choose entity type for CRUD operations:")
                .AddChoices(EntityType.CustomTable, EntityType.Contact));

        var recordCount = AnsiConsole.Prompt(
            new TextPrompt<int>("Number of records to create:")
                .DefaultValue(5)
                .Validate(count => count is > 0 and <= 50 
                    ? SpectreValidationResult.Success() 
                    : SpectreValidationResult.Error("Must be between 1 and 50")));

        var includeCreate = AnsiConsole.Confirm("Include Create operations?", true);
        var includeRetrieve = AnsiConsole.Confirm("Include Retrieve operations?", true);
        var includeUpdate = AnsiConsole.Confirm("Include Update operations?", true);
        var includeDelete = AnsiConsole.Confirm("Include Delete operations?", false);
        var cleanupAfter = AnsiConsole.Confirm("Cleanup records after demo?", true);

        return new CrudOptions
        {
            EntityType = entityType,
            RecordCount = recordCount,
            IncludeCreate = includeCreate,
            IncludeRetrieve = includeRetrieve,
            IncludeUpdate = includeUpdate,
            IncludeDelete = includeDelete,
            CleanupAfter = cleanupAfter
        };
    }

    public BatchOptions GetBatchOptions()
    {
        var entityType = AnsiConsole.Prompt(
            new SelectionPrompt<EntityType>()
                .Title("Choose entity type for batch operations:")
                .AddChoices(EntityType.CustomTable, EntityType.Contact));

        var recordCount = AnsiConsole.Prompt(
            new TextPrompt<int>("Number of records for batch operations:")
                .DefaultValue(100)
                .Validate(count => count is > 0 and <= 1000 
                    ? SpectreValidationResult.Success() 
                    : SpectreValidationResult.Error("Must be between 1 and 1000")));

        var batchSize = AnsiConsole.Prompt(
            new TextPrompt<int>("Batch size:")
                .DefaultValue(50)
                .Validate(size => size is > 0 and <= 200 
                    ? SpectreValidationResult.Success() 
                    : SpectreValidationResult.Error("Must be between 1 and 200")));

        var enableProgressReporting = AnsiConsole.Confirm("Enable progress reporting?", recordCount > 50);
        var cleanupAfter = AnsiConsole.Confirm("Cleanup records after demo?", true);

        return new BatchOptions
        {
            EntityType = entityType,
            RecordCount = recordCount,
            BatchSize = batchSize,
            EnableProgressReporting = enableProgressReporting,
            CleanupAfter = cleanupAfter
        };
    }

    public TableManagementOptions GetTableManagementOptions()
    {
        var operation = AnsiConsole.Prompt(
            new SelectionPrompt<TableOperation>()
                .Title("Choose table management operation:")
                .AddChoices([
                    TableOperation.Create,
                    TableOperation.GetMetadata,
                    TableOperation.CheckExists,
                    TableOperation.Delete
                ]));

        string? tableName = null;
        if (operation != TableOperation.Create)
        {
            tableName = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter table name:")
                    .DefaultValue("contact")
                    .Validate(name => !string.IsNullOrWhiteSpace(name) 
                        ? SpectreValidationResult.Success() 
                        : SpectreValidationResult.Error("Table name cannot be empty")));
        }

        return new TableManagementOptions
        {
            Operation = operation,
            TableName = tableName
        };
    }

    public QueryOptions GetQueryOptions()
    {
        var entityName = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter entity name to query:")
                .DefaultValue("contact")
                .Validate(name => !string.IsNullOrWhiteSpace(name) 
                    ? SpectreValidationResult.Success() 
                    : SpectreValidationResult.Error("Entity name cannot be empty")));

        var queryType = AnsiConsole.Prompt(
            new SelectionPrompt<QueryType>()
                .Title("Choose query type:")
                .AddChoices(QueryType.QueryExpression, QueryType.FetchXml, QueryType.Both));

        return new QueryOptions
        {
            EntityName = entityName,
            QueryType = queryType
        };
    }

    public ValidationOptions GetValidationOptions()
    {
        var tableName = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter table name to validate:")
                .DefaultValue("contact")
                .Validate(name => !string.IsNullOrWhiteSpace(name) 
                    ? SpectreValidationResult.Success() 
                    : SpectreValidationResult.Error("Table name cannot be empty")));

        var validateTableAccess = AnsiConsole.Confirm("Validate table access?", true);
        var validateConnection = AnsiConsole.Confirm("Validate connection?", true);
        
        bool validateSchema = AnsiConsole.Confirm("Validate schema (check specific columns)?", false);
        string[]? expectedColumns = null;

        if (validateSchema)
        {
            var columnsInput = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter expected column names (comma-separated):")
                    .DefaultValue("firstname,lastname,emailaddress1")
                    .AllowEmpty());

            if (!string.IsNullOrWhiteSpace(columnsInput))
            {
                expectedColumns = columnsInput.Split(',').Select(c => c.Trim()).ToArray();
            }
        }

        return new ValidationOptions
        {
            TableName = tableName,
            ValidateTableAccess = validateTableAccess,
            ValidateConnection = validateConnection,
            ValidateSchema = validateSchema,
            ExpectedColumns = expectedColumns
        };
    }

    public PerformanceOptions GetPerformanceOptions()
    {
        var testType = AnsiConsole.Prompt(
            new SelectionPrompt<PerformanceTestType>()
                .Title("Choose performance test type:")
                .AddChoices([
                    PerformanceTestType.BatchVsIndividual,
                    PerformanceTestType.DifferentBatchSizes,
                    PerformanceTestType.ConcurrentOperations
                ]));

        var recordCount = AnsiConsole.Prompt(
            new TextPrompt<int>("Number of records for performance testing:")
                .DefaultValue(200)
                .Validate(count => count is > 0 and <= 2000 
                    ? SpectreValidationResult.Success() 
                    : SpectreValidationResult.Error("Must be between 1 and 2000")));

        var batchSize = AnsiConsole.Prompt(
            new TextPrompt<int>("Batch size:")
                .DefaultValue(100)
                .Validate(size => size is > 0 and <= 500 
                    ? SpectreValidationResult.Success() 
                    : SpectreValidationResult.Error("Must be between 1 and 500")));

        return new PerformanceOptions
        {
            TestType = testType,
            RecordCount = recordCount,
            BatchSize = batchSize
        };
    }

    public void DisplayEntityRecord(Entity entity, string entityName)
    {
        Table table = new Table()
            .Title($"Record: {entity.Id}")
            .AddColumn("Field")
            .AddColumn("Value");

        foreach (var attribute in entity.Attributes.Take(10)) // Limit to first 10 attributes
        {
            string value = attribute.Value?.ToString() ?? "";
            if (value.Length > 50)
                value = value[..47] + "...";
            
            table.AddRow(attribute.Key, value);
        }

        AnsiConsole.Write(table);
    }

    public void DisplayBatchResult(BatchOperationResult result, TimeSpan elapsed)
    {
        Table table = new Table()
            .Title($"Batch {result.OperationType} Results")
            .AddColumn("Metric")
            .AddColumn("Value");

        table.AddRow("Operation Type", result.OperationType.ToString());
        table.AddRow("Total Records", result.TotalRecords.ToString("N0"));
        table.AddRow("Successful", $"[green]{result.SuccessCount:N0}[/]");
        table.AddRow("Failed", result.FailureCount > 0 ? $"[red]{result.FailureCount:N0}[/]" : "0");
        table.AddRow("Success Rate", $"{result.SuccessRate:F1}%");
        table.AddRow("Duration", elapsed.ToString(@"mm\:ss\.fff"));
        table.AddRow("Records/Second", $"{result.SuccessCount / elapsed.TotalSeconds:F2}");

        AnsiConsole.Write(table);

        if (result.HasErrors)
        {
            AnsiConsole.MarkupLine("\n[yellow]Sample errors:[/]");
            foreach (BatchError error in result.Errors.Take(3))
                AnsiConsole.MarkupLine($"[red]• {error.ErrorMessage}[/]");

            if (result.Errors.Count > 3)
                AnsiConsole.MarkupLine($"[dim]... and {result.Errors.Count - 3} more errors[/]");
        }
    }

    public void DisplayBatchRetrieveResult(BatchRetrieveResult result, TimeSpan elapsed)
    {
        Table table = new Table()
            .Title("Batch Retrieve Results")
            .AddColumn("Metric")
            .AddColumn("Value");

        table.AddRow("Records Requested", result.TotalRecords.ToString("N0"));
        table.AddRow("Records Retrieved", $"[green]{result.RetrievedEntities.Count:N0}[/]");
        table.AddRow("Records Not Found", result.NotFoundReferences.Count.ToString("N0"));
        table.AddRow("Failed Retrievals", result.FailureCount > 0 ? $"[red]{result.FailureCount:N0}[/]" : "0");
        table.AddRow("Success Rate", $"{result.SuccessRate:F2}%");
        table.AddRow("Duration", elapsed.ToString(@"mm\:ss\.fff"));
        table.AddRow("Records/Second", $"{result.RetrievedEntities.Count / elapsed.TotalSeconds:F2}");

        AnsiConsole.Write(table);
    }

    public void DisplayBatchProgress(BatchProgress progress)
    {
        AnsiConsole.MarkupLine($"[cyan]Progress: {progress.FormattedProgress} | {progress.FormattedBatchProgress} | " +
                              $"Rate: {progress.CurrentRate:F1}/sec | ETA: {progress.FormattedTimeRemaining}[/]");
    }

    public void DisplayTableMetadata(TableMetadata metadata)
    {
        Table table = new Table()
            .Title($"Table Metadata: {metadata.LogicalName}")
            .AddColumn("Property")
            .AddColumn("Value");

        table.AddRow("Logical Name", metadata.LogicalName);
        table.AddRow("Display Name", metadata.DisplayName);
        table.AddRow("Description", metadata.Description);
        table.AddRow("Ownership Type", metadata.OwnershipType.ToString());
        table.AddRow("Primary ID Attribute", metadata.PrimaryIdAttribute);
        table.AddRow("Primary Name Attribute", metadata.PrimaryNameAttribute);
        table.AddRow("Column Count", metadata.ColumnNames.Count.ToString());
        table.AddRow("Created On", metadata.CreatedOn?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown");
        table.AddRow("Modified On", metadata.ModifiedOn?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown");

        AnsiConsole.Write(table);

        if (metadata.ColumnNames.Count > 0)
        {
            AnsiConsole.MarkupLine("\n[bold]Columns:[/]");
            foreach (string columnName in metadata.ColumnNames.Take(10))
            {
                AnsiConsole.MarkupLine($"• {columnName}");
            }
            
            if (metadata.ColumnNames.Count > 10)
                AnsiConsole.MarkupLine($"[dim]... and {metadata.ColumnNames.Count - 10} more columns[/]");
        }
    }

    public void DisplayQueryResults(EntityCollection results, string queryType)
    {
        AnsiConsole.MarkupLine($"[bold]{queryType} Results:[/]");
        AnsiConsole.MarkupLine($"Total records returned: [green]{results.Entities.Count}[/]");

        if (results.Entities.Count > 0)
        {
            // Display first few records
            foreach (Entity entity in results.Entities.Take(3))
            {
                DisplayEntityRecord(entity, entity.LogicalName);
                AnsiConsole.WriteLine();
            }

            if (results.Entities.Count > 3)
                AnsiConsole.MarkupLine($"[dim]... and {results.Entities.Count - 3} more records[/]");
        }
    }
}