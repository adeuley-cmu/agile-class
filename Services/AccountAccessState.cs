namespace AgileClass.Services;

public sealed class AccountAccessState
{
    public bool IsRegistration { get; private set; }
    public bool IsSubmitting { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    public void SetRouteMode(bool isRegistration)
    {
        if (IsRegistration != isRegistration) ResetForm();
        IsRegistration = isRegistration;
    }

    public void ResetForm()
    {
        DisplayName = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        ErrorMessage = string.Empty;
        IsSubmitting = false;
    }
}
