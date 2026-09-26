using AgileClass.Models;
using Xunit;

namespace AgileClass.Tests;

public sealed class AccountModelTests
{
    [Fact]
    public void Account_stores_identity_and_creation_metadata()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);
        var account = new Account
        {
            Id = id,
            Email = "andrew@example.org",
            DisplayName = "Andrew",
            CreatedAt = createdAt
        };

        Assert.Equal(id, account.Id);
        Assert.Equal("andrew@example.org", account.Email);
        Assert.Equal("Andrew", account.DisplayName);
        Assert.Equal(createdAt, account.CreatedAt);
    }
}
