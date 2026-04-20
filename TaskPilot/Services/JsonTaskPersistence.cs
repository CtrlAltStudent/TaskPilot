using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskPilot.Models;

namespace TaskPilot.Services;

public sealed class JsonTaskPersistence : ITaskPersistence
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public JsonTaskPersistence(string filePath)
    {
        _filePath = filePath;
    }

    public IReadOnlyList<TaskItem> Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return Array.Empty<TaskItem>();

            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
                return Array.Empty<TaskItem>();

            var doc = JsonSerializer.Deserialize<TasksFileDto>(json, JsonOptions);
            if (doc?.Tasks is null || doc.Tasks.Count == 0)
                return Array.Empty<TaskItem>();

            var list = new List<TaskItem>();
            foreach (var dto in doc.Tasks)
            {
                list.Add(new TaskItem
                {
                    Id = dto.Id,
                    Title = dto.Title ?? string.Empty,
                    Description = dto.Description ?? string.Empty,
                    Category = dto.Category ?? string.Empty,
                    DueDate = dto.DueDate == default ? DateTime.Today : dto.DueDate.Date,
                    Priority = dto.Priority,
                    IsCompleted = dto.IsCompleted
                });
            }

            return list;
        }
        catch (JsonException ex)
        {
            System.Windows.MessageBox.Show(
                $"Nie udało się wczytać pliku zadań (niepoprawny JSON).\nSzczegóły: {ex.Message}\n\nZostanie użyta pusta lista.",
                "TaskPilot — błąd odczytu",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return Array.Empty<TaskItem>();
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
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var dto = new TasksFileDto
        {
            Tasks = tasks.Select(t => new TaskItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Category = t.Category,
                DueDate = t.DueDate.Date,
                Priority = t.Priority,
                IsCompleted = t.IsCompleted
            }).ToList()
        };

        var json = JsonSerializer.Serialize(dto, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private sealed class TasksFileDto
    {
        public List<TaskItemDto>? Tasks { get; set; }
    }

    private sealed class TaskItemDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public DateTime DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public bool IsCompleted { get; set; }
    }
}
