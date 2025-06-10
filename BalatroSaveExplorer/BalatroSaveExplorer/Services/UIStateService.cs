using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using System.Windows.Threading;
using BalatroSaveExplorer.Converters;
using BalatroSaveExplorer.Models;
using BalatroSaveExplorer.Utilities;

namespace BalatroSaveExplorer.Services;

/// <summary>
/// Service responsible for managing UI state, progress updates, taskbar notifications,
/// and user interface interactions.
/// </summary>
public class UIStateService : INotifyPropertyChanged
{
  private readonly Logger _logger;
  private readonly DispatcherTimer _flashTimer;
  private int _flashCount;
  private double _progressValue;
  private TaskbarItemProgressState _taskbarProgressState;
  private string _statusText;
  private bool _isProcessing;

  public event PropertyChangedEventHandler? PropertyChanged;
  public event EventHandler<string>? StatusChanged;
  public event EventHandler<double>? ProgressChanged;

  public UIStateService(Logger logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _statusText = "Ready";

    // Initialize flash timer for taskbar notifications
    _flashTimer = new DispatcherTimer();
    _flashTimer.Interval = TimeSpan.FromMilliseconds(500);
    _flashTimer.Tick += FlashTimer_Tick;
  }

  // Properties for data binding
  public double ProgressValue
  {
    get => _progressValue;
    private set => SetProperty(ref _progressValue, value);
  }

  public TaskbarItemProgressState TaskbarProgressState
  {
    get => _taskbarProgressState;
    private set => SetProperty(ref _taskbarProgressState, value);
  }

  public string StatusText
  {
    get => _statusText;
    private set => SetProperty(ref _statusText, value);
  }

  public bool IsProcessing
  {
    get => _isProcessing;
    private set => SetProperty(ref _isProcessing, value);
  }

  /// <summary>
  /// Updates the progress bar and taskbar progress.
  /// </summary>
  /// <param name="value">Progress value (0-100)</param>
  /// <param name="state">Taskbar progress state</param>
  public void UpdateProgress(double value, TaskbarItemProgressState state = TaskbarItemProgressState.Normal)
  {
    ProgressValue = Math.Max(0, Math.Min(100, value)) / 100.0; // Normalize to 0-1
    TaskbarProgressState = state;

    ProgressChanged?.Invoke(this, _progressValue);

    _logger.Log($"Progress updated: {value}% ({state})");
  }

  /// <summary>
  /// Updates the status text.
  /// </summary>
  /// <param name="status">New status message</param>
  public void UpdateStatus(string status)
  {
    StatusText = status ?? "Ready";
    StatusChanged?.Invoke(this, _statusText);
    _logger.Log($"Status updated: {_statusText}");
  }

  /// <summary>
  /// Sets a status message for UI display.
  /// </summary>
  /// <param name="message">The status message to display</param>
  public void SetStatusMessage(string message)
  {
    Application.Current.Dispatcher.Invoke(() =>
    {
      StatusText = message;
      _logger.Log($"Status: {message}");
    });
  }

  /// <summary>
  /// Updates the state of the Save as Lua button based on current file and existing Lua files.
  /// </summary>
  /// <param name="button">The Save as Lua button to update</param>
  /// <param name="currentFilePath">The currently loaded file path</param>
  public void UpdateSaveAsLuaButtonState(Button button, string? currentFilePath)
  {
    if (button == null) return;

    Application.Current.Dispatcher.Invoke(() =>
    {
      if (string.IsNullOrEmpty(currentFilePath))
      {
        // No file loaded - disable button
        button.IsEnabled = false;
        button.Content = "Save as Lua";
        return;
      }

      string luaPath = Path.ChangeExtension(currentFilePath, ".lua");
      bool luaFileExists = File.Exists(luaPath);

      if (luaFileExists)
      {
        // File exists - gray out button and change text
        button.IsEnabled = false;
        button.Content = "Lua file exists";
        _logger.Log($"Lua file already exists: {luaPath}");
      }
      else
      {
        // File doesn't exist - enable button
        button.IsEnabled = true;
        button.Content = "Save as Lua";
      }
    });
  }

  /// <summary>
  /// Starts a flash notification in the taskbar.
  /// </summary>
  /// <param name="flashCount">Number of times to flash</param>
  public void StartFlashNotification(int flashCount = 6)
  {
    _flashCount = 0;
    _flashTimer.Stop();
    _flashTimer.Start();
    _logger.Log("Started taskbar flash notification");
  }

  /// <summary>
  /// Stops the flash notification.
  /// </summary>
  public void StopFlashNotification()
  {
    _flashTimer.Stop();
    UpdateProgress(0, TaskbarItemProgressState.None);
    _logger.Log("Stopped flash notification");
  }

