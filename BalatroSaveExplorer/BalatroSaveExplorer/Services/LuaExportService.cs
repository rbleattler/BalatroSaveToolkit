using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BalatroSaveExplorer.Utilities;
using BalatroSaveExplorer.Services;

namespace BalatroSaveExplorer.Services;

/// <summary>
/// Service responsible for exporting decompressed JKR content to Lua files.
/// Handles file dialogs, overwrite confirmation, and file writing operations.
/// </summary>
public class LuaExportService
{
  private readonly Logger _logger;

  public LuaExportService(Logger logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Exports content to a Lua file with user interaction for file selection and overwrite confirmation.
  /// </summary>
  /// <param name="content">The content to export</param>
  /// <param name="sourceFilePath">The original file path (used for default naming)</param>
  /// <param name="outputDirectory">Optional output directory from settings</param>
  /// <param name="confirmOverwrites">Whether to confirm file overwrites</param>
  /// <returns>True if export was successful, false if cancelled or failed</returns>
  public async Task<bool> SaveAsLuaAsync(string content, string sourceFilePath, string? outputDirectory = null, bool confirmOverwrites = true)
  {
    if (string.IsNullOrEmpty(content))
    {
      _logger.Log("Cannot export empty content to Lua file");
      StatusBarService.Instance.SetActivity("No content to export.", true);
      return false;
    }

    if (string.IsNullOrEmpty(sourceFilePath))
    {
      _logger.Log("Source file path is required for Lua export");
      StatusBarService.Instance.SetActivity("Source file path is required.", true);
      return false;
    }

    try
    {
      string defaultFileName = GenerateDefaultLuaFileName(sourceFilePath);

      var outputPath = FileOperations.ShowSaveLuaFileDialog(
          "Save as Lua File",
          defaultFileName,
          outputDirectory);

      if (string.IsNullOrEmpty(outputPath))
      {
        _logger.Log("Lua export cancelled by user");
        return false;
      }

      // Check if file exists and confirm overwrite if required
      if (File.Exists(outputPath) && confirmOverwrites)
      {
        // Instead of MessageBox, show a status bar prompt (non-blocking)
        StatusBarService.Instance.SetActivity($"The file '{Path.GetFileName(outputPath)}' already exists. Overwrite not implemented in status bar version.", true);
        return false;
      }
      _logger.Log($"Saving decompressed content to: {outputPath}");

      // Write the Lua content
      await WriteLuaFileAsync(content, outputPath);

      _logger.Log($"Successfully saved {content.Length} characters to {outputPath}");

      StatusBarService.Instance.SetActivity($"File saved successfully as: {Path.GetFileName(outputPath)}");

      return true;
    }
    catch (Exception ex)
    {
      _logger.Log($"Lua export failed: {ex.Message}");
      StatusBarService.Instance.SetActivity($"Lua export failed: {ex.Message}", true);
      return false;
    }
  }

  /// <summary>
  /// Generates a default Lua filename based on the source file path.
  /// </summary>
  /// <param name="sourceFilePath">The source file path</param>
  /// <returns>Default Lua filename</returns>
  public string GenerateDefaultLuaFileName(string sourceFilePath)
  {
    if (string.IsNullOrEmpty(sourceFilePath))
      return "export.lua";

    return Path.GetFileNameWithoutExtension(sourceFilePath) + ".lua";
  }

  /// <summary>
  /// Checks if the user wants to overwrite an existing file.
  /// </summary>
  /// <param name="outputPath">The output file path</param>
  /// <returns>True if overwrite is confirmed, false otherwise</returns>
  public bool CheckFileOverwrite(string outputPath)
  {
    var result = MessageBox.Show(
        $"The file '{Path.GetFileName(outputPath)}' already exists. Do you want to overwrite it?",
        "File Exists",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);

    return result == MessageBoxResult.Yes;
  }

  /// <summary>
  /// Writes content to a Lua file with proper formatting.
  /// </summary>
  /// <param name="content">The content to write</param>
  /// <param name="outputPath">The output file path</param>
  private async Task WriteLuaFileAsync(string content, string outputPath)
  {
    // Prepend "return = " to make it valid Lua syntax
    string luaContent = "return = " + content;

    // Write the content to the .lua file
    await File.WriteAllTextAsync(outputPath, luaContent, Encoding.UTF8);
  }
}
