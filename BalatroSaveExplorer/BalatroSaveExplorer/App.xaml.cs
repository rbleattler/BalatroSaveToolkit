using System.Configuration;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using BalatroSaveExplorer.Services;

namespace BalatroSaveExplorer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize the settings system
        var settingsManager = SettingsManager.Instance;
        settingsManager.ValidateSettings();

        // Initialize and apply the theme system
        var themeManager = ThemeManager.Instance;
        themeManager.ApplyCurrentTheme();

        // Ensure the theme is properly applied
        Resources.MergedDictionaries.RemoveAt(0); // Remove the default LightTheme
        themeManager.ApplyCurrentTheme(); // Re-apply the theme

        // Log the startup
        System.Diagnostics.Debug.WriteLine($"BalatroSaveExplorer started. Settings loaded from: {settingsManager.GetSettingsFilePath()}");

        #region Load Styles

        // // assume your xaml files are in /Styles/ and marked BuildAction=Page
        // var asm = Assembly.GetExecutingAssembly();
        // var appName = asm.GetName().Name;
        // var stylesDir = "Styles";
        // var themesDir = "Themes";

        // LoadXaml(asm, appName, themesDir); //These don't seem to work for now...
        // LoadXaml(asm, appName, stylesDir); //These don't seem to work for now...

        // void LoadXaml(Assembly asm, string? appName, string stylesDir)
        // {
        //     var packPrefix = $"pack://application:,,,/{appName};component/{stylesDir}/";

        //     // if you want to read from disk (eg. when running from VS)
        //     var diskDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, stylesDir);
        //     if (Directory.Exists(diskDir))
        //     {
        //         foreach (var file in Directory.EnumerateFiles(diskDir, "*.xaml"))
        //         {
        //             var uri = new Uri(packPrefix + Path.GetFileName(file), UriKind.Absolute);
        //             Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
        //         }
        //     }
        //     else
        //     {
        //         // fallback: load from embedded resources via manifest
        //         var manifestNames = asm.GetManifestResourceNames()
        //                                .Where(n => n.Contains($".{stylesDir}.") && n.EndsWith(".xaml"));
        //         foreach (var manifestName in manifestNames)
        //         {
        //             using var stream = asm.GetManifestResourceStream(manifestName);
        //             if (stream == null) continue;
        //             var dict = (ResourceDictionary)System.Windows.Markup.XamlReader.Load(stream);
        //             Resources.MergedDictionaries.Add(dict);
        //         }
        //     }
        // }


        #endregion Load Styles




    }
    protected override void OnExit(ExitEventArgs e)
    {
        // Ensure settings are saved on exit
        SettingsManager.Instance.SaveSettings();

        // Cleanup theme manager
        ThemeManager.Instance.Dispose();

        base.OnExit(e);
    }
}

