using Dataverse.Client.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Dataverse.Client.Interfaces;

/// <summary>
/// Service for handling batch operations against Microsoft Dataverse.
/// Provides optimized batch processing for CRUD operations with error handling and retry logic.
/// </summary>
public interface IBatchProcessor
{
    /// <summary>
    /// Creates multiple records in batches using ExecuteMultiple requests.
    /// </summary>
    /// <param name="entities">Collection of entities to create</param>
    /// <param name="batchSize">Size of each batch (default: configured batch size)</param>
    /// <returns>Results of the batch operation</returns>
    Task<BatchOperationResult> CreateRecordsAsync(IEnumerable<Entity> entities, int? batchSize = null);

    /// <summary>
    /// Updates multiple records in batches using ExecuteMultiple requests.
    /// </summary>
    /// <param name="entities">Collection of entities to update</param>
    /// <param name="batchSize">Size of each batch (default: configured batch size)</param>
    /// <returns>Results of the batch operation</returns>
    Task<BatchOperationResult> UpdateRecordsAsync(IEnumerable<Entity> entities, int? batchSize = null);

    /// <summary>
    /// Deletes multiple records in batches using ExecuteMultiple requests.
    /// </summary>
    /// <param name="entityReferences">Collection of entity references to delete</param>
    /// <param name="batchSize">Size of each batch (default: configured batch size)</param>
    /// <returns>Results of the batch operation</returns>
    Task<BatchOperationResult> DeleteRecordsAsync(IEnumerable<EntityReference> entityReferences, int? batchSize = null);

    /// <summary>
    /// Retrieves multiple records in batches using ExecuteMultiple requests.
    /// </summary>
    /// <param name="entityReferences">Collection of entity references to retrieve</param>
    /// <param name="columns">Columns to retrieve</param>
    /// <param name="batchSize">Size of each batch (default: configured batch size)</param>
    /// <returns>Results of the batch retrieve operation</returns>
    Task<BatchRetrieveResult> RetrieveRecordsAsync(IEnumerable<EntityReference> entityReferences, ColumnSet columns, int? batchSize = null);
}