  /// <summary>
  /// Shows a processing state with progress indication.
  /// </summary>
  /// <param name="message">Processing message</param>
  public void ShowProcessing(string message)
  {
    IsProcessing = true;
    UpdateStatus(message);
    UpdateProgress(0, TaskbarItemProgressState.Indeterminate);
    _logger.Log($"Started processing: {message}");
  }

  /// <summary>
  /// Hides the processing state.
  /// </summary>
  public void HideProcessing()
  {
    IsProcessing = false;
    UpdateProgress(0, TaskbarItemProgressState.None);
    UpdateStatus("Ready");
    _logger.Log("Processing completed");
  }

  /// <summary>
  /// Updates the tree view with new data.
  /// </summary>
  /// <param name="treeNodes">New tree node collection</param>
  public void UpdateTreeView(ObservableCollection<TreeNodeViewModel> treeNodes)
  {
    // This method is intended to be called from the UI thread to update the tree view
    // The actual tree view binding should be handled by the UI layer
    _logger.Log($"Tree view updated with {treeNodes?.Count ?? 0} nodes");
  }

  /// <summary>
  /// Expands tree nodes to a specified level.
  /// </summary>
  /// <param name="level">Level to expand to</param>
  public void ExpandTreeToLevel(int level)
  {
    // This is a placeholder - actual tree expansion should be handled by the UI layer
    // that has access to the TreeView control
    _logger.Log($"Expand tree to level {level} requested");
  }

  /// <summary>
  /// Collapses all tree nodes.
  /// </summary>
  public void CollapseAllTreeNodes()
  {
    // This is a placeholder - actual tree collapse should be handled by the UI layer
    // that has access to the TreeView control
    _logger.Log("Collapse all tree nodes requested");
  }

  /// <summary>
  /// Shows an error message to the user.
  /// </summary>
  /// <param name="message">Error message</param>
  /// <param name="title">Dialog title</param>
  public void ShowError(string message, string title = "Error")
  {
    Application.Current.Dispatcher.Invoke(() =>
    {
      MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    });
    _logger.Log($"ERROR shown to user: {title}: {message}");
  }

  /// <summary>
  /// Shows an information message to the user.
  /// </summary>
  /// <param name="message">Information message</param>
  /// <param name="title">Dialog title</param>
  public void ShowInformation(string message, string title = "Information")
  {
    Application.Current.Dispatcher.Invoke(() =>
    {
      MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    });
    _logger.Log($"Information shown to user: {title}: {message}");
  }

  /// <summary>
  /// Shows a confirmation dialog to the user.
  /// </summary>
  /// <param name="message">Confirmation message</param>
  /// <param name="title">Dialog title</param>
  /// <returns>True if user confirmed</returns>
  public bool ShowConfirmation(string message, string title = "Confirm")
  {
    bool result = false;
    Application.Current.Dispatcher.Invoke(() =>
    {
      result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    });
    _logger.Log($"Confirmation dialog shown: {title}: {message} - Result: {result}");
    return result;
  }

  /// <summary>
  /// Populates the raw content tab with decompressed content.
  /// </summary>
  /// <param name="content">Decompressed content to display</param>
  /// <param name="textBox">TextBox control to populate</param>
  public void PopulateRawContentTab(string content, TextBox textBox)
  {
    try
    {
      if (textBox == null) return;

      Application.Current.Dispatcher.Invoke(() =>
      {
        textBox.Text = content ?? "";
        _logger.Log("Raw content tab populated");
      });
    }
    catch (Exception ex)
    {
      _logger.Log($"Failed to populate raw content tab: {ex.Message}");
    }
  }

  /// <summary>
  /// Populates the file info tab with file metadata.
  /// </summary>
  /// <param name="filePath">Path to the file</param>
  /// <param name="fileInfoTextBox">TextBox control to populate</param>
  public void PopulateFileInfoTab(string filePath, TextBox fileInfoTextBox)
  {
    if (string.IsNullOrEmpty(filePath) || fileInfoTextBox == null) return;

    try
    {
      var fileInfo = new System.IO.FileInfo(filePath);
      var content = $"""
        File Path: {filePath}
        File Size: {FormatFileSize(fileInfo.Length)}
        Created: {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss}
        Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}
        Accessed: {fileInfo.LastAccessTime:yyyy-MM-dd HH:mm:ss}
        """;

      Application.Current.Dispatcher.Invoke(() =>
      {
        fileInfoTextBox.Text = content;
      });
      _logger.Log($"File info tab populated for: {filePath}");
    }
    catch (Exception ex)
    {
      _logger.Log($"Failed to populate file info tab: {ex.Message}");
    }
  }

