using System.Globalization;
using System.Windows.Data;
using TaskPilot.Models;

namespace TaskPilot.Converters;

public sealed class PriorityDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TaskPriority p)
            return string.Empty;

        return p switch
        {
            TaskPriority.Low => "Niski",
            TaskPriority.Medium => "Średni",
            TaskPriority.High => "Wysoki",
            _ => value.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
