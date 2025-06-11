using System;
using System.Windows;
using System.Windows.Controls;

namespace BalatroSaveExplorer.Controls.Tabs.SettingsTabs;

public partial class AboutTab : UserControl
{
  // Event handlers exposed as actions that the parent SettingsTab can connect to
  public Action<object, RoutedEventArgs>? OpenSettingsFolderButtonClick { get; set; }

  public AboutTab()
  {
    InitializeComponent();
  }

  private void OpenSettingsFolderButton_Click(object sender, RoutedEventArgs e)
  {
    OpenSettingsFolderButtonClick?.Invoke(sender, e);
  }

  // Property accessor for the control
  public TextBox SettingsFilePathTextBoxControl => SettingsFilePathTextBox;

  /// <summary>
  /// Sets the settings file path for display
  /// </summary>
  public void SetSettingsFilePath(string path)
  {
    SettingsFilePathTextBox.Text = path;
  }
}
