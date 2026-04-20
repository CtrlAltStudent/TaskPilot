using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskPilot.Models;

namespace TaskPilot.Services;

/// <summary>
/// Wspólny format pliku JSON (domyślny zapis i import/eksport).
/// </summary>
public static class TaskListJsonSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static string Serialize(IEnumerable<TaskItem> tasks)
    {
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

        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    public static bool TryDeserialize(string json, out List<TaskItem> tasks, out string? error)
    {
        tasks = new List<TaskItem>();
        error = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            error = "Plik jest pusty.";
            return false;
        }

        try
        {
            var doc = JsonSerializer.Deserialize<TasksFileDto>(json, JsonOptions);
            if (doc?.Tasks is null || doc.Tasks.Count == 0)
            {
                error = "Brak tablicy \"tasks\" lub jest pusta.";
                return false;
            }

            foreach (var dto in doc.Tasks)
            {
                tasks.Add(new TaskItem
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

            return true;
        }
        catch (JsonException ex)
        {
            error = $"Niepoprawny JSON: {ex.Message}";
            return false;
        }
    }

    public static bool TryReadFile(string path, out List<TaskItem> tasks, out string? error)
    {
        tasks = new List<TaskItem>();
        error = null;

        try
        {
            if (!File.Exists(path))
            {
                error = "Plik nie istnieje.";
                return false;
            }

            var json = File.ReadAllText(path);
            return TryDeserialize(json, out tasks, out error);
        }
        catch (IOException ex)
        {
            error = ex.Message;
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static bool TryWriteFile(string path, IEnumerable<TaskItem> tasks, out string? error)
    {
        error = null;
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var json = Serialize(tasks);
            File.WriteAllText(path, json);
            return true;
        }
        catch (IOException ex)
        {
            error = ex.Message;
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            error = ex.Message;
            return false;
        }
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
