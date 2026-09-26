using AgileClass.Models;
using Xunit;

namespace AgileClass.Tests;

public sealed class ResourceModelTests
{
    [Fact]
    public void Resource_preserves_multiple_locations_and_contact_details()
    {
        var locations = new[]
        {
            new ResourceLocation { FullAddress = "100 Main Street" },
            new ResourceLocation { FullAddress = "200 Oak Avenue" }
        };
        var resource = new CommunityResource
        {
            Id = Guid.NewGuid(),
            Name = "Community Food Hub",
            Category = "Food",
            Description = "Weekly groceries and meals.",
            Phone = "555-0100",
            Website = "https://example.org",
            Locations = locations,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Assert.Equal("Community Food Hub", resource.Name);
        Assert.Equal("Food", resource.Category);
        Assert.Equal(2, resource.Locations.Count);
        Assert.Contains(resource.Locations, location => location.FullAddress == "200 Oak Avenue");
        Assert.Equal("555-0100", resource.Phone);
    }

    [Fact]
    public void Resource_location_defaults_are_safe_for_new_entries()
    {
        var location = new ResourceLocation { FullAddress = string.Empty };

        Assert.NotEqual(Guid.Empty, location.Id);
        Assert.Equal(string.Empty, location.FullAddress);
        Assert.Null(location.Latitude);
        Assert.Null(location.Longitude);
    }
}
