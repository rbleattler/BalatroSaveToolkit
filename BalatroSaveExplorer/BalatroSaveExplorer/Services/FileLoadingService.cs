using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BalatroSaveExplorer.Models;

namespace BalatroSaveExplorer.Services;

/// <summary>
/// Service responsible for orchestrating the file loading process.
/// Coordinates between multiple services to load, process, and display JKR files.
/// </summary>
public class FileLoadingService
{
  private readonly Logger _logger;
  private readonly JkrFileService _jkrFileService;
  private readonly BackupService _backupService;
  private readonly UIStateService _uiStateService;
  private readonly FileWatchingService _fileWatchingService;

  public FileLoadingService(
      Logger logger,
      JkrFileService jkrFileService,
      BackupService backupService,
      UIStateService uiStateService,
      FileWatchingService fileWatchingService)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _jkrFileService = jkrFileService ?? throw new ArgumentNullException(nameof(jkrFileService));
    _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
    _uiStateService = uiStateService ?? throw new ArgumentNullException(nameof(uiStateService));
    _fileWatchingService = fileWatchingService ?? throw new ArgumentNullException(nameof(fileWatchingService));
  }

  /// <summary>
  /// Loads a JKR file and updates the UI with the processed content.
  /// </summary>
  /// <param name="filePath">The path to the file to load</param>
  /// <param name="context">The loading context containing UI elements and state</param>
  /// <returns>A result indicating success or failure</returns>
  public async Task<FileLoadingResult> LoadFileAsync(string filePath, FileLoadingContext context)
  {
    if (string.IsNullOrEmpty(filePath))
    {
      return FileLoadingResult.CreateFailure("File path cannot be empty");
    }

    if (context == null)
    {
      return FileLoadingResult.CreateFailure("Loading context cannot be null");
    }

    try
    {
      // Update UI to show loading state
      _uiStateService.SetStatusMessage("Loading file...");
      _logger.Log($"Loading file: {filePath}");

      var settings = SettingsManager.Instance.Settings;

      // Create backup if enabled
      if (settings.EnableAutoBackup)
      {
        await _backupService.AutoBackupIfNeededAsync(filePath);
      }

      // Process the file using the service
      var result = await _jkrFileService.ProcessFileAsync(filePath);

      if (!result.Success)
      {
        return FileLoadingResult.CreateFailure(result.ErrorMessage ?? "Unknown error processing file");
      }      // Update application state
      if (!string.IsNullOrEmpty(result.Content))
      {
        context.SetCurrentFile(filePath, result.Content);
      }
      else
      {
        return FileLoadingResult.CreateFailure("File content is empty or null");
      }

      // Auto-save decompressed content if enabled
      if (settings.AutoSaveDecompressedFiles && !string.IsNullOrEmpty(result.Content))
      {
        await _jkrFileService.SaveDecompressedToTempAsync(result.Content, filePath);
      }

      // Update tree view
      context.TreeNodes.Clear();
      if (result.TreeNodes != null)
      {
        foreach (var node in result.TreeNodes)
        {
          context.TreeNodes.Add(node);
        }
      }

      // Populate UI tabs
      if (!string.IsNullOrEmpty(result.Content))
      {
        _uiStateService.PopulateRawContentTab(result.Content, context.RawContentTextBox);
      }

      _uiStateService.PopulateFileInfoControls(filePath, result.Content,
          context.FilePathTextBox, context.FileSizeTextBox, context.LastModifiedTextBox,
          context.CompressionInfoTextBox, context.ContentTypeTextBox, context.EntriesCountTextBox);

      // Update status and start file watching
      _uiStateService.SetStatusMessage($"Loaded: {System.IO.Path.GetFileName(filePath)}");
      _logger.Log("File loaded successfully");

      _fileWatchingService.StartWatching(filePath);

      return FileLoadingResult.CreateSuccess(filePath, result.Content);
    }
    catch (Exception ex)
    {
      string errorMsg = $"Error loading file: {ex.Message}";
      _uiStateService.SetStatusMessage("Error loading file");
      _logger.Log(errorMsg);
      MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

      // Clear current file info on error
      context.ClearCurrentFile();

      return FileLoadingResult.CreateFailure(errorMsg);
    }
  }
}

/// <summary>
/// Context object containing UI elements and state needed for file loading.
/// </summary>
public class FileLoadingContext
{
  public ObservableCollection<TreeNodeViewModel> TreeNodes { get; }
  public TextBox RawContentTextBox { get; }
  public TextBox FilePathTextBox { get; }
  public TextBox FileSizeTextBox { get; }
  public TextBox LastModifiedTextBox { get; }
  public TextBox CompressionInfoTextBox { get; }
  public TextBox ContentTypeTextBox { get; }
  public TextBox EntriesCountTextBox { get; }

  private Action<string?, string?> _setCurrentFileAction;
  private Action _clearCurrentFileAction;

  public FileLoadingContext(
      ObservableCollection<TreeNodeViewModel> treeNodes,
      TextBox rawContentTextBox,
      TextBox filePathTextBox,
      TextBox fileSizeTextBox,
      TextBox lastModifiedTextBox,
      TextBox compressionInfoTextBox,
      TextBox contentTypeTextBox,
      TextBox entriesCountTextBox,
      Action<string?, string?> setCurrentFileAction,
      Action clearCurrentFileAction)
  {
    TreeNodes = treeNodes ?? throw new ArgumentNullException(nameof(treeNodes));
    RawContentTextBox = rawContentTextBox ?? throw new ArgumentNullException(nameof(rawContentTextBox));
    FilePathTextBox = filePathTextBox ?? throw new ArgumentNullException(nameof(filePathTextBox));
    FileSizeTextBox = fileSizeTextBox ?? throw new ArgumentNullException(nameof(fileSizeTextBox));
    LastModifiedTextBox = lastModifiedTextBox ?? throw new ArgumentNullException(nameof(lastModifiedTextBox));
    CompressionInfoTextBox = compressionInfoTextBox ?? throw new ArgumentNullException(nameof(compressionInfoTextBox));
    ContentTypeTextBox = contentTypeTextBox ?? throw new ArgumentNullException(nameof(contentTypeTextBox));
    EntriesCountTextBox = entriesCountTextBox ?? throw new ArgumentNullException(nameof(entriesCountTextBox));
    _setCurrentFileAction = setCurrentFileAction ?? throw new ArgumentNullException(nameof(setCurrentFileAction));
    _clearCurrentFileAction = clearCurrentFileAction ?? throw new ArgumentNullException(nameof(clearCurrentFileAction));
  }

  public void SetCurrentFile(string filePath, string content)
  {
    _setCurrentFileAction(filePath, content);
  }

  public void ClearCurrentFile()
  {
    _clearCurrentFileAction();
  }
}

/// <summary>
/// Result of a file loading operation.
/// </summary>
public class FileLoadingResult
{
  public bool Success { get; private set; }
  public string? ErrorMessage { get; private set; }
  public string? FilePath { get; private set; }
  public string? Content { get; private set; }
  private FileLoadingResult(bool success, string? errorMessage, string? filePath, string? content)
  {
    Success = success;
    ErrorMessage = errorMessage;
    FilePath = filePath;
    Content = content;
  }
  public static FileLoadingResult CreateSuccess(string filePath, string content)
  {
    return new FileLoadingResult(true, null, filePath, content);
  }
  public static FileLoadingResult CreateFailure(string errorMessage)
  {
    return new FileLoadingResult(false, errorMessage, null, null);
  }
}
