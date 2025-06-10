using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BalatroSaveExplorer.Models;

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
    }

    /// <summary>
    /// Updates the derived Balatro paths in the UI controls
    /// </summary>
    public void UpdateBalatroDerivedPaths(string saveRoot,
        TextBox infoBalatroSaveRootTextBox,
        TextBox infoBalatroSettingsFilePathTextBox,
        TextBox infoCurrentProfileNumberTextBox,
        TextBox infoCurrentProfileSettingsPathTextBox,
        TextBox infoCurrentProfileMetaPathTextBox,
        TextBox infoCurrentProfileSavePathTextBox)
    {
        if (string.IsNullOrWhiteSpace(saveRoot))
        {
            // Clear Info tab controls
            infoBalatroSaveRootTextBox.Text = "";
            infoBalatroSettingsFilePathTextBox.Text = "";
            infoCurrentProfileNumberTextBox.Text = "";
            infoCurrentProfileSettingsPathTextBox.Text = "";
            infoCurrentProfileMetaPathTextBox.Text = "";
            infoCurrentProfileSavePathTextBox.Text = "";
            return;
        }

        try
        {
            var settingsPath = Path.Combine(saveRoot, "settings.jkr");

            // Update Info tab controls
            infoBalatroSaveRootTextBox.Text = saveRoot;
            infoBalatroSettingsFilePathTextBox.Text = settingsPath;
            infoCurrentProfileNumberTextBox.Text = "1"; // Default profile

            var profilePath = Path.Combine(saveRoot, "1");
            infoCurrentProfileSettingsPathTextBox.Text = Path.Combine(profilePath, "profile.jkr");
            infoCurrentProfileMetaPathTextBox.Text = Path.Combine(profilePath, "meta.jkr");
            infoCurrentProfileSavePathTextBox.Text = Path.Combine(profilePath, "save.jkr");
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
            MessageBox.Show("Settings file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            MessageBox.Show("Profile settings file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            MessageBox.Show("Profile meta file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            MessageBox.Show("Profile save file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
    }
}
