using Dataverse.Client.Interfaces;
using Dataverse.Client.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Dataverse.Samples.Common;

public static class SampleDataGenerator
{
    private static readonly Random _random = new();

    // Expanded list of first names for better variety
    private static readonly string[] FirstNames =
    [
        // Original names
        "John", "Jane", "Michael", "Sarah", "David", "Lisa", "Robert", "Emma", "Chris", "Amy",
        // Additional male names
        "James", "William", "Alexander", "Benjamin", "Daniel", "Matthew", "Andrew", "Joshua", "Christopher", "Anthony",
        "Mark", "Steven", "Paul", "Kenneth", "Kevin", "Brian", "George", "Edward", "Ronald", "Timothy",
        "Jason", "Jeffrey", "Ryan", "Jacob", "Gary", "Nicholas", "Eric", "Jonathan", "Stephen", "Larry",
        "Justin", "Scott", "Brandon", "Frank", "Gregory", "Raymond", "Samuel", "Patrick", "Jack", "Dennis",
        // Additional female names
        "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Barbara", "Susan", "Jessica", "Karen", "Nancy",
        "Dorothy", "Betty", "Helen", "Sandra", "Donna", "Carol", "Ruth", "Sharon", "Michelle", "Laura",
        "Emily", "Kimberly", "Deborah", "Dorothy", "Amy", "Angela", "Ashley", "Brenda", "Cynthia", "Marie",
        "Janet", "Catherine", "Frances", "Ann", "Joyce", "Samantha", "Debra", "Rachel", "Carolyn", "Virginia",
        // Modern/diverse names
        "Aiden", "Sofia", "Mason", "Olivia", "Ethan", "Ava", "Noah", "Isabella", "Liam", "Mia",
        "Lucas", "Abigail", "Oliver", "Madison", "Elijah", "Charlotte", "Owen", "Harper", "Gabriel", "Evelyn",
        "Carter", "Amelia", "Wyatt", "Ella", "Luke", "Chloe", "Grayson", "Victoria", "Jack", "Grace"
    ];

    // Expanded list of last names for better variety
    private static readonly string[] LastNames =
    [
        // Original names
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Wilson", "Moore",
        // Additional common surnames
        "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Young", "Allen",
        "King", "Wright", "Lopez", "Hill", "Scott", "Green", "Adams", "Baker", "Gonzalez", "Nelson",
        "Carter", "Mitchell", "Perez", "Roberts", "Turner", "Phillips", "Campbell", "Parker", "Evans", "Edwards",
        "Collins", "Stewart", "Sanchez", "Morris", "Rogers", "Reed", "Cook", "Morgan", "Bell", "Murphy",
        // International surnames for diversity
        "Rodriguez", "Martinez", "Hernandez", "Lewis", "Lee", "Walker", "Hall", "Young", "Robinson", "Clark",
        "Nguyen", "Kumar", "Singh", "Patel", "Kim", "Li", "Wang", "Zhang", "Chen", "Liu",
        "O'Connor", "McDonald", "Murphy", "Kelly", "Ryan", "Sullivan", "Walsh", "McCarthy", "O'Brien", "Connor",
        "Fischer", "Weber", "Meyer", "Wagner", "Becker", "Schulz", "Hoffmann", "Schäfer", "Koch", "Bauer",
        "Rossi", "Russo", "Ferrari", "Esposito", "Bianchi", "Romano", "Colombo", "Ricci", "Marino", "Greco"
    ];

    private static readonly string[] Companies =
    [
        "Contoso Ltd", "Fabrikam Inc", "Adventure Works", "Northwind Traders", "Wide World Importers",
        "Fourth Coffee", "Tailspin Toys", "Wingtip Toys", "Alpine Ski House", "City Power & Light",
        "Consolidated Messenger", "Graphic Design Institute", "Humongous Insurance", "Litware Inc",
        "Lucerne Publishing", "Margie's Travel", "Proseware Inc", "School of Fine Art", "The Phone Company",
        "Trey Research", "VanArsdel Ltd", "Woodgrove Bank"
    ];

