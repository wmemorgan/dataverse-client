using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Dataverse.Client.Services;

/// <summary>
/// Concrete implementation of IDataverseMetadataClient for managing Dataverse metadata operations.
/// </summary>
public class DataverseMetadataClient : IDataverseMetadataClient
{
    #region Private Fields

    private readonly ServiceClient _serviceClient;
    private readonly ILogger<DataverseMetadataClient> _logger;
    private readonly DataverseClientOptions _options;
    private bool _disposed;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the DataverseMetadataClient class.
    /// </summary>
    public DataverseMetadataClient(
        ServiceClient serviceClient,
        IOptions<DataverseClientOptions> options,
        ILogger<DataverseMetadataClient> logger)
    {
        ArgumentNullException.ThrowIfNull(serviceClient, nameof(serviceClient));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _serviceClient = serviceClient;
        _options = options.Value;
        _logger = logger;

        _logger.LogInformation("DataverseMetadataClient initialized with connection to {OrganizationUri}",
            _serviceClient.ConnectedOrgUriActual);
    }

    #endregion

    #region Table Management

    /// <inheritdoc />
    public async Task<string> CreateTableAsync(TableDefinition tableDefinition)
    {
        ArgumentNullException.ThrowIfNull(tableDefinition);

        if (string.IsNullOrWhiteSpace(tableDefinition.LogicalName))
            throw new ArgumentException("LogicalName cannot be null or empty", nameof(tableDefinition));

        try
        {
            _logger.LogInformation("Creating table '{TableName}'", tableDefinition.LogicalName);

            // Check if table already exists
            if (await TableExistsAsync(tableDefinition.LogicalName))
            {
                _logger.LogWarning("Table '{TableName}' already exists", tableDefinition.LogicalName);
                return tableDefinition.LogicalName;
            }

            // Create the entity metadata
            EntityMetadata entityMetadata = new()
            {
                SchemaName = tableDefinition.LogicalName,
                DisplayName = new Label(tableDefinition.DisplayName, 1033),
                DisplayCollectionName = new Label(tableDefinition.DisplayCollectionName, 1033),
                Description = new Label(tableDefinition.Description, 1033),
                OwnershipType = tableDefinition.OwnershipType,
                IsActivity = false,
                HasActivities = tableDefinition.HasActivities,
                HasNotes = tableDefinition.HasNotes
            };

            // Create the primary attribute
            string primaryAttributeName = string.IsNullOrWhiteSpace(tableDefinition.PrimaryAttribute.LogicalName)
                ? $"{tableDefinition.LogicalName}_name"
                : tableDefinition.PrimaryAttribute.LogicalName;

            StringAttributeMetadata primaryAttribute = new()
            {
                SchemaName = primaryAttributeName,
                DisplayName = new Label(tableDefinition.PrimaryAttribute.DisplayName, 1033),
                RequiredLevel =
                    new AttributeRequiredLevelManagedProperty(tableDefinition.PrimaryAttribute.RequiredLevel),
                MaxLength = tableDefinition.PrimaryAttribute.MaxLength,
                FormatName = StringFormatName.Text
            };

            // Create the table
            CreateEntityRequest createEntityRequest = new()
            {
                Entity = entityMetadata, PrimaryAttribute = primaryAttribute
            };

            CreateEntityResponse createEntityResponse = (CreateEntityResponse)await Task.Run(() =>
                _serviceClient.Execute(createEntityRequest));

            _logger.LogInformation("Table '{TableName}' created successfully with ID {EntityId}",
                tableDefinition.LogicalName, createEntityResponse.EntityId);

            // Add additional columns if specified
            foreach (ColumnDefinition column in tableDefinition.Columns)
                await AddColumnAsync(tableDefinition.LogicalName, column);

            return tableDefinition.LogicalName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create table '{TableName}': {Message}",
                tableDefinition.LogicalName, ex.Message);
            throw new DataverseException("TABLE_CREATION_FAILED",
                $"Failed to create table '{tableDefinition.LogicalName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task DeleteTableAsync(string logicalName)
    {
        if (string.IsNullOrWhiteSpace(logicalName))
            throw new ArgumentException("LogicalName cannot be null or empty", nameof(logicalName));

        try
        {
            _logger.LogInformation("Deleting table '{TableName}'", logicalName);

            // Check if table exists
            if (!await TableExistsAsync(logicalName))
            {
                _logger.LogWarning("Table '{TableName}' does not exist", logicalName);
                return;
            }

            DeleteEntityRequest deleteEntityRequest = new() { LogicalName = logicalName };

            await Task.Run(() => _serviceClient.Execute(deleteEntityRequest));

            _logger.LogInformation("Table '{TableName}' deleted successfully", logicalName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete table '{TableName}': {Message}", logicalName, ex.Message);
            throw new DataverseException("TABLE_DELETION_FAILED",
                $"Failed to delete table '{logicalName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> TableExistsAsync(string logicalName)
    {
        if (string.IsNullOrWhiteSpace(logicalName))
            throw new ArgumentException("LogicalName cannot be null or empty", nameof(logicalName));

        try
        {
            RetrieveEntityRequest retrieveEntityRequest = new()
            {
                LogicalName = logicalName, EntityFilters = EntityFilters.Entity
            };

            await Task.Run(() => _serviceClient.Execute(retrieveEntityRequest));
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<TableMetadata> GetTableMetadataAsync(string logicalName)
    {
        if (string.IsNullOrWhiteSpace(logicalName))
            throw new ArgumentException("LogicalName cannot be null or empty", nameof(logicalName));

        try
        {
            _logger.LogDebug("Retrieving metadata for table '{TableName}'", logicalName);

            RetrieveEntityRequest retrieveEntityRequest = new()
            {
                LogicalName = logicalName, EntityFilters = EntityFilters.Entity | EntityFilters.Attributes
            };

            RetrieveEntityResponse response = (RetrieveEntityResponse)await Task.Run(() =>
                _serviceClient.Execute(retrieveEntityRequest));

            EntityMetadata entityMetadata = response.EntityMetadata;

            TableMetadata tableMetadata = new()
            {
                LogicalName = entityMetadata.LogicalName ?? string.Empty,
                DisplayName = entityMetadata.DisplayName?.UserLocalizedLabel?.Label ?? string.Empty,
                Description = entityMetadata.Description?.UserLocalizedLabel?.Label ?? string.Empty,
                OwnershipType = entityMetadata.OwnershipType ?? OwnershipTypes.None,
                PrimaryIdAttribute = entityMetadata.PrimaryIdAttribute ?? string.Empty,
                PrimaryNameAttribute = entityMetadata.PrimaryNameAttribute ?? string.Empty,
                ColumnNames = entityMetadata.Attributes?.Select(a => a.LogicalName).ToList() ?? [],
                CreatedOn = entityMetadata.CreatedOn,
                ModifiedOn = entityMetadata.ModifiedOn
            };

            _logger.LogInformation("Retrieved metadata for table '{TableName}' with {ColumnCount} columns",
                logicalName, tableMetadata.ColumnNames.Count);

            return tableMetadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve metadata for table '{TableName}': {Message}",
                logicalName, ex.Message);
            throw new DataverseException("TABLE_METADATA_RETRIEVAL_FAILED",
                $"Failed to retrieve metadata for table '{logicalName}': {ex.Message}", ex);
        }
    }

    #endregion

    #region Column Management

    /// <inheritdoc />
    public async Task AddColumnAsync(string tableName, ColumnDefinition columnDefinition)
    {
        ArgumentNullException.ThrowIfNull(columnDefinition);

        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("TableName cannot be null or empty", nameof(tableName));

        if (string.IsNullOrWhiteSpace(columnDefinition.LogicalName))
            throw new ArgumentException("Column LogicalName cannot be null or empty", nameof(columnDefinition));

        try
        {
            _logger.LogInformation("Adding column '{ColumnName}' to table '{TableName}'",
                columnDefinition.LogicalName, tableName);

            AttributeMetadata attributeMetadata = CreateAttributeMetadata(columnDefinition);

            CreateAttributeRequest createAttributeRequest = new()
            {
                EntityName = tableName, Attribute = attributeMetadata
            };

            await Task.Run(() => _serviceClient.Execute(createAttributeRequest));

            _logger.LogInformation("Column '{ColumnName}' added successfully to table '{TableName}'",
                columnDefinition.LogicalName, tableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add column '{ColumnName}' to table '{TableName}': {Message}",
                columnDefinition.LogicalName, tableName, ex.Message);
            throw new DataverseException("COLUMN_CREATION_FAILED",
                $"Failed to add column '{columnDefinition.LogicalName}' to table '{tableName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task DeleteColumnAsync(string tableName, string columnName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("TableName cannot be null or empty", nameof(tableName));

        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("ColumnName cannot be null or empty", nameof(columnName));

        try
        {
            _logger.LogInformation("Deleting column '{ColumnName}' from table '{TableName}'", columnName, tableName);

            DeleteAttributeRequest deleteAttributeRequest = new()
            {
                EntityLogicalName = tableName, LogicalName = columnName
            };

            await Task.Run(() => _serviceClient.Execute(deleteAttributeRequest));

            _logger.LogInformation("Column '{ColumnName}' deleted successfully from table '{TableName}'",
                columnName, tableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete column '{ColumnName}' from table '{TableName}': {Message}",
                columnName, tableName, ex.Message);
            throw new DataverseException("COLUMN_DELETION_FAILED",
                $"Failed to delete column '{columnName}' from table '{tableName}': {ex.Message}", ex);
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Creates attribute metadata based on the column definition.
    /// </summary>
    private static AttributeMetadata CreateAttributeMetadata(ColumnDefinition columnDefinition)
    {
        AttributeRequiredLevelManagedProperty requiredLevel = new(columnDefinition.RequiredLevel);
        Label displayName = new(columnDefinition.DisplayName, 1033);
        Label description = new(columnDefinition.Description, 1033);

        return columnDefinition.DataType switch
        {
            ColumnDataType.Text => new StringAttributeMetadata
            {
                SchemaName = columnDefinition.LogicalName,
                DisplayName = displayName,
                Description = description,
                RequiredLevel = requiredLevel,
                MaxLength = columnDefinition.MaxLength ?? 100,
                FormatName = columnDefinition.StringFormat ?? StringFormatName.Text
            },
            ColumnDataType.Email => new StringAttributeMetadata
            {
                SchemaName = columnDefinition.LogicalName,
                DisplayName = displayName,
                Description = description,
                RequiredLevel = requiredLevel,
                MaxLength = columnDefinition.MaxLength ?? 100,
                FormatName = StringFormatName.Email
            },
            ColumnDataType.Phone => new StringAttributeMetadata
            {
                SchemaName = columnDefinition.LogicalName,
                DisplayName = displayName,
                Description = description,
                RequiredLevel = requiredLevel,
                MaxLength = columnDefinition.MaxLength ?? 50,
                FormatName = StringFormatName.Phone
            },
            ColumnDataType.Url => new StringAttributeMetadata
            {
                SchemaName = columnDefinition.LogicalName,
                DisplayName = displayName,
                Description = description,
                RequiredLevel = requiredLevel,
                MaxLength = columnDefinition.MaxLength ?? 200,
                FormatName = StringFormatName.Url
            },
            ColumnDataType.Memo => new MemoAttributeMetadata
            {
                SchemaName = columnDefinition.LogicalName,
                DisplayName = displayName,
                Description = description,
                RequiredLevel = requiredLevel,
                MaxLength = columnDefinition.MaxLength ?? 2000
            },
            ColumnDataType.Integer => new IntegerAttributeMetadata
            {
                SchemaName = columnDefinition.LogicalName,
                DisplayName = displayName,
                Description = description,
                RequiredLevel = requiredLevel,
                MinValue = columnDefinition.MinValue ?? int.MinValue,
                MaxValue = columnDefinition.MaxValue ?? int.MaxValue
            },
            ColumnDataType.Decimal => new DecimalAttributeMetadata
            {
                SchemaName = columnDefinition.LogicalName,
                DisplayName = displayName,
                Description = description,
                RequiredLevel = requiredLevel,
                MinValue = columnDefinition.MinValue ?? decimal.MinValue,
                MaxValue = columnDefinition.MaxValue ?? decimal.MaxValue,
                Precision = 2
            },
            ColumnDataType.Boolean => new BooleanAttributeMetadata
            {
                SchemaName = columnDefinition.LogicalName,
                DisplayName = displayName,
                Description = description,
                RequiredLevel = requiredLevel,
                OptionSet = new BooleanOptionSetMetadata(
                    new OptionMetadata(new Label("Yes", 1033), 1),
                    new OptionMetadata(new Label("No", 1033), 0)
                )
            },
            ColumnDataType.DateTime => new DateTimeAttributeMetadata
            {
                SchemaName = columnDefinition.LogicalName,
                DisplayName = displayName,
                Description = description,
                RequiredLevel = requiredLevel,
                Format = DateTimeFormat.DateAndTime
            },
            ColumnDataType.Currency => new MoneyAttributeMetadata
            {
                SchemaName = columnDefinition.LogicalName,
                DisplayName = displayName,
                Description = description,
                RequiredLevel = requiredLevel,
                MinValue = columnDefinition.MinValue ?? 0,
                MaxValue = columnDefinition.MaxValue ?? 1000000000,
                Precision = 2
            },
            _ => throw new ArgumentException($"Unsupported column data type: {columnDefinition.DataType}")
        };
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Releases all resources used by the DataverseMetadataClient.
    /// </summary>
    public void Dispose() => Dispose(true);

    /// <summary>
    /// Releases the unmanaged resources used by the DataverseMetadataClient and optionally releases the managed resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                _logger.LogInformation("DataverseMetadataClient disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occurred during DataverseMetadataClient disposal");
            }

            _disposed = true;
        }
    }

    #endregion
}
