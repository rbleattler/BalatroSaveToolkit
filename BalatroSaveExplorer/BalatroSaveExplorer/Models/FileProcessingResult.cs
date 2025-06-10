using System;
using System.Collections.Generic;
using BalatroSaveExplorer.Models;

namespace BalatroSaveExplorer.Models;

/// <summary>
/// Represents the result of a file processing operation.
/// </summary>
public class FileProcessingResult
{
  public bool Success { get; set; }
  public string? ErrorMessage { get; set; }
  public string? FilePath { get; set; }
  public string? Content { get; set; }
  public IEnumerable<TreeNodeViewModel>? TreeNodes { get; set; }
  public TimeSpan ProcessingTime { get; set; }
  public long FileSize { get; set; }
  public DateTime ProcessedAt { get; set; }
  public string? BackupPath { get; set; }
  public ProcessingType Type { get; set; }

  public FileProcessingResult()
  {
    ProcessedAt = DateTime.Now;
    TreeNodes = new List<TreeNodeViewModel>();
  }
  public static FileProcessingResult CreateSuccess(string filePath, string content, IEnumerable<TreeNodeViewModel> treeNodes)
  {
    return new FileProcessingResult
    {
      Success = true,
      FilePath = filePath,
      Content = content,
      TreeNodes = treeNodes,
      Type = ProcessingType.Open
    };
  }

  public static FileProcessingResult CreateFailure(string filePath, string errorMessage, ProcessingType type = ProcessingType.Open)
  {
    return new FileProcessingResult
    {
      Success = false,
      FilePath = filePath,
      ErrorMessage = errorMessage,
      Type = type
    };
  }

  public static FileProcessingResult CreateSaveSuccess(string filePath, string? backupPath = null)
  {
    return new FileProcessingResult
    {
      Success = true,
      FilePath = filePath,
      BackupPath = backupPath,
      Type = ProcessingType.Save
    };
  }

  public static FileProcessingResult CreateBackupSuccess(string filePath, string backupPath)
  {
    return new FileProcessingResult
    {
      Success = true,
      FilePath = filePath,
      BackupPath = backupPath,
      Type = ProcessingType.Backup
    };
  }
}

/// <summary>
/// Represents the type of file processing operation.
/// </summary>
public enum ProcessingType
{
  Open,
  Save,
  Backup,
  Restore,
  Validate,
  Compress,
  Decompress
}