    private const string TestTableLogicalName = "cr123_testtable";
    private const string TestTableDisplayName = "Test Table DataverseClient";
    private const string TestTableDescription = "Test table created by Dataverse.Client demo for testing purposes";

    #region Table Management

    /// <summary>
    /// Creates a test table definition that can be used with IDataverseMetadataClient.
    /// </summary>
    /// <returns>A TableDefinition for the test table</returns>
    public static TableDefinition CreateTestTableDefinition() =>
        new()
        {
            LogicalName = TestTableLogicalName,
            DisplayName = TestTableDisplayName,
            DisplayCollectionName = $"{TestTableDisplayName}s",
            Description = TestTableDescription,
            OwnershipType = OwnershipTypes.UserOwned,
            PrimaryAttribute = new PrimaryAttributeDefinition
            {
                LogicalName = $"{TestTableLogicalName}_name",
                DisplayName = "Name",
                MaxLength = 100,
                RequiredLevel = AttributeRequiredLevel.ApplicationRequired
            },
            Columns =
            [
                new ColumnDefinition
                {
                    LogicalName = $"{TestTableLogicalName}_email",
                    DisplayName = "Email",
                    Description = "Email address of the person",
                    DataType = ColumnDataType.Email,
                    MaxLength = 100,
                    RequiredLevel = AttributeRequiredLevel.None
                },
                new ColumnDefinition
                {
                    LogicalName = $"{TestTableLogicalName}_phone",
                    DisplayName = "Phone",
                    Description = "Phone number of the person",
                    DataType = ColumnDataType.Phone,
                    MaxLength = 50,
                    RequiredLevel = AttributeRequiredLevel.None
                },
                new ColumnDefinition
                {
                    LogicalName = $"{TestTableLogicalName}_age",
                    DisplayName = "Age",
                    Description = "Age of the person",
                    DataType = ColumnDataType.Integer,
                    MinValue = 0,
                    MaxValue = 150,
                    RequiredLevel = AttributeRequiredLevel.None
                },
                new ColumnDefinition
                {
                    LogicalName = $"{TestTableLogicalName}_description",
                    DisplayName = "Description",
                    Description = "Description or notes about the person",
                    DataType = ColumnDataType.Memo,
                    MaxLength = 2000,
                    RequiredLevel = AttributeRequiredLevel.None
                }
            ],
            HasActivities = false,
            HasNotes = false
        };

