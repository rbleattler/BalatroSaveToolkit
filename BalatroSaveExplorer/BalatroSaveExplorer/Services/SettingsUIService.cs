using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using BalatroSaveExplorer.Models;

namespace BalatroSaveExplorer.Services;

/// <summary>
/// Service responsible for managing settings UI operations and user interactions.
/// Handles loading settings into controls, saving control values back to settings,
/// and managing settings-related user actions.
/// </summary>
public class SettingsUIService
{
    private readonly Logger _logger;

    public SettingsUIService(Logger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }    /// <summary>
         /// Loads the working settings into the UI controls
         /// </summary>
    public void LoadSettingsIntoControls(AppSettings workingSettings,
        TextBox defaultJkrDirectoryTextBox,
        TextBox defaultLuaExportDirectoryTextBox,
        CheckBox showLogsOnStartupCheckBox,
        CheckBox autoSaveDecompressedFilesCheckBox,
        CheckBox confirmFileOverwritesCheckBox,
        CheckBox enableAutoBackupCheckBox,
        TextBox backupDirectoryTextBox,
        TextBox balatroSaveRootTextBox,
        TextBox maxLogEntriesTextBox,
        CheckBox enableFileWatchingCheckBox,
        CheckBox flashTaskbarOnUpdateCheckBox,
        CheckBox autoRefreshOnFileChangeCheckBox,
        ComboBox logLevelComboBox,
        ComboBox themeComboBox,
        Action updateBalatroDerivedPaths,
        // Save management controls
        CheckBox? enableAutoSaveCheckBox = null,
        TextBox? autoSaveIntervalTextBox = null,
        ComboBox? autoSaveIntervalUnitComboBox = null,
        CheckBox? enableSaveRetentionCheckBox = null,
        TextBox? saveRetentionValueTextBox = null,
        ComboBox? saveRetentionUnitComboBox = null)
    {
        defaultJkrDirectoryTextBox.Text = workingSettings.DefaultJkrDirectory;
        defaultLuaExportDirectoryTextBox.Text = workingSettings.DefaultLuaExportDirectory;
        showLogsOnStartupCheckBox.IsChecked = workingSettings.ShowLogsOnStartup;
        autoSaveDecompressedFilesCheckBox.IsChecked = workingSettings.AutoSaveDecompressedFiles;
        confirmFileOverwritesCheckBox.IsChecked = workingSettings.ConfirmFileOverwrites;
        enableAutoBackupCheckBox.IsChecked = workingSettings.EnableAutoBackup;
        backupDirectoryTextBox.Text = workingSettings.BackupDirectory;
        balatroSaveRootTextBox.Text = workingSettings.BalatroSaveRoot;
        maxLogEntriesTextBox.Text = workingSettings.MaxLogEntries.ToString();

        // File watching settings
        enableFileWatchingCheckBox.IsChecked = workingSettings.EnableFileWatching;
        flashTaskbarOnUpdateCheckBox.IsChecked = workingSettings.FlashTaskbarOnUpdate;
        autoRefreshOnFileChangeCheckBox.IsChecked = workingSettings.AutoRefreshOnFileChange;

        // Save management settings
        if (enableAutoSaveCheckBox != null)
            enableAutoSaveCheckBox.IsChecked = workingSettings.EnableAutoSave;
        if (autoSaveIntervalTextBox != null)
            autoSaveIntervalTextBox.Text = workingSettings.AutoSaveInterval.ToString();
        if (enableSaveRetentionCheckBox != null)
            enableSaveRetentionCheckBox.IsChecked = workingSettings.EnableSaveRetention;
        if (saveRetentionValueTextBox != null)
            saveRetentionValueTextBox.Text = workingSettings.SaveRetentionValue.ToString();

        // Set combobox selections
        if (autoSaveIntervalUnitComboBox != null)
        {
            foreach (ComboBoxItem item in autoSaveIntervalUnitComboBox.Items)
            {
                if (item.Content.ToString() == workingSettings.AutoSaveIntervalUnit)
                {
                    autoSaveIntervalUnitComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        if (saveRetentionUnitComboBox != null)
        {
            foreach (ComboBoxItem item in saveRetentionUnitComboBox.Items)
            {
                if (item.Content.ToString() == workingSettings.SaveRetentionUnit)
                {
                    saveRetentionUnitComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        // Set the log level combobox
        foreach (ComboBoxItem item in logLevelComboBox.Items)
        {
            if (item.Content.ToString() == workingSettings.LogLevel)
            {
                logLevelComboBox.SelectedItem = item;
                break;
            }
        }

        // Set the theme combobox
        foreach (ComboBoxItem item in themeComboBox.Items)
        {
            if (item.Tag.ToString() == workingSettings.Theme.ToString())
            {
                themeComboBox.SelectedItem = item;
                break;
            }
        }

        // Update derived path displays
        updateBalatroDerivedPaths();
    }    /// <summary>
         /// Saves the UI control values back to the working settings
         /// </summary>
    public void SaveControlsToSettings(AppSettings workingSettings,
        TextBox defaultJkrDirectoryTextBox,
        TextBox defaultLuaExportDirectoryTextBox,
        CheckBox showLogsOnStartupCheckBox,
        CheckBox autoSaveDecompressedFilesCheckBox,
        CheckBox confirmFileOverwritesCheckBox,
        CheckBox enableAutoBackupCheckBox,
        TextBox backupDirectoryTextBox,
        TextBox balatroSaveRootTextBox,
        TextBox maxLogEntriesTextBox,
        CheckBox enableFileWatchingCheckBox,
        CheckBox flashTaskbarOnUpdateCheckBox,
        CheckBox autoRefreshOnFileChangeCheckBox,
        ComboBox logLevelComboBox,
        ComboBox themeComboBox,
        // Save management controls
        CheckBox? enableAutoSaveCheckBox = null,
        TextBox? autoSaveIntervalTextBox = null,
        ComboBox? autoSaveIntervalUnitComboBox = null,
        CheckBox? enableSaveRetentionCheckBox = null,
        TextBox? saveRetentionValueTextBox = null,
        ComboBox? saveRetentionUnitComboBox = null)
    {
        workingSettings.DefaultJkrDirectory = defaultJkrDirectoryTextBox.Text;
        workingSettings.DefaultLuaExportDirectory = defaultLuaExportDirectoryTextBox.Text;
        workingSettings.ShowLogsOnStartup = showLogsOnStartupCheckBox.IsChecked ?? false;
        workingSettings.AutoSaveDecompressedFiles = autoSaveDecompressedFilesCheckBox.IsChecked ?? false;
        workingSettings.ConfirmFileOverwrites = confirmFileOverwritesCheckBox.IsChecked ?? true;
        workingSettings.EnableAutoBackup = enableAutoBackupCheckBox.IsChecked ?? true;
        workingSettings.BackupDirectory = backupDirectoryTextBox.Text;
        workingSettings.BalatroSaveRoot = balatroSaveRootTextBox.Text;

        // File watching settings
        workingSettings.EnableFileWatching = enableFileWatchingCheckBox.IsChecked ?? true;
        workingSettings.FlashTaskbarOnUpdate = flashTaskbarOnUpdateCheckBox.IsChecked ?? true;
        workingSettings.AutoRefreshOnFileChange = autoRefreshOnFileChangeCheckBox.IsChecked ?? true;

        // Save management settings
        if (enableAutoSaveCheckBox != null)
            workingSettings.EnableAutoSave = enableAutoSaveCheckBox.IsChecked ?? false;
        if (enableSaveRetentionCheckBox != null)
            workingSettings.EnableSaveRetention = enableSaveRetentionCheckBox.IsChecked ?? true;

        if (autoSaveIntervalTextBox != null && int.TryParse(autoSaveIntervalTextBox.Text, out int autoSaveInterval))
        {
            workingSettings.AutoSaveInterval = autoSaveInterval;
        }

        if (saveRetentionValueTextBox != null && int.TryParse(saveRetentionValueTextBox.Text, out int saveRetentionValue))
        {
            workingSettings.SaveRetentionValue = saveRetentionValue;
        }

        if (autoSaveIntervalUnitComboBox?.SelectedItem is ComboBoxItem autoSaveUnitItem)
        {
            workingSettings.AutoSaveIntervalUnit = autoSaveUnitItem.Content.ToString() ?? "Minutes";
        }

        if (saveRetentionUnitComboBox?.SelectedItem is ComboBoxItem saveRetentionUnitItem)
        {
            workingSettings.SaveRetentionUnit = saveRetentionUnitItem.Content.ToString() ?? "Days";
        }

        if (int.TryParse(maxLogEntriesTextBox.Text, out int maxLogEntries))
        {
            workingSettings.MaxLogEntries = maxLogEntries;
        }

        if (logLevelComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            workingSettings.LogLevel = selectedItem.Content.ToString() ?? "Info";
        }

        // Theme settings
        if (themeComboBox.SelectedItem is ComboBoxItem selectedThemeItem)
        {
            if (Enum.TryParse<AppTheme>(selectedThemeItem.Tag.ToString(), out AppTheme theme))
            {
                workingSettings.Theme = theme;
            }
        }
    }

    /// <summary>
    /// Handles browse for JKR directory
    /// </summary>
    public void BrowseJkrDirectory(TextBox defaultJkrDirectoryTextBox)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Default JKR Directory",
            InitialDirectory = defaultJkrDirectoryTextBox.Text
        };

        if (dialog.ShowDialog() == true)
        {
            defaultJkrDirectoryTextBox.Text = dialog.FolderName;
        }
    }

    /// <summary>
    /// Handles browse for Lua export directory
    /// </summary>
    public void BrowseLuaExportDirectory(TextBox defaultLuaExportDirectoryTextBox)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Default Lua Export Directory",
            InitialDirectory = defaultLuaExportDirectoryTextBox.Text
        };

        if (dialog.ShowDialog() == true)
        {
            defaultLuaExportDirectoryTextBox.Text = dialog.FolderName;
        }
    }

    /// <summary>
    /// Handles browse for backup directory
    /// </summary>
    public void BrowseBackupDirectory(TextBox backupDirectoryTextBox)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Backup Directory",
            InitialDirectory = backupDirectoryTextBox.Text
        };

        if (dialog.ShowDialog() == true)
        {
            backupDirectoryTextBox.Text = dialog.FolderName;
        }
    }

