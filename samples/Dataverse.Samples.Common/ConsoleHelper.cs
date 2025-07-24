using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Spectre.Console;

namespace Dataverse.Samples.Common;

public static class ConsoleHelper
{
    public static async Task<bool> TestConnectionAsync(IDataverseClient client) =>
        await AnsiConsole.Status()
            .StartAsync("Testing Dataverse connection...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                bool isConnected = await client.ValidateConnectionAsync();

                if (isConnected)
                {
                    AnsiConsole.MarkupLine("[green]✅ Connection successful![/]");
                    return true;
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]❌ Connection failed![/]");
                    return false;
                }
            });

    public static void DisplayConnectionInfo(ConnectionInfo connectionInfo)
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

    public static void DisplayBatchResults(BatchOperationResult result)
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
        table.AddRow("Duration", result.Duration?.ToString(@"mm\:ss\.fff") ?? "N/A");
        table.AddRow("Operation ID", result.OperationId);

        AnsiConsole.Write(table);

        if (result.HasErrors)
        {
            AnsiConsole.MarkupLine("\n[yellow]Errors found:[/]");
            foreach (BatchError error in result.Errors.Take(5))
                AnsiConsole.MarkupLine($"[red]• {error.ErrorMessage}[/]");

            if (result.Errors.Count > 5)
                AnsiConsole.MarkupLine($"[dim]... and {result.Errors.Count - 5} more errors[/]");
        }
    }

    public static void ShowHeader(string title)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText(title)
            .Centered()
            .Color(Color.Blue));

        AnsiConsole.Write(new Rule("[yellow]Dataverse.Client Sample Application[/]")
            .RuleStyle("grey")
            .Centered());

        AnsiConsole.WriteLine();
    }
}
