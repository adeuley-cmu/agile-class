namespace AgileClass.Models;

public sealed class ResourceLocation
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string FullAddress { get; init; }
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }

    public string DisplayAddress => string.Join(", ", new[] { FullAddress, City, State, PostalCode }.Where(value => !string.IsNullOrWhiteSpace(value)));
}
