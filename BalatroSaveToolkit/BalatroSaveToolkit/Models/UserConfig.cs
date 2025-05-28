namespace BalatroSaveToolkit.Models;

public class UserConfig {
    public string? Theme = "System";
    public string? Language = "en-US";
    public double? RefreshRate = 1.0;                // refresh rate in minutes (default is 1 minute)
    public bool? EnableToasts = false;               // enable or disable toast notifications
    public bool? EnableSounds = false;               // enable or disable sounds
    public string? SaveBasePath = GetSaveLocation(); // save location for user data
    public int? UserProfile = 1;

    static string? GetSaveLocation() {
        // The default save location differs depending on the platform
        // in Windows, it is in the '%AppData%\Balatro' directory. for now, we'll use System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) to get the appdata folder, and append 'Balatro' to it. We'll worry about mac/linux later.


        return System.IO.Path.Combine(
                                      Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                                      "Balatro"
                                     );
    }
}