  /// <summary>
  /// Populates file info controls with metadata about a loaded file
  /// </summary>
  /// <param name="filePath">Path to the file</param>
  /// <param name="content">The decompressed file content for entry counting</param>
  /// <param name="filePathTextBox">TextBox for file path</param>
  /// <param name="fileSizeTextBox">TextBox for file size</param>
  /// <param name="lastModifiedTextBox">TextBox for last modified date</param>
  /// <param name="compressionInfoTextBox">TextBox for compression info</param>
  /// <param name="contentTypeTextBox">TextBox for content type</param>
  /// <param name="entriesCountTextBox">TextBox for entries count</param>
  public void PopulateFileInfoControls(string filePath, string? content,
      TextBox filePathTextBox, TextBox fileSizeTextBox, TextBox lastModifiedTextBox,
      TextBox compressionInfoTextBox, TextBox contentTypeTextBox, TextBox entriesCountTextBox)
  {
    Application.Current.Dispatcher.Invoke(() =>
    {
      try
      {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
          // Clear all controls if file doesn't exist
          filePathTextBox.Text = "";
          fileSizeTextBox.Text = "";
          lastModifiedTextBox.Text = "";
          compressionInfoTextBox.Text = "";
          contentTypeTextBox.Text = "";
          entriesCountTextBox.Text = "";
          return;
        }

        var fileInfo = new FileInfo(filePath);

        // Populate the individual controls
        filePathTextBox.Text = filePath;
        fileSizeTextBox.Text = FileOperations.FormatFileSize(fileInfo.Length);
        lastModifiedTextBox.Text = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
        compressionInfoTextBox.Text = "JKR Compressed";
        contentTypeTextBox.Text = "Lua Table";

        // Count entries in the decompressed content
        int entryCount = CountEntries(content);
        entriesCountTextBox.Text = entryCount.ToString();

        _logger.Log($"File info populated for: {filePath}");
      }
      catch (Exception ex)
      {
        _logger.Log($"Failed to populate file info: {ex.Message}");
      }
    });
  }

  /// <summary>
  /// Formats file size in human-readable format.
  /// </summary>
  /// <param name="bytes">Size in bytes</param>
  /// <returns>Formatted size string</returns>
  private string FormatFileSize(long bytes)
  {
    string[] sizes = { "B", "KB", "MB", "GB", "TB" };
    int order = 0;
    double size = bytes;
    while (size >= 1024 && order < sizes.Length - 1)
    {
      order++;
      size /= 1024;
    }
    return $"{size:0.##} {sizes[order]}";
  }

  /// <summary>
  /// Refreshes the current file display.
  /// </summary>
  /// <param name="refreshAction">Action to perform the refresh</param>
  public void RefreshCurrentFile(Func<Task> refreshAction)
  {
    if (refreshAction == null) return;

    Application.Current.Dispatcher.Invoke(async () =>
    {
      try
      {
        ShowProcessing("Refreshing file...");
        await refreshAction();
        HideProcessing();
        UpdateStatus("File refreshed");
      }
      catch (Exception ex)
      {
        HideProcessing();
        ShowError($"Failed to refresh file: {ex.Message}");
      }
    });
  }

  /// <summary>
  /// Counts the number of entries in decompressed JKR content
  /// </summary>
  /// <param name="content">Content to count entries in</param>
  /// <returns>Number of entries</returns>
  public int CountEntries(string? content)
  {
    if (string.IsNullOrEmpty(content))
      return 0;

    try
    {
      var parsedData = LuaTableConverter.ParseLuaTable(content);
      return CountDictionaryEntries(parsedData);
    }
    catch
    {
      return 0;
    }
  }

  /// <summary>
  /// Recursively counts entries in a dictionary
  /// </summary>
  /// <param name="data">Dictionary to count</param>
  /// <returns>Total number of entries</returns>
  private int CountDictionaryEntries(Dictionary<string, object> data)
  {
    int count = data.Count;

    foreach (var value in data.Values)
    {
      if (value is Dictionary<string, object> nestedDict)
      {
        count += CountDictionaryEntries(nestedDict);
      }
      else if (value is List<object> list)
      {
        count += CountListEntries(list);
      }
    }

    return count;
  }

  /// <summary>
  /// Recursively counts entries in a list
  /// </summary>
  /// <param name="list">List to count</param>
  /// <returns>Total number of entries</returns>
  private int CountListEntries(List<object> list)
  {
    int count = list.Count;

    foreach (var item in list)
    {
      if (item is Dictionary<string, object> dict)
      {
        count += CountDictionaryEntries(dict);
      }
      else if (item is List<object> nestedList)
      {
        count += CountListEntries(nestedList);
      }
    }

    return count;
  }

  private void FlashTimer_Tick(object? sender, EventArgs e)
  {
    _flashCount++;

    // Alternate between normal and paused states to create flashing effect
    TaskbarProgressState = _flashCount % 2 == 0 ? TaskbarItemProgressState.Normal : TaskbarItemProgressState.Paused;

    // Stop flashing after 6 cycles (3 complete flashes)
    if (_flashCount >= 6)
    {
      StopFlashNotification();
    }
  }

  protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
  {
    if (Equals(field, value)) return false;
    field = value;
    OnPropertyChanged(propertyName);
    return true;
  }
}
