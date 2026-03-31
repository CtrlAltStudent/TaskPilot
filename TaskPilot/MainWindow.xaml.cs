using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;
using TaskPilot.Models;
using TaskPilot.Services;
using TaskPilot.ViewModels;

namespace TaskPilot;

public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        var persistence = new JsonTaskPersistence(TaskStoragePaths.DefaultFilePath);
        _viewModel = new MainViewModel(persistence);
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        DataContext = _viewModel;

        ThemeService.ApplyTheme(Application.Current, _viewModel.SelectedThemeMode);
        SystemEvents.UserPreferenceChanged += SystemEventsOnUserPreferenceChanged;

        Closing += MainWindow_OnClosing;
        Closed += MainWindow_OnClosed;
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        if (DataContext is MainViewModel vm && !vm.TryClose())
            e.Cancel = true;
    }

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        SystemEvents.UserPreferenceChanged -= SystemEventsOnUserPreferenceChanged;
        if (_viewModel is not null)
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedThemeMode) && _viewModel is not null)
            ThemeService.ApplyTheme(Application.Current, _viewModel.SelectedThemeMode);
    }

    private void SystemEventsOnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (_viewModel is null || _viewModel.SelectedThemeMode != ThemeMode.System)
            return;

        Dispatcher.Invoke(() => ThemeService.ApplyTheme(Application.Current, ThemeMode.System));
    }
}
