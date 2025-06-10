using BalatroSaveExplorer.Models;

namespace BalatroSaveExplorer.Services;

/// <summary>
/// Service responsible for settings business logic and management.
/// Handles settings operations that don't involve UI directly.
/// </summary>
public static class SettingsService
{
    /// <summary>
    /// Creates a deep copy of the settings object
    /// </summary>
    public static AppSettings CloneSettings(AppSettings original)
    {
        return new AppSettings
        {
            DefaultJkrDirectory = original.DefaultJkrDirectory,
            DefaultLuaExportDirectory = original.DefaultLuaExportDirectory,
            ShowLogsOnStartup = original.ShowLogsOnStartup,
            AutoSaveDecompressedFiles = original.AutoSaveDecompressedFiles,
            ConfirmFileOverwrites = original.ConfirmFileOverwrites,
            LogLevel = original.LogLevel,
            MaxLogEntries = original.MaxLogEntries,
            EnableAutoBackup = original.EnableAutoBackup,
            BackupDirectory = original.BackupDirectory,
            BalatroSaveRoot = original.BalatroSaveRoot,
            Theme = original.Theme,
            EnableFileWatching = original.EnableFileWatching,
            FlashTaskbarOnUpdate = original.FlashTaskbarOnUpdate,
            AutoRefreshOnFileChange = original.AutoRefreshOnFileChange
        };
    }

    /// <summary>
    /// Applies the working settings to the global settings manager
    /// </summary>
    public static void ApplySettingsChanges(AppSettings workingSettings)
    {
        var settings = SettingsManager.Instance.Settings;

        settings.DefaultJkrDirectory = workingSettings.DefaultJkrDirectory;
        settings.DefaultLuaExportDirectory = workingSettings.DefaultLuaExportDirectory;
        settings.ShowLogsOnStartup = workingSettings.ShowLogsOnStartup;
        settings.AutoSaveDecompressedFiles = workingSettings.AutoSaveDecompressedFiles;
        settings.ConfirmFileOverwrites = workingSettings.ConfirmFileOverwrites;
        settings.LogLevel = workingSettings.LogLevel;
        settings.MaxLogEntries = workingSettings.MaxLogEntries;
        settings.EnableAutoBackup = workingSettings.EnableAutoBackup;
        settings.BackupDirectory = workingSettings.BackupDirectory;
        settings.BalatroSaveRoot = workingSettings.BalatroSaveRoot;

        // File watching settings
        settings.EnableFileWatching = workingSettings.EnableFileWatching;
        settings.FlashTaskbarOnUpdate = workingSettings.FlashTaskbarOnUpdate;
        settings.AutoRefreshOnFileChange = workingSettings.AutoRefreshOnFileChange;

        // Theme settings
        settings.Theme = workingSettings.Theme;

        SettingsManager.Instance.SaveSettings();

        // Apply theme immediately
        ThemeManager.Instance.ApplyCurrentTheme();
    }
}
