namespace AgileClass.Models;

public sealed class ResourceLocation
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string FullAddress { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
}
