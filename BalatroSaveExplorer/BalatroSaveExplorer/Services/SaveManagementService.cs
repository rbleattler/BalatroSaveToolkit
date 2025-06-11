using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using BalatroSaveExplorer.Models;

namespace BalatroSaveExplorer.Services;

/// <summary>
/// Service responsible for managing automatic saves, save backups, and save restoration
/// </summary>
public class SaveManagementService : IDisposable
{
  private readonly Logger _logger;
  private readonly BackupService _backupService;
  private readonly AppSettings _settings; private System.Timers.Timer? _autoSaveTimer;
  private bool _disposed = false;

  public SaveManagementService(Logger logger, BackupService backupService, AppSettings settings)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
    _settings = settings ?? throw new ArgumentNullException(nameof(settings));

    InitializeAutoSave();
  }

  /// <summary>
  /// Event raised when a save backup is created
  /// </summary>
  public event EventHandler<string>? SaveBackupCreated;

  /// <summary>
  /// Event raised when save list is updated
  /// </summary>
  public event EventHandler? SaveListUpdated;

  /// <summary>
  /// Gets or sets debug logging state for save operations
  /// </summary>
  public bool IsDebugEnabled { get; set; }

  /// <summary>
  /// Gets the current profile save file path
  /// </summary>
  public string GetCurrentSaveFilePath()
  {
    return _settings.CurrentProfileSavePath;
  }

  /// <summary>
  /// Gets the backup directory path
  /// </summary>
  public string GetBackupDirectory()
  {
    return _settings.BackupDirectory;
  }

  /// <summary>
  /// Gets the list of available save backups
  /// </summary>
  public IEnumerable<SaveBackupInfo> GetSaveBackups()
  {
    try
    {
      var backupFiles = _backupService.GetBackupList();
      return backupFiles.Select(file => new SaveBackupInfo
      {
        FilePath = file,
        FileName = Path.GetFileName(file),
        CreatedDate = File.GetCreationTime(file),
        ModifiedDate = File.GetLastWriteTime(file),
        Size = new FileInfo(file).Length
      }).OrderByDescending(s => s.CreatedDate);
    }
    catch (Exception ex)
    {
      _logger.Log($"Failed to get save backups: {ex.Message}");
      return Enumerable.Empty<SaveBackupInfo>();
    }
  }

  /// <summary>
  /// Manually creates a backup of the current profile save
  /// </summary>
  public async Task<bool> CreateManualSaveBackupAsync()
  {
    try
    {
      var currentSavePath = _settings.CurrentProfileSavePath;
      if (!File.Exists(currentSavePath))
      {
        _logger.Log($"Current profile save file not found: {currentSavePath}");
        return false;
      }

      var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
      var backupName = $"P{_settings.CurrentProfileNumber}_{timestamp}_manual.jkr";

      var backupPath = await _backupService.CreateBackupAsync(currentSavePath, backupName);

      SaveBackupCreated?.Invoke(this, backupPath);
      SaveListUpdated?.Invoke(this, EventArgs.Empty);

      return true;
    }
    catch (Exception ex)
    {
      _logger.Log($"Failed to create manual save backup: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Creates a manual backup and returns the result
  /// </summary>
  public BackupResult CreateManualBackup()
  {
    try
    {
      var currentSavePath = _settings.CurrentProfileSavePath;
      if (!File.Exists(currentSavePath))
      {
        return new BackupResult
        {
          Success = false,
          ErrorMessage = $"Current profile save file not found: {currentSavePath}"
        };
      }

      var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
      var backupName = $"P{_settings.CurrentProfileNumber}_{timestamp}_manual.jkr";

      var task = _backupService.CreateBackupAsync(currentSavePath, backupName);
      task.Wait();
      var backupPath = task.Result;

      SaveBackupCreated?.Invoke(this, backupPath);
      SaveListUpdated?.Invoke(this, EventArgs.Empty);

      return new BackupResult
      {
        Success = true,
        BackupFileName = Path.GetFileName(backupPath),
        BackupPath = backupPath
      };
    }
    catch (Exception ex)
    {
      _logger.Log($"Failed to create manual save backup: {ex.Message}");
      return new BackupResult
      {
        Success = false,
        ErrorMessage = ex.Message
      };
    }
  }

  /// <summary>
  /// Restores a save backup to the current profile
  /// </summary>
  public async Task<bool> RestoreSaveBackupAsync(string backupFilePath)
  {
    try
    {
      var currentSavePath = _settings.CurrentProfileSavePath;

      // Create a backup of the current save before restoring
      if (File.Exists(currentSavePath))
      {
        var preRestoreBackupName = $"P{_settings.CurrentProfileNumber}_pre_restore_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.jkr";
        await _backupService.CreateBackupAsync(currentSavePath, preRestoreBackupName);
      }

      // Restore the backup
      var success = await _backupService.RestoreBackupAsync(backupFilePath, currentSavePath);

      if (success)
      {
        _logger.Log($"Successfully restored save from backup: {Path.GetFileName(backupFilePath)}");
        SaveListUpdated?.Invoke(this, EventArgs.Empty);
      }

      return success;
    }
    catch (Exception ex)
    {
      _logger.Log($"Failed to restore save backup: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Restores a backup and returns the result
  /// </summary>
  public BackupResult RestoreFromBackup(string backupFilePath)
  {
    try
    {
      var currentSavePath = _settings.CurrentProfileSavePath;

      // Create a backup of the current save before restoring
      if (File.Exists(currentSavePath))
      {
        var preRestoreBackupName = $"P{_settings.CurrentProfileNumber}_pre_restore_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.jkr";
        var backupTask = _backupService.CreateBackupAsync(currentSavePath, preRestoreBackupName);
        backupTask.Wait();
      }

      // Restore the backup
      var restoreTask = _backupService.RestoreBackupAsync(backupFilePath, currentSavePath);
      restoreTask.Wait();
      var success = restoreTask.Result;

      if (success)
      {
        _logger.Log($"Successfully restored save from backup: {Path.GetFileName(backupFilePath)}");
        SaveListUpdated?.Invoke(this, EventArgs.Empty);

        return new BackupResult
        {
          Success = true,
          BackupFileName = Path.GetFileName(backupFilePath)
        };
      }
      else
      {
        return new BackupResult
        {
          Success = false,
          ErrorMessage = "Failed to restore backup"
        };
      }
    }
    catch (Exception ex)
    {
      _logger.Log($"Failed to restore save backup: {ex.Message}");
      return new BackupResult
      {
        Success = false,
        ErrorMessage = ex.Message
      };
    }
  }

  /// <summary>
  /// Opens the saves/backup directory in Windows Explorer
  /// </summary>
  public void OpenSavesFolder()
  {
    try
    {
      if (Directory.Exists(_settings.BackupDirectory))
      {
        System.Diagnostics.Process.Start("explorer.exe", _settings.BackupDirectory);
      }
      else
      {
        _logger.Log($"Backup directory does not exist: {_settings.BackupDirectory}");
      }
    }
    catch (Exception ex)
    {
      _logger.Log($"Failed to open saves folder: {ex.Message}");
    }
  }    /// <summary>
       /// Deletes a save backup file
       /// </summary>
  public Task<bool> DeleteSaveBackupAsync(string backupFilePath)
  {
    try
    {
      if (File.Exists(backupFilePath))
      {
        File.Delete(backupFilePath);
        _logger.Log($"Deleted save backup: {Path.GetFileName(backupFilePath)}");
        SaveListUpdated?.Invoke(this, EventArgs.Empty);
        return Task.FromResult(true);
      }
      return Task.FromResult(false);
    }
    catch (Exception ex)
    {
      _logger.Log($"Failed to delete save backup: {ex.Message}");
      return Task.FromResult(false);
    }
  }

  /// <summary>
  /// Performs automatic cleanup of old saves based on retention settings
  /// </summary>
  public async Task CleanupOldSavesAsync()
  {
    if (!_settings.EnableSaveRetention)
      return;

    try
    {
      var cutoffDate = GetRetentionCutoffDate();
      var backups = GetSaveBackups().Where(b => b.CreatedDate < cutoffDate).ToList();

      foreach (var backup in backups)
      {
        await DeleteSaveBackupAsync(backup.FilePath);
      }

      if (backups.Any())
      {
        _logger.Log($"Cleaned up {backups.Count} old save backups older than {cutoffDate:yyyy-MM-dd}");
      }
    }
    catch (Exception ex)
    {
      _logger.Log($"Failed to cleanup old saves: {ex.Message}");
    }
  }

  /// <summary>
  /// Initializes the auto-save timer based on settings
  /// </summary>
  private void InitializeAutoSave()
  {
    UpdateAutoSaveTimer();

    // Subscribe to settings changes
    _settings.PropertyChanged += (s, e) =>
    {
      if (e.PropertyName == nameof(AppSettings.EnableAutoSave) ||
              e.PropertyName == nameof(AppSettings.AutoSaveInterval) ||
              e.PropertyName == nameof(AppSettings.AutoSaveIntervalUnit))
      {
        UpdateAutoSaveTimer();
      }
    };
  }

  /// <summary>
  /// Updates the auto-save timer based on current settings
  /// </summary>
  private void UpdateAutoSaveTimer()
  {
    _autoSaveTimer?.Stop();
    _autoSaveTimer?.Dispose();
    _autoSaveTimer = null;

    if (!_settings.EnableAutoSave || _settings.AutoSaveInterval <= 0)
      return;

    var intervalMs = GetIntervalInMilliseconds(_settings.AutoSaveInterval, _settings.AutoSaveIntervalUnit); _autoSaveTimer = new System.Timers.Timer(intervalMs);
    _autoSaveTimer.Elapsed += async (s, e) => await PerformAutoSave();
    _autoSaveTimer.AutoReset = true;
    _autoSaveTimer.Start();

    _logger.Log($"Auto-save enabled: every {_settings.AutoSaveInterval} {_settings.AutoSaveIntervalUnit.ToLower()}");
  }

  /// <summary>
  /// Performs an automatic save backup
  /// </summary>
  private async Task PerformAutoSave()
  {
    try
    {
      var currentSavePath = _settings.CurrentProfileSavePath;
      if (!File.Exists(currentSavePath))
        return;

      var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
      var backupName = $"P{_settings.CurrentProfileNumber}_{timestamp}_auto.jkr";

      var backupPath = await _backupService.CreateBackupAsync(currentSavePath, backupName);

      SaveBackupCreated?.Invoke(this, backupPath);
      SaveListUpdated?.Invoke(this, EventArgs.Empty);

      // Perform cleanup if enabled
      await CleanupOldSavesAsync();
    }
    catch (Exception ex)
    {
      _logger.Log($"Auto-save failed: {ex.Message}");
    }
  }

  /// <summary>
  /// Converts interval settings to milliseconds
  /// </summary>
  private static double GetIntervalInMilliseconds(int value, string unit)
  {
    return unit.ToLower() switch
    {
      "minutes" => value * 60 * 1000,
      "hours" => value * 60 * 60 * 1000,
      "days" => value * 24 * 60 * 60 * 1000,
      _ => value * 60 * 1000 // Default to minutes
    };
  }

  /// <summary>
  /// Gets the cutoff date for retention cleanup
  /// </summary>
  private DateTime GetRetentionCutoffDate()
  {
    var now = DateTime.Now;
    return _settings.SaveRetentionUnit.ToLower() switch
    {
      "hours" => now.AddHours(-_settings.SaveRetentionValue),
      "days" => now.AddDays(-_settings.SaveRetentionValue),
      "weeks" => now.AddDays(-_settings.SaveRetentionValue * 7),
      "months" => now.AddMonths(-_settings.SaveRetentionValue),
      _ => now.AddDays(-_settings.SaveRetentionValue) // Default to days
    };
  }

  public void Dispose()
  {
    if (_disposed)
      return;

    _autoSaveTimer?.Stop();
    _autoSaveTimer?.Dispose();
    _autoSaveTimer = null;

    _disposed = true;
  }
}

/// <summary>
/// Information about a save backup file
/// </summary>
public class SaveBackupInfo
{
  public string FilePath { get; set; } = string.Empty;
  public string FileName { get; set; } = string.Empty;
  public DateTime CreatedDate { get; set; }
  public DateTime ModifiedDate { get; set; }
  public long Size { get; set; }

  public string FormattedSize => FormatFileSize(Size);
  public string FormattedDate => CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");

  private static string FormatFileSize(long bytes)
  {
    if (bytes < 1024) return $"{bytes} B";
    if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
    if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
    return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
  }
}

/// <summary>
/// Result of a backup operation
/// </summary>
public class BackupResult
{
  public bool Success { get; set; }
  public string? ErrorMessage { get; set; }
  public string? BackupFileName { get; set; }
  public string? BackupPath { get; set; }
}
