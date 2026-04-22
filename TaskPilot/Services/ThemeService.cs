using Microsoft.Win32;
using TaskPilot.Models;
using System.Windows;

namespace TaskPilot.Services;

public static class ThemeService
{
    private const string LightThemeDictionary = "Themes/AppTheme.xaml";
    private const string DarkThemeDictionary = "Themes/DarkTheme.xaml";

    public static bool IsSystemDarkTheme()
    {
        const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        using var key = Registry.CurrentUser.OpenSubKey(keyPath);
        var value = key?.GetValue("AppsUseLightTheme");

        // 0 means dark, 1 means light. Default to light when missing.
        return value is int intValue && intValue == 0;
    }

    public static void ApplyTheme(System.Windows.Application application, ThemeMode mode)
    {
        var isDark = mode == ThemeMode.Dark || (mode == ThemeMode.System && IsSystemDarkTheme());
        var target = isDark ? DarkThemeDictionary : LightThemeDictionary;

        var dictionaries = application.Resources.MergedDictionaries;
        var currentTheme = dictionaries.FirstOrDefault(d =>
            d.Source is not null &&
            (d.Source.OriginalString.EndsWith(LightThemeDictionary, StringComparison.OrdinalIgnoreCase) ||
             d.Source.OriginalString.EndsWith(DarkThemeDictionary, StringComparison.OrdinalIgnoreCase)));

        if (currentTheme is not null &&
            currentTheme.Source is not null &&
            currentTheme.Source.OriginalString.EndsWith(target, StringComparison.OrdinalIgnoreCase))
            return;

        var newTheme = new ResourceDictionary
        {
            Source = new Uri(target, UriKind.Relative)
        };

        if (currentTheme is null)
            dictionaries.Insert(0, newTheme);
        else
        {
            var index = dictionaries.IndexOf(currentTheme);
            dictionaries[index] = newTheme;
        }
    }
}
