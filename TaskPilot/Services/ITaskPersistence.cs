using TaskPilot.Models;

namespace TaskPilot.Services;

public interface ITaskPersistence
{
    IReadOnlyList<TaskItem> Load();

    void Save(IEnumerable<TaskItem> tasks);
}
