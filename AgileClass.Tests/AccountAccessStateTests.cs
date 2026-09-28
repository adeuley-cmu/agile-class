using AgileClass.Services;
using Xunit;

namespace AgileClass.Tests;

public sealed class AccountAccessStateTests
{
    [Fact]
    public void Switching_route_mode_clears_previous_form_state()
    {
        var state = new AccountAccessState();
        state.SetRouteMode(true);
        state.DisplayName = "Andrew";
        state.Email = "andrew@example.org";
        state.Password = "registration-password";
        state.ConfirmPassword = "registration-password";
        state.ErrorMessage = "Passwords do not match.";
        state.IsSubmitting = true;

        state.SetRouteMode(false);

        Assert.False(state.IsRegistration);
        Assert.False(state.IsSubmitting);
        Assert.Equal(string.Empty, state.DisplayName);
        Assert.Equal(string.Empty, state.Email);
        Assert.Equal(string.Empty, state.Password);
        Assert.Equal(string.Empty, state.ConfirmPassword);
        Assert.Equal(string.Empty, state.ErrorMessage);
    }

    [Fact]
    public void Staying_on_same_route_preserves_form_state()
    {
        var state = new AccountAccessState();
        state.Email = "andrew@example.org";
        state.ErrorMessage = "The email address or password is incorrect.";

        state.SetRouteMode(false);

        Assert.Equal("andrew@example.org", state.Email);
        Assert.Equal("The email address or password is incorrect.", state.ErrorMessage);
    }
}
