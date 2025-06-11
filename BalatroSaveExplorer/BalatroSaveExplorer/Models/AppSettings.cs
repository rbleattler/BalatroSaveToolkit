using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text.RegularExpressions;

namespace BalatroSaveExplorer.Models;

/// <summary>
/// Represents the application settings with property change notifications
/// </summary>
public class AppSettings : INotifyPropertyChanged
{
    private static string _appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BalatroSaveExplorer");
    public static string AppDataDirectory => _appDataDirectory;
    private string _balatroSaveRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Balatro");
    private string _defaultJkrDirectory = Path.Combine(_appDataDirectory, "Saves");
    private string _defaultLuaExportDirectory = Path.Combine(_appDataDirectory, "LuaExports");
    private string _backupDirectory = Path.Combine(_appDataDirectory, "BalatroBackups");
    private bool _showLogsOnStartup = false;

    private bool _autoSaveDecompressedFiles = false;
    private bool _confirmFileOverwrites = true;
    private string _logLevel = "Info"; private int _maxLogEntries = 1000;
    private bool _enableAutoBackup = true;

    // Theme settings
    private AppTheme _theme = AppTheme.System;    // File watching settings
    private bool _enableFileWatching = true;
    private bool _flashTaskbarOnUpdate = true;
    private bool _autoRefreshOnFileChange = true;

    // Save management settings
    private bool _enableAutoSave = false;
    private int _autoSaveInterval = 5;
    private string _autoSaveIntervalUnit = "Minutes";
    private bool _enableSaveRetention = true;
    private int _saveRetentionValue = 7;
    private string _saveRetentionUnit = "Days";

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
                    var match = Regex.Match(settingsContent, @"profile\s*=\s*(\d+)");
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
    public string CurrentProfileSettingsPath => Path.Combine(BalatroSaveRoot, CurrentProfileNumber.ToString(), "profile.jkr");    /// <summary>
                                                                                                                                  /// Path to current profile's meta file (derived)
                                                                                                                                  /// </summary>
    public string CurrentProfileMetaPath => Path.Combine(BalatroSaveRoot, CurrentProfileNumber.ToString(), "meta.jkr");

    /// <summary>
    /// Path to current profile's save file (derived)
    /// </summary>
    public string CurrentProfileSavePath => Path.Combine(BalatroSaveRoot, CurrentProfileNumber.ToString(), "save.jkr");

    /// <summary>
    /// Application theme setting
    /// </summary>
    public AppTheme Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }

    /// <summary>
    /// Whether to enable file watching for derived JKR files
    /// </summary>
    public bool EnableFileWatching
    {
        get => _enableFileWatching;
        set => SetProperty(ref _enableFileWatching, value);
    }

    /// <summary>
    /// Whether to flash taskbar when watched files are updated
    /// </summary>
    public bool FlashTaskbarOnUpdate
    {
        get => _flashTaskbarOnUpdate;
        set => SetProperty(ref _flashTaskbarOnUpdate, value);
    }    /// <summary>
         /// Whether to automatically refresh the view when watched files change
         /// </summary>
    public bool AutoRefreshOnFileChange
    {
        get => _autoRefreshOnFileChange;
        set => SetProperty(ref _autoRefreshOnFileChange, value);
    }

    /// <summary>
    /// Whether to enable automatic save backups
    /// </summary>
    public bool EnableAutoSave
    {
        get => _enableAutoSave;
        set => SetProperty(ref _enableAutoSave, value);
    }

    /// <summary>
    /// Auto save interval value
    /// </summary>
    public int AutoSaveInterval
    {
        get => _autoSaveInterval;
        set => SetProperty(ref _autoSaveInterval, value);
    }

    /// <summary>
    /// Auto save interval unit (Seconds, Minutes, Hours, Days)
    /// </summary>
    public string AutoSaveIntervalUnit
    {
        get => _autoSaveIntervalUnit;
        set => SetProperty(ref _autoSaveIntervalUnit, value);
    }

    /// <summary>
    /// Whether to enable save retention (auto-delete old saves)
    /// </summary>
    public bool EnableSaveRetention
    {
        get => _enableSaveRetention;
        set => SetProperty(ref _enableSaveRetention, value);
    }

    /// <summary>
    /// Save retention value
    /// </summary>
    public int SaveRetentionValue
    {
        get => _saveRetentionValue;
        set => SetProperty(ref _saveRetentionValue, value);
    }

    /// <summary>
    /// Save retention unit (Hours, Days, Weeks, Months)
    /// </summary>
    public string SaveRetentionUnit
    {
        get => _saveRetentionUnit;
        set => SetProperty(ref _saveRetentionUnit, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
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
