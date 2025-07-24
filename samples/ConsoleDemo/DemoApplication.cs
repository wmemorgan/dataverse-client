using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Microsoft.Extensions.Logging;

namespace ConsoleDemo;

/// <summary>
/// Main application class that orchestrates the demo workflow.
/// </summary>
public class DemoApplication(
    IDataverseClient dataverseClient,
    IDataverseOperations dataverseOperations,
    IUserInterface userInterface,
    ILogger<DemoApplication> logger)
{
    /// <summary>
    /// Runs the main demo application workflow.
    /// </summary>
    public async Task RunAsync()
    {
        userInterface.ShowWelcome();

        try
        {
            // Test connection
            if (!await userInterface.TestConnectionAsync(dataverseClient))
            {
                userInterface.ShowError("Failed to connect to Dataverse. Please check your configuration.");
                return;
            }

            // Display connection info
            ConnectionInfo connectionInfo = dataverseClient.GetConnectionInfo();
            userInterface.DisplayConnectionInfo(connectionInfo);

            // Main menu loop
            bool continueRunning = true;
            while (continueRunning)
            {
                DemoOption selectedOption = userInterface.ShowMainMenu();
                continueRunning = await HandleMenuSelectionAsync(selectedOption);
            }

            userInterface.ShowSuccess("Demo completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Demo application encountered an error");
            userInterface.ShowError($"Application error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the selected menu option and executes the corresponding functionality.
    /// </summary>
    private async Task<bool> HandleMenuSelectionAsync(DemoOption option)
    {
        try
        {
            switch (option)
            {
                case DemoOption.BasicCrudOperations:
                    await RunBasicCrudDemoAsync();
                    break;

                case DemoOption.BatchOperations:
                    await RunBatchOperationsDemoAsync();
                    break;

                case DemoOption.TableManagement:
                    await RunTableManagementDemoAsync();
                    break;

                case DemoOption.QueryOperations:
                    await RunQueryOperationsDemoAsync();
                    break;

                case DemoOption.ValidationOperations:
                    await RunValidationOperationsDemoAsync();
                    break;

                case DemoOption.PerformanceTesting:
                    await RunPerformanceTestingAsync();
                    break;

                case DemoOption.Exit:
                    return false;

                default:
                    userInterface.ShowWarning("Invalid option selected.");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling menu selection: {Option}", option);
            userInterface.ShowError($"Operation failed: {ex.Message}");
        }

        userInterface.PauseForUser();
        return true;
    }

    /// <summary>
    /// Demonstrates basic CRUD operations.
    /// </summary>
    private async Task RunBasicCrudDemoAsync()
    {
        userInterface.ShowSectionHeader("Basic CRUD Operations Demo");

        CrudOptions crudOptions = userInterface.GetCrudOptions();

        switch (crudOptions.EntityType)
        {
            case EntityType.CustomTable:
                await dataverseOperations.DemonstrateCustomTableCrudAsync(crudOptions);
                break;
            case EntityType.Contact:
                await dataverseOperations.DemonstrateContactCrudAsync(crudOptions);
                break;
        }
    }

    /// <summary>
    /// Demonstrates batch operations.
    /// </summary>
    private async Task RunBatchOperationsDemoAsync()
    {
        userInterface.ShowSectionHeader("Batch Operations Demo");

        BatchOptions batchOptions = userInterface.GetBatchOptions();

        switch (batchOptions.EntityType)
        {
            case EntityType.CustomTable:
                await dataverseOperations.DemonstrateBatchOperationsAsync(batchOptions);
                break;
            case EntityType.Contact:
                await dataverseOperations.DemonstrateContactBatchOperationsAsync(batchOptions);
                break;
        }
    }

    /// <summary>
    /// Demonstrates table management operations.
    /// </summary>
    private async Task RunTableManagementDemoAsync()
    {
        userInterface.ShowSectionHeader("Table Management Demo");

        TableManagementOptions tableOptions = userInterface.GetTableManagementOptions();
        await dataverseOperations.DemonstrateTableManagementAsync(tableOptions);
    }

    /// <summary>
    /// Demonstrates query operations.
    /// </summary>
    private async Task RunQueryOperationsDemoAsync()
    {
        userInterface.ShowSectionHeader("Query Operations Demo");

        QueryOptions queryOptions = userInterface.GetQueryOptions();
        await dataverseOperations.DemonstrateQueryOperationsAsync(queryOptions);
    }

    /// <summary>
    /// Demonstrates validation operations.
    /// </summary>
    private async Task RunValidationOperationsDemoAsync()
    {
        userInterface.ShowSectionHeader("Validation Operations Demo");

        ValidationOptions validationOptions = userInterface.GetValidationOptions();
        await dataverseOperations.DemonstrateValidationOperationsAsync(validationOptions);
    }

    /// <summary>
    /// Demonstrates performance testing scenarios.
    /// </summary>
    private async Task RunPerformanceTestingAsync()
    {
        userInterface.ShowSectionHeader("Performance Testing Demo");

        PerformanceOptions performanceOptions = userInterface.GetPerformanceOptions();
        await dataverseOperations.DemonstratePerformanceTestingAsync(performanceOptions);
    }
}
