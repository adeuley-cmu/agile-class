namespace AgileClass.Models;

public sealed class Account
{
    public Guid Id { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
