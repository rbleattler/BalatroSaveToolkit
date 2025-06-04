using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;

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
    private int _maxLogEntries = 1000;
    private bool _enableAutoBackup = true;
    private string _backupDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BalatroBackups");

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
    }

    /// <summary>
    /// Directory for storing automatic backups
    /// </summary>
    public string BackupDirectory
    {
        get => _backupDirectory;
        set => SetProperty(ref _backupDirectory, value);
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
}
