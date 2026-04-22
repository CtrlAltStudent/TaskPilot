namespace TaskPilot.Services;

/// <summary>
/// Wybór implementacji zapisu: domyślnie JSON; SQLite po ustawieniu zmiennej środowiskowej TASKPILOT_STORAGE=sqlite.
/// </summary>
public static class PersistenceResolver
{
    private const string StorageEnvVar = "TASKPILOT_STORAGE";

    public static ITaskPersistence CreateDefault()
    {
        var mode = Environment.GetEnvironmentVariable(StorageEnvVar);
        if (string.Equals(mode, "sqlite", StringComparison.OrdinalIgnoreCase))
            return new SqliteTaskPersistence(TaskStoragePaths.DefaultSqlitePath);

        return new JsonTaskPersistence(TaskStoragePaths.DefaultFilePath);
    }
}
