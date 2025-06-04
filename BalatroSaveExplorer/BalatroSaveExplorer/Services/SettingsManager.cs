using System.IO;
using System.Text.Json;
using BalatroSaveExplorer.Models;

namespace BalatroSaveExplorer.Services;

/// <summary>
/// Manages application settings including persistence to/from JSON file
/// </summary>
public class SettingsManager
{
    private static SettingsManager? _instance;
    private static readonly object _lock = new object();

    private readonly string _settingsFilePath;
    private AppSettings _settings;

    /// <summary>
    /// Gets the singleton instance of the SettingsManager
    /// </summary>
    public static SettingsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new SettingsManager();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Gets the current application settings
    /// </summary>
    public AppSettings Settings => _settings;

    private SettingsManager()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "BalatroSaveExplorer");

        // Ensure the application data folder exists
        Directory.CreateDirectory(appFolder);

        _settingsFilePath = Path.Combine(appFolder, "settings.json");
        _settings = LoadSettings();

        // Subscribe to property changes to auto-save
        _settings.PropertyChanged += (sender, e) => SaveSettings();
    }

    /// <summary>
    /// Loads settings from the JSON file, or creates default settings if file doesn't exist
    /// </summary>
    private AppSettings LoadSettings()
    {
        if (!File.Exists(_settingsFilePath))
        {
            var defaultSettings = new AppSettings();
            SaveSettingsToFile(defaultSettings);
            return defaultSettings;
        }

        try
        {
            var json = File.ReadAllText(_settingsFilePath);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<AppSettings>(json, options) ?? new AppSettings();
        }
        catch (Exception ex)
        {
            // If settings file is corrupted, create new default settings
            Console.WriteLine($"Error loading settings: {ex.Message}. Using default settings.");
            var defaultSettings = new AppSettings();
            SaveSettingsToFile(defaultSettings);
            return defaultSettings;
        }
    }

    /// <summary>
    /// Saves the current settings to the JSON file
    /// </summary>
    public void SaveSettings()
    {
        SaveSettingsToFile(_settings);
    }

    /// <summary>
    /// Saves the specified settings to the JSON file
    /// </summary>
    private void SaveSettingsToFile(AppSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Resets all settings to their default values
    /// </summary>
    public void ResetToDefaults()
    {
        _settings = new AppSettings();
        SaveSettings();
    }

    /// <summary>
    /// Gets the full path to the settings file
    /// </summary>
    public string GetSettingsFilePath() => _settingsFilePath;

    /// <summary>
    /// Creates a backup of the current settings file
    /// </summary>
    public void BackupSettings()
    {
        if (File.Exists(_settingsFilePath))
        {
            var backupPath = _settingsFilePath + $".backup.{DateTime.Now:yyyyMMdd_HHmmss}";
            File.Copy(_settingsFilePath, backupPath);
        }
    }    /// <summary>
    /// Validates that all directory paths in settings exist and are accessible
    /// </summary>
    public void ValidateSettings()
    {
        try
        {
            // Validate and create directories if they don't exist
            if (!Directory.Exists(_settings.DefaultJkrDirectory))
            {
                _settings.DefaultJkrDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            if (!Directory.Exists(_settings.DefaultLuaExportDirectory))
            {
                _settings.DefaultLuaExportDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            if (!Directory.Exists(_settings.BackupDirectory))
            {
                Directory.CreateDirectory(_settings.BackupDirectory);
            }

            // Validate Balatro save root directory
            if (!Directory.Exists(_settings.BalatroSaveRoot))
            {
                // Try to find default Balatro directory
                var defaultBalatroPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Balatro");
                if (Directory.Exists(defaultBalatroPath))
                {
                    _settings.BalatroSaveRoot = defaultBalatroPath;
                }
                else
                {
                    // Create the directory structure as a fallback
                    try
                    {
                        Directory.CreateDirectory(_settings.BalatroSaveRoot);
                    }
                    catch
                    {
                        // If we can't create it, reset to default AppData path
                        _settings.BalatroSaveRoot = defaultBalatroPath;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating settings: {ex.Message}");
        }
    }
}
