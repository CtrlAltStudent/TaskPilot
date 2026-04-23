using System.Windows;

namespace TaskPilot;

public partial class ShortcutsWindow : Window
{
    public ShortcutsWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
