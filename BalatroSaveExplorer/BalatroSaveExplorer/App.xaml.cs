using System.Configuration;
using System.Data;
using System.Windows;
using BalatroSaveExplorer.Services;

namespace BalatroSaveExplorer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize the settings system
        var settingsManager = SettingsManager.Instance;
        settingsManager.ValidateSettings();

        // Initialize and apply the theme system
        var themeManager = ThemeManager.Instance;
        themeManager.ApplyCurrentTheme();

        // Ensure the theme is properly applied
        Resources.MergedDictionaries.RemoveAt(0); // Remove the default LightTheme
        themeManager.ApplyCurrentTheme(); // Re-apply the theme

        // Log the startup
        System.Diagnostics.Debug.WriteLine($"BalatroSaveExplorer started. Settings loaded from: {settingsManager.GetSettingsFilePath()}");
    }protected override void OnExit(ExitEventArgs e)
    {
        // Ensure settings are saved on exit
        SettingsManager.Instance.SaveSettings();

        // Cleanup theme manager
        ThemeManager.Instance.Dispose();

        base.OnExit(e);
    }
}

