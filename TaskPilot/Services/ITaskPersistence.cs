using TaskPilot.Models;

namespace TaskPilot.Services;

public interface ITaskPersistence
{
    /// <summary>Krótki opis lokalizacji zapisu (ścieżka pliku JSON lub bazy SQLite).</summary>
    string StorageDescription { get; }

    IReadOnlyList<TaskItem> Load();

    void Save(IEnumerable<TaskItem> tasks);
}
