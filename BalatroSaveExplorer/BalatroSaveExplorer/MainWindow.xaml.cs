using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.Collections.ObjectModel;
using BalatroSaveExplorer.Services;
using BalatroSaveExplorer.Windows;
using BalatroSaveExplorer.Models;
using System.Windows.Shell;
using System.Windows.Threading;
using System.Diagnostics;

namespace BalatroSaveExplorer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Logger _logger;
    private readonly ObservableCollection<TreeNodeViewModel> _treeNodes;
    private string? _currentFilePath;
    private string? _currentDecompressedContent;
    private FileSystemWatcher? _fileWatcher;
    private DateTime _lastFileUpdate;
    private bool _isWatchingDerivedFile;
    private DispatcherTimer _flashTimer;
    private int _flashCount;

    // Settings management fields
    private AppSettings _workingSettings;    public MainWindow()
    {
        InitializeComponent();

        _logger = new Logger();
        _treeNodes = new ObservableCollection<TreeNodeViewModel>();
        DataTreeView.ItemsSource = _treeNodes;

        // Initialize flash timer for taskbar notifications
        _flashTimer = new DispatcherTimer();
        _flashTimer.Interval = TimeSpan.FromMilliseconds(500);
        _flashTimer.Tick += FlashTimer_Tick;

        // Subscribe to logger events
        _logger.LogAdded += OnLogAdded;

        // Initialize settings
        _workingSettings = CloneSettings(SettingsManager.Instance.Settings);
        LoadSettingsIntoControls();

        // Set the settings file path for display
        SettingsFilePathTextBox.Text = SettingsManager.Instance.GetSettingsFilePath();

        // Apply settings on startup
        ApplySettings();

        _logger.Log("Application started");
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
    }    private void LoadFileButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = SettingsManager.Instance.Settings;
        var openFileDialog = new OpenFileDialog
        {
            Filter = "JKR Files (*.jkr)|*.jkr|All Files (*.*)|*.*",
            Title = "Select JKR File to Load",
            InitialDirectory = Directory.Exists(settings.DefaultJkrDirectory) ? settings.DefaultJkrDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };        if (openFileDialog.ShowDialog() == true)
        {
            LoadFile(openFileDialog.FileName);
        }    }

    #region Settings Management

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
            BackupDirectory = original.BackupDirectory,
            BalatroSaveRoot = original.BalatroSaveRoot,
            EnableFileWatching = original.EnableFileWatching,
            FlashTaskbarOnUpdate = original.FlashTaskbarOnUpdate,
            AutoRefreshOnFileChange = original.AutoRefreshOnFileChange
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
    }

    /// <summary>
    /// Applies the working settings to the global settings manager
    /// </summary>
    private void ApplySettingsChanges()
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
            UpdateBalatroDerivedPaths();
        }
    }

    private void ApplySettingsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveControlsToSettings();
            ApplySettingsChanges();
            _workingSettings = CloneSettings(SettingsManager.Instance.Settings);
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

    private void ResetToDefaultsButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to reset all settings to their default values?",
            "Reset Settings",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _workingSettings = new AppSettings(); // This creates a new instance with defaults
            LoadSettingsIntoControls();
            _logger.Log("Settings reset to defaults");
        }
    }

    private void BalatroSaveRootTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateBalatroDerivedPaths();
    }

    private void UpdateBalatroDerivedPaths()
    {
        var saveRoot = BalatroSaveRootTextBox.Text;

        if (string.IsNullOrWhiteSpace(saveRoot))
        {
            BalatroSettingsFilePathTextBox.Text = "";
            CurrentProfileNumberTextBox.Text = "";
            CurrentProfileSettingsPathTextBox.Text = "";
            CurrentProfileMetaPathTextBox.Text = "";
            CurrentProfileSavePathTextBox.Text = "";
            return;
        }

        try
        {
            var settingsPath = Path.Combine(saveRoot, "settings.jkr");
            BalatroSettingsFilePathTextBox.Text = settingsPath;

            // Try to determine current profile (this is a placeholder - would need actual logic)
            CurrentProfileNumberTextBox.Text = "1"; // Default profile

            var profilePath = Path.Combine(saveRoot, "1");
            CurrentProfileSettingsPathTextBox.Text = Path.Combine(profilePath, "profile.jkr");
            CurrentProfileMetaPathTextBox.Text = Path.Combine(profilePath, "meta.jkr");
            CurrentProfileSavePathTextBox.Text = Path.Combine(profilePath, "save.jkr");
        }
        catch (Exception ex)
        {
            _logger.Log($"Error updating derived paths: {ex.Message}");
        }
    }

    private void LoadBalatroSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var filePath = BalatroSettingsFilePathTextBox.Text;
        if (File.Exists(filePath))
        {
            LoadFile(filePath);
            MainTabControl.SelectedItem = TreeViewTab;
        }
        else
        {
            MessageBox.Show("Settings file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void LoadProfileSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var filePath = CurrentProfileSettingsPathTextBox.Text;
        if (File.Exists(filePath))
        {
            LoadFile(filePath);
            MainTabControl.SelectedItem = TreeViewTab;
        }
        else
        {
            MessageBox.Show("Profile settings file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void LoadProfileMetaButton_Click(object sender, RoutedEventArgs e)
    {
        var filePath = CurrentProfileMetaPathTextBox.Text;
        if (File.Exists(filePath))
        {
            LoadFile(filePath);
            MainTabControl.SelectedItem = TreeViewTab;
        }
        else
        {
            MessageBox.Show("Profile meta file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void LoadProfileSaveButton_Click(object sender, RoutedEventArgs e)
    {
        var filePath = CurrentProfileSavePathTextBox.Text;
        if (File.Exists(filePath))
        {
            LoadFile(filePath);
            MainTabControl.SelectedItem = TreeViewTab;
        }
        else
        {
            MessageBox.Show("Profile save file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
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

    public void LoadFile(string filePath)
    {
        try
        {
            StatusLabel.Content = "Loading file...";
            _logger.Log($"Loading file: {filePath}");

            var settings = SettingsManager.Instance.Settings;

            // Create backup if enabled
            if (settings.EnableAutoBackup)
            {
                CreateBackup(filePath, settings.BackupDirectory);
            }

            // Read and decompress the file content
            string content = ReadAndDecompressJkrFile(filePath);
            _logger.Log($"Decompressed file size: {content.Length} characters");

            // Store the current file info
            _currentFilePath = filePath;
            _currentDecompressedContent = content;

            // Auto-save decompressed content if enabled
            if (settings.AutoSaveDecompressedFiles)
            {
                SaveDecompressedToTemp(content, filePath);
            }

            // Parse the Lua table
            var parsedData = LuaTableConverter.ParseLuaTable(content);
            _logger.Log($"Successfully parsed Lua table with {parsedData.Count} root items");            // Clear existing tree and populate with new data
            _treeNodes.Clear();
            PopulateTreeView(parsedData);

            // Populate raw content tab
            PopulateRawContentTab(content);

            // Populate file info tab
            PopulateFileInfoTab(filePath);

            StatusLabel.Content = $"Loaded: {System.IO.Path.GetFileName(filePath)}";
            _logger.Log("File loaded successfully");// Update Save as Lua button state
            UpdateSaveAsLuaButtonState();

            // Setup file watching for derived Balatro files
            SetupFileWatching(filePath);
        }
        catch (Exception ex)
        {
            string errorMsg = $"Error loading file: {ex.Message}";
            StatusLabel.Content = "Error loading file";
            _logger.Log(errorMsg);
            MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Clear current file info on error
            _currentFilePath = null;
            _currentDecompressedContent = null;
            UpdateSaveAsLuaButtonState();
        }
    }

    private void PopulateTreeView(Dictionary<string, object> data)
    {
        foreach (var kvp in data)
        {
            var node = CreateTreeNode(kvp.Key, kvp.Value);
            _treeNodes.Add(node);
        }
    }

    private TreeNodeViewModel CreateTreeNode(string key, object? value)
    {
        var node = new TreeNodeViewModel
        {
            DisplayName = key
        };

        if (value is Dictionary<string, object> dict)
        {
            node.ValueDisplay = $"{{ {dict.Count} items }}";
            foreach (var kvp in dict)
            {
                node.Children.Add(CreateTreeNode(kvp.Key, kvp.Value));
            }
        }
        else if (value is List<object> list)
        {
            node.ValueDisplay = $"[ {list.Count} items ]";
            for (int i = 0; i < list.Count; i++)
            {
                node.Children.Add(CreateTreeNode($"[{i}]", list[i]));
            }
        }
        else
        {
            node.ValueDisplay = value?.ToString() ?? "null";
        }

        return node;
    }

    private void ShowLogsCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        LogPanelRow.Height = new GridLength(200);
        _logger.Log("Log panel shown");
    }

    private void ShowLogsCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        LogPanelRow.Height = new GridLength(0);
        _logger.Log("Log panel hidden");
    }
    private void ClearLogsButton_Click(object sender, RoutedEventArgs e)
    {
        _logger.ClearLogs();
        LogTextBox.Text = "";
    }    private void SaveAsLuaButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFilePath) || string.IsNullOrEmpty(_currentDecompressedContent))
        {
            MessageBox.Show("No file is currently loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var settings = SettingsManager.Instance.Settings;

            // Use SaveFileDialog with default directory from settings
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Lua Files (*.lua)|*.lua|All Files (*.*)|*.*",
                Title = "Save as Lua File",
                InitialDirectory = Directory.Exists(settings.DefaultLuaExportDirectory) ? settings.DefaultLuaExportDirectory : Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                FileName = Path.GetFileNameWithoutExtension(_currentFilePath) + ".lua"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string outputPath = saveFileDialog.FileName;

                // Check if file exists and confirm overwrite if settings require it
                if (File.Exists(outputPath) && settings.ConfirmFileOverwrites)
                {
                    var result = MessageBox.Show(
                        $"The file '{Path.GetFileName(outputPath)}' already exists. Do you want to overwrite it?",
                        "File Exists",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                _logger.Log($"Saving decompressed content to: {outputPath}");

                // Prepend "return = " to the content
                string luaContent = "return = " + _currentDecompressedContent;

                // Write the content to the .lua file
                File.WriteAllText(outputPath, luaContent, Encoding.UTF8);

                _logger.Log($"Successfully saved {luaContent.Length} characters to {outputPath}");
                StatusLabel.Content = $"Saved: {Path.GetFileName(outputPath)}";

                // Update button state since the file now exists
                UpdateSaveAsLuaButtonState();

                MessageBox.Show($"File saved successfully as:\n{outputPath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            string errorMsg = $"Error saving file: {ex.Message}";
            _logger.Log(errorMsg);
            MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateSaveAsLuaButtonState()
    {
        if (string.IsNullOrEmpty(_currentFilePath))
        {
            // No file loaded - disable button
            SaveAsLuaButton.IsEnabled = false;
            SaveAsLuaButton.Content = "Save as Lua";
            return;
        }

        string luaPath = System.IO.Path.ChangeExtension(_currentFilePath, ".lua");
        bool luaFileExists = File.Exists(luaPath);

        if (luaFileExists)
        {
            // File exists - gray out button and change text
            SaveAsLuaButton.IsEnabled = false;
            SaveAsLuaButton.Content = "Lua file exists";
            _logger.Log($"Lua file already exists: {luaPath}");
        }
        else
        {
            // File doesn't exist - enable button
            SaveAsLuaButton.IsEnabled = true;
            SaveAsLuaButton.Content = "Save as Lua";
        }
    }

    private void OnLogAdded(object? sender, string logMessage)
    {
        // Update UI on main thread
        Dispatcher.Invoke(() =>
        {
            LogTextBox.AppendText(logMessage + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        });
    }    protected override void OnClosed(EventArgs e)
    {
        // Clean up file watching resources
        StopFileWatching();

        _logger.SaveToFile();
        base.OnClosed(e);
    }
    private string ReadAndDecompressJkrFile(string filePath)
    {
        try
        {
            _logger.Log("Starting decompression...");

            using var compressedStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            long originalSize = compressedStream.Length;
            _logger.Log($"File size: {originalSize} bytes");

            using var outputStream = new MemoryStream();
            using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);

            deflateStream.CopyTo(outputStream);
            outputStream.Position = 0;

            using var reader = new StreamReader(outputStream, Encoding.UTF8);
            string decompressedContent = reader.ReadToEnd();

            _logger.Log($"Decompression successful. Original: {originalSize} bytes → Decompressed: {decompressedContent.Length} characters");

            if (decompressedContent.StartsWith("return "))
            {
                decompressedContent = decompressedContent.Substring("return ".Length);
            }
            return decompressedContent;
        }
        catch (InvalidDataException ex)
        {
            _logger.Log($"Not a compressed file: {ex.Message}");
            _logger.Log("Reading as plain text...");
            return File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            _logger.Log($"Decompression failed: {ex.Message}");
            _logger.Log("Attempting to read as plain text...");            // Fallback to reading as plain text (in case file is not compressed)
            return File.ReadAllText(filePath);
        }
    }

    /// <summary>
    /// Creates a backup of the specified file
    /// </summary>
    private void CreateBackup(string filePath, string backupDirectory)
    {
        try
        {
            // Ensure backup directory exists
            Directory.CreateDirectory(backupDirectory);

            var fileName = Path.GetFileName(filePath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
            var backupPath = Path.Combine(backupDirectory, backupFileName);

            File.Copy(filePath, backupPath);
            _logger.Log($"Created backup: {backupPath}");
        }
        catch (Exception ex)
        {
            _logger.Log($"Failed to create backup: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves decompressed content to a temporary file
    /// </summary>
    private void SaveDecompressedToTemp(string content, string originalFilePath)
    {
        try
        {
            var tempDir = Path.GetTempPath();
            var fileName = Path.GetFileNameWithoutExtension(originalFilePath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var tempFileName = $"{fileName}_decompressed_{timestamp}.lua";
            var tempPath = Path.Combine(tempDir, tempFileName);            File.WriteAllText(tempPath, content, Encoding.UTF8);
            _logger.Log($"Auto-saved decompressed content to: {tempPath}");
        }
        catch (Exception ex)
        {
            _logger.Log($"Failed to auto-save decompressed content: {ex.Message}");
        }
    }

    /// <summary>
    /// Sets up file watching for a derived JKR file
    /// </summary>
    private void SetupFileWatching(string filePath)
    {
        var settings = SettingsManager.Instance.Settings;

        if (!settings.EnableFileWatching)
        {
            return;
        }

        // Stop any existing watcher
        StopFileWatching();

        // Check if this is a derived Balatro file
        if (IsDerivedBalatroFile(filePath))
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                var fileName = Path.GetFileName(filePath);

                if (!string.IsNullOrEmpty(directory) && !string.IsNullOrEmpty(fileName) && Directory.Exists(directory))
                {
                    _fileWatcher = new FileSystemWatcher(directory, fileName);
                    _fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
                    _fileWatcher.Changed += OnFileChanged;
                    _fileWatcher.EnableRaisingEvents = true;
                    _isWatchingDerivedFile = true;
                    _lastFileUpdate = File.GetLastWriteTime(filePath);

                    UpdateWatchingIndicator(true);
                    _logger.Log($"Started watching file: {filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Failed to setup file watching: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Stops file watching
    /// </summary>
    private void StopFileWatching()
    {
        if (_fileWatcher != null)
        {
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Dispose();
            _fileWatcher = null;
            _isWatchingDerivedFile = false;
            UpdateWatchingIndicator(false);
            _logger.Log("Stopped file watching");
        }
    }

    /// <summary>
    /// Checks if the file is a derived Balatro file that should be watched
    /// </summary>
    private bool IsDerivedBalatroFile(string filePath)
    {
        var settings = SettingsManager.Instance.Settings;
        var normalizedPath = Path.GetFullPath(filePath).ToLowerInvariant();
        var normalizedRoot = Path.GetFullPath(settings.BalatroSaveRoot).ToLowerInvariant();

        return normalizedPath.StartsWith(normalizedRoot) &&
               (normalizedPath.EndsWith("profile.jkr") ||
                normalizedPath.EndsWith("meta.jkr") ||
                normalizedPath.EndsWith("save.jkr") ||
                normalizedPath.EndsWith("settings.jkr"));
    }

    /// <summary>
    /// Handles file change events
    /// </summary>
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            // Debounce multiple rapid file changes
            var lastWrite = File.GetLastWriteTime(e.FullPath);
            if (lastWrite <= _lastFileUpdate.AddMilliseconds(500))
            {
                return;
            }

            _lastFileUpdate = lastWrite;

            Dispatcher.Invoke(() =>
            {
                var settings = SettingsManager.Instance.Settings;

                _logger.Log($"File changed: {e.FullPath}");
                UpdateLastUpdateIndicator(lastWrite);

                // Flash taskbar if enabled
                if (settings.FlashTaskbarOnUpdate)
                {
                    FlashTaskbar();
                }

                // Auto-refresh if enabled
                if (settings.AutoRefreshOnFileChange)
                {
                    RefreshCurrentFile();
                }
            });
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => _logger.Log($"Error handling file change: {ex.Message}"));
        }
    }

    /// <summary>
    /// Updates the file watching indicator
    /// </summary>
    private void UpdateWatchingIndicator(bool isWatching)
    {
        if (isWatching)
        {
            WatchingIndicator.Fill = Brushes.LimeGreen;
            WatchingIndicator.Visibility = Visibility.Visible;
            LastUpdateLabel.Visibility = Visibility.Visible;
            UpdateLastUpdateIndicator(_lastFileUpdate);
        }
        else
        {
            WatchingIndicator.Visibility = Visibility.Collapsed;
            LastUpdateLabel.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Updates the last update time indicator
    /// </summary>
    private void UpdateLastUpdateIndicator(DateTime updateTime)
    {
        LastUpdateLabel.Content = $"Updated: {updateTime:HH:mm:ss}";
    }

    /// <summary>
    /// Flashes the taskbar icon
    /// </summary>
    private void FlashTaskbar()
    {
        _flashCount = 0;
        _flashTimer.Start();
    }

    /// <summary>
    /// Timer tick event for taskbar flashing
    /// </summary>
    private void FlashTimer_Tick(object? sender, EventArgs e)
    {
        var taskbarItemInfo = TaskbarItemInfo ?? new TaskbarItemInfo();

        if (_flashCount % 2 == 0)
        {
            taskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            taskbarItemInfo.ProgressValue = 1.0;
        }
        else
        {
            taskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
        }

        TaskbarItemInfo = taskbarItemInfo;
        _flashCount++;

        if (_flashCount >= 6) // Flash 3 times
        {
            _flashTimer.Stop();
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
        }
    }

    /// <summary>
    /// Refreshes the current file by reloading it
    /// </summary>
    private void RefreshCurrentFile()
    {
        if (!string.IsNullOrEmpty(_currentFilePath) && File.Exists(_currentFilePath))
        {
            _logger.Log("Auto-refreshing current file...");

            // Temporarily disable file watching to avoid recursive updates
            var wasWatching = _isWatchingDerivedFile;
            if (wasWatching)
            {
                _fileWatcher!.EnableRaisingEvents = false;
            }

            try
            {
                LoadFile(_currentFilePath);
            }
            finally
            {
                // Re-enable file watching
                if (wasWatching && _fileWatcher != null)
                {
                    _fileWatcher.EnableRaisingEvents = true;
                }
            }
        }
    }

    private void PopulateRawContentTab(string content)
    {
        try
        {
            // Format the content with "return " prefix for proper Lua syntax
            string formattedContent = "return " + content;
            RawContentTextBox.Text = formattedContent;
            _logger.Log($"Raw content tab populated with {formattedContent.Length} characters");
        }
        catch (Exception ex)
        {
            _logger.Log($"Error populating raw content tab: {ex.Message}");
            RawContentTextBox.Text = "Error displaying raw content: " + ex.Message;
        }
    }

    private void PopulateFileInfoTab(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);

            FilePathTextBox.Text = filePath;
            FileSizeTextBox.Text = $"{fileInfo.Length:N0} bytes ({FormatFileSize(fileInfo.Length)})";
            LastModifiedTextBox.Text = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");

            // Try to determine compression info
            string compressionInfo = "Unknown";
            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var deflateStream = new DeflateStream(stream, CompressionMode.Decompress);
                compressionInfo = "Deflate compressed";
            }
            catch
            {
                compressionInfo = "Not compressed (plain text)";
            }

            CompressionInfoTextBox.Text = compressionInfo;
            ContentTypeTextBox.Text = "Balatro Save Data (JKR)";

            // Count entries by parsing if possible
            if (!string.IsNullOrEmpty(_currentDecompressedContent))
            {
                try
                {
                    var parsedData = LuaTableConverter.ParseLuaTable(_currentDecompressedContent);
                    int totalEntries = CountEntries(parsedData);
                    EntriesCountTextBox.Text = $"{totalEntries:N0} entries";
                }
                catch
                {
                    EntriesCountTextBox.Text = "Unable to count entries";
                }
            }
            else
            {
                EntriesCountTextBox.Text = "No content loaded";
            }

            _logger.Log("File info tab populated successfully");
        }
        catch (Exception ex)
        {
            _logger.Log($"Error populating file info tab: {ex.Message}");
            FilePathTextBox.Text = "Error: " + ex.Message;
        }
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private int CountEntries(Dictionary<string, object> data)
    {
        int count = data.Count;
        foreach (var value in data.Values)
        {
            if (value is Dictionary<string, object> dict)
            {
                count += CountEntries(dict);
            }
            else if (value is List<object> list)
            {
                count += CountListEntries(list);
            }
        }
        return count;
    }

    private int CountListEntries(List<object> list)
    {
        int count = list.Count;
        foreach (var item in list)
        {
            if (item is Dictionary<string, object> dict)
            {
                count += CountEntries(dict);
            }
            else if (item is List<object> subList)
            {
                count += CountListEntries(subList);
            }
        }
        return count;
    }

    #endregion

    #region UI Event Handlers


    #endregion
}