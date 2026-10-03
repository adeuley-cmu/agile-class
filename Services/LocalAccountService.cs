using System.Security.Cryptography;
using AgileClass.Models;
using Microsoft.Maui.Storage;
using SQLite;

namespace AgileClass.Services;

public interface ILocalAccountService
{
    Account? CurrentAccount { get; }
    bool IsSignedIn { get; }
    event Action? AuthenticationStateChanged;
    Task EnsureInitializedAsync();
    Task<(bool Succeeded, string Error)> RegisterAsync(string email, string displayName, string password);
    Task<bool> SignInAsync(string email, string password);
    Task<(bool Succeeded, string Error)> UpdateAccountAsync(string email, string displayName, string? newPassword);
    Task<(bool Succeeded, string Error)> DeleteAccountAsync(string password);
    void SignOut();
}

public sealed class LocalAccountService : ILocalAccountService
{
    private const string CurrentAccountPreference = "current-account-id";
    private const int PasswordIterations = 100_000;
    private readonly SQLiteAsyncConnection database;
    private readonly Task initialization;
    private Account? currentAccount;

    public LocalAccountService(LocalDatabase localDatabase)
    {
        database = localDatabase.Connection;
        initialization = InitializeAsync();
    }

    public Account? CurrentAccount => currentAccount;
    public bool IsSignedIn => currentAccount is not null;
    public event Action? AuthenticationStateChanged;
    public Task EnsureInitializedAsync() => initialization;

    public async Task<(bool Succeeded, string Error)> RegisterAsync(string email, string displayName, string password)
    {
        await initialization;
        var normalizedEmail = NormalizeEmail(email);
        var name = displayName.Trim();
        var validation = Validate(normalizedEmail, name, password, true);
        if (validation is not null) return (false, validation);
        if (await database.Table<StoredAccount>().Where(item => item.Email == normalizedEmail).FirstOrDefaultAsync() is not null)
            return (false, "An account with that email already exists.");
        var stored = new StoredAccount { Id = Guid.NewGuid().ToString(), Email = normalizedEmail, DisplayName = name, PasswordSalt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)), CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks };
        stored.PasswordHash = HashPassword(password, stored.PasswordSalt);
        await database.InsertAsync(stored);
        SetCurrentAccount(ToModel(stored));
        return (true, string.Empty);
    }

    public async Task<bool> SignInAsync(string email, string password)
    {
        await initialization;
        var normalizedEmail = NormalizeEmail(email);
        var stored = await database.Table<StoredAccount>().Where(item => item.Email == normalizedEmail).FirstOrDefaultAsync();
        if (stored is null || !CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(stored.PasswordHash), Convert.FromBase64String(HashPassword(password, stored.PasswordSalt)))) return false;
        SetCurrentAccount(ToModel(stored));
        return true;
    }

    public async Task<(bool Succeeded, string Error)> UpdateAccountAsync(string email, string displayName, string? newPassword)
    {
        await initialization;
        if (currentAccount is null) return (false, "You must be signed in to edit your account.");
        var normalizedEmail = NormalizeEmail(email);
        var name = displayName.Trim();
        var currentAccountId = currentAccount.Id.ToString();
        var validation = Validate(normalizedEmail, name, newPassword, false);
        if (validation is not null) return (false, validation);
        if (await database.Table<StoredAccount>().Where(item => item.Email == normalizedEmail && item.Id != currentAccountId).FirstOrDefaultAsync() is not null) return (false, "An account with that email already exists.");
        var stored = await database.Table<StoredAccount>().Where(item => item.Id == currentAccountId).FirstOrDefaultAsync();
        if (stored is null) { SignOut(); return (false, "Your account could not be found."); }
        stored.Email = normalizedEmail; stored.DisplayName = name;
        if (!string.IsNullOrWhiteSpace(newPassword)) { stored.PasswordSalt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)); stored.PasswordHash = HashPassword(newPassword, stored.PasswordSalt); }
        await database.UpdateAsync(stored);
        SetCurrentAccount(ToModel(stored));
        return (true, string.Empty);
    }

    public async Task<(bool Succeeded, string Error)> DeleteAccountAsync(string password)
    {
        await initialization;
        if (currentAccount is null) return (false, "You must be signed in to delete your account.");

        var accountId = currentAccount.Id.ToString();
        var stored = await database.Table<StoredAccount>().Where(item => item.Id == accountId).FirstOrDefaultAsync();
        if (stored is null) { SignOut(); return (false, "Your account could not be found."); }
        if (!PasswordMatches(password, stored)) return (false, "The password is incorrect.");

        await database.DeleteAsync(stored);
        currentAccount = null;
        Preferences.Remove(CurrentAccountPreference);
        AuthenticationStateChanged?.Invoke();
        return (true, string.Empty);
    }

    public void SignOut() { currentAccount = null; Preferences.Remove(CurrentAccountPreference); AuthenticationStateChanged?.Invoke(); }

    private async Task InitializeAsync()
    {
        await database.CreateTableAsync<StoredAccount>();
        if (!Guid.TryParse(Preferences.Get(CurrentAccountPreference, string.Empty), out var id)) return;
        var accountId = id.ToString();
        var stored = await database.Table<StoredAccount>().Where(item => item.Id == accountId).FirstOrDefaultAsync();
        if (stored is not null) currentAccount = ToModel(stored);
    }

    private void SetCurrentAccount(Account account) { currentAccount = account; Preferences.Set(CurrentAccountPreference, account.Id.ToString()); AuthenticationStateChanged?.Invoke(); }
    private static Account ToModel(StoredAccount stored) => new() { Id = Guid.Parse(stored.Id), Email = stored.Email, DisplayName = stored.DisplayName, CreatedAt = new DateTimeOffset(stored.CreatedAtTicks, TimeSpan.Zero) };
    private static string? Validate(string email, string name, string? password, bool required) { if (!email.Contains('@') || email.Length > 254) return "Enter a valid email address."; if (name.Length is < 2 or > 80) return "Display name must be between 2 and 80 characters."; if (required && string.IsNullOrWhiteSpace(password)) return "Password is required."; if (!string.IsNullOrWhiteSpace(password) && password.Length < 8) return "Password must be at least 8 characters."; return null; }
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    private static string HashPassword(string password, string salt) => Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(salt), PasswordIterations, HashAlgorithmName.SHA256, 32));
    private static bool PasswordMatches(string password, StoredAccount stored) => CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(stored.PasswordHash), Convert.FromBase64String(HashPassword(password, stored.PasswordSalt)));

    [Table("StoredAccount")]
    private sealed class StoredAccount
    {
        [PrimaryKey] public string Id { get; set; } = string.Empty;
        [Indexed(Unique = true)] public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public long CreatedAtTicks { get; set; }
    }
}
