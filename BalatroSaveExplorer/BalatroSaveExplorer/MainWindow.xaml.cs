using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.Collections.ObjectModel;
using BalatroSaveExplorer.Services;
using BalatroSaveExplorer.Models;
using System.Windows.Shell;
using System.Windows.Threading;
using System.Diagnostics;
using BalatroSaveExplorer.Utilities;
using BalatroSaveExplorer.Converters;

namespace BalatroSaveExplorer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{    // Services
    private readonly Logger _logger;
    private readonly JkrFileService _jkrFileService;
    private readonly BackupService _backupService;
    private readonly FileWatchingService _fileWatchingService;
    private readonly UIStateService _uiStateService;
    private readonly LuaExportService _luaExportService;
    private readonly FileLoadingService _fileLoadingService;
    private readonly SettingsUIService _settingsUIService;
    private readonly BalatroPathService _balatroPathService;
    private readonly FileChangeHandlerService _fileChangeHandlerService;

    // UI-specific fields only
    private readonly ObservableCollection<TreeNodeViewModel> _treeNodes;
    private string? _currentFilePath;
    private string? _currentDecompressedContent;

    // Settings management fields
    private AppSettings _workingSettings;    // e.g. 0.8 = 80%
    private const double MinPct = 0.85;

    public MainWindow()
    {
        InitializeComponent();

        // Initialize logger first
        _logger = new Logger();        // Initialize services
        _jkrFileService = new JkrFileService(_logger);
        _backupService = new BackupService(_logger);
        _fileWatchingService = new FileWatchingService(_logger);
        _uiStateService = new UIStateService(_logger);
        _luaExportService = new LuaExportService(_logger);
        _fileLoadingService = new FileLoadingService(_logger, _jkrFileService, _backupService, _uiStateService, _fileWatchingService);
        _settingsUIService = new SettingsUIService(_logger);
        _balatroPathService = new BalatroPathService(_logger);
        _fileChangeHandlerService = new FileChangeHandlerService(_logger, _uiStateService);// Initialize UI components
        _treeNodes = new ObservableCollection<TreeNodeViewModel>();
        DataTreeView.ItemsSource = _treeNodes;

        // Subscribe to logger events
        _logger.LogAdded += OnLogAdded;

        // Subscribe to service events
        _fileWatchingService.FileChanged += OnFileWatchingService_FileChanged;        // Initialize settings
        _workingSettings = SettingsService.CloneSettings(SettingsManager.Instance.Settings);
        LoadSettingsIntoControls();

        // Set the settings file path for display
        SettingsFilePathTextBox.Text = SettingsManager.Instance.GetSettingsFilePath();

        // Apply settings on startup
        ApplySettings();

        _logger.Log("Application started");

        // Initialize global minimum width
        SizeChanged += MainWindow_SizeChanged;
        UpdateGlobalMinWidth(ActualWidth);
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        => UpdateGlobalMinWidth(e.NewSize.Width);

    private void UpdateGlobalMinWidth(double windowWidth)
    {
        Application.Current.Resources["AppMinWidth"] = windowWidth * MinPct;
    }

    /// <summary>
    /// Applies current settings to the UI
    /// </summary>
    private void ApplySettings()
    {
        var settings = SettingsManager.Instance.Settings;        // Show logs panel if configured to do so
        if (settings.ShowLogsOnStartup)
        {
            ShowLogsCheckBox.IsChecked = true;
            LogPanelRow.Height = new GridLength(200);
        }
    }
    private async void LoadFileButton_Click(object sender, RoutedEventArgs e)
    {
        var filePath = FileOperations.ShowOpenJkrFileDialog();
        if (!string.IsNullOrEmpty(filePath))
        {
            await LoadFile(filePath);
        }
    }

    #region Settings Management
    /// <summary>
    /// Loads the working settings into the UI controls
    /// </summary>
    private void LoadSettingsIntoControls()
    {
        _settingsUIService.LoadSettingsIntoControls(_workingSettings,
            DefaultJkrDirectoryTextBox, DefaultLuaExportDirectoryTextBox,
            ShowLogsOnStartupCheckBox, AutoSaveDecompressedFilesCheckBox,
            ConfirmFileOverwritesCheckBox, EnableAutoBackupCheckBox,
            BackupDirectoryTextBox, BalatroSaveRootTextBox, MaxLogEntriesTextBox,
            EnableFileWatchingCheckBox, FlashTaskbarOnUpdateCheckBox,
            AutoRefreshOnFileChangeCheckBox, LogLevelComboBox, ThemeComboBox,
            UpdateBalatroDerivedPaths);
    }    /// <summary>
         /// Saves the UI control values back to the working settings
         /// </summary>
    private void SaveControlsToSettings()
    {
        _settingsUIService.SaveControlsToSettings(_workingSettings,
            DefaultJkrDirectoryTextBox, DefaultLuaExportDirectoryTextBox,
            ShowLogsOnStartupCheckBox, AutoSaveDecompressedFilesCheckBox,
            ConfirmFileOverwritesCheckBox, EnableAutoBackupCheckBox,
            BackupDirectoryTextBox, BalatroSaveRootTextBox, MaxLogEntriesTextBox,
            EnableFileWatchingCheckBox, FlashTaskbarOnUpdateCheckBox,
            AutoRefreshOnFileChangeCheckBox, LogLevelComboBox, ThemeComboBox);
    }    /// <summary>
         /// Applies the working settings to the global settings manager
         /// </summary>
    private void ApplySettingsChanges()
    {
        SettingsService.ApplySettingsChanges(_workingSettings);
    }
    private void BrowseJkrDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsUIService.BrowseJkrDirectory(DefaultJkrDirectoryTextBox);
    }

