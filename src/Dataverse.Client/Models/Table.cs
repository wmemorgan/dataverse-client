using Microsoft.Xrm.Sdk.Metadata;

namespace Dataverse.Client.Models;

/// <summary>
/// Represents a table definition for creating new tables in Dataverse.
/// </summary>
public class TableDefinition
{
    /// <summary>
    /// Gets or sets the logical name (schema name) of the table.
    /// </summary>
    public string LogicalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the table.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plural display name of the table.
    /// </summary>
    public string DisplayCollectionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the table.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ownership type of the table.
    /// </summary>
    public OwnershipTypes OwnershipType { get; set; } = OwnershipTypes.UserOwned;

    /// <summary>
    /// Gets or sets the primary attribute definition.
    /// </summary>
    public PrimaryAttributeDefinition PrimaryAttribute { get; set; } = new();

    /// <summary>
    /// Gets or sets additional columns to create with the table.
    /// </summary>
    public List<ColumnDefinition> Columns { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the table can have activities.
    /// </summary>
    public bool HasActivities { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the table can have notes.
    /// </summary>
    public bool HasNotes { get; set; } = false;
}

/// <summary>
/// Represents the primary attribute definition for a table.
/// </summary>
public class PrimaryAttributeDefinition
{
    /// <summary>
    /// Gets or sets the logical name of the primary attribute.
    /// </summary>
    public string LogicalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the primary attribute.
    /// </summary>
    public string DisplayName { get; set; } = "Name";

    /// <summary>
    /// Gets or sets the maximum length of the primary attribute.
    /// </summary>
    public int MaxLength { get; set; } = 100;

    /// <summary>
    /// Gets or sets whether the primary attribute is required.
    /// </summary>
    public AttributeRequiredLevel RequiredLevel { get; set; } = AttributeRequiredLevel.ApplicationRequired;
}

/// <summary>
/// Represents a column definition for creating new columns.
/// </summary>
public class ColumnDefinition
{
    /// <summary>
    /// Gets or sets the logical name of the column.
    /// </summary>
    public string LogicalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the column.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the column.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data type of the column.
    /// </summary>
    public ColumnDataType DataType { get; set; }

    /// <summary>
    /// Gets or sets whether the column is required.
    /// </summary>
    public AttributeRequiredLevel RequiredLevel { get; set; } = AttributeRequiredLevel.None;

    /// <summary>
    /// Gets or sets the maximum length for string columns.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the minimum value for numeric columns.
    /// </summary>
    public int? MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum value for numeric columns.
    /// </summary>
    public int? MaxValue { get; set; }

    /// <summary>
    /// Gets or sets the format for string columns.
    /// </summary>
    public StringFormatName? StringFormat { get; set; }
}

/// <summary>
/// Represents metadata information about a table.
/// </summary>
public class TableMetadata
{
    /// <summary>
    /// Gets or sets the logical name of the table.
    /// </summary>
    public string LogicalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the table.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the table.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ownership type of the table.
    /// </summary>
    public OwnershipTypes OwnershipType { get; set; }

    /// <summary>
    /// Gets or sets the primary key attribute name.
    /// </summary>
    public string PrimaryIdAttribute { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary name attribute name.
    /// </summary>
    public string PrimaryNameAttribute { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of column names in the table.
    /// </summary>
    public List<string> ColumnNames { get; set; } = [];

    /// <summary>
    /// Gets or sets when the table was created.
    /// </summary>
    public DateTime? CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets when the table was last modified.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }
}

/// <summary>
/// Enumeration of supported column data types.
/// </summary>
public enum ColumnDataType
{
    Text,
    Integer,
    Decimal,
    Boolean,
    DateTime,
    Lookup,
    Memo,
    Email,
    Phone,
    Url,
    Currency,
    Picklist
}
