using TaskPilot.Models;

namespace TaskPilot.Services;

public static class TaskValidator
{
    public static bool TryValidate(TaskItem task, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(task.Title))
        {
            error = "Tytuł zadania nie może być pusty.";
            return false;
        }

        if (task.Category.Length > 80)
        {
            error = "Kategoria nie może przekraczać 80 znaków.";
            return false;
        }

        if (!task.IsCompleted && task.DueDate.Date < DateTime.Today)
        {
            error = "Termin zadania nie może być wcześniejszy niż dzisiejsza data (dla zadań niewykonanych).";
            return false;
        }

        return true;
    }

    public static bool TryValidateAll(IEnumerable<TaskItem> tasks, out string? error)
    {
        foreach (var t in tasks)
        {
            if (!TryValidate(t, out error))
                return false;
        }

        error = null;
        return true;
    }
}
