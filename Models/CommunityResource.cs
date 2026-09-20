namespace AgileClass.Models;

public sealed class CommunityResource
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required IReadOnlyList<ResourceLocation> Locations { get; init; }
    public string Description { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Website { get; init; } = string.Empty;
    public bool IsVerified { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
