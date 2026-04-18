using System.Globalization;
using System.Windows.Data;
using TaskPilot.Models;

namespace TaskPilot.Converters;

public sealed class TaskStatusFilterDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TaskStatusFilter f)
            return string.Empty;

        return f switch
        {
            TaskStatusFilter.All => "Wszystkie",
            TaskStatusFilter.Active => "Aktywne",
            TaskStatusFilter.Completed => "Wykonane",
            _ => f.ToString()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
