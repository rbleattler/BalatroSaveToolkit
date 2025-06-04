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
    }    /// <summary>
         /// Creates a deep copy of the settings object
         /// </summary>
    private AppSettings CloneSettings(AppSettings original)
    {        return new AppSettings
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
            EnableFileWatching = original.EnableFileWatching,
            FlashTaskbarOnUpdate = original.FlashTaskbarOnUpdate,
            AutoRefreshOnFileChange = original.AutoRefreshOnFileChange
        };
    }    /// <summary>
         /// Loads the working settings into the UI controls
         /// </summary>
    private void LoadSettingsIntoControls()
    {
        DefaultJkrDirectoryTextBox.Text = _workingSettings.DefaultJkrDirectory;
        DefaultLuaExportDirectoryTextBox.Text = _workingSettings.DefaultLuaExportDirectory;
        ShowLogsOnStartupCheckBox.IsChecked = _workingSettings.ShowLogsOnStartup;
        AutoSaveDecompressedFilesCheckBox.IsChecked = _workingSettings.AutoSaveDecompressedFiles;
        ConfirmFileOverwritesCheckBox.IsChecked = _workingSettings.ConfirmFileOverwrites;        EnableAutoBackupCheckBox.IsChecked = _workingSettings.EnableAutoBackup;
        BackupDirectoryTextBox.Text = _workingSettings.BackupDirectory;
        BalatroSaveRootTextBox.Text = _workingSettings.BalatroSaveRoot;
        MaxLogEntriesTextBox.Text = _workingSettings.MaxLogEntries.ToString();

        // File watching settings
        EnableFileWatchingCheckBox.IsChecked = _workingSettings.EnableFileWatching;
        FlashTaskbarOnUpdateCheckBox.IsChecked = _workingSettings.FlashTaskbarOnUpdate;
        AutoRefreshOnFileChangeCheckBox.IsChecked = _workingSettings.AutoRefreshOnFileChange;

        // Set the log level combobox
        foreach (ComboBoxItem item in LogLevelComboBox.Items)
        {
            if (item.Content.ToString() == _workingSettings.LogLevel)
            {
                LogLevelComboBox.SelectedItem = item;
                break;
            }
        }

        // Update derived path displays
        UpdateBalatroDerivedPaths();
    }    /// <summary>
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
        _workingSettings.BalatroSaveRoot = BalatroSaveRootTextBox.Text;

        // File watching settings
        _workingSettings.EnableFileWatching = EnableFileWatchingCheckBox.IsChecked ?? true;
        _workingSettings.FlashTaskbarOnUpdate = FlashTaskbarOnUpdateCheckBox.IsChecked ?? true;
        _workingSettings.AutoRefreshOnFileChange = AutoRefreshOnFileChangeCheckBox.IsChecked ?? true;

        if (int.TryParse(MaxLogEntriesTextBox.Text, out int maxLogEntries))
        {
            _workingSettings.MaxLogEntries = maxLogEntries;
        }

        if (LogLevelComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            _workingSettings.LogLevel = selectedItem.Content.ToString() ?? "Info";
        }
    }    /// <summary>
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
        settings.MaxLogEntries = _workingSettings.MaxLogEntries;        settings.EnableAutoBackup = _workingSettings.EnableAutoBackup;
        settings.BackupDirectory = _workingSettings.BackupDirectory;
        settings.BalatroSaveRoot = _workingSettings.BalatroSaveRoot;

        // File watching settings
        settings.EnableFileWatching = _workingSettings.EnableFileWatching;
        settings.FlashTaskbarOnUpdate = _workingSettings.FlashTaskbarOnUpdate;
        settings.AutoRefreshOnFileChange = _workingSettings.AutoRefreshOnFileChange;

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
            _workingSettings.ConfirmFileOverwrites = defaultSettings.ConfirmFileOverwrites; _workingSettings.LogLevel = defaultSettings.LogLevel;
            _workingSettings.MaxLogEntries = defaultSettings.MaxLogEntries;            _workingSettings.EnableAutoBackup = defaultSettings.EnableAutoBackup;
            _workingSettings.BackupDirectory = defaultSettings.BackupDirectory;
            _workingSettings.BalatroSaveRoot = defaultSettings.BalatroSaveRoot;

            // File watching settings
            _workingSettings.EnableFileWatching = defaultSettings.EnableFileWatching;
            _workingSettings.FlashTaskbarOnUpdate = defaultSettings.FlashTaskbarOnUpdate;
            _workingSettings.AutoRefreshOnFileChange = defaultSettings.AutoRefreshOnFileChange;

            LoadSettingsIntoControls();
        }
    }

    private void BrowseBalatroSaveRootButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Balatro Save Root Directory",
            InitialDirectory = BalatroSaveRootTextBox.Text
        };

        if (dialog.ShowDialog() == true)
        {
            BalatroSaveRootTextBox.Text = dialog.FolderName;
            _workingSettings.BalatroSaveRoot = dialog.FolderName;
            UpdateBalatroDerivedPaths();
        }
    }

    /// <summary>
    /// Updates the derived path displays for Balatro integration
    /// </summary>
    private void UpdateBalatroDerivedPaths()
    {
        try
        {
            BalatroSettingsFilePathTextBox.Text = _workingSettings.BalatroSettingsFilePath;
            CurrentProfileNumberTextBox.Text = $"Profile {_workingSettings.CurrentProfileNumber}";
            CurrentProfileSettingsPathTextBox.Text = _workingSettings.CurrentProfileSettingsPath;
            CurrentProfileMetaPathTextBox.Text = _workingSettings.CurrentProfileMetaPath;
            CurrentProfileSavePathTextBox.Text = _workingSettings.CurrentProfileSavePath;
        }
        catch (Exception ex)
        {
            // If there's an error reading the profile, show error message in the profile text box
            CurrentProfileNumberTextBox.Text = $"Error: {ex.Message}";
            CurrentProfileSettingsPathTextBox.Text = "N/A";
            CurrentProfileMetaPathTextBox.Text = "N/A";
            CurrentProfileSavePathTextBox.Text = "N/A";
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

    private void BalatroSaveRootTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_workingSettings != null && sender is TextBox textBox)
        {
            _workingSettings.BalatroSaveRoot = textBox.Text;
            UpdateBalatroDerivedPaths();
        }
    }

    private void LoadBalatroSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        LoadJkrFile(_workingSettings.BalatroSettingsFilePath, "Balatro Settings");
    }

    private void LoadProfileSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        LoadJkrFile(_workingSettings.CurrentProfileSettingsPath, "Profile Settings");
    }

    private void LoadProfileMetaButton_Click(object sender, RoutedEventArgs e)
    {
        LoadJkrFile(_workingSettings.CurrentProfileMetaPath, "Profile Meta");
    }

    private void LoadProfileSaveButton_Click(object sender, RoutedEventArgs e)
    {
        LoadJkrFile(_workingSettings.CurrentProfileSavePath, "Profile Save");
    }    /// <summary>
         /// Helper method to load a JKR file in the main application
         /// </summary>
    private void LoadJkrFile(string filePath, string fileType)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"{fileType} file not found:\n{filePath}", "File Not Found",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Close the settings window and trigger loading in the main window
            DialogResult = true;

            // Find the main window and call its public LoadFile method
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.LoadFile(filePath);
            }
            else
            {
                MessageBox.Show($"Unable to access main window. Please use the Load JKR File button instead.",
                              "Load Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading {fileType}: {ex.Message}", "Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
