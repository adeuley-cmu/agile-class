namespace AgileClass.Services;

public sealed class ResourceLoadingState
{
    public bool IsLoading { get; private set; } = true;

    public void Complete() => IsLoading = false;
}
