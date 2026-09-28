using AgileClass.Services;
using Xunit;

namespace AgileClass.Tests;

public sealed class ResourceLoadingStateTests
{
    [Fact]
    public void Loading_state_starts_pending_and_completes_after_load()
    {
        var state = new ResourceLoadingState();

        Assert.True(state.IsLoading);

        state.Complete();

        Assert.False(state.IsLoading);
    }
}
