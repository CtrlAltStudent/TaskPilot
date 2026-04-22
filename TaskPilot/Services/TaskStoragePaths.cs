using System.IO;

namespace TaskPilot.Services;

public static class TaskStoragePaths
{
    public static string DefaultFilePath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TaskPilot",
            "tasks.json");

    public static string DefaultSqlitePath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TaskPilot",
            "tasks.db");
}
