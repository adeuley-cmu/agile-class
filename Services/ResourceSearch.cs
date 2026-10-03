using AgileClass.Models;

namespace AgileClass.Services;

public sealed class ResourceFilter
{
    public string City { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public double? CenterLatitude { get; init; }
    public double? CenterLongitude { get; init; }
    public double? RadiusMiles { get; init; }
    public bool Families { get; init; }
    public bool Students { get; init; }
    public bool Seniors { get; init; }
    public bool Under25 { get; init; }
    public int? HouseholdSize { get; init; }
}

public sealed record ResourceSearchResult(CommunityResource Resource, double? DistanceMiles);

public static class ResourceSearch
{
    public static IReadOnlyList<ResourceSearchResult> Apply(IEnumerable<CommunityResource> resources, ResourceFilter filter)
    {
        var results = resources
            .Select(resource => new ResourceSearchResult(resource, DistanceToResource(resource, filter.CenterLatitude, filter.CenterLongitude)))
            .Where(result => Matches(result, filter))
            .OrderBy(result => result.DistanceMiles.HasValue ? 0 : 1)
            .ThenBy(result => result.DistanceMiles)
            .ThenBy(result => result.Resource.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return results;
    }

    public static double? DistanceMiles(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        const double earthRadiusMiles = 3958.7613;
        var latitudeDelta = DegreesToRadians(latitude2 - latitude1);
        var longitudeDelta = DegreesToRadians(longitude2 - longitude1);
        var latitude1Radians = DegreesToRadians(latitude1);
        var latitude2Radians = DegreesToRadians(latitude2);
        var haversine = Math.Pow(Math.Sin(latitudeDelta / 2), 2) +
            Math.Cos(latitude1Radians) * Math.Cos(latitude2Radians) * Math.Pow(Math.Sin(longitudeDelta / 2), 2);
        return earthRadiusMiles * 2 * Math.Asin(Math.Sqrt(haversine));
    }

    private static bool Matches(ResourceSearchResult result, ResourceFilter filter)
    {
        var resource = result.Resource;
        if (!string.IsNullOrWhiteSpace(filter.City) && !resource.Locations.Any(location => string.Equals(location.City.Trim(), filter.City.Trim(), StringComparison.OrdinalIgnoreCase))) return false;
        if (filter.RadiusMiles is null && !string.IsNullOrWhiteSpace(filter.PostalCode) && !resource.Locations.Any(location => NormalizePostalCode(location.PostalCode) == NormalizePostalCode(filter.PostalCode))) return false;
        if (filter.Families && !resource.ServesFamilies) return false;
        if (filter.Students && !resource.ServesStudents) return false;
        if (filter.Seniors && !resource.ServesSeniors) return false;
        if (filter.Under25 && !resource.ServesUnder25) return false;
        if (filter.HouseholdSize is int householdSize && ((resource.MinimumHouseholdSize is int minimum && householdSize < minimum) || (resource.MaximumHouseholdSize is int maximum && householdSize > maximum))) return false;
        if (filter.RadiusMiles is double radius && (result.DistanceMiles is null || result.DistanceMiles > radius)) return false;
        return true;
    }

    private static double? DistanceToResource(CommunityResource resource, double? centerLatitude, double? centerLongitude)
    {
        if (centerLatitude is not double latitude || centerLongitude is not double longitude) return null;
        return resource.Locations
            .Where(location => location.Latitude is not null && location.Longitude is not null)
            .Select(location => DistanceMiles(latitude, longitude, location.Latitude!.Value, location.Longitude!.Value))
            .MinOrDefault();
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

    private static string NormalizePostalCode(string postalCode) => new string(postalCode.Where(char.IsLetterOrDigit).Take(5).ToArray()).ToUpperInvariant();

    private static double? MinOrDefault(this IEnumerable<double?> values)
    {
        var numbers = values.Where(value => value is not null).Select(value => value!.Value).ToArray();
        return numbers.Length == 0 ? null : numbers.Min();
    }
}
