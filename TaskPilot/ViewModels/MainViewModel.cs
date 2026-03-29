using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskPilot.Models;
using TaskPilot.Services;

namespace TaskPilot.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly ITaskPersistence _persistence;
    private TaskItem? _selectedTask;
    private int _nextId;
    private string _statusMessage = string.Empty;
    private string? _detailValidationMessage;

    public MainViewModel(ITaskPersistence persistence)
    {
        _persistence = persistence;
        Tasks = new ObservableCollection<TaskItem>();

        foreach (var item in _persistence.Load())
        {
            Tasks.Add(item);
            AttachTask(item);
        }

        _nextId = Tasks.Count == 0 ? 1 : Tasks.Max(t => t.Id) + 1;

        Tasks.CollectionChanged += OnTasksCollectionChanged;

        AddTaskCommand = new RelayCommand(AddTask);
        DeleteTaskCommand = new RelayCommand(DeleteTask, () => SelectedTask != null);
        MarkCompleteCommand = new RelayCommand(MarkComplete, () => SelectedTask is { IsCompleted: false });
        SaveNowCommand = new RelayCommand(SaveNow);

        RefreshDetailValidation();
        TryPersist(silent: true);
        if (Tasks.Count == 0)
            StatusMessage = "Brak zadań — dodaj pierwsze lub wczytaj z pliku JSON.";
        else
            StatusMessage = $"Wczytano {Tasks.Count} zadań. Plik: {TaskStoragePaths.DefaultFilePath}";
    }

    public ObservableCollection<TaskItem> Tasks { get; }

    public IReadOnlyList<TaskPriority> PriorityOptions { get; } = Enum.GetValues<TaskPriority>().ToList();

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string? DetailValidationMessage
    {
        get => _detailValidationMessage;
        private set => SetProperty(ref _detailValidationMessage, value);
    }

    public TaskItem? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (!SetProperty(ref _selectedTask, value))
                return;

            OnPropertyChanged(nameof(IsTaskSelected));
            CommandManager.InvalidateRequerySuggested();
            RefreshDetailValidation();
        }
    }

    public bool IsTaskSelected => SelectedTask != null;

    public ICommand AddTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }
    public ICommand MarkCompleteCommand { get; }
    public ICommand SaveNowCommand { get; }

    private void OnTasksCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (TaskItem item in e.NewItems)
                AttachTask(item);
        }

        if (e.OldItems is not null)
        {
            foreach (TaskItem item in e.OldItems)
                DetachTask(item);
        }

        TryPersist(silent: true);
    }

    private void AttachTask(TaskItem item)
    {
        item.PropertyChanged += OnTaskPropertyChanged;
    }

    private void DetachTask(TaskItem item)
    {
        item.PropertyChanged -= OnTaskPropertyChanged;
    }

    private void OnTaskPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender == SelectedTask)
            RefreshDetailValidation();

        TryPersist(silent: true);

        if (e.PropertyName == nameof(TaskItem.IsCompleted))
            CommandManager.InvalidateRequerySuggested();
    }

    private void RefreshDetailValidation()
    {
        if (SelectedTask is null)
        {
            DetailValidationMessage = null;
            return;
        }

        if (!TaskValidator.TryValidate(SelectedTask, out var err))
            DetailValidationMessage = err;
        else
            DetailValidationMessage = null;
    }

    private void TryPersist(bool silent)
    {
        if (!TaskValidator.TryValidateAll(Tasks, out var err))
        {
            StatusMessage = silent
                ? $"Uwaga: dane nie zostały zapisane — {err}"
                : $"Nie zapisano: {err}";
            return;
        }

        try
        {
            _persistence.Save(Tasks);
            StatusMessage = $"Zapisano {Tasks.Count} zadań. {TaskStoragePaths.DefaultFilePath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Błąd zapisu pliku: {ex.Message}";
            if (!silent)
                MessageBox.Show(StatusMessage, "TaskPilot", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SaveNow()
    {
        if (!TaskValidator.TryValidateAll(Tasks, out var err))
        {
            MessageBox.Show(err ?? "Walidacja nie powiodła się.", "TaskPilot — walidacja", MessageBoxButton.OK,
                MessageBoxImage.Warning);
            StatusMessage = $"Nie zapisano: {err}";
            return;
        }

        TryPersist(silent: false);
    }

    private void AddTask()
    {
        var task = new TaskItem
        {
            Id = _nextId++,
            Title = "Nowe zadanie",
            Description = string.Empty,
            DueDate = DateTime.Today,
            Priority = TaskPriority.Medium,
            IsCompleted = false
        };

        Tasks.Add(task);
        SelectedTask = task;
    }

    private void DeleteTask()
    {
        if (SelectedTask is null)
            return;

        var index = Tasks.IndexOf(SelectedTask);
        var removed = SelectedTask;
        Tasks.Remove(removed);
        SelectedTask = Tasks.Count == 0
            ? null
            : Tasks[Math.Clamp(index, 0, Tasks.Count - 1)];
    }

    private void MarkComplete()
    {
        if (SelectedTask is null)
            return;

        SelectedTask.IsCompleted = true;
        CommandManager.InvalidateRequerySuggested();
    }

    public bool TryClose()
    {
        if (TaskValidator.TryValidateAll(Tasks, out _))
        {
            try
            {
                _persistence.Save(Tasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nie udało się zapisać przy zamykaniu.\n{ex.Message}", "TaskPilot",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        var r = MessageBox.Show(
            "Są błędy walidacji (np. pusty tytuł lub termin w przeszłości u niewykonanego zadania).\n\nZamknąć BEZ zapisu zmian na dysku?",
            "TaskPilot",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        return r == MessageBoxResult.Yes;
    }
}
