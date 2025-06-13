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
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // Services
    private readonly Logger _logger;
    private readonly JkrFileService _jkrFileService;
    private readonly BackupService _backupService;
    private readonly FileWatchingService _fileWatchingService;
    private readonly UIStateService _uiStateService;
    private readonly FileLoadingService _fileLoadingService;
    private readonly SettingsUIService _settingsUIService;
    private readonly BalatroPathService _balatroPathService;
    private readonly FileChangeHandlerService _fileChangeHandlerService;
    private readonly SaveManagementService _saveManagementService;

    // UI-specific fields only
    private string? _currentFilePath;
    private string? _currentDecompressedContent;

    // Settings management fields
    private AppSettings _workingSettings;

    // e.g. 0.8 = 80%
    private const double MinPct = 0.85;

    public MainWindow()
    {
        InitializeComponent();        // Initialize logger first
        _logger = new Logger();        // Initialize services
        _jkrFileService = new JkrFileService(_logger);
        _backupService = new BackupService(_logger);
        _fileWatchingService = new FileWatchingService(_logger);
        _uiStateService = new UIStateService(_logger);
        _fileLoadingService = new FileLoadingService(_logger, _jkrFileService, _backupService, _uiStateService, _fileWatchingService);
        _settingsUIService = new SettingsUIService(_logger);
        _balatroPathService = new BalatroPathService(_logger);
        _fileChangeHandlerService = new FileChangeHandlerService(_logger, _uiStateService);

        // Initialize settings first, then SaveManagementService
        _workingSettings = SettingsService.CloneSettings(SettingsManager.Instance.Settings);
        _saveManagementService = new SaveManagementService(_logger, _backupService, SettingsManager.Instance.Settings);        // Subscribe to logger events
        _logger.LogAdded += OnLogAdded;        // Subscribe to service events
        _fileWatchingService.FileChanged += OnFileWatchingService_FileChanged;

        // Set up InfoTab callbacks
        InfoTabControl.LoadFileCallback = LoadFile;
        InfoTabControl.SwitchToTreeViewTabCallback = () => MainTabControl.SelectedItem = TreeViewTab;        // Set up SettingsTab callbacks and dependencies
        SettingsTabControl.SettingsUIService = _settingsUIService;
        SettingsTabControl.Logger = _logger;
        SettingsTabControl.LogPanelRow = LogPanelRow;
        SettingsTabControl.WorkingSettings = _workingSettings;
        SettingsTabControl.SaveManagementService = _saveManagementService;
        SettingsTabControl.ApplySettingsCallback = (settings, saveControls, applyChanges, getSettings, setSettings) =>
        {
            _settingsUIService.ApplySettings(saveControls, applyChanges, getSettings, setSettings);
        };
        SettingsTabControl.ResetToDefaultsCallback = (setSettings, loadControls) =>
        {
            _settingsUIService.ResetToDefaults(setSettings, loadControls);
        }; SettingsTabControl.BrowseBalatroSaveRootCallback = (path, updateCallback) =>
        {
            // The SettingsTab UserControl will handle this internally via SettingsUIService
            _settingsUIService.BrowseBalatroSaveRoot(SettingsTabControl.BalatroSaveRootTextBoxControl, updateCallback);
        };
        SettingsTabControl.UpdateBalatroDerivedPathsCallback = UpdateBalatroDerivedPaths; SettingsTabControl.ShowLogPanelCallback = (logPanelRow, logger) => LogPanelManager.ShowLogPanel(logPanelRow, logger);
        SettingsTabControl.HideLogPanelCallback = (logPanelRow, logger) => LogPanelManager.HideLogPanel(logPanelRow, logger);        // Set the settings file path for display and load settings into controls
        SettingsTabControl.SetSettingsFilePath(SettingsManager.Instance.GetSettingsFilePath());
        SettingsTabControl.LoadSettingsIntoControls();

        // Set up SaveTab with SaveManagementService
        SaveTabControl.SaveManagementService = _saveManagementService;
        SaveTabControl.UpdateCurrentProfileInfo(1, _saveManagementService.GetCurrentSaveFilePath());
        SaveTabControl.RefreshSaveBackupsList();

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
    }    /// <summary>
         /// Applies current settings to the UI
         /// </summary>
    private void ApplySettings()
    {
        var settings = SettingsManager.Instance.Settings;

        // Show logs panel if configured to do so
        if (settings.ShowLogsOnStartup)
        {
            SettingsTabControl.SetShowLogsCheckBoxState(true);
            LogPanelRow.Height = new GridLength(200);
        }
    }

    // This is still used by allowing files to be dragged onto the window, despite the button being gone
    private async void LoadFileButton_Click(object sender, RoutedEventArgs e)
    {
        var filePath = FileOperations.ShowOpenJkrFileDialog();
        if (!string.IsNullOrEmpty(filePath))
        {
            await LoadFile(filePath);
        }
    }

    #region Settings Management
    private void UpdateBalatroDerivedPaths()
    {
        _balatroPathService.UpdateBalatroDerivedPaths(SettingsTabControl.BalatroSaveRootTextBoxControl.Text,
            (rootPath, settingsPath, profileNumber, profileSettingsPath, profileMetaPath, profileSavePath) =>
            {
                InfoTabControl.UpdateBalatroDerivedPaths(rootPath, settingsPath, profileNumber,
                    profileSettingsPath, profileMetaPath, profileSavePath);
            });
    }
    #endregion

    #region File Operations

    private async Task LoadFile(string filePath)
    {
        // Create loading context with UI elements from FileInfoTab
        var context = new FileLoadingContext(
            TreeViewTabControl.TreeNodes,
            FileInfoTabControl.GetRawContentTextBox(),
            FileInfoTabControl.GetFilePathTextBox(),
            FileInfoTabControl.GetFileSizeTextBox(),
            FileInfoTabControl.GetLastModifiedTextBox(),
            FileInfoTabControl.GetCompressionInfoTextBox(),
            FileInfoTabControl.GetContentTypeTextBox(),
            FileInfoTabControl.GetEntriesCountTextBox(), (path, content) =>
            {
                _currentFilePath = path;
                _currentDecompressedContent = content;
                // Update TreeViewTab with current file info for SaveAsLua functionality
                TreeViewTabControl.SetCurrentFile(path, content);
            },
            () =>
            {
                _currentFilePath = null;
                _currentDecompressedContent = null;
                // Clear TreeViewTab file info
                TreeViewTabControl.SetCurrentFile(null, null);
            });

        // Use the FileLoadingService to load the file
        var result = await _fileLoadingService.LoadFileAsync(filePath, context);
    }
    private void ClearLogsButton_Click(object sender, RoutedEventArgs e)
    {
        LogPanelManager.ClearLogs(_logger, LogTextBox);
    }

    private void OnLogAdded(object? sender, string logMessage)
    {
        LogPanelManager.HandleLogAdded(LogTextBox, logMessage, Dispatcher);
    }
    protected override void OnClosed(EventArgs e)
    {
        // Clean up file watching resources
        _fileWatchingService.StopWatching();

        // Clean up save management service
        _saveManagementService.Dispose();

        _logger.SaveToFile();
        base.OnClosed(e);
    }/// <summary>
     /// Handles file change events from the file watching service
     /// </summary>
    private void OnFileWatchingService_FileChanged(object? sender, string filePath)
    {
        _fileChangeHandlerService.HandleFileChange(filePath, _currentFilePath, LoadFile, Dispatcher);
    }

    private void OpenSettingsFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var settingsPath = SettingsManager.Instance.GetSettingsFilePath();
            var settingsDirectory = Path.GetDirectoryName(settingsPath);

            if (!string.IsNullOrEmpty(settingsDirectory) && Directory.Exists(settingsDirectory))
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = settingsDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.Log($"Error opening settings folder: {ex.Message}");
            MessageBox.Show($"Could not open settings folder: {ex.Message}", "Error",
                           MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Drag-and-drop support for .jkr files
    private void Window_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1 && Path.GetExtension(files[0]).Equals(".jkr", StringComparison.OrdinalIgnoreCase))
            {
                // Only allow drop if not over a file path TextBox
                if (!IsOverFilePathRegion(e.OriginalSource))
                {
                    e.Effects = DragDropEffects.Copy;
                    e.Handled = true;
                    return;
                }
            }
        }
        e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private async void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1 && Path.GetExtension(files[0]).Equals(".jkr", StringComparison.OrdinalIgnoreCase))
            {
                if (!IsOverFilePathRegion(e.OriginalSource))
                {
                    await LoadFile(files[0]);
                    e.Handled = true;
                }
            }
        }
    }

    /// <summary>
    /// Determines if the drag/drop target is a file path TextBox or its child.
    /// </summary>
    private bool IsOverFilePathRegion(object? originalSource)
    {
        // Check if the original source is a TextBox used for file paths
        DependencyObject? current = originalSource as DependencyObject;
        while (current != null)
        {
            if (current is TextBox tb)
            {
                // Check for known file path TextBox names
                var name = tb.Name?.ToLowerInvariant();
                if (name != null && (name.Contains("filepath") || name.Contains("balatrosaveroot") || name.Contains("settingsfilepath")))
                    return true;
            }
            current = VisualTreeHelper.GetParent(current);
        }
        return false;
    }

    #endregion
}