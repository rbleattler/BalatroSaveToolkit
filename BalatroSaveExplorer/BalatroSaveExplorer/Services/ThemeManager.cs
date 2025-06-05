using System.Windows;
using Microsoft.Win32;
using BalatroSaveExplorer.Models;

namespace BalatroSaveExplorer.Services;

/// <summary>
/// Manages application theming including system theme detection and theme switching
/// </summary>
public class ThemeManager
{
  private static ThemeManager? _instance;
  private static readonly object _lock = new object();

  /// <summary>
  /// Gets the singleton instance of the ThemeManager
  /// </summary>
  public static ThemeManager Instance
  {
    get
    {
      if (_instance == null)
      {
        lock (_lock)
        {
          _instance ??= new ThemeManager();
        }
      }
      return _instance;
    }
  }

  /// <summary>
  /// Event fired when the theme changes
  /// </summary>
  public event EventHandler? ThemeChanged;

  private ThemeManager()
  {
    // Listen for system theme changes
    SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
  }

  /// <summary>
  /// Gets whether the system is currently using dark theme
  /// </summary>
  public bool IsSystemDarkTheme
  {
    get
    {
      try
      {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        var value = key?.GetValue("AppsUseLightTheme");
        return value is int intValue && intValue == 0;
      }
      catch
      {
        return false; // Default to light theme if we can't detect
      }
    }
  }

  /// <summary>
  /// Applies the specified theme to the application
  /// </summary>
  /// <param name="theme">The theme to apply</param>
  public void ApplyTheme(AppTheme theme)
  {
    var isDarkTheme = theme switch
    {
      AppTheme.Dark => true,
      AppTheme.Light => false,
      AppTheme.System => IsSystemDarkTheme,
      _ => false
    };

    var themeUri = isDarkTheme
        ? new Uri("pack://application:,,,/Themes/DarkTheme.xaml", UriKind.Absolute)
        : new Uri("pack://application:,,,/Themes/LightTheme.xaml", UriKind.Absolute);

    ApplyThemeResourceDictionary(themeUri);
    ThemeChanged?.Invoke(this, EventArgs.Empty);
  }

  /// <summary>
  /// Applies the theme based on current settings
  /// </summary>
  public void ApplyCurrentTheme()
  {
    var settings = SettingsManager.Instance.Settings;
    ApplyTheme(settings.Theme);
  }    /// <summary>
       /// Applies a theme resource dictionary to the application
       /// </summary>
  private void ApplyThemeResourceDictionary(Uri themeUri)
  {
    try
    {
      var app = Application.Current;
      if (app == null) return;

      // Remove existing theme dictionaries
      var existingThemes = app.Resources.MergedDictionaries
          .Where(d => d.Source?.ToString().Contains("/Themes/") == true)
          .ToList();

      foreach (var theme in existingThemes)
      {
        app.Resources.MergedDictionaries.Remove(theme);
      }

      // Add the new theme dictionary
      var newTheme = new ResourceDictionary { Source = themeUri };
      app.Resources.MergedDictionaries.Insert(0, newTheme);
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
    }
  }

  /// <summary>
  /// Handles system preference changes to update theme when system theme changes
  /// </summary>
  private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
  {
    if (e.Category == UserPreferenceCategory.General)
    {
      var settings = SettingsManager.Instance.Settings;
      if (settings.Theme == AppTheme.System)
      {
        ApplyCurrentTheme();
      }
    }
  }

  /// <summary>
  /// Cleanup resources
  /// </summary>
  public void Dispose()
  {
    SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
  }
}