    /// <summary>
    /// Creates a test table using the metadata client.
    /// </summary>
    /// <param name="metadataClient">The Dataverse metadata client</param>
    /// <returns>The logical name of the created table</returns>
    public static async Task<string> CreateTestTableAsync(IDataverseMetadataClient metadataClient)
    {
        ArgumentNullException.ThrowIfNull(metadataClient);

        try
        {
            // Check if table already exists
            if (await metadataClient.TableExistsAsync(TestTableLogicalName))
            {
                Console.WriteLine($"Table '{TestTableLogicalName}' already exists.");
                return TestTableLogicalName;
            }

            Console.WriteLine($"Creating test table '{TestTableLogicalName}'...");

            TableDefinition tableDefinition = CreateTestTableDefinition();
            string createdTableName = await metadataClient.CreateTableAsync(tableDefinition);

            Console.WriteLine($"? Test table '{createdTableName}' created successfully!");
            return createdTableName;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Failed to create test table: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Deletes the test table using the metadata client.
    /// </summary>
    /// <param name="metadataClient">The Dataverse metadata client</param>
    /// <returns>True if the table was deleted successfully</returns>
    public static async Task<bool> DeleteTestTableAsync(IDataverseMetadataClient metadataClient)
    {
        ArgumentNullException.ThrowIfNull(metadataClient);

        try
        {
            // Check if table exists
            if (!await metadataClient.TableExistsAsync(TestTableLogicalName))
            {
                Console.WriteLine($"Table '{TestTableLogicalName}' does not exist.");
                return true;
            }

            Console.WriteLine($"Deleting test table '{TestTableLogicalName}'...");

            await metadataClient.DeleteTableAsync(TestTableLogicalName);

            Console.WriteLine($"? Test table '{TestTableLogicalName}' deleted successfully!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Failed to delete test table: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Checks if the test table exists using the metadata client.
    /// </summary>
    /// <param name="metadataClient">The Dataverse metadata client</param>
    /// <returns>True if the table exists</returns>
    public static async Task<bool> TestTableExistsAsync(IDataverseMetadataClient metadataClient)
    {
        ArgumentNullException.ThrowIfNull(metadataClient);
        return await metadataClient.TableExistsAsync(TestTableLogicalName);
    }

    /// <summary>
    /// Gets metadata for the test table.
    /// </summary>
    /// <param name="metadataClient">The Dataverse metadata client</param>
    /// <returns>TableMetadata for the test table</returns>
    public static async Task<TableMetadata> GetTestTableMetadataAsync(IDataverseMetadataClient metadataClient)
    {
        ArgumentNullException.ThrowIfNull(metadataClient);
        return await metadataClient.GetTableMetadataAsync(TestTableLogicalName);
    }

    #endregion

    #region Sample Data Generation

    /// <summary>
    /// Creates a sample record for the test table with randomized data.
    /// </summary>
    /// <param name="recordNumber">Optional record number for generating sequential data</param>
    /// <returns>A new Entity for the test table</returns>
    public static Entity CreateSampleTestRecord(int? recordNumber = null)
    {
        string firstName = FirstNames[_random.Next(FirstNames.Length)];
        string lastName = LastNames[_random.Next(LastNames.Length)];
        string fullName = recordNumber.HasValue
            ? $"{firstName} {lastName} #{recordNumber}"
            : $"{firstName} {lastName}";

        Entity testRecord = new(TestTableLogicalName)
        {
            [$"{TestTableLogicalName}_name"] = fullName,
            [$"{TestTableLogicalName}_email"] = GenerateEmailAddress(firstName, lastName, recordNumber),
            [$"{TestTableLogicalName}_phone"] = GeneratePhoneNumber(),
            [$"{TestTableLogicalName}_age"] = _random.Next(18, 80),
            [$"{TestTableLogicalName}_description"] = GenerateDescription(recordNumber)
        };

        return testRecord;
    }

    /// <summary>
    /// Creates multiple sample records for the test table with enhanced variety.
    /// </summary>
    /// <param name="count">Number of records to create</param>
    /// <param name="useSequentialNumbering">Whether to include sequential numbering in names</param>
    /// <returns>List of Entity objects for the test table</returns>
    public static List<Entity> CreateSampleTestRecords(int count, bool useSequentialNumbering = true)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than zero", nameof(count));

        List<Entity> records = new(count);

        for (int i = 0; i < count; i++)
        {
            int? recordNumber = useSequentialNumbering ? i + 1 : null;
            records.Add(CreateSampleTestRecord(recordNumber));
        }

        return records;
    }

    /// <summary>
    /// Creates sample records optimized for batch testing with configurable patterns.
    /// </summary>
    /// <param name="totalRecords">Total number of records to create</param>
    /// <param name="batchSize">Expected batch size for optimization</param>
    /// <param name="includeVariations">Include data variations for testing edge cases</param>
    /// <returns>List of Entity objects optimized for batch operations</returns>
    public static List<Entity> CreateBatchTestRecords(int totalRecords, int batchSize = 100,
        bool includeVariations = true)
    {
        if (totalRecords <= 0)
            throw new ArgumentException("Total records must be greater than zero", nameof(totalRecords));

        List<Entity> records = new(totalRecords);
        int batchCount = (int)Math.Ceiling((double)totalRecords / batchSize);

        Console.WriteLine(
            $"Generating {totalRecords} test records for batch operations ({batchCount} batches of ~{batchSize} records)...");

        for (int i = 0; i < totalRecords; i++)
        {
            Entity record = CreateSampleTestRecord(i + 1);

            // Add variations for testing robustness
            if (includeVariations) AddTestVariations(record, i, totalRecords);

            records.Add(record);

            // Progress reporting for large datasets
            if (totalRecords > 1000 && (i + 1) % 1000 == 0)
                Console.WriteLine($"Generated {i + 1}/{totalRecords} records...");
        }

        Console.WriteLine($"? Generated {totalRecords} test records for batch operations");
        return records;
    }

    /// <summary>
    /// Gets the logical name of the test table.
    /// </summary>
    public static string GetTestTableLogicalName() => TestTableLogicalName;

    /// <summary>
    /// Gets the expected column names for the test table.
    /// </summary>
    public static string[] GetTestTableColumnNames() =>
    [
        $"{TestTableLogicalName}_name",
        $"{TestTableLogicalName}_email",
        $"{TestTableLogicalName}_phone",
        $"{TestTableLogicalName}_age",
        $"{TestTableLogicalName}_description"
    ];

    #endregion

    #region Existing Methods (Contact/Account) - Enhanced with better variety

    public static Entity CreateSampleContact(int? contactNumber = null)
    {
        string firstName = FirstNames[_random.Next(FirstNames.Length)];
        string lastName = LastNames[_random.Next(LastNames.Length)];

        Entity contact = new("contact")
        {
            ["firstname"] = firstName,
            ["lastname"] = lastName,
            ["emailaddress1"] = GenerateEmailAddress(firstName, lastName, contactNumber),
            ["telephone1"] = GeneratePhoneNumber(),
            ["jobtitle"] = GetRandomJobTitle(),
            ["description"] = contactNumber.HasValue
                ? $"Sample contact #{contactNumber} created by Dataverse.Client demo at {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                : $"Sample contact created by Dataverse.Client demo at {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
        };

        return contact;
    }

    public static Entity CreateSampleAccount(int? accountNumber = null)
    {
        string companyName = Companies[_random.Next(Companies.Length)];
        string suffix = accountNumber?.ToString() ?? _random.Next(1000, 9999).ToString();

        Entity account = new("account")
        {
            ["name"] = $"{companyName} - {suffix}",
            ["telephone1"] = GeneratePhoneNumber(),
            ["websiteurl"] = $"https://www.{companyName.Replace(" ", "").Replace("&", "and").ToLower()}.com",
            ["description"] = accountNumber.HasValue
                ? $"Sample account #{accountNumber} created by Dataverse.Client demo at {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                : $"Sample account created by Dataverse.Client demo at {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            ["numberofemployees"] = _random.Next(10, 1000)
        };

        return account;
    }

    public static List<Entity> CreateSampleContacts(int count, bool useSequentialNumbering = true)
    {
        List<Entity> contacts = new(count);
        for (int i = 0; i < count; i++)
        {
            int? contactNumber = useSequentialNumbering ? i + 1 : null;
            contacts.Add(CreateSampleContact(contactNumber));
        }

        return contacts;
    }

    public static List<Entity> CreateSampleAccounts(int count, bool useSequentialNumbering = true)
    {
        List<Entity> accounts = new(count);
        for (int i = 0; i < count; i++)
        {
            int? accountNumber = useSequentialNumbering ? i + 1 : null;
            accounts.Add(CreateSampleAccount(accountNumber));
        }

        return accounts;
    }

    #endregion

    #region Helper Methods for Enhanced Data Generation

    /// <summary>
    /// Generates a realistic email address with optional numbering.
    /// </summary>
    private static string GenerateEmailAddress(string firstName, string lastName, int? number = null)
    {
        string[] domains = ["example.com", "contoso.com", "fabrikam.com", "adventure-works.com", "northwind.com"];
        string domain = domains[_random.Next(domains.Length)];

        string localPart = $"{firstName.ToLower()}.{lastName.ToLower()}";
        if (number.HasValue) localPart += $"{number}";

        return $"{localPart}@{domain}";
    }

    /// <summary>
    /// Generates a realistic phone number with various formats.
    /// </summary>
    private static string GeneratePhoneNumber()
    {
        string[] formats =
        [
            "+1-555-{0}",
            "(555) {0}",
            "555-{0}",
            "+1 555 {0}",
            "1-555-{0}"
        ];

        string format = formats[_random.Next(formats.Length)];
        string number = $"{_random.Next(1000, 9999)}";

        return string.Format(format, number);
    }

    /// <summary>
    /// Generates a description with optional record numbering.
    /// </summary>
    private static string GenerateDescription(int? recordNumber = null)
    {
        string[] templates =
        [
            "Sample test record created by Dataverse.Client demo at {0}",
            "Demo record for testing batch operations - created {0}",
            "Test data entry generated automatically on {0}",
            "Batch operation test record - timestamp: {0}",
            "Generated test data for Dataverse client validation - {0}"
        ];

        string template = templates[_random.Next(templates.Length)];
        string baseDescription = string.Format(template, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        if (recordNumber.HasValue) baseDescription = $"Record #{recordNumber}: {baseDescription}";

        return baseDescription;
    }

    /// <summary>
    /// Adds test variations to records for robustness testing.
    /// </summary>
    private static void AddTestVariations(Entity record, int index, int totalRecords)
    {
        // Add edge cases for testing
        if (index % 100 == 0) // Every 100th record
            // Test with longer names
            record[$"{TestTableLogicalName}_name"] += " - Extended Name for Testing";

        if (index % 250 == 0) // Every 250th record
            // Test with special characters
            record[$"{TestTableLogicalName}_description"] += " | Special chars: àáâãäåæçèéêë";

        if (index % 500 == 0) // Every 500th record
            // Test with maximum age
            record[$"{TestTableLogicalName}_age"] = 150;

        // Add batch boundary markers
        if (index == 0)
            record[$"{TestTableLogicalName}_description"] += " [FIRST RECORD]";
        else if (index == totalRecords - 1) record[$"{TestTableLogicalName}_description"] += " [LAST RECORD]";
    }

    private static string GetRandomJobTitle()
    {
        string[] jobTitles =
        [
            "Software Engineer", "Product Manager", "Sales Representative", "Marketing Specialist",
            "Business Analyst", "Project Manager", "Data Scientist", "UX Designer",
            "Customer Success Manager", "Operations Manager", "DevOps Engineer", "Quality Assurance Engineer",
            "Technical Writer", "Solutions Architect", "Database Administrator", "Network Administrator",
            "Cybersecurity Analyst", "Financial Analyst", "Human Resources Manager", "Legal Counsel",
            "Executive Assistant", "Account Executive", "Marketing Director", "Chief Technology Officer",
            "Software Architect", "Mobile Developer", "Frontend Developer", "Backend Developer",
            "Full Stack Developer", "Machine Learning Engineer", "Cloud Engineer", "Site Reliability Engineer"
        ];
        return jobTitles[_random.Next(jobTitles.Length)];
    }

    #endregion

    #region Enhanced Helper Methods for Batch Operations

    /// <summary>
    /// Creates a complete test environment with enhanced batch testing capabilities.
    /// </summary>
    /// <param name="metadataClient">The metadata client for table operations</param>
    /// <param name="dataClient">The data client for record operations</param>
    /// <param name="recordCount">Number of sample records to create</param>
    /// <param name="useBatchOperations">Whether to use batch operations for record creation</param>
    /// <param name="batchSize">Batch size for batch operations</param>
    /// <returns>List of created record IDs</returns>
    public static async Task<List<Guid>> CreateCompleteTestEnvironmentAsync(
        IDataverseMetadataClient metadataClient,
        IDataverseClient dataClient,
        int recordCount = 5,
        bool useBatchOperations = false,
        int batchSize = 100)
    {
        ArgumentNullException.ThrowIfNull(metadataClient);
        ArgumentNullException.ThrowIfNull(dataClient);

        // Create table
        await CreateTestTableAsync(metadataClient);

        // Create sample records
        List<Guid> createdIds = [];

        if (useBatchOperations && recordCount > 1)
        {
            Console.WriteLine($"Creating {recordCount} records using batch operations...");
            List<Entity> sampleRecords = CreateBatchTestRecords(recordCount, batchSize, true);

            BatchOperationResult result = await dataClient.CreateBatchAsync(sampleRecords,
                new BatchConfiguration { BatchSize = batchSize, EnableProgressReporting = recordCount > 100 });

            createdIds.AddRange(result.CreatedRecords.Select(er => er.Id));

            Console.WriteLine(
                $"? Batch creation completed: {result.CreatedRecords.Count} successful, {result.TotalRecords - result.CreatedRecords.Count} failed");
        }
        else
        {
            Console.WriteLine($"Creating {recordCount} records using individual operations...");
            List<Entity> sampleRecords = CreateSampleTestRecords(recordCount);

            foreach (Entity record in sampleRecords)
            {
                Guid id = await dataClient.CreateAsync(record);
                createdIds.Add(id);
            }
        }

        Console.WriteLine($"? Created complete test environment with {createdIds.Count} records");
        return createdIds;
    }

    /// <summary>
    /// Cleans up the complete test environment with enhanced batch deletion capabilities.
    /// </summary>
    /// <param name="metadataClient">The metadata client for table operations</param>
    /// <param name="dataClient">The data client for record operations</param>
    /// <param name="recordIds">Optional list of specific record IDs to delete</param>
    /// <param name="useBatchOperations">Whether to use batch operations for record deletion</param>
    /// <param name="batchSize">Batch size for batch operations</param>
    public static async Task CleanupTestEnvironmentAsync(
        IDataverseMetadataClient metadataClient,
        IDataverseClient dataClient,
        List<Guid>? recordIds = null,
        bool useBatchOperations = false,
        int batchSize = 100)
    {
        ArgumentNullException.ThrowIfNull(metadataClient);
        ArgumentNullException.ThrowIfNull(dataClient);

        try
        {
            // Delete specific records if provided
            if (recordIds?.Count > 0)
            {
                if (useBatchOperations && recordIds.Count > 1)
                {
                    Console.WriteLine($"Deleting {recordIds.Count} records using batch operations...");
                    List<EntityReference> entityRefs =
                        [.. recordIds.Select(id => new EntityReference(TestTableLogicalName, id))];

                    BatchOperationResult result = await dataClient.DeleteBatchAsync(entityRefs,
                        new BatchConfiguration
                        {
                            BatchSize = batchSize,
                            EnableProgressReporting = recordIds.Count > 100
                        });

                    Console.WriteLine(
                        $"? Batch deletion completed: {result.DeletedRecords.Count} successful, {entityRefs.Count - result.DeletedRecords.Count} failed");
                }
                else
                {
                    Console.WriteLine($"Deleting {recordIds.Count} records using individual operations...");
                    foreach (Guid id in recordIds)
                    {
                        try
                        {
                            await dataClient.DeleteAsync(TestTableLogicalName, id);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"? Failed to delete record {id}: {ex.Message}");
                        }
                    }
                }
            }

            // Delete the table (this will also delete all remaining records)
            await DeleteTestTableAsync(metadataClient);

            Console.WriteLine("? Test environment cleanup completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Failed to cleanup test environment: {ex.Message}");
            throw;
        }
    }

    #endregion
}
