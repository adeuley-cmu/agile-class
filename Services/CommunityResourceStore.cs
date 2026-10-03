using AgileClass.Models;
using SQLite;

namespace AgileClass.Services;

public interface ICommunityResourceStore
{
    Task<IReadOnlyList<CommunityResource>> GetAllAsync();
    Task SaveAsync(CommunityResource resource, Guid ownerAccountId);
    Task DeleteAsync(Guid resourceId, Guid ownerAccountId);
}

public sealed class CommunityResourceStore : ICommunityResourceStore
{
    private readonly SQLiteAsyncConnection database;
    private readonly Task initialization;

    public CommunityResourceStore(LocalDatabase localDatabase)
    {
        database = localDatabase.Connection;
        initialization = InitializeAsync();
    }

    public async Task<IReadOnlyList<CommunityResource>> GetAllAsync()
    {
        await initialization;
        var resources = await database.Table<StoredResource>().OrderBy(item => item.Name).ToListAsync();
        var locations = await database.Table<StoredLocation>().ToListAsync();
        var grouped = locations.GroupBy(item => item.ResourceId).ToDictionary(group => group.Key, group => group.ToArray());
        return resources.Select(resource => new CommunityResource
        {
            Id = Guid.Parse(resource.Id),
            OwnerAccountId = ParseGuid(resource.OwnerAccountId),
            Name = resource.Name,
            Category = resource.Category,
            Description = resource.Description,
            Phone = resource.Phone,
            Website = resource.Website,
            IsVerified = resource.IsVerified,
            ServesFamilies = resource.ServesFamilies,
            ServesStudents = resource.ServesStudents,
            ServesSeniors = resource.ServesSeniors,
            ServesUnder25 = resource.ServesUnder25,
            MinimumHouseholdSize = resource.MinimumHouseholdSize,
            MaximumHouseholdSize = resource.MaximumHouseholdSize,
            CreatedAt = new DateTimeOffset(resource.CreatedAtTicks, TimeSpan.Zero),
            Locations = (grouped.GetValueOrDefault(resource.Id) ?? []).Select(location => new ResourceLocation
            {
                Id = Guid.Parse(location.Id), FullAddress = location.FullAddress,
                City = location.City, State = location.State, PostalCode = location.PostalCode,
                Latitude = location.Latitude, Longitude = location.Longitude
            }).ToArray()
        }).ToArray();
    }

    public async Task SaveAsync(CommunityResource resource, Guid ownerAccountId)
    {
        if (ownerAccountId == Guid.Empty) throw new UnauthorizedAccessException("A signed-in account is required to save a resource.");
        await initialization;
        var existing = await database.Table<StoredResource>().Where(item => item.Id == resource.Id.ToString()).FirstOrDefaultAsync();
        if (existing is not null && ParseGuid(existing.OwnerAccountId) != ownerAccountId)
            throw new UnauthorizedAccessException("Only the resource owner can update this resource.");

        var id = resource.Id.ToString();
        var stored = new StoredResource
        {
            Id = id, OwnerAccountId = ownerAccountId.ToString(), Name = resource.Name, Category = resource.Category,
            Description = resource.Description, Phone = resource.Phone, Website = resource.Website,
            IsVerified = resource.IsVerified, ServesFamilies = resource.ServesFamilies,
            ServesStudents = resource.ServesStudents, ServesSeniors = resource.ServesSeniors,
            ServesUnder25 = resource.ServesUnder25, MinimumHouseholdSize = resource.MinimumHouseholdSize,
            MaximumHouseholdSize = resource.MaximumHouseholdSize, CreatedAtTicks = resource.CreatedAt.UtcTicks
        };

        await database.RunInTransactionAsync(connection =>
        {
            connection.InsertOrReplace(stored);
            connection.Execute("DELETE FROM StoredLocation WHERE ResourceId = ?", id);
            foreach (var location in resource.Locations)
                connection.Insert(new StoredLocation { Id = location.Id.ToString(), ResourceId = id, FullAddress = location.FullAddress, City = location.City, State = location.State, PostalCode = location.PostalCode, Latitude = location.Latitude, Longitude = location.Longitude });
        });
    }

    public async Task DeleteAsync(Guid resourceId, Guid ownerAccountId)
    {
        if (ownerAccountId == Guid.Empty) throw new UnauthorizedAccessException("A signed-in account is required to delete a resource.");
        await initialization;
        var id = resourceId.ToString();
        var stored = await database.Table<StoredResource>().Where(item => item.Id == id).FirstOrDefaultAsync();
        if (stored is null) return;
        if (ParseGuid(stored.OwnerAccountId) != ownerAccountId) throw new UnauthorizedAccessException("Only the resource owner can delete this resource.");
        await database.RunInTransactionAsync(connection =>
        {
            connection.Execute("DELETE FROM StoredLocation WHERE ResourceId = ?", id);
            connection.Execute("DELETE FROM StoredResource WHERE Id = ?", id);
        });
    }

    private async Task InitializeAsync()
    {
        await database.CreateTableAsync<StoredResource>();
        await database.CreateTableAsync<StoredLocation>();
        await AddColumnIfMissingAsync("StoredResource", "OwnerAccountId", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync("StoredResource", "ServesFamilies", "INTEGER NOT NULL DEFAULT 0");
        await AddColumnIfMissingAsync("StoredResource", "ServesStudents", "INTEGER NOT NULL DEFAULT 0");
        await AddColumnIfMissingAsync("StoredResource", "ServesSeniors", "INTEGER NOT NULL DEFAULT 0");
        await AddColumnIfMissingAsync("StoredResource", "ServesUnder25", "INTEGER NOT NULL DEFAULT 0");
        await AddColumnIfMissingAsync("StoredResource", "MinimumHouseholdSize", "INTEGER");
        await AddColumnIfMissingAsync("StoredResource", "MaximumHouseholdSize", "INTEGER");
        await AddColumnIfMissingAsync("StoredLocation", "City", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync("StoredLocation", "State", "TEXT NOT NULL DEFAULT ''");
        await AddColumnIfMissingAsync("StoredLocation", "PostalCode", "TEXT NOT NULL DEFAULT ''");
    }

    private async Task AddColumnIfMissingAsync(string tableName, string columnName, string definition)
    {
        var columns = await database.QueryAsync<TableColumn>($"PRAGMA table_info({tableName})");
        if (!columns.Any(column => string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase)))
            await database.ExecuteAsync($"ALTER TABLE {tableName} ADD COLUMN {columnName} {definition}");
    }

    private static Guid? ParseGuid(string value) => Guid.TryParse(value, out var id) ? id : null;

    private sealed class TableColumn
    {
        [Column("name")] public string Name { get; set; } = string.Empty;
    }

    [Table("StoredResource")]
    private sealed class StoredResource
    {
        [PrimaryKey] public string Id { get; set; } = string.Empty;
        public string OwnerAccountId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public bool ServesFamilies { get; set; }
        public bool ServesStudents { get; set; }
        public bool ServesSeniors { get; set; }
        public bool ServesUnder25 { get; set; }
        public int? MinimumHouseholdSize { get; set; }
        public int? MaximumHouseholdSize { get; set; }
        public long CreatedAtTicks { get; set; }
    }

    [Table("StoredLocation")]
    private sealed class StoredLocation
    {
        [PrimaryKey] public string Id { get; set; } = string.Empty;
        public string ResourceId { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