    private void BrowseLuaExportDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsUIService.BrowseLuaExportDirectory(DefaultLuaExportDirectoryTextBox);
    }

    private void BrowseBackupDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsUIService.BrowseBackupDirectory(BackupDirectoryTextBox);
    }

    private void BrowseBalatroSaveRootButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsUIService.BrowseBalatroSaveRoot(BalatroSaveRootTextBox, UpdateBalatroDerivedPaths);
    }
    private void ApplySettingsButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsUIService.ApplySettings(
            SaveControlsToSettings,
            ApplySettingsChanges,
            () => _workingSettings,
            (settings) => _workingSettings = settings);
    }

    private void ResetToDefaultsButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsUIService.ResetToDefaults(
            (settings) => _workingSettings = settings,
            LoadSettingsIntoControls);
    }
    private void BalatroSaveRootTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateBalatroDerivedPaths();
    }
    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Only apply theme change if the selection is from user interaction
        // (not from programmatic loading)
        if (ThemeComboBox.SelectedItem is ComboBoxItem selectedItem && IsLoaded)
        {
            _settingsUIService.HandleThemeChange(_workingSettings, selectedItem, IsLoaded);
        }
    }
    private void UpdateBalatroDerivedPaths()
    {
        _balatroPathService.UpdateBalatroDerivedPaths(BalatroSaveRootTextBox.Text,
            InfoBalatroSaveRootTextBox, InfoBalatroSettingsFilePathTextBox,
            InfoCurrentProfileNumberTextBox, InfoCurrentProfileSettingsPathTextBox,
            InfoCurrentProfileMetaPathTextBox, InfoCurrentProfileSavePathTextBox);
    }
    private async void LoadBalatroSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        await _balatroPathService.LoadBalatroSettingsFileAsync(
            InfoBalatroSettingsFilePathTextBox.Text,
            LoadFile,
            () => MainTabControl.SelectedItem = TreeViewTab);
    }

    private async void LoadProfileSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        await _balatroPathService.LoadProfileSettingsAsync(
            InfoCurrentProfileSettingsPathTextBox.Text,
            LoadFile,
            () => MainTabControl.SelectedItem = TreeViewTab);
    }

    private async void LoadProfileMetaButton_Click(object sender, RoutedEventArgs e)
    {
        await _balatroPathService.LoadProfileMetaAsync(
            InfoCurrentProfileMetaPathTextBox.Text,
            LoadFile,
            () => MainTabControl.SelectedItem = TreeViewTab);
    }

    private async void LoadProfileSaveButton_Click(object sender, RoutedEventArgs e)
    {
        await _balatroPathService.LoadProfileSaveAsync(
            InfoCurrentProfileSavePathTextBox.Text,
            LoadFile,
            () => MainTabControl.SelectedItem = TreeViewTab);
    }

    private void OpenSettingsFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var settingsPath = SettingsManager.Instance.GetSettingsFilePath();
            var directory = Path.GetDirectoryName(settingsPath);

            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            {
                Process.Start("explorer.exe", directory);
            }
            else
            {
                MessageBox.Show("Settings directory not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.Log($"Error opening settings folder: {ex.Message}");
            MessageBox.Show($"Error opening settings folder: {ex.Message}", "Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region File Operations

    private async Task LoadFile(string filePath)
    {
        // Create loading context with UI elements
        var context = new FileLoadingContext(
            _treeNodes,
            RawContentTextBox,
            FilePathTextBox,
            FileSizeTextBox,
            LastModifiedTextBox,
            CompressionInfoTextBox,
            ContentTypeTextBox,
            EntriesCountTextBox,
            (path, content) =>
            {
                _currentFilePath = path;
                _currentDecompressedContent = content;
            },
            () =>
            {
                _currentFilePath = null;
                _currentDecompressedContent = null;
            });

        // Use the FileLoadingService to load the file
        var result = await _fileLoadingService.LoadFileAsync(filePath, context);

        // Update Lua button state after loading (success or failure)
        UpdateSaveAsLuaButtonState();
    }

    private void ShowLogsCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        LogPanelManager.ShowLogPanel(LogPanelRow, _logger);
    }

    private void ShowLogsCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        LogPanelManager.HideLogPanel(LogPanelRow, _logger);
    }

    private void ClearLogsButton_Click(object sender, RoutedEventArgs e)
    {
        LogPanelManager.ClearLogs(_logger, LogTextBox);
    }

    private async void SaveAsLuaButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFilePath) || string.IsNullOrEmpty(_currentDecompressedContent))
        {
            MessageBox.Show("No file is currently loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var settings = SettingsManager.Instance.Settings;

        // Use the LuaExportService to handle the export
        var success = await _luaExportService.SaveAsLuaAsync(
            _currentDecompressedContent,
            _currentFilePath,
            settings.DefaultLuaExportDirectory,
            settings.ConfirmFileOverwrites);

        if (success)
        {
            _uiStateService.SetStatusMessage($"Saved: {Path.GetFileName(_currentFilePath)}.lua");
            // Update button state since the file now exists
            _uiStateService.UpdateSaveAsLuaButtonState(SaveAsLuaButton, _currentFilePath);
        }
    }

    private void UpdateSaveAsLuaButtonState()
    {
        _uiStateService.UpdateSaveAsLuaButtonState(SaveAsLuaButton, _currentFilePath);
    }

    private void OnLogAdded(object? sender, string logMessage)
    {
        LogPanelManager.HandleLogAdded(LogTextBox, logMessage, Dispatcher);
    }
    protected override void OnClosed(EventArgs e)
    {
        // Clean up file watching resources
        _fileWatchingService.StopWatching();

        _logger.SaveToFile();
        base.OnClosed(e);
    }    /// <summary>
         /// Handles file change events from the file watching service
         /// </summary>
    private void OnFileWatchingService_FileChanged(object? sender, string filePath)
    {
        _fileChangeHandlerService.HandleFileChange(filePath, _currentFilePath, LoadFile, Dispatcher);
    }

    #endregion
}