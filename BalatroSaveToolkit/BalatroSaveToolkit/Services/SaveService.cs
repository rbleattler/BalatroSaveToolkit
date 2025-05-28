namespace BalatroSaveToolkit.Services;

// Monitors the game save location, defined by UserConfig.SaveBasePath + UserConfig.UserProfile (I.E. "C:\Users\Username\AppData\Roaming\Balatro\1") 
// When the 'save.jkr' file is created or modified, it will create a new copy in the 'Saves' directory (UserConfig.SaveBasePath + "BalatroSaveToolkit\Saves\" + UserConfig.UserProfile)
// titled based on YY-MM-DD-hh-mm-ss-{Ante}-{Blind}-{Hand}.jkr (the Ante, Blind, and Hand will be determined by logic not yet implemented)
public class SaveService {
    private readonly UserConfig _userConfig;
    private readonly string _saveDirectory;
    private readonly string _saveFileName = "save.jkr";
    private readonly string _savesDirectory;

    public SaveService(UserConfig userConfig) {
        _userConfig = userConfig ?? throw new ArgumentNullException(nameof(userConfig));
        _saveDirectory = System.IO.Path.Combine(
                                                _userConfig.SaveBasePath ?? string.Empty,
                                                _userConfig.UserProfile?.ToString() ?? "1"
                                               );
        _savesDirectory = System.IO.Path.Combine(
                                                 _userConfig.SaveBasePath ?? string.Empty,
                                                 "BalatroSaveToolkit",
                                                 "Saves",
                                                 _userConfig.UserProfile?.ToString() ?? "1"
                                                );

        if (!Directory.Exists(_savesDirectory)) { Directory.CreateDirectory(_savesDirectory); }
    }

    // Method to start monitoring the save file
    public void StartMonitoring() {
        // Implementation for monitoring the save file goes here
    }

    // Method to stop monitoring the save file
    public void StopMonitoring() {
        // Implementation for stopping the monitoring goes here
    }
}
