using TaskPilot.ViewModels;

namespace TaskPilot.Models;

public sealed class TaskItem : ObservableObject
{
    private int _id;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private DateTime _dueDate = DateTime.Today;
    private TaskPriority _priority = TaskPriority.Medium;
    private bool _isCompleted;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public DateTime DueDate
    {
        get => _dueDate;
        set => SetProperty(ref _dueDate, value);
    }

    public TaskPriority Priority
    {
        get => _priority;
        set => SetProperty(ref _priority, value);
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set => SetProperty(ref _isCompleted, value);
    }
}
