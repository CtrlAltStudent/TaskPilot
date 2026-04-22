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
    private string _category = string.Empty;
    private string _assignedTo = string.Empty;
    private string _clientProject = string.Empty;
    private DateTime _createdUtc = DateTime.UtcNow;
    private DateTime _updatedUtc = DateTime.UtcNow;

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

    /// <summary>
    /// Dowolna etykieta kategorii (np. Praca, Dom); puste = brak kategorii.
    /// </summary>
    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    /// <summary>Osoba odpowiedzialna (dowolny tekst, np. inicjały lub e-mail).</summary>
    public string AssignedTo
    {
        get => _assignedTo;
        set => SetProperty(ref _assignedTo, value);
    }

    /// <summary>Klient / projekt / kosztownik (kontekst pracy).</summary>
    public string ClientProject
    {
        get => _clientProject;
        set => SetProperty(ref _clientProject, value);
    }

    /// <summary>Moment utworzenia rekordu (UTC).</summary>
    public DateTime CreatedUtc
    {
        get => _createdUtc;
        set => SetProperty(ref _createdUtc, NormalizeUtc(value));
    }

    /// <summary>Ostatnia zmiana danych zadania (UTC).</summary>
    public DateTime UpdatedUtc
    {
        get => _updatedUtc;
        set => SetProperty(ref _updatedUtc, NormalizeUtc(value));
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

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
