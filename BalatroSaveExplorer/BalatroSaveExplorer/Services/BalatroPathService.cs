using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BalatroSaveExplorer.Models;
using BalatroSaveExplorer.Services;

namespace BalatroSaveExplorer.Services;

/// <summary>
/// Service responsible for managing Balatro-specific file paths and operations.
/// Handles path derivation, file loading operations for Balatro files.
/// </summary>
public class BalatroPathService
{
    private readonly Logger _logger;

    public BalatroPathService(Logger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }    /// <summary>
         /// Updates the derived Balatro paths using a callback function
         /// </summary>
    public void UpdateBalatroDerivedPaths(string saveRoot, Action<string, string, string, string, string, string> updateCallback)
    {
        if (string.IsNullOrWhiteSpace(saveRoot))
        {
            // Clear paths
            updateCallback("", "", "", "", "", "");
            return;
        }

        try
        {
            var settingsPath = Path.Combine(saveRoot, "settings.jkr");
            var profileNumber = "1"; // Default profile
            var profilePath = Path.Combine(saveRoot, "1");
            var profileSettingsPath = Path.Combine(profilePath, "profile.jkr");
            var profileMetaPath = Path.Combine(profilePath, "meta.jkr");
            var profileSavePath = Path.Combine(profilePath, "save.jkr");

            updateCallback(saveRoot, settingsPath, profileNumber, profileSettingsPath, profileMetaPath, profileSavePath);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error updating derived paths: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads a Balatro settings file if it exists
    /// </summary>
    public async Task<bool> LoadBalatroSettingsFileAsync(string filePath, Func<string, Task> loadFileFunc, Action selectTreeViewTab)
    {
        if (File.Exists(filePath))
        {
            await loadFileFunc(filePath);
            selectTreeViewTab();
            return true;
        }
        else
        {
            StatusBarService.Instance.SetActivity("Settings file not found.", true);
            return false;
        }
    }

    /// <summary>
    /// Loads a profile settings file if it exists
    /// </summary>
    public async Task<bool> LoadProfileSettingsAsync(string filePath, Func<string, Task> loadFileFunc, Action selectTreeViewTab)
    {
        if (File.Exists(filePath))
        {
            await loadFileFunc(filePath);
            selectTreeViewTab();
            return true;
        }
        else
        {
            StatusBarService.Instance.SetActivity("Profile settings file not found.", true);
            return false;
        }
    }

    /// <summary>
    /// Loads a profile meta file if it exists
    /// </summary>
    public async Task<bool> LoadProfileMetaAsync(string filePath, Func<string, Task> loadFileFunc, Action selectTreeViewTab)
    {
        if (File.Exists(filePath))
        {
            await loadFileFunc(filePath);
            selectTreeViewTab();
            return true;
        }
        else
        {
            StatusBarService.Instance.SetActivity("Profile meta file not found.", true);
            return false;
        }
    }

    /// <summary>
    /// Loads a profile save file if it exists
    /// </summary>
    public async Task<bool> LoadProfileSaveAsync(string filePath, Func<string, Task> loadFileFunc, Action selectTreeViewTab)
    {
        if (File.Exists(filePath))
        {
            await loadFileFunc(filePath);
            selectTreeViewTab();
            return true;
        }
        else
        {
            StatusBarService.Instance.SetActivity("Profile save file not found.", true);
            return false;
        }
    }
}
