using Dataverse.Client.Models;

namespace Dataverse.Client.Interfaces;

/// <summary>
/// Interface for Microsoft Dataverse metadata and schema management operations.
/// </summary>
public interface IDataverseMetadataClient : IDisposable
{
    #region Table Management

    /// <summary>
    /// Creates a new table in Dataverse with the specified metadata.
    /// </summary>
    /// <param name="tableDefinition">The table definition including schema, display names, and settings</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created table's logical name.</returns>
    Task<string> CreateTableAsync(TableDefinition tableDefinition);

    /// <summary>
    /// Deletes a table from Dataverse.
    /// </summary>
    /// <param name="logicalName">The logical name of the table to delete</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteTableAsync(string logicalName);

    /// <summary>
    /// Checks if a table exists in the current environment.
    /// </summary>
    /// <param name="logicalName">The logical name of the table to check</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists.</returns>
    Task<bool> TableExistsAsync(string logicalName);

    /// <summary>
    /// Retrieves metadata for a specific table.
    /// </summary>
    /// <param name="logicalName">The logical name of the table</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the table metadata.</returns>
    Task<TableMetadata> GetTableMetadataAsync(string logicalName);

    #endregion

    #region Column Management

    /// <summary>
    /// Adds a column to an existing table.
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <param name="columnDefinition">The column definition</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task AddColumnAsync(string tableName, ColumnDefinition columnDefinition);

    /// <summary>
    /// Deletes a column from a table.
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <param name="columnName">The logical name of the column to delete</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteColumnAsync(string tableName, string columnName);

    #endregion
}
