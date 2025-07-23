using Dataverse.Client.Models;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;

namespace Dataverse.Client.Interfaces;

/// <summary>
/// Comprehensive client for Microsoft Dataverse operations including 
/// CRUD operations, batch processing, querying, and validation.
/// </summary>
public interface IDataverseClient
{
    #region Connection management

    /// <summary>
    /// Validates the current connection to Dataverse by performing a lightweight operation.
    /// This method can be used to verify connectivity before performing other operations.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the connection is valid, false otherwise.</returns>
    Task<bool> ValidateConnectionAsync();

    /// <summary>
    /// Gets comprehensive information about the current Dataverse connection including
    /// organization details, user information, and connection state.
    /// </summary>
    /// <returns>A ConnectionInfo object containing detailed connection information.</returns>
    ConnectionInfo GetConnectionInfo();

    #endregion 

    #region Individual CRUD operations

    /// <summary>
    /// Creates a new record in Dataverse.
    /// This method performs a single create operation and is suitable for individual record creation.
    /// For bulk operations, consider using CreateBatchAsync for better performance.
    /// </summary>
    /// <param name="entity">The entity to create. Must have the logical name and required attributes set.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the GUID of the created record.</returns>
    Task<Guid> CreateAsync(Entity entity);

    /// <summary>
    /// Retrieves a specific record from Dataverse by its unique identifier.
    /// </summary>
    /// <param name="entityName">The logical name of the entity to retrieve.</param>
    /// <param name="id">The unique identifier of the record to retrieve.</param>
    /// <param name="columns">The columns to retrieve. Use ColumnSet.AllColumns for all columns or specify individual columns for better performance.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the retrieved Entity with the requested columns populated.</returns>
    Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columns);

    /// <summary>
    /// Updates an existing record in Dataverse.
    /// Only the attributes present in the entity will be updated; other attributes remain unchanged.
    /// </summary>
    /// <param name="entity">The entity to update. Must have the Id property set and contain the attributes to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(Entity entity);

    /// <summary>
    /// Deletes a record from Dataverse.
    /// This operation is irreversible unless the entity supports logical deletion.
    /// </summary>
    /// <param name="entityName">The logical name of the entity containing the record to delete.</param>
    /// <param name="id">The unique identifier of the record to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAsync(string entityName, Guid id);

    #endregion

    #region Batch CRUD operations (high-performance)

    /// <summary>
    /// Creates multiple records in Dataverse using batch processing for optimal performance.
    /// This method processes records in configurable batch sizes and provides comprehensive
    /// error handling and progress reporting capabilities.
    /// </summary>
    /// <param name="entities">The collection of entities to create. Each entity must have the logical name and required attributes set.</param>
    /// <param name="config">Optional batch configuration to override default settings such as batch size and retry behavior.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains detailed information about the batch operation including success/failure counts, errors, and performance metrics.</returns>
    Task<BatchOperationResult> CreateBatchAsync(IEnumerable<Entity> entities, BatchConfiguration? config = null);

    /// <summary>
    /// Updates multiple records in Dataverse using batch processing for optimal performance.
    /// Only the attributes present in each entity will be updated; other attributes remain unchanged.
    /// </summary>
    /// <param name="entities">The collection of entities to update. Each entity must have the Id property set and contain the attributes to update.</param>
    /// <param name="config">Optional batch configuration to override default settings such as batch size and retry behavior.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains detailed information about the batch operation including success/failure counts, errors, and performance metrics.</returns>
    Task<BatchOperationResult> UpdateBatchAsync(IEnumerable<Entity> entities, BatchConfiguration? config = null);

    /// <summary>
    /// Deletes multiple records from Dataverse using batch processing for optimal performance.
    /// These operations are irreversible unless the entities support logical deletion.
    /// </summary>
    /// <param name="entityRefs">The collection of entity references identifying the records to delete.</param>
    /// <param name="config">Optional batch configuration to override default settings such as batch size and retry behavior.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains detailed information about the batch operation including success/failure counts, errors, and performance metrics.</returns>
    Task<BatchOperationResult> DeleteBatchAsync(IEnumerable<EntityReference> entityRefs, BatchConfiguration? config = null);

    /// <summary>
    /// Retrieves multiple records from Dataverse using batch processing for optimal performance.
    /// This method efficiently retrieves large numbers of records while handling missing records gracefully.
    /// </summary>
    /// <param name="entityRefs">The collection of entity references identifying the records to retrieve.</param>
    /// <param name="columns">The columns to retrieve for all records. Use ColumnSet.AllColumns for all columns or specify individual columns for better performance.</param>
    /// <param name="config">Optional batch configuration to override default settings such as batch size and retry behavior.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the retrieved entities, information about missing records, and comprehensive operation metrics.</returns>
    Task<BatchRetrieveResult> RetrieveBatchAsync(IEnumerable<EntityReference> entityRefs, ColumnSet columns, BatchConfiguration? config = null);

    #endregion

    #region Query operations

    /// <summary>
    /// Executes a QueryExpression against Dataverse to retrieve multiple records.
    /// This method supports complex filtering, sorting, and joining operations.
    /// </summary>
    /// <param name="query">The QueryExpression defining the query criteria, columns to retrieve, and sorting options.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is an EntityCollection containing the matching records and pagination information.</returns>
    Task<EntityCollection> RetrieveMultipleAsync(QueryExpression query);

    /// <summary>
    /// Executes a FetchXML query against Dataverse to retrieve multiple records.
    /// FetchXML provides a powerful XML-based query language with advanced aggregation and grouping capabilities.
    /// </summary>
    /// <param name="fetchXml">The FetchXML query string defining the query criteria and output format.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is an EntityCollection containing the matching records and any aggregation results.</returns>
    Task<EntityCollection> RetrieveMultipleAsync(string fetchXml);

    #endregion

    #region Validation operations

    /// <summary>
    /// Validates that the current user has access to perform operations on the specified table.
    /// This method checks both the table's existence and the user's permissions.
    /// </summary>
    /// <param name="tableName">The logical name of the table to validate access for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains detailed validation information including any access restrictions or errors.</returns>
    Task<ValidationResult> ValidateTableAccessAsync(string tableName);

    /// <summary>
    /// Validates that the specified table exists and contains the expected columns.
    /// This method is useful for ensuring schema compatibility before performing operations.
    /// </summary>
    /// <param name="tableName">The logical name of the table to validate.</param>
    /// <param name="expectedColumns">The collection of column names that are expected to exist in the table.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains detailed validation information including missing columns, type mismatches, or access issues.</returns>
    Task<ValidationResult> ValidateSchemaAsync(string tableName, IEnumerable<string> expectedColumns);

    #endregion 
}
