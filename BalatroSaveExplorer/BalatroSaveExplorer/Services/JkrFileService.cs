using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BalatroSaveExplorer.Models;
using BalatroSaveExplorer.Converters;

namespace BalatroSaveExplorer.Services;

/// <summary>
/// Service responsible for handling JKR file operations including decompression,
/// JSON processing, and file content management.
/// </summary>
public class JkrFileService
{
  private readonly Logger _logger;

  public JkrFileService(Logger logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }    // TODO: Migrate from MainWindow
       // - DecompressJkrFile method
       // - ParseJsonContent method
       // - ValidateJkrFile method
       // - GetFileInfo method
       // - CompressToJkr method (for saving)

  /// <summary>
  /// Processes a JKR file from start to finish.
  /// </summary>
  /// <param name="filePath">Path to the JKR file</param>
  /// <returns>File processing result</returns>
  public async Task<FileProcessingResult> ProcessFileAsync(string filePath)
  {
    try
    {
      var fileInfo = GetJkrFileInfo(filePath);
      var result = new FileProcessingResult
      {
        FilePath = filePath,
        FileSize = fileInfo.Length,
        ProcessedAt = DateTime.Now
      };

      var startTime = DateTime.Now;

      // Decompress the file
      var content = await DecompressJkrFileAsync(filePath);
      result.Content = content;

      // Parse the content
      var treeNodes = await ParseJsonContentAsync(content);
      result.TreeNodes = treeNodes;

      result.ProcessingTime = DateTime.Now - startTime;
      result.Success = true;

      return result;
    }
    catch (Exception ex)
    {
      _logger.Log($"Error processing file {filePath}: {ex.Message}");
      return FileProcessingResult.CreateFailure(filePath, ex.Message);
    }
  }
  /// <summary>
  /// Decompresses a JKR file and returns the JSON content.
  /// </summary>
  /// <param name="filePath">Path to the JKR file</param>
  /// <returns>Decompressed JSON content as string</returns>
  public async Task<string> DecompressJkrFileAsync(string filePath)
  {
    return await Task.Run(() =>
    {
      try
      {
        _logger.Log("Starting decompression...");

        using var compressedStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        long originalSize = compressedStream.Length;
        _logger.Log($"File size: {originalSize} bytes");

        using var outputStream = new MemoryStream();
        using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);

        deflateStream.CopyTo(outputStream);
        outputStream.Position = 0;

        using var reader = new StreamReader(outputStream, Encoding.UTF8);
        string decompressedContent = reader.ReadToEnd();

        _logger.Log($"Decompression successful. Original: {originalSize} bytes → Decompressed: {decompressedContent.Length} characters");

        if (decompressedContent.StartsWith("return "))
        {
          decompressedContent = decompressedContent.Substring("return ".Length);
        }

        return decompressedContent;
      }
      catch (InvalidDataException ex)
      {
        _logger.Log($"Not a compressed file: {ex.Message}");
        _logger.Log("Reading as plain text...");
        return File.ReadAllText(filePath);
      }
      catch (Exception ex)
      {
        _logger.Log($"Decompression failed: {ex.Message}");
        _logger.Log("Attempting to read as plain text...");
        // Fallback to reading as plain text (in case file is not compressed)
        return File.ReadAllText(filePath);
      }
    });
  }    /// <summary>
       /// Parses JSON content and builds tree structure.
       /// </summary>
       /// <param name="jsonContent">Raw JSON content</param>
       /// <returns>Parsed tree node collection</returns>
  public Task<IEnumerable<TreeNodeViewModel>> ParseJsonContentAsync(string jsonContent)
  {
    return Task.Run(() =>
    {
      try
      {
        // Parse the Lua table
        var parsedData = LuaTableConverter.ParseLuaTable(jsonContent);
        _logger.Log($"Successfully parsed Lua table with {parsedData.Count} root items");

        // Convert to tree nodes
        var treeNodes = new List<TreeNodeViewModel>();
        foreach (var kvp in parsedData)
        {
          treeNodes.Add(CreateTreeNode(kvp.Key, kvp.Value));
        }

        return (IEnumerable<TreeNodeViewModel>)treeNodes;
      }
      catch (Exception ex)
      {
        _logger.Log($"Error parsing Lua content: {ex.Message}");
        return new List<TreeNodeViewModel>();
      }
    });
  }    /// <summary>
       /// Validates if a file is a valid JKR file.
       /// </summary>
       /// <param name="filePath">Path to validate</param>
       /// <returns>True if valid JKR file</returns>
  public bool ValidateJkrFile(string filePath)
  {
    try
    {
      if (!File.Exists(filePath))
        return false;

      var extension = Path.GetExtension(filePath).ToLowerInvariant();
      if (extension != ".jkr")
        return false;

      var fileInfo = new FileInfo(filePath);
      if (fileInfo.Length == 0)
        return false;

      // Try to read a small portion to see if it's accessible
      using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
      var buffer = new byte[Math.Min(1024, fileInfo.Length)];
      stream.Read(buffer, 0, buffer.Length);

      return true;
    }
    catch (Exception ex)
    {
      _logger.Log($"File validation failed for {filePath}: {ex.Message}");
      return false;
    }
  }/// <summary>
   /// Creates a tree node for the given key-value pair.
   /// </summary>
   /// <param name="key">The key name</param>
   /// <param name="value">The value object</param>
   /// <returns>Tree node view model</returns>
  public TreeNodeViewModel CreateTreeNode(string key, object? value)
  {
    var node = new TreeNodeViewModel
    {
      DisplayName = key
    };

    if (value is Dictionary<string, object> dict)
    {
      node.ValueDisplay = $"{{ {dict.Count} items }}";
      foreach (var kvp in dict)
      {
        node.Children.Add(CreateTreeNode(kvp.Key, kvp.Value));
      }
    }
    else if (value is List<object> list)
    {
      node.ValueDisplay = $"[ {list.Count} items ]";
      for (int i = 0; i < list.Count; i++)
      {
        node.Children.Add(CreateTreeNode($"[{i}]", list[i]));
      }
    }
    else
    {
      node.ValueDisplay = value?.ToString() ?? "null";
    }
    return node;
  }

  /// <summary>
  /// Gets basic information about a JKR file.
  /// </summary>
  /// <param name="filePath">Path to the file</param>
  /// <returns>File information object</returns>
  public FileInfo GetJkrFileInfo(string filePath)
  {
    return new FileInfo(filePath);
  }    /// <summary>
       /// Compresses JSON content back to JKR format.
       /// </summary>
       /// <param name="jsonContent">JSON content to compress</param>
       /// <param name="outputPath">Output file path</param>
       /// <returns>Success status</returns>
  public async Task<bool> CompressToJkrAsync(string jsonContent, string outputPath)
  {
    return await Task.Run(() =>
    {
      try
      {
        _logger.Log($"Compressing content to: {outputPath}");

        // Prepend "return " if not already present
        var contentToCompress = jsonContent;
        if (!contentToCompress.StartsWith("return "))
        {
          contentToCompress = "return " + contentToCompress;
        }

        var inputBytes = Encoding.UTF8.GetBytes(contentToCompress);

        using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var deflateStream = new DeflateStream(outputStream, CompressionMode.Compress);

        deflateStream.Write(inputBytes, 0, inputBytes.Length);
        deflateStream.Flush();

        _logger.Log($"Successfully compressed {inputBytes.Length} bytes to {outputPath}");
        return true;
      }
      catch (Exception ex)
      {
        _logger.Log($"Compression failed: {ex.Message}");
        return false;
      }
    });
  }

  /// <summary>
  /// Saves decompressed content to a temporary file.
  /// </summary>
  /// <param name="content">Decompressed content to save</param>
  /// <param name="originalFilePath">Path to the original file</param>
  /// <returns>Path to the temporary file</returns>
  public async Task<string> SaveDecompressedToTempAsync(string content, string originalFilePath)
  {
    return await Task.Run(() =>
    {
      try
      {
        var tempDir = Path.GetTempPath();
        var fileName = Path.GetFileNameWithoutExtension(originalFilePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var tempFileName = $"{fileName}_decompressed_{timestamp}.lua";
        var tempPath = Path.Combine(tempDir, tempFileName);

        File.WriteAllText(tempPath, content, Encoding.UTF8);
        _logger.Log($"Auto-saved decompressed content to: {tempPath}");
        return tempPath;
      }
      catch (Exception ex)
      {
        _logger.Log($"Failed to auto-save decompressed content: {ex.Message}");
        throw;
      }
    });
  }
}
