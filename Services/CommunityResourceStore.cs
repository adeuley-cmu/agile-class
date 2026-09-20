using AgileClass.Models;
using SQLite;

namespace AgileClass.Services;

public interface ICommunityResourceStore
{
    Task<IReadOnlyList<CommunityResource>> GetAllAsync();
    Task SaveAsync(CommunityResource resource);
    Task DeleteAsync(Guid resourceId);
}

public sealed class CommunityResourceStore : ICommunityResourceStore
{
    private readonly SQLiteAsyncConnection database;
    private readonly Task initialization;

    public CommunityResourceStore()
    {
        database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "agileclass.db3"));
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
            Id = Guid.Parse(resource.Id), Name = resource.Name, Category = resource.Category,
            Description = resource.Description, Phone = resource.Phone, Website = resource.Website,
            IsVerified = resource.IsVerified, CreatedAt = new DateTimeOffset(resource.CreatedAtTicks, TimeSpan.Zero),
            Locations = (grouped.GetValueOrDefault(resource.Id) ?? []).Select(location => new ResourceLocation
            {
                Id = Guid.Parse(location.Id), FullAddress = location.FullAddress,
                Latitude = location.Latitude, Longitude = location.Longitude
            }).ToArray()
        }).ToArray();
    }

    public async Task SaveAsync(CommunityResource resource)
    {
        await initialization;
        var stored = new StoredResource
        {
            Id = resource.Id.ToString(), Name = resource.Name, Category = resource.Category,
            Description = resource.Description, Phone = resource.Phone, Website = resource.Website,
            IsVerified = resource.IsVerified, CreatedAtTicks = resource.CreatedAt.UtcTicks
        };
        await database.InsertOrReplaceAsync(stored);
        await database.ExecuteAsync("DELETE FROM StoredLocation WHERE ResourceId = ?", stored.Id);
        foreach (var location in resource.Locations)
            await database.InsertAsync(new StoredLocation { Id = location.Id.ToString(), ResourceId = stored.Id, FullAddress = location.FullAddress, Latitude = location.Latitude, Longitude = location.Longitude });
    }

    public async Task DeleteAsync(Guid resourceId)
    {
        await initialization;
        var id = resourceId.ToString();
        await database.ExecuteAsync("DELETE FROM StoredLocation WHERE ResourceId = ?", id);
        await database.ExecuteAsync("DELETE FROM StoredResource WHERE Id = ?", id);
    }

    private async Task InitializeAsync()
    {
        await database.CreateTableAsync<StoredResource>();
        await database.CreateTableAsync<StoredLocation>();
    }

    [Table("StoredResource")]
    private sealed class StoredResource
    {
        [PrimaryKey] public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public long CreatedAtTicks { get; set; }
    }

    [Table("StoredLocation")]
    private sealed class StoredLocation
    {
        [PrimaryKey] public string Id { get; set; } = string.Empty;
        public string ResourceId { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
