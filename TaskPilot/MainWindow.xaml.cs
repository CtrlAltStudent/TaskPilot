using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Win32;
using Application = System.Windows.Application;
using TaskPilot.Models;
using TaskPilot.Services;
using TaskPilot.ViewModels;

namespace TaskPilot;

public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;
    private NotifyIcon? _notifyIcon;
    private ToolStripMenuItem? _menuTodayItem;

    public MainWindow()
    {
        InitializeComponent();
        var persistence = PersistenceResolver.CreateDefault();
        _viewModel = new MainViewModel(persistence);
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        DataContext = _viewModel;

        ThemeService.ApplyTheme(Application.Current, _viewModel.SelectedThemeMode);
        SystemEvents.UserPreferenceChanged += SystemEventsOnUserPreferenceChanged;

        Loaded += MainWindow_OnLoaded;
        StateChanged += MainWindow_OnStateChanged;
        Closing += MainWindow_OnClosing;
        Closed += MainWindow_OnClosed;
    }

    private void MainWindow_OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.F1)
        {
            ShowShortcutsHelp();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        {
            SearchTextBox.Focus();
            SearchTextBox.SelectAll();
            e.Handled = true;
        }
    }

    private void ShowShortcutsHelp_Click(object sender, RoutedEventArgs e)
    {
        ShowShortcutsHelp();
    }

    private void ShowShortcutsHelp()
    {
        var dlg = new ShortcutsWindow { Owner = this };
        dlg.ShowDialog();
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MainWindow_OnLoaded;
        InitializeTrayIcon();
    }

    private void MainWindow_OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized && _notifyIcon is not null)
            Hide();
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        if (DataContext is MainViewModel vm && !vm.TryClose())
            e.Cancel = true;
    }

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        DisposeTrayIcon();
        SystemEvents.UserPreferenceChanged -= SystemEventsOnUserPreferenceChanged;
        if (_viewModel is not null)
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
    }

    private void InitializeTrayIcon()
    {
        if (_notifyIcon is not null)
            return;

        _menuTodayItem = new ToolStripMenuItem("Dziś termin: —") { Enabled = false };

        var menu = new ContextMenuStrip();
        menu.Items.Add("Pokaż TaskPilot", null, (_, _) => Dispatcher.BeginInvoke(new Action(ShowFromTray)));
        menu.Items.Add(_menuTodayItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Zakończ", null, (_, _) => Dispatcher.BeginInvoke(new Action(ExitFromTray)));
        menu.Opening += TrayContextMenu_Opening;

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            Text = "TaskPilot — menedżer zadań",
            ContextMenuStrip = menu
        };
        _notifyIcon.MouseDoubleClick += OnNotifyIconMouseDoubleClick;
    }

    private void OnNotifyIconMouseDoubleClick(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            Dispatcher.BeginInvoke(new Action(ShowFromTray));
    }

    private void TrayContextMenu_Opening(object? sender, CancelEventArgs e)
    {
        if (_menuTodayItem is null || _viewModel is null)
            return;

        _menuTodayItem.Text = $"Dziś termin: {_viewModel.KpiDueTodayCount} (niewykonane)";
    }

    private void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitFromTray()
    {
        Close();
    }

    private void DisposeTrayIcon()
    {
        if (_notifyIcon is null)
            return;

        _notifyIcon.MouseDoubleClick -= OnNotifyIconMouseDoubleClick;
        if (_notifyIcon.ContextMenuStrip is { } menu)
            menu.Opening -= TrayContextMenu_Opening;

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _notifyIcon = null;
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
