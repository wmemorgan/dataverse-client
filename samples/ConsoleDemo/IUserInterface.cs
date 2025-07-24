using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Microsoft.Xrm.Sdk;
using DataverseValidationResult = Dataverse.Client.Models.ValidationResult;

namespace ConsoleDemo;

/// <summary>
/// Interface for user interaction to support different UI implementations.
/// </summary>
public interface IUserInterface
{
    // Display methods
    void ShowWelcome();
    void ShowSectionHeader(string title);
    void ShowInfo(string message);
    void ShowSuccess(string message);
    void ShowWarning(string message);
    void ShowError(string message);
    void PauseForUser();

    // Connection methods
    Task<bool> TestConnectionAsync(IDataverseClient client);
    void DisplayConnectionInfo(ConnectionInfo connectionInfo);

    // Menu methods
    DemoOption ShowMainMenu();
    CrudOptions GetCrudOptions();
    BatchOptions GetBatchOptions();
    TableManagementOptions GetTableManagementOptions();
    QueryOptions GetQueryOptions();
    ValidationOptions GetValidationOptions();
    PerformanceOptions GetPerformanceOptions();

    // Result display methods
    void DisplayEntityRecord(Entity entity, string entityName);
    void DisplayBatchResult(BatchOperationResult result, TimeSpan elapsed);
    void DisplayBatchRetrieveResult(BatchRetrieveResult result, TimeSpan elapsed);
    void DisplayBatchProgress(BatchProgress progress);
    void DisplayTableMetadata(TableMetadata metadata);
    void DisplayQueryResults(EntityCollection results, string queryType);
    void DisplayValidationResult(DataverseValidationResult result);
    void DisplayPerformanceComparison(int recordCount, TimeSpan individualTime, TimeSpan batchTime);
    void DisplayBatchSizeComparison(List<(int BatchSize, TimeSpan Duration, int SuccessCount)> results);
    void DisplayConcurrentOperationResults(BatchOperationResult[] results, TimeSpan totalTime);
}