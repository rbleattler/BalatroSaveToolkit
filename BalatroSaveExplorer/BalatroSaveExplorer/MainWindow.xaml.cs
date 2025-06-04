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

namespace BalatroSaveExplorer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Logger _logger;
    private readonly ObservableCollection<TreeNodeViewModel> _treeNodes;
    private string? _currentFilePath;
    private string? _currentDecompressedContent;    public MainWindow()
    {
        InitializeComponent();

        _logger = new Logger();
        _treeNodes = new ObservableCollection<TreeNodeViewModel>();
        DataTreeView.ItemsSource = _treeNodes;

        // Subscribe to logger events
        _logger.LogAdded += OnLogAdded;

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
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow()
        {
            Owner = this
        };

        if (settingsWindow.ShowDialog() == true)
        {
            // Settings were saved, apply them
            ApplySettings();
            _logger.Log("Settings updated and applied");
        }
    }

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
            _logger.Log($"Successfully parsed Lua table with {parsedData.Count} root items");

            // Clear existing tree and populate with new data
            _treeNodes.Clear();
            PopulateTreeView(parsedData);

            StatusLabel.Content = $"Loaded: {System.IO.Path.GetFileName(filePath)}";
            _logger.Log("File loaded successfully");

            // Update Save as Lua button state
            UpdateSaveAsLuaButtonState();
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
    }
    protected override void OnClosed(EventArgs e)
    {
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
            var tempPath = Path.Combine(tempDir, tempFileName);

            File.WriteAllText(tempPath, content, Encoding.UTF8);
            _logger.Log($"Auto-saved decompressed content to: {tempPath}");
        }
        catch (Exception ex)
        {
            _logger.Log($"Failed to auto-save decompressed content: {ex.Message}");
        }
    }
}