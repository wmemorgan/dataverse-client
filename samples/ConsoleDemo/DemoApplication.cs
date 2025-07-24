using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Microsoft.Extensions.Logging;

namespace ConsoleDemo;

/// <summary>
/// Main application class that orchestrates the demo workflow.
/// </summary>
public class DemoApplication
{
    private readonly IDataverseClient _dataverseClient;
    private readonly IDataverseMetadataClient _metadataClient;
    private readonly IDataverseOperations _dataverseOperations;
    private readonly IUserInterface _userInterface;
    private readonly ILogger<DemoApplication> _logger;

    public DemoApplication(
        IDataverseClient dataverseClient,
        IDataverseMetadataClient metadataClient,
        IDataverseOperations dataverseOperations,
        IUserInterface userInterface,
        ILogger<DemoApplication> logger)
    {
        _dataverseClient = dataverseClient;
        _metadataClient = metadataClient;
        _dataverseOperations = dataverseOperations;
        _userInterface = userInterface;
        _logger = logger;
    }

    /// <summary>
    /// Runs the main demo application workflow.
    /// </summary>
    public async Task RunAsync()
    {
        _userInterface.ShowWelcome();

        try
        {
            // Test connection
            if (!await _userInterface.TestConnectionAsync(_dataverseClient))
            {
                _userInterface.ShowError("Failed to connect to Dataverse. Please check your configuration.");
                return;
            }

            // Display connection info
            ConnectionInfo connectionInfo = _dataverseClient.GetConnectionInfo();
            _userInterface.DisplayConnectionInfo(connectionInfo);

            // Main menu loop
            bool continueRunning = true;
            while (continueRunning)
            {
                DemoOption selectedOption = _userInterface.ShowMainMenu();
                continueRunning = await HandleMenuSelectionAsync(selectedOption);
            }

            _userInterface.ShowSuccess("Demo completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Demo application encountered an error");
            _userInterface.ShowError($"Application error: {ex.Message}");
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
                    _userInterface.ShowWarning("Invalid option selected.");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling menu selection: {Option}", option);
            _userInterface.ShowError($"Operation failed: {ex.Message}");
        }

        _userInterface.PauseForUser();
        return true;
    }

    /// <summary>
    /// Demonstrates basic CRUD operations.
    /// </summary>
    private async Task RunBasicCrudDemoAsync()
    {
        _userInterface.ShowSectionHeader("Basic CRUD Operations Demo");

        var crudOptions = _userInterface.GetCrudOptions();
        
        switch (crudOptions.EntityType)
        {
            case EntityType.CustomTable:
                await _dataverseOperations.DemonstrateCustomTableCrudAsync(crudOptions);
                break;
            case EntityType.Contact:
                await _dataverseOperations.DemonstrateContactCrudAsync(crudOptions);
                break;
        }
    }

    /// <summary>
    /// Demonstrates batch operations.
    /// </summary>
    private async Task RunBatchOperationsDemoAsync()
    {
        _userInterface.ShowSectionHeader("Batch Operations Demo");

        var batchOptions = _userInterface.GetBatchOptions();
        
        switch (batchOptions.EntityType)
        {
            case EntityType.CustomTable:
                await _dataverseOperations.DemonstrateBatchOperationsAsync(batchOptions);
                break;
            case EntityType.Contact:
                await _dataverseOperations.DemonstrateContactBatchOperationsAsync(batchOptions);
                break;
        }
    }

    /// <summary>
    /// Demonstrates table management operations.
    /// </summary>
    private async Task RunTableManagementDemoAsync()
    {
        _userInterface.ShowSectionHeader("Table Management Demo");

        var tableOptions = _userInterface.GetTableManagementOptions();
        await _dataverseOperations.DemonstrateTableManagementAsync(tableOptions);
    }

    /// <summary>
    /// Demonstrates query operations.
    /// </summary>
    private async Task RunQueryOperationsDemoAsync()
    {
        _userInterface.ShowSectionHeader("Query Operations Demo");

        var queryOptions = _userInterface.GetQueryOptions();
        await _dataverseOperations.DemonstrateQueryOperationsAsync(queryOptions);
    }

    /// <summary>
    /// Demonstrates validation operations.
    /// </summary>
    private async Task RunValidationOperationsDemoAsync()
    {
        _userInterface.ShowSectionHeader("Validation Operations Demo");

        var validationOptions = _userInterface.GetValidationOptions();
        await _dataverseOperations.DemonstrateValidationOperationsAsync(validationOptions);
    }

    /// <summary>
    /// Demonstrates performance testing scenarios.
    /// </summary>
    private async Task RunPerformanceTestingAsync()
    {
        _userInterface.ShowSectionHeader("Performance Testing Demo");

        var performanceOptions = _userInterface.GetPerformanceOptions();
        await _dataverseOperations.DemonstratePerformanceTestingAsync(performanceOptions);
    }
}