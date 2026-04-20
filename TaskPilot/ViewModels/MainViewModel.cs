using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TaskPilot.Models;
using TaskPilot.Services;

namespace TaskPilot.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private const string CategoryFilterAll = "Wszystkie";
    private const string CategoryFilterNone = "Bez kategorii";

    private readonly ITaskPersistence _persistence;
    private readonly CollectionViewSource _tasksViewSource;
    private TaskItem? _selectedTask;
    private int _nextId;
    private string _statusMessage = string.Empty;
    private string? _detailValidationMessage;
    private ThemeMode _selectedThemeMode = ThemeMode.System;
    private TaskStatusFilter _statusFilter = TaskStatusFilter.All;
    private TaskPriorityFilterOption _priorityFilter = TaskPriorityFilterOption.All;
    private TaskSortOption _sortOption = TaskSortOption.DueDateAscending;
    private string _searchText = string.Empty;
    private string _categoryFilterSelection = CategoryFilterAll;

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

        _tasksViewSource = new CollectionViewSource { Source = Tasks };
        _tasksViewSource.Filter += OnTasksFilter;
        ApplyTaskListFilters();
        ApplySortDescriptions();
        EnableLiveSorting();
        RefreshCategoryFilterChoices();

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

    public ICollectionView TasksView => _tasksViewSource.View;

    public IReadOnlyList<TaskPriority> PriorityOptions { get; } = Enum.GetValues<TaskPriority>().ToList();
    public IReadOnlyList<ThemeMode> ThemeOptions { get; } = Enum.GetValues<ThemeMode>().ToList();
    public IReadOnlyList<TaskStatusFilter> StatusFilterOptions { get; } = Enum.GetValues<TaskStatusFilter>().ToList();
    public IReadOnlyList<TaskPriorityFilterOption> PriorityFilterOptions { get; } = Enum.GetValues<TaskPriorityFilterOption>().ToList();
    public IReadOnlyList<TaskSortOption> SortOptions { get; } = Enum.GetValues<TaskSortOption>().ToList();

    public ObservableCollection<string> CategoryFilterChoices { get; } = new();

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

    public ThemeMode SelectedThemeMode
    {
        get => _selectedThemeMode;
        set => SetProperty(ref _selectedThemeMode, value);
    }

    public TaskStatusFilter SelectedStatusFilter
    {
        get => _statusFilter;
        set
        {
            if (!SetProperty(ref _statusFilter, value))
                return;

            ApplyTaskListFilters();
        }
    }

    public TaskPriorityFilterOption SelectedPriorityFilter
    {
        get => _priorityFilter;
        set
        {
            if (!SetProperty(ref _priorityFilter, value))
                return;

            ApplyTaskListFilters();
        }
    }

    public TaskSortOption SelectedSortOption
    {
        get => _sortOption;
        set
        {
            if (!SetProperty(ref _sortOption, value))
                return;

            ApplySortDescriptions();
        }
    }

    /// <summary>
    /// Tekst wyszukiwania po tytule i opisie (bez rozróżniania wielkości liter); pusty = brak filtru tekstowego.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (!SetProperty(ref _searchText, value))
                return;

            ApplyTaskListFilters();
        }
    }

    public string SelectedCategoryFilter
    {
        get => _categoryFilterSelection;
        set
        {
            if (!SetProperty(ref _categoryFilterSelection, value))
                return;

            ApplyTaskListFilters();
        }
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
        RefreshCategoryFilterChoices();
    }

    private void RefreshCategoryFilterChoices()
    {
        var prev = _categoryFilterSelection;
        CategoryFilterChoices.Clear();
        CategoryFilterChoices.Add(CategoryFilterAll);
        CategoryFilterChoices.Add(CategoryFilterNone);
        foreach (var name in Tasks
                     .Select(t => (t.Category ?? string.Empty).Trim())
                     .Where(s => s.Length > 0)
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
            CategoryFilterChoices.Add(name);

        var stillValid = CategoryFilterChoices.Any(x => string.Equals(x, prev, StringComparison.Ordinal));
        if (!stillValid)
        {
            _categoryFilterSelection = CategoryFilterAll;
            OnPropertyChanged(nameof(SelectedCategoryFilter));
        }

        ApplyTaskListFilters();
    }

    private void OnTasksFilter(object sender, FilterEventArgs e)
    {
        if (e.Item is not TaskItem task)
        {
            e.Accepted = false;
            return;
        }

        var statusOk = _statusFilter switch
        {
            TaskStatusFilter.All => true,
            TaskStatusFilter.Active => !task.IsCompleted,
            TaskStatusFilter.Completed => task.IsCompleted,
            _ => true
        };

        var priorityOk = _priorityFilter switch
        {
            TaskPriorityFilterOption.All => true,
            TaskPriorityFilterOption.Low => task.Priority == TaskPriority.Low,
            TaskPriorityFilterOption.Medium => task.Priority == TaskPriority.Medium,
            TaskPriorityFilterOption.High => task.Priority == TaskPriority.High,
            _ => true
        };

        var needle = _searchText.Trim();
        var searchOk = needle.Length == 0
                       || task.Title.Contains(needle, StringComparison.OrdinalIgnoreCase)
                       || task.Description.Contains(needle, StringComparison.OrdinalIgnoreCase)
                       || (!string.IsNullOrEmpty(task.Category) &&
                           task.Category.Contains(needle, StringComparison.OrdinalIgnoreCase));

        var categoryOk = _categoryFilterSelection switch
        {
            CategoryFilterAll => true,
            CategoryFilterNone => string.IsNullOrWhiteSpace(task.Category),
            _ => string.Equals((task.Category ?? string.Empty).Trim(), _categoryFilterSelection.Trim(),
                StringComparison.OrdinalIgnoreCase)
        };

        e.Accepted = statusOk && priorityOk && searchOk && categoryOk;
    }

    private void ApplyTaskListFilters()
    {
        TasksView.Refresh();
        SyncSelectionAfterFilter();
    }

    private void ApplySortDescriptions()
    {
        TasksView.SortDescriptions.Clear();

        switch (_sortOption)
        {
            case TaskSortOption.DueDateAscending:
                TasksView.SortDescriptions.Add(new SortDescription(nameof(TaskItem.DueDate), ListSortDirection.Ascending));
                break;
            case TaskSortOption.DueDateDescending:
                TasksView.SortDescriptions.Add(new SortDescription(nameof(TaskItem.DueDate), ListSortDirection.Descending));
                break;
            case TaskSortOption.TitleAscending:
                TasksView.SortDescriptions.Add(new SortDescription(nameof(TaskItem.Title), ListSortDirection.Ascending));
                break;
            case TaskSortOption.TitleDescending:
                TasksView.SortDescriptions.Add(new SortDescription(nameof(TaskItem.Title), ListSortDirection.Descending));
                break;
            default:
                TasksView.SortDescriptions.Add(new SortDescription(nameof(TaskItem.DueDate), ListSortDirection.Ascending));
                break;
        }
    }

    private void EnableLiveSorting()
    {
        if (TasksView is not ListCollectionView list)
            return;

        list.IsLiveSorting = true;
        list.LiveSortingProperties.Clear();
        list.LiveSortingProperties.Add(nameof(TaskItem.DueDate));
        list.LiveSortingProperties.Add(nameof(TaskItem.Title));
    }

    private void SyncSelectionAfterFilter()
    {
        if (SelectedTask is null)
            return;

        var visible = false;
        foreach (var item in TasksView)
        {
            if (ReferenceEquals(item, SelectedTask))
            {
                visible = true;
                break;
            }
        }

        if (!visible)
            SelectedTask = TasksView.Cast<TaskItem>().FirstOrDefault();
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
        {
            CommandManager.InvalidateRequerySuggested();
            ApplyTaskListFilters();
        }
        else if (e.PropertyName == nameof(TaskItem.Category))
        {
            RefreshCategoryFilterChoices();
        }
        else if (e.PropertyName is nameof(TaskItem.Priority) or nameof(TaskItem.Title) or nameof(TaskItem.Description))
        {
            ApplyTaskListFilters();
        }
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
            Category = string.Empty,
            DueDate = DateTime.Today,
            Priority = TaskPriority.Medium,
            IsCompleted = false
        };

        Tasks.Add(task);
        SelectedTask = task;
        ApplyTaskListFilters();
    }

    private void DeleteTask()
    {
        if (SelectedTask is null)
            return;

        var confirm = MessageBox.Show(
            $"Czy na pewno chcesz usunąć zadanie:\n\"{SelectedTask.Title}\"?",
            "TaskPilot — potwierdzenie usunięcia",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            StatusMessage = "Usuwanie anulowane.";
            return;
        }

        var index = Tasks.IndexOf(SelectedTask);
        var removed = SelectedTask;
        Tasks.Remove(removed);
        SelectedTask = Tasks.Count == 0
            ? null
            : Tasks[Math.Clamp(index, 0, Tasks.Count - 1)];

        ApplyTaskListFilters();
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
