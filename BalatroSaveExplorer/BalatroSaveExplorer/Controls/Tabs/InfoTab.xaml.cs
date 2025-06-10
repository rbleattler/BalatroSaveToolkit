using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BalatroSaveExplorer.Controls.Tabs;

public partial class InfoTab : UserControl
{
  // Delegates for callbacks to MainWindow
  public Func<string, Task>? LoadFileCallback { get; set; }
  public Action? SwitchToTreeViewTabCallback { get; set; }

  public InfoTab()
  {
    InitializeComponent();
  }

  private async void InfoLoadBalatroSettingsButton_Click(object sender, RoutedEventArgs e)
  {
    if (LoadFileCallback != null && !string.IsNullOrEmpty(InfoBalatroSettingsFilePathTextBox.Text))
    {
      await LoadFileCallback(InfoBalatroSettingsFilePathTextBox.Text);
      SwitchToTreeViewTabCallback?.Invoke();
    }
  }

  private async void InfoLoadProfileSettingsButton_Click(object sender, RoutedEventArgs e)
  {
    if (LoadFileCallback != null && !string.IsNullOrEmpty(InfoCurrentProfileSettingsPathTextBox.Text))
    {
      await LoadFileCallback(InfoCurrentProfileSettingsPathTextBox.Text);
      SwitchToTreeViewTabCallback?.Invoke();
    }
  }

  private async void InfoLoadProfileMetaButton_Click(object sender, RoutedEventArgs e)
  {
    if (LoadFileCallback != null && !string.IsNullOrEmpty(InfoCurrentProfileMetaPathTextBox.Text))
    {
      await LoadFileCallback(InfoCurrentProfileMetaPathTextBox.Text);
      SwitchToTreeViewTabCallback?.Invoke();
    }
  }

  private async void InfoLoadProfileSaveButton_Click(object sender, RoutedEventArgs e)
  {
    if (LoadFileCallback != null && !string.IsNullOrEmpty(InfoCurrentProfileSavePathTextBox.Text))
    {
      await LoadFileCallback(InfoCurrentProfileSavePathTextBox.Text);
      SwitchToTreeViewTabCallback?.Invoke();
    }
  }

  /// <summary>
  /// Updates the Balatro derived paths displayed in the Info tab
  /// </summary>
  public void UpdateBalatroDerivedPaths(string rootPath, string settingsPath, string profileNumber,
                                       string profileSettingsPath, string profileMetaPath, string profileSavePath)
  {
    InfoBalatroSaveRootTextBox.Text = rootPath;
    InfoBalatroSettingsFilePathTextBox.Text = settingsPath;
    InfoCurrentProfileNumberTextBox.Text = profileNumber;
    InfoCurrentProfileSettingsPathTextBox.Text = profileSettingsPath;
    InfoCurrentProfileMetaPathTextBox.Text = profileMetaPath;
    InfoCurrentProfileSavePathTextBox.Text = profileSavePath;
  }
}