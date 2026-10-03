using AgileClass.Models;
using Microsoft.Maui.ApplicationModel;

namespace AgileClass.Services;

public interface IMapNavigationService
{
    Task OpenDirectionsAsync(ResourceLocation location);
}

public sealed class MapNavigationService : IMapNavigationService
{
    public Task OpenDirectionsAsync(ResourceLocation location) => Launcher.Default.OpenAsync(MapUrlBuilder.BuildDirectionsUrl(location));
}