    /// <summary>
    /// Handles browse for Balatro save root directory
    /// </summary>
    public void BrowseBalatroSaveRoot(TextBox balatroSaveRootTextBox, Action updateBalatroDerivedPaths)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Balatro Save Root Directory",
            InitialDirectory = balatroSaveRootTextBox.Text
        };

        if (dialog.ShowDialog() == true)
        {
            balatroSaveRootTextBox.Text = dialog.FolderName;
            updateBalatroDerivedPaths();
        }
    }

    /// <summary>
    /// Handles applying settings changes
    /// </summary>
    public void ApplySettings(Action saveControlsToSettings, Action applySettingsChanges, Func<AppSettings> getWorkingSettings, Action<AppSettings> setWorkingSettings)
    {
        try
        {
            saveControlsToSettings();
            applySettingsChanges();
            setWorkingSettings(SettingsService.CloneSettings(SettingsManager.Instance.Settings));
            _logger.Log("Settings applied successfully");
            MessageBox.Show("Settings have been applied successfully.", "Settings",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error applying settings: {ex.Message}");
            MessageBox.Show($"Error applying settings: {ex.Message}", "Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles resetting settings to defaults
    /// </summary>
    public void ResetToDefaults(Action<AppSettings> setWorkingSettings, Action loadSettingsIntoControls)
    {
        var result = MessageBox.Show(
            "Are you sure you want to reset all settings to their default values?",
            "Reset Settings",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            setWorkingSettings(new AppSettings()); // This creates a new instance with defaults
            loadSettingsIntoControls();
            _logger.Log("Settings reset to defaults");
        }
    }

    /// <summary>
    /// Handles theme selection changes
    /// </summary>
    public void HandleThemeChange(AppSettings workingSettings, ComboBoxItem selectedItem, bool isLoaded)
    {
        if (selectedItem != null && isLoaded)
        {
            if (Enum.TryParse<AppTheme>(selectedItem.Tag.ToString(), out AppTheme theme))
            {
                // Update working settings immediately
                workingSettings.Theme = theme;

                // Apply theme immediately for instant feedback
                var settings = SettingsManager.Instance.Settings;
                settings.Theme = theme;
                ThemeManager.Instance.ApplyCurrentTheme();

                _logger.Log($"Theme changed to: {theme}");
            }
        }
    }
}
