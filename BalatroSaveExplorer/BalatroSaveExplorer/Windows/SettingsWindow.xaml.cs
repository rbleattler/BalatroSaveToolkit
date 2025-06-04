using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using BalatroSaveExplorer.Models;
using BalatroSaveExplorer.Services;
using Microsoft.Win32;

namespace BalatroSaveExplorer.Windows;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly AppSettings _originalSettings;
    private readonly AppSettings _workingSettings;

    public SettingsWindow()
    {
        InitializeComponent();

        // Create a copy of the current settings to work with
        _originalSettings = SettingsManager.Instance.Settings;
        _workingSettings = CloneSettings(_originalSettings);

        LoadSettingsIntoControls();

        // Set the settings file path for display
        SettingsFilePathTextBox.Text = SettingsManager.Instance.GetSettingsFilePath();
    }

    /// <summary>
    /// Creates a deep copy of the settings object
    /// </summary>
    private AppSettings CloneSettings(AppSettings original)
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
            BackupDirectory = original.BackupDirectory
        };
    }

    /// <summary>
    /// Loads the working settings into the UI controls
    /// </summary>
    private void LoadSettingsIntoControls()
    {
        DefaultJkrDirectoryTextBox.Text = _workingSettings.DefaultJkrDirectory;
        DefaultLuaExportDirectoryTextBox.Text = _workingSettings.DefaultLuaExportDirectory;
        ShowLogsOnStartupCheckBox.IsChecked = _workingSettings.ShowLogsOnStartup;
        AutoSaveDecompressedFilesCheckBox.IsChecked = _workingSettings.AutoSaveDecompressedFiles;
        ConfirmFileOverwritesCheckBox.IsChecked = _workingSettings.ConfirmFileOverwrites;
        EnableAutoBackupCheckBox.IsChecked = _workingSettings.EnableAutoBackup;
        BackupDirectoryTextBox.Text = _workingSettings.BackupDirectory;
        MaxLogEntriesTextBox.Text = _workingSettings.MaxLogEntries.ToString();

        // Set the log level combobox
        foreach (ComboBoxItem item in LogLevelComboBox.Items)
        {
            if (item.Content.ToString() == _workingSettings.LogLevel)
            {
                LogLevelComboBox.SelectedItem = item;
                break;
            }
        }
    }

    /// <summary>
    /// Saves the UI control values back to the working settings
    /// </summary>
    private void SaveControlsToSettings()
    {
        _workingSettings.DefaultJkrDirectory = DefaultJkrDirectoryTextBox.Text;
        _workingSettings.DefaultLuaExportDirectory = DefaultLuaExportDirectoryTextBox.Text;
        _workingSettings.ShowLogsOnStartup = ShowLogsOnStartupCheckBox.IsChecked ?? false;
        _workingSettings.AutoSaveDecompressedFiles = AutoSaveDecompressedFilesCheckBox.IsChecked ?? false;
        _workingSettings.ConfirmFileOverwrites = ConfirmFileOverwritesCheckBox.IsChecked ?? true;
        _workingSettings.EnableAutoBackup = EnableAutoBackupCheckBox.IsChecked ?? true;
        _workingSettings.BackupDirectory = BackupDirectoryTextBox.Text;

        if (int.TryParse(MaxLogEntriesTextBox.Text, out int maxLogEntries))
        {
            _workingSettings.MaxLogEntries = maxLogEntries;
        }

        if (LogLevelComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            _workingSettings.LogLevel = selectedItem.Content.ToString() ?? "Info";
        }
    }

    /// <summary>
    /// Applies the working settings to the global settings manager
    /// </summary>
    private void ApplySettings()
    {
        var settings = SettingsManager.Instance.Settings;

        settings.DefaultJkrDirectory = _workingSettings.DefaultJkrDirectory;
        settings.DefaultLuaExportDirectory = _workingSettings.DefaultLuaExportDirectory;
        settings.ShowLogsOnStartup = _workingSettings.ShowLogsOnStartup;
        settings.AutoSaveDecompressedFiles = _workingSettings.AutoSaveDecompressedFiles;
        settings.ConfirmFileOverwrites = _workingSettings.ConfirmFileOverwrites;
        settings.LogLevel = _workingSettings.LogLevel;
        settings.MaxLogEntries = _workingSettings.MaxLogEntries;
        settings.EnableAutoBackup = _workingSettings.EnableAutoBackup;
        settings.BackupDirectory = _workingSettings.BackupDirectory;

        SettingsManager.Instance.SaveSettings();
    }

    private void BrowseJkrDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Default JKR Directory",
            InitialDirectory = DefaultJkrDirectoryTextBox.Text
        };

        if (dialog.ShowDialog() == true)
        {
            DefaultJkrDirectoryTextBox.Text = dialog.FolderName;
        }
    }

    private void BrowseLuaExportDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Default Lua Export Directory",
            InitialDirectory = DefaultLuaExportDirectoryTextBox.Text
        };

        if (dialog.ShowDialog() == true)
        {
            DefaultLuaExportDirectoryTextBox.Text = dialog.FolderName;
        }
    }

    private void BrowseBackupDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Backup Directory",
            InitialDirectory = BackupDirectoryTextBox.Text
        };

        if (dialog.ShowDialog() == true)
        {
            BackupDirectoryTextBox.Text = dialog.FolderName;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveControlsToSettings();
            ApplySettings();
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ResetToDefaultsButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to reset all settings to their default values? This cannot be undone.",
            "Reset Settings",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // Create new default settings and load them into controls
            var defaultSettings = new AppSettings();
            _workingSettings.DefaultJkrDirectory = defaultSettings.DefaultJkrDirectory;
            _workingSettings.DefaultLuaExportDirectory = defaultSettings.DefaultLuaExportDirectory;
            _workingSettings.ShowLogsOnStartup = defaultSettings.ShowLogsOnStartup;
            _workingSettings.AutoSaveDecompressedFiles = defaultSettings.AutoSaveDecompressedFiles;
            _workingSettings.ConfirmFileOverwrites = defaultSettings.ConfirmFileOverwrites;
            _workingSettings.LogLevel = defaultSettings.LogLevel;
            _workingSettings.MaxLogEntries = defaultSettings.MaxLogEntries;
            _workingSettings.EnableAutoBackup = defaultSettings.EnableAutoBackup;
            _workingSettings.BackupDirectory = defaultSettings.BackupDirectory;

            LoadSettingsIntoControls();
        }
    }

    private void OpenSettingsFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var settingsPath = SettingsManager.Instance.GetSettingsFilePath();
            var directoryPath = Path.GetDirectoryName(settingsPath);

            if (!string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath))
            {
                Process.Start("explorer.exe", directoryPath);
            }
            else
            {
                MessageBox.Show("Settings directory not found.", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening settings folder: {ex.Message}", "Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
