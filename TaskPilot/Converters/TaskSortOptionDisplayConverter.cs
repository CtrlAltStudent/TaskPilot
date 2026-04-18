using System.Globalization;
using System.Windows.Data;
using TaskPilot.Models;

namespace TaskPilot.Converters;

public sealed class TaskSortOptionDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TaskSortOption s)
            return string.Empty;

        return s switch
        {
            TaskSortOption.DueDateAscending => "Termin: rosnąco",
            TaskSortOption.DueDateDescending => "Termin: malejąco",
            TaskSortOption.TitleAscending => "Tytuł: A–Z",
            TaskSortOption.TitleDescending => "Tytuł: Z–A",
            _ => s.ToString()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
