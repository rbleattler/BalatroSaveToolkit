using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.Collections.ObjectModel;

namespace BalatroSaveExplorer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Logger _logger;
    private readonly ObservableCollection<TreeNodeViewModel> _treeNodes; public MainWindow()
    {
        InitializeComponent();

        _logger = new Logger();
        _treeNodes = new ObservableCollection<TreeNodeViewModel>();
        DataTreeView.ItemsSource = _treeNodes;

        // Subscribe to logger events
        _logger.LogAdded += OnLogAdded;

        _logger.Log("Application started");
    }

    private void LoadFileButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "JKR Files (*.jkr)|*.jkr|All Files (*.*)|*.*",
            Title = "Select JKR File to Load"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            LoadFile(openFileDialog.FileName);
        }
    }
    private void LoadFile(string filePath)
    {
        try
        {
            StatusLabel.Content = "Loading file...";
            _logger.Log($"Loading file: {filePath}");

            // Read and decompress the file content
            string content = ReadAndDecompressJkrFile(filePath);
            _logger.Log($"Decompressed file size: {content.Length} characters");

            // Parse the Lua table
            var parsedData = LuaTableConverter.ParseLuaTable(content);
            _logger.Log($"Successfully parsed Lua table with {parsedData.Count} root items");

            // Clear existing tree and populate with new data
            _treeNodes.Clear();
            PopulateTreeView(parsedData);

            StatusLabel.Content = $"Loaded: {System.IO.Path.GetFileName(filePath)}";
            _logger.Log("File loaded successfully");
        }
        catch (Exception ex)
        {
            string errorMsg = $"Error loading file: {ex.Message}";
            StatusLabel.Content = "Error loading file";
            _logger.Log(errorMsg);
            MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            _logger.Log("Attempting to read as plain text...");

            // Fallback to reading as plain text (in case file is not compressed)
            return File.ReadAllText(filePath);
        }
    }
}