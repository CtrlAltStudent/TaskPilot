using System.Globalization;
using System.Windows.Data;
using TaskPilot.Models;

namespace TaskPilot.Converters;

public sealed class ThemeModeDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ThemeMode mode)
            return string.Empty;

        return mode switch
        {
            ThemeMode.System => "System",
            ThemeMode.Light => "Jasny",
            ThemeMode.Dark => "Ciemny",
            _ => mode.ToString()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
