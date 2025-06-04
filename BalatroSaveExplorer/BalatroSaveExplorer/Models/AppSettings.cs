using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace BalatroSaveExplorer.Models;

/// <summary>
/// Represents the application settings with property change notifications
/// </summary>
public class AppSettings : INotifyPropertyChanged
{
    private string _defaultJkrDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private string _defaultLuaExportDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    private bool _showLogsOnStartup = false;
    private bool _autoSaveDecompressedFiles = false;
    private bool _confirmFileOverwrites = true;
    private string _logLevel = "Info";
    private int _maxLogEntries = 1000;    private bool _enableAutoBackup = true;
    private string _backupDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BalatroBackups");
    private string _balatroSaveRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Balatro");

    /// <summary>
    /// Default directory for opening JKR files
    /// </summary>
    public string DefaultJkrDirectory
    {
        get => _defaultJkrDirectory;
        set => SetProperty(ref _defaultJkrDirectory, value);
    }

    /// <summary>
    /// Default directory for exporting Lua files
    /// </summary>
    public string DefaultLuaExportDirectory
    {
        get => _defaultLuaExportDirectory;
        set => SetProperty(ref _defaultLuaExportDirectory, value);
    }

    /// <summary>
    /// Whether to show the logs panel on application startup
    /// </summary>
    public bool ShowLogsOnStartup
    {
        get => _showLogsOnStartup;
        set => SetProperty(ref _showLogsOnStartup, value);
    }

    /// <summary>
    /// Whether to automatically save decompressed content to temp files
    /// </summary>
    public bool AutoSaveDecompressedFiles
    {
        get => _autoSaveDecompressedFiles;
        set => SetProperty(ref _autoSaveDecompressedFiles, value);
    }

    /// <summary>
    /// Whether to confirm before overwriting existing files
    /// </summary>
    public bool ConfirmFileOverwrites
    {
        get => _confirmFileOverwrites;
        set => SetProperty(ref _confirmFileOverwrites, value);
    }

    /// <summary>
    /// Log level (Debug, Info, Warning, Error)
    /// </summary>
    public string LogLevel
    {
        get => _logLevel;
        set => SetProperty(ref _logLevel, value);
    }

    /// <summary>
    /// Maximum number of log entries to keep in memory
    /// </summary>
    public int MaxLogEntries
    {
        get => _maxLogEntries;
        set => SetProperty(ref _maxLogEntries, value);
    }

    /// <summary>
    /// Whether to enable automatic backup of JKR files before processing
    /// </summary>
    public bool EnableAutoBackup
    {
        get => _enableAutoBackup;
        set => SetProperty(ref _enableAutoBackup, value);
    }    /// <summary>
    /// Directory for storing automatic backups
    /// </summary>
    public string BackupDirectory
    {
        get => _backupDirectory;
        set => SetProperty(ref _backupDirectory, value);
    }

    /// <summary>
    /// Root directory for Balatro save files (configurable)
    /// </summary>
    public string BalatroSaveRoot
    {
        get => _balatroSaveRoot;
        set => SetProperty(ref _balatroSaveRoot, value);
    }

    /// <summary>
    /// Path to Balatro's main settings file (derived)
    /// </summary>
    public string BalatroSettingsFilePath => Path.Combine(BalatroSaveRoot, "settings.jkr");    /// <summary>
    /// Current profile number (derived from settings.jkr)
    /// </summary>
    public int CurrentProfileNumber
    {
        get
        {
            try
            {
                if (File.Exists(BalatroSettingsFilePath))
                {
                    // Read and parse the settings.jkr file to get the current profile
                    var settingsContent = ReadAndDecompressJkrFile(BalatroSettingsFilePath);

                    // Simple regex-based parsing to find profile value
                    var match = System.Text.RegularExpressions.Regex.Match(settingsContent, @"profile\s*=\s*(\d+)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int profileNum))
                    {
                        return profileNum;
                    }
                }
            }
            catch
            {
                // If we can't read the file or parse it, fall back to default
            }
            return 1; // Default profile
        }
    }

    /// <summary>
    /// Path to current profile's settings file (derived)
    /// </summary>
    public string CurrentProfileSettingsPath => Path.Combine(BalatroSaveRoot, CurrentProfileNumber.ToString(), "profile.jkr");

    /// <summary>
    /// Path to current profile's meta file (derived)
    /// </summary>
    public string CurrentProfileMetaPath => Path.Combine(BalatroSaveRoot, CurrentProfileNumber.ToString(), "meta.jkr");

    /// <summary>
    /// Path to current profile's save file (derived)
    /// </summary>
    public string CurrentProfileSavePath => Path.Combine(BalatroSaveRoot, CurrentProfileNumber.ToString(), "save.jkr");

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Helper method to read and decompress JKR files for profile detection
    /// </summary>
    private string ReadAndDecompressJkrFile(string filePath)
    {
        try
        {
            using var compressedStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();
            using var deflateStream = new System.IO.Compression.DeflateStream(compressedStream, System.IO.Compression.CompressionMode.Decompress);

            deflateStream.CopyTo(outputStream);
            var decompressedBytes = outputStream.ToArray();
            var content = System.Text.Encoding.UTF8.GetString(decompressedBytes);

            // Remove "return = " prefix if present
            if (content.StartsWith("return = "))
            {
                content = content.Substring(9);
            }

            return content;
        }
        catch
        {
            // If decompression fails, try reading as plain text
            return File.ReadAllText(filePath);
        }
    }
}
