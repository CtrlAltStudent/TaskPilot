using System.Globalization;
using System.Windows.Data;
using TaskPilot.Models;

namespace TaskPilot.Converters;

public sealed class TaskPriorityFilterOptionDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TaskPriorityFilterOption f)
            return string.Empty;

        return f switch
        {
            TaskPriorityFilterOption.All => "Wszystkie",
            TaskPriorityFilterOption.Low => "Niski",
            TaskPriorityFilterOption.Medium => "Średni",
            TaskPriorityFilterOption.High => "Wysoki",
            _ => f.ToString()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
