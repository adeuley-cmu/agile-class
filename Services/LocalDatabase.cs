using SQLite;

namespace AgileClass.Services;

public sealed class LocalDatabase
{
    public LocalDatabase()
    {
        Connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "agileclass.db3"));
    }

    public SQLiteAsyncConnection Connection { get; }
}
