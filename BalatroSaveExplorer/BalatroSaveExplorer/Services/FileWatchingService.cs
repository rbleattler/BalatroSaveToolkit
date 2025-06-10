using System;
using System.IO;
using System.Threading.Tasks;
using BalatroSaveExplorer.Models;
using BalatroSaveExplorer.Services;

namespace BalatroSaveExplorer.Services;

/// <summary>
/// Service responsible for monitoring file changes and handling file watching operations.
/// </summary>
public class FileWatchingService : IDisposable
{
  private readonly Logger _logger;
  private FileSystemWatcher? _fileWatcher;
  private DateTime _lastFileUpdate;
  private bool _isWatchingDerivedFile;
  private string? _watchedFilePath;

  public event EventHandler<string>? FileChanged;
  public event EventHandler<string>? FileDeleted;
  public event EventHandler<string>? FileCreated;

  public FileWatchingService(Logger logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  // TODO: Migrate from MainWindow
  // - StartWatching method
  // - StopWatching method
  // - OnFileChanged event handler
  // - OnFileDeleted event handler
  // - OnFileCreated event handler
  // - File watching state management
  /// <summary>
  /// Starts watching the specified file or directory.
  /// </summary>
  /// <param name="filePath">Path to watch</param>
  /// <param name="includeSubdirectories">Whether to include subdirectories</param>
  public void StartWatching(string filePath, bool includeSubdirectories = false)
  {
    if (string.IsNullOrEmpty(filePath))
    {
      _logger.Log("Cannot start watching: file path is null or empty");
      return;
    }

    // Stop any existing watcher
    StopWatching();

    // Check if this is a derived Balatro file that should be watched
    if (IsDerivedBalatroFile(filePath))
    {
      try
      {
        var directory = Path.GetDirectoryName(filePath);
        var fileName = Path.GetFileName(filePath);

        if (!string.IsNullOrEmpty(directory) && !string.IsNullOrEmpty(fileName) && Directory.Exists(directory))
        {
          SetupWatcher(directory, fileName);
          _watchedFilePath = filePath;
          _isWatchingDerivedFile = true;
          _lastFileUpdate = File.GetLastWriteTime(filePath);

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
  /// Stops watching the current file or directory.
  /// </summary>
  public void StopWatching()
  {
    CleanupWatcher();
    _watchedFilePath = null;
    _isWatchingDerivedFile = false;
    _logger.Log("Stopped file watching");
  }
  /// <summary>
  /// Gets the current file watching state.
  /// </summary>
  /// <returns>Current watching state information</returns>
  public FileWatchingState GetWatchingState()
  {
    return new FileWatchingState
    {
      IsWatching = IsWatching,
      WatchedPath = _watchedFilePath,
      LastUpdate = _lastFileUpdate,
      IsWatchingDerivedFile = _isWatchingDerivedFile
    };
  }

  /// <summary>
  /// Checks if the service is currently watching a file.
  /// </summary>
  /// <returns>True if watching a file</returns>
  public bool IsWatching => _fileWatcher?.EnableRaisingEvents ?? false;

  /// <summary>
  /// Gets the path of the currently watched file.
  /// </summary>
  public string? WatchedFilePath => _watchedFilePath;
  private void OnFileSystemChanged(object sender, FileSystemEventArgs e)
  {
    HandleFileSystemEvent(e);
  }

  private void OnFileSystemDeleted(object sender, FileSystemEventArgs e)
  {
    HandleFileSystemEvent(e);
  }

  private void OnFileSystemCreated(object sender, FileSystemEventArgs e)
  {
    HandleFileSystemEvent(e);
  }

  /// <summary>
  /// Handles file system events from the watcher.
  /// </summary>
  /// <param name="e">File system event args</param>
  private void HandleFileSystemEvent(FileSystemEventArgs e)
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

      _logger.Log($"File changed: {e.FullPath}");

      // Raise appropriate event based on change type
      switch (e.ChangeType)
      {
        case WatcherChangeTypes.Changed:
          FileChanged?.Invoke(this, e.FullPath);
          break;
        case WatcherChangeTypes.Created:
          FileCreated?.Invoke(this, e.FullPath);
          break;
        case WatcherChangeTypes.Deleted:
          FileDeleted?.Invoke(this, e.FullPath);
          break;
      }
    }
    catch (Exception ex)
    {
      _logger.Log($"Error handling file change: {ex.Message}");
    }
  }

  /// <summary>
  /// Validates if a file path should be watched (derived Balatro file).
  /// </summary>
  /// <param name="filePath">File path to validate</param>
  /// <returns>True if file should be watched</returns>
  private bool IsDerivedBalatroFile(string filePath)
  {
    try
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
    catch (Exception ex)
    {
      _logger.Log($"Error checking if file is derived Balatro file: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Sets up the file system watcher for the specified path.
  /// </summary>
  /// <param name="directory">Directory to watch</param>
  /// <param name="fileName">File name to watch</param>
  private void SetupWatcher(string directory, string fileName)
  {
    _fileWatcher = new FileSystemWatcher(directory, fileName);
    _fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
    _fileWatcher.Changed += OnFileSystemChanged;
    _fileWatcher.Created += OnFileSystemCreated;
    _fileWatcher.Deleted += OnFileSystemDeleted;
    _fileWatcher.EnableRaisingEvents = true;
  }

  /// <summary>
  /// Cleans up watcher resources.
  /// </summary>
  private void CleanupWatcher()
  {
    if (_fileWatcher != null)
    {
      _fileWatcher.EnableRaisingEvents = false;
      _fileWatcher.Dispose();
      _fileWatcher = null;
    }
  }
  public void Dispose()
  {
    StopWatching();
    _fileWatcher?.Dispose();
  }
}
