using System.ComponentModel;
using System.Windows;
using TaskPilot.Services;
using TaskPilot.ViewModels;

namespace TaskPilot;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var persistence = new JsonTaskPersistence(TaskStoragePaths.DefaultFilePath);
        DataContext = new MainViewModel(persistence);
        Closing += MainWindow_OnClosing;
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        if (DataContext is MainViewModel vm && !vm.TryClose())
            e.Cancel = true;
    }
}
