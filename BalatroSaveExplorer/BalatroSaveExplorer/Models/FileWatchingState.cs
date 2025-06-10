using System;

namespace BalatroSaveExplorer.Models;

/// <summary>
/// Represents the current state of file watching operations.
/// </summary>
public class FileWatchingState
{
  public bool IsWatching { get; set; }
  public string? WatchedPath { get; set; }
  public DateTime LastUpdate { get; set; }
  public bool IsWatchingDerivedFile { get; set; }
  public int WatchInterval { get; set; }
  public FileWatchingMode Mode { get; set; }
  public DateTime WatchStartTime { get; set; }
  public int ChangeCount { get; set; }
  public string? LastChangeType { get; set; }

  public FileWatchingState()
  {
    LastUpdate = DateTime.MinValue;
    WatchStartTime = DateTime.MinValue;
    WatchInterval = 1000; // Default to 1 second
    Mode = FileWatchingMode.File;
  }

  public TimeSpan WatchDuration => IsWatching ? DateTime.Now - WatchStartTime : TimeSpan.Zero;

  public void StartWatching(string path, FileWatchingMode mode = FileWatchingMode.File)
  {
    IsWatching = true;
    WatchedPath = path;
    Mode = mode;
    WatchStartTime = DateTime.Now;
    ChangeCount = 0;
    LastChangeType = null;
  }

  public void StopWatching()
  {
    IsWatching = false;
    WatchedPath = null;
    IsWatchingDerivedFile = false;
  }

  public void RecordChange(string changeType)
  {
    LastUpdate = DateTime.Now;
    LastChangeType = changeType;
    ChangeCount++;
  }

  public bool IsWatchingFile(string filePath)
  {
    return IsWatching &&
           !string.IsNullOrEmpty(WatchedPath) &&
           string.Equals(WatchedPath, filePath, StringComparison.OrdinalIgnoreCase);
  }
}

/// <summary>
/// Represents different file watching modes.
/// </summary>
public enum FileWatchingMode
{
  File,
  Directory,
  DirectoryWithSubfolders
}
