using AgileClass.Models;

namespace AgileClass.Services;

public static class MapUrlBuilder
{
    public static string BuildDirectionsUrl(ResourceLocation location)
    {
        var destination = location.Latitude is double latitude && location.Longitude is double longitude
            ? $"{latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
            : location.DisplayAddress;
        var encodedDestination = Uri.EscapeDataString(destination);
        return OperatingSystem.IsIOS() || OperatingSystem.IsMacCatalyst()
            ? $"http://maps.apple.com/?daddr={encodedDestination}"
            : $"https://www.google.com/maps/dir/?api=1&destination={encodedDestination}";
    }
}
