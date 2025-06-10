using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BalatroSaveExplorer.Utilities;

/// <summary>
/// Utility class for common file operations including file dialogs,
/// file system operations, and file validation.
/// </summary>
public static class FileOperations
{
  // TODO: Migrate from MainWindow
  // - File dialog operations
  // - File extension validation
  // - Path manipulation utilities
  // - File size calculations
  // - Directory operations
  /// <summary>
  /// Shows an open file dialog for JKR files.
  /// </summary>
  /// <param name="title">Dialog title</param>
  /// <param name="initialDirectory">Initial directory to open</param>
  /// <returns>Selected file path or null if cancelled</returns>
  public static string? ShowOpenJkrFileDialog(string title = "Open JKR File", string? initialDirectory = null)
  {
    var openFileDialog = new OpenFileDialog
    {
      Filter = "JKR Files (*.jkr)|*.jkr|All Files (*.*)|*.*",
      Title = title,
      InitialDirectory = !string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory)
        ? initialDirectory
        : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
    };

    return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
  }
  /// <summary>
  /// Shows a save file dialog for JKR files.
  /// </summary>
  /// <param name="title">Dialog title</param>
  /// <param name="defaultFileName">Default file name</param>
  /// <param name="initialDirectory">Initial directory to open</param>
  /// <returns>Selected file path or null if cancelled</returns>
  public static string? ShowSaveJkrFileDialog(string title = "Save JKR File", string? defaultFileName = null, string? initialDirectory = null)
  {
    var saveFileDialog = new SaveFileDialog
    {
      Filter = "JKR Files (*.jkr)|*.jkr|All Files (*.*)|*.*",
      Title = title,
      InitialDirectory = !string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory)
        ? initialDirectory
        : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
      FileName = defaultFileName ?? string.Empty
    };

    return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
  }
  /// <summary>
  /// Shows a save file dialog for Lua files.
  /// </summary>
  /// <param name="title">Dialog title</param>
  /// <param name="defaultFileName">Default file name</param>
  /// <param name="initialDirectory">Initial directory to open</param>
  /// <returns>Selected file path or null if cancelled</returns>
  public static string? ShowSaveLuaFileDialog(string title = "Save Lua File", string? defaultFileName = null, string? initialDirectory = null)
  {
    var saveFileDialog = new SaveFileDialog
    {
      Filter = "Lua Files (*.lua)|*.lua|All Files (*.*)|*.*",
      Title = title,
      InitialDirectory = !string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory)
        ? initialDirectory
        : Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
      FileName = defaultFileName ?? string.Empty
    };

    return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
  }
  /// <summary>
  /// Validates if a file has a valid JKR extension.
  /// </summary>
  /// <param name="filePath">File path to validate</param>
  /// <returns>True if valid JKR extension</returns>
  public static bool IsValidJkrExtension(string filePath)
  {
    if (string.IsNullOrEmpty(filePath))
      return false;

    return Path.GetExtension(filePath).Equals(".jkr", StringComparison.OrdinalIgnoreCase);
  }
  /// <summary>
  /// Gets the file size in a human-readable format.
  /// </summary>
  /// <param name="filePath">Path to the file</param>
  /// <returns>Formatted file size string</returns>
  public static string GetFormattedFileSize(string filePath)
  {
    if (!File.Exists(filePath))
      return "File not found";

    var fileInfo = new FileInfo(filePath);
    return FormatFileSize(fileInfo.Length);
  }

  /// <summary>
  /// Formats a file size in bytes to a human-readable format.
  /// </summary>
  /// <param name="bytes">Size in bytes</param>
  /// <returns>Formatted file size string</returns>
  public static string FormatFileSize(long bytes)
  {
    string[] sizes = { "B", "KB", "MB", "GB" };
    double len = bytes;
    int order = 0;
    while (len >= 1024 && order < sizes.Length - 1)
    {
      order++;
      len = len / 1024;
    }
    return $"{len:0.##} {sizes[order]}";
  }
  /// <summary>
  /// Safely copies a file with error handling.
  /// </summary>
  /// <param name="sourcePath">Source file path</param>
  /// <param name="destinationPath">Destination file path</param>
  /// <param name="overwrite">Whether to overwrite existing file</param>
  /// <returns>True if successful</returns>
  public static async Task<bool> SafeCopyFileAsync(string sourcePath, string destinationPath, bool overwrite = false)
  {
    try
    {
      // Ensure the destination directory exists
      var destDir = Path.GetDirectoryName(destinationPath);
      if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
      {
        Directory.CreateDirectory(destDir);
      }

      await Task.Run(() => File.Copy(sourcePath, destinationPath, overwrite));
      return true;
    }
    catch
    {
      return false;
    }
  }
  /// <summary>
  /// Creates a directory if it doesn't exist.
  /// </summary>
  /// <param name="directoryPath">Directory path to create</param>
  /// <returns>True if directory exists or was created successfully</returns>
  public static bool EnsureDirectoryExists(string directoryPath)
  {
    try
    {
      if (string.IsNullOrEmpty(directoryPath))
        return false;

      if (Directory.Exists(directoryPath))
        return true;

      Directory.CreateDirectory(directoryPath);
      return true;
    }
    catch
    {
      return false;
    }
  }
  /// <summary>
  /// Gets available drives on the system.
  /// </summary>
  /// <returns>List of available drive letters</returns>
  public static IEnumerable<string> GetAvailableDrives()
  {
    try
    {
      return DriveInfo.GetDrives()
        .Where(d => d.IsReady)
        .Select(d => d.Name)
        .ToList();
    }
    catch
    {
      return Enumerable.Empty<string>();
    }
  }
  /// <summary>
  /// Generates a unique file name if the target already exists.
  /// </summary>
  /// <param name="basePath">Base file path</param>
  /// <returns>Unique file path</returns>
  public static string GenerateUniqueFileName(string basePath)
  {
    if (!File.Exists(basePath))
      return basePath;

    var directory = Path.GetDirectoryName(basePath) ?? string.Empty;
    var nameWithoutExt = Path.GetFileNameWithoutExtension(basePath);
    var extension = Path.GetExtension(basePath);

    int counter = 1;
    string newPath;

    do
    {
      var newName = $"{nameWithoutExt}_{counter}{extension}";
      newPath = Path.Combine(directory, newName);
      counter++;
    }
    while (File.Exists(newPath));

    return newPath;
  }
  /// <summary>
  /// Validates if a path is valid and accessible.
  /// </summary>
  /// <param name="path">Path to validate</param>
  /// <returns>True if path is valid and accessible</returns>
  public static bool IsValidAndAccessiblePath(string path)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(path))
        return false;

      // Try to get the full path to validate format
      var fullPath = Path.GetFullPath(path);

      // Check if it's a file or directory
      if (File.Exists(fullPath) || Directory.Exists(fullPath))
        return true;

      // If it doesn't exist, check if the parent directory exists (for new files)
      var parentDir = Path.GetDirectoryName(fullPath);
      return !string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir);
    }
    catch
    {
      return false;
    }
  }
  /// <summary>
  /// Gets the relative path between two paths.
  /// </summary>
  /// <param name="fromPath">Source path</param>
  /// <param name="toPath">Target path</param>
  /// <returns>Relative path</returns>
  public static string GetRelativePath(string fromPath, string toPath)
  {
    try
    {
      if (string.IsNullOrEmpty(fromPath) || string.IsNullOrEmpty(toPath))
        return toPath ?? string.Empty;

      Uri fromUri = new Uri(Path.GetFullPath(fromPath));
      Uri toUri = new Uri(Path.GetFullPath(toPath));

      if (fromUri.Scheme != toUri.Scheme)
        return toPath; // Cannot make relative path

      Uri relativeUri = fromUri.MakeRelativeUri(toUri);
      string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

      return relativePath.Replace('/', Path.DirectorySeparatorChar);
    }
    catch
    {
      return toPath;
    }
  }
}
