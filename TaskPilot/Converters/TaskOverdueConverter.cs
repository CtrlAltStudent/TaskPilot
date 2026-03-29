using System.Globalization;
using System.Windows.Data;
using TaskPilot.Models;

namespace TaskPilot.Converters;

public sealed class TaskOverdueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TaskItem t)
            return false;

        return !t.IsCompleted && t.DueDate.Date < DateTime.Today;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
