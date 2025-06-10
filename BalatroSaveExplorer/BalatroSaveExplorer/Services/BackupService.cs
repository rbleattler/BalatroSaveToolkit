using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BalatroSaveExplorer.Services;

/// <summary>
/// Service responsible for backup operations including automatic backups,
/// backup restoration, and backup management.
/// </summary>
public class BackupService
{
  private readonly Logger _logger;
  private readonly string _backupDirectory;

  public BackupService(Logger logger, string backupDirectory = "Backups")
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _backupDirectory = backupDirectory;

    // Ensure backup directory exists
    if (!Directory.Exists(_backupDirectory))
    {
      Directory.CreateDirectory(_backupDirectory);
    }
  }

  // TODO: Migrate from MainWindow
  // - CreateBackup method
  // - RestoreBackup method
  // - GetBackupList method
  // - CleanupOldBackups method
  // - GetBackupInfo method    /// <summary>
  /// Creates a backup of the specified file.
  /// </summary>
  /// <param name="sourceFilePath">Path to the file to backup</param>
  /// <param name="backupName">Optional backup name (defaults to timestamp)</param>
  /// <returns>Path to the created backup file</returns>
  public async Task<string> CreateBackupAsync(string sourceFilePath, string? backupName = null)
  {
    return await Task.Run(() =>
    {
      try
      {
        if (!File.Exists(sourceFilePath))
        {
          throw new FileNotFoundException($"Source file not found: {sourceFilePath}");
        }

        var sourceFileName = Path.GetFileNameWithoutExtension(sourceFilePath);
        var sourceExtension = Path.GetExtension(sourceFilePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        var backupFileName = backupName ?? $"{sourceFileName}_backup_{timestamp}{sourceExtension}";
        var backupPath = Path.Combine(_backupDirectory, backupFileName);

        // Ensure unique backup name
        int counter = 1;
        while (File.Exists(backupPath) || Directory.Exists(backupPath))
        {
          var nameWithoutExt = Path.GetFileNameWithoutExtension(backupFileName);
          backupPath = Path.Combine(_backupDirectory, $"{nameWithoutExt}_{counter}{sourceExtension}");
          counter++;
        }

        File.Copy(sourceFilePath, backupPath);
        _logger.Log($"Backup created: {backupPath}");

        return backupPath;
      }
      catch (Exception ex)
      {
        _logger.Log($"Failed to create backup: {ex.Message}");
        throw;
      }
    });
  }
  /// <summary>
  /// Restores a backup file to the specified location.
  /// </summary>
  /// <param name="backupFilePath">Path to the backup file</param>
  /// <param name="destinationPath">Where to restore the file</param>
  /// <returns>Success status</returns>
  public async Task<bool> RestoreBackupAsync(string backupFilePath, string destinationPath)
  {
    return await Task.Run(() =>
    {
      try
      {
        if (!File.Exists(backupFilePath))
        {
          throw new FileNotFoundException($"Backup file not found: {backupFilePath}");
        }

        // Ensure the destination directory exists
        var destDir = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
        {
          Directory.CreateDirectory(destDir);
        }

        File.Copy(backupFilePath, destinationPath, true);
        _logger.Log($"Backup restored from {backupFilePath} to {destinationPath}");

        return true;
      }
      catch (Exception ex)
      {
        _logger.Log($"Failed to restore backup: {ex.Message}");
        throw;
      }
    });
  }/// <summary>
   /// Gets a list of all available backups.
   /// </summary>
   /// <returns>List of backup file paths</returns>
  public IEnumerable<string> GetBackupList()
  {
    try
    {
      if (!Directory.Exists(_backupDirectory))
      {
        return new List<string>();
      }

      return Directory.GetFiles(_backupDirectory, "*.jkr")
          .OrderByDescending(f => File.GetCreationTime(f));
    }
    catch (Exception ex)
    {
      _logger.Log($"Failed to get backup list: {ex.Message}");
      return new List<string>();
    }
  }
  /// <summary>
  /// Cleans up old backups based on retention policy.
  /// </summary>
  /// <param name="maxBackups">Maximum number of backups to keep</param>
  /// <returns>Number of backups deleted</returns>
  public async Task<int> CleanupOldBackupsAsync(int maxBackups = 10)
  {
    return await Task.Run(() =>
    {
      try
      {
        var backups = GetBackupList().ToList();

        if (backups.Count <= maxBackups)
        {
          return 0; // No cleanup needed
        }

        var backupsToDelete = backups.Skip(maxBackups);
        int deletedCount = 0;

        foreach (var backup in backupsToDelete)
        {
          try
          {
            File.Delete(backup);
            deletedCount++;
            _logger.Log($"Deleted old backup: {backup}");
          }
          catch (Exception ex)
          {
            _logger.Log($"Failed to delete backup {backup}: {ex.Message}");
          }
        }

        _logger.Log($"Cleanup completed. Deleted {deletedCount} old backups.");
        return deletedCount;
      }
      catch (Exception ex)
      {
        _logger.Log($"Failed to cleanup old backups: {ex.Message}");
        return 0;
      }
    });
  }
  /// <summary>
  /// Gets information about a backup file.
  /// </summary>
  /// <param name="backupFilePath">Path to the backup file</param>
  /// <returns>Backup information</returns>
  public FileInfo GetBackupInfo(string backupFilePath)
  {
    try
    {
      if (!File.Exists(backupFilePath))
      {
        throw new FileNotFoundException($"Backup file not found: {backupFilePath}");
      }

      return new FileInfo(backupFilePath);
    }
    catch (Exception ex)
    {
      _logger.Log($"Failed to get backup info for {backupFilePath}: {ex.Message}");
      throw;
    }
  }/// <summary>
   /// Checks if automatic backup is enabled and performs backup if needed.
   /// </summary>
   /// <param name="filePath">File to potentially backup</param>
   /// <returns>True if backup was created</returns>
  public async Task<bool> AutoBackupIfNeededAsync(string filePath)
  {
    try
    {
      // This will be implemented to check settings when we integrate it
      // For now, always create backup
      var backupPath = await CreateBackupAsync(filePath);
      return !string.IsNullOrEmpty(backupPath);
    }
    catch (Exception ex)
    {
      _logger.Log($"Auto-backup failed: {ex.Message}");
      return false;
    }
  }
}
