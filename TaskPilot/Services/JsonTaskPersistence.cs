using System.IO;
using TaskPilot.Models;

namespace TaskPilot.Services;

public sealed class JsonTaskPersistence : ITaskPersistence
{
    private readonly string _filePath;

    public JsonTaskPersistence(string filePath)
    {
        _filePath = filePath;
    }

    public string StorageDescription => $"Plik JSON: {_filePath}";

    public IReadOnlyList<TaskItem> Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return Array.Empty<TaskItem>();

            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
                return Array.Empty<TaskItem>();

            if (!TaskListJsonSerializer.TryDeserialize(json, out var tasks, out var err))
            {
                System.Windows.MessageBox.Show(
                    $"Nie udało się wczytać pliku zadań.\n{err}\n\nZostanie użyta pusta lista.",
                    "TaskPilot — błąd odczytu",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return Array.Empty<TaskItem>();
            }

            return tasks;
        }
        catch (IOException ex)
        {
            System.Windows.MessageBox.Show(
                $"Błąd odczytu pliku.\n{ex.Message}",
                "TaskPilot",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return Array.Empty<TaskItem>();
        }
    }

    public void Save(IEnumerable<TaskItem> tasks)
    {
        if (!TaskListJsonSerializer.TryWriteFile(_filePath, tasks, out var err))
            throw new IOException(err ?? "Nie udało się zapisać pliku.");
    }
}
