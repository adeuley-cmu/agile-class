using AgileClass.Models;
using AgileClass.Services;
using Xunit;

namespace AgileClass.Tests;

public sealed class ResourceSearchTests
{
    [Fact]
    public void Radius_filter_returns_nearest_resources_and_excludes_missing_coordinates()
    {
        var resources = new[]
        {
            CreateResource("Near", 40.000, -80.000),
            CreateResource("Far", 41.000, -80.000),
            CreateResource("Unknown")
        };

        var results = ResourceSearch.Apply(resources, new ResourceFilter
        {
            CenterLatitude = 40,
            CenterLongitude = -80,
            RadiusMiles = 20
        });

        var result = Assert.Single(results);
        Assert.Equal("Near", result.Resource.Name);
        Assert.Equal(0, result.DistanceMiles);
    }

    [Fact]
    public void City_and_audience_filters_are_combined()
    {
        var resources = new[]
        {
            CreateResource("Family help", city: "Pittsburgh", families: true),
            CreateResource("Student help", city: "Pittsburgh", students: true),
            CreateResource("Family help elsewhere", city: "Cleveland", families: true)
        };

        var results = ResourceSearch.Apply(resources, new ResourceFilter { City = "pittsburgh", Families = true });

        var result = Assert.Single(results);
        Assert.Equal("Family help", result.Resource.Name);
    }

    [Fact]
    public void Household_filter_honors_optional_minimum_and_maximum()
    {
        var resources = new[]
        {
            CreateResource("Small households", minimumHouseholdSize: 1, maximumHouseholdSize: 3),
            CreateResource("Large households", minimumHouseholdSize: 4)
        };

        var results = ResourceSearch.Apply(resources, new ResourceFilter { HouseholdSize = 2 });

        var result = Assert.Single(results);
        Assert.Equal("Small households", result.Resource.Name);
    }

    [Fact]
    public void ZIP_filter_matches_normalized_five_digit_codes_without_radius()
    {
        var resources = new[]
        {
            CreateResource("Matching ZIP", postalCode: "15213-1234"),
            CreateResource("Different ZIP", postalCode: "15222")
        };

        var results = ResourceSearch.Apply(resources, new ResourceFilter { PostalCode = "15213" });

        var result = Assert.Single(results);
        Assert.Equal("Matching ZIP", result.Resource.Name);
    }

    [Fact]
    public void Map_url_prefers_coordinates_when_available()
    {
        var location = new ResourceLocation { FullAddress = "100 Main Street", Latitude = 40.5, Longitude = -80.25 };

        var url = MapUrlBuilder.BuildDirectionsUrl(location);

        Assert.Contains("40.5%2C-80.25", url);
    }

    private static CommunityResource CreateResource(
        string name,
        double? latitude = null,
        double? longitude = null,
        string city = "",
        string postalCode = "",
        bool families = false,
        bool students = false,
        int? minimumHouseholdSize = null,
        int? maximumHouseholdSize = null) => new()
        {
            Name = name,
            Category = "Other",
            Locations = [new ResourceLocation { FullAddress = name, City = city, PostalCode = postalCode, Latitude = latitude, Longitude = longitude }],
            ServesFamilies = families,
            ServesStudents = students,
            MinimumHouseholdSize = minimumHouseholdSize,
            MaximumHouseholdSize = maximumHouseholdSize
        };
}
