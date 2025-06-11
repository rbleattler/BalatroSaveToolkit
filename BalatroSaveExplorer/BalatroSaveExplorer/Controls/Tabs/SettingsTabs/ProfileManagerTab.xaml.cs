using System;
using System.Windows;
using System.Windows.Controls;

namespace BalatroSaveExplorer.Controls.Tabs.SettingsTabs;

public partial class ProfileManagerTab : UserControl
{
  // Event handlers exposed as actions that the parent SettingsTab can connect to
  public Action<object, RoutedEventArgs>? BrowseBalatroSaveRootButtonClick { get; set; }
  public Action<object, TextChangedEventArgs>? BalatroSaveRootTextBoxTextChanged { get; set; }

  public ProfileManagerTab()
  {
    InitializeComponent();
  }

  private void BrowseBalatroSaveRootButton_Click(object sender, RoutedEventArgs e)
  {
    BrowseBalatroSaveRootButtonClick?.Invoke(sender, e);
  }

  private void BalatroSaveRootTextBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    BalatroSaveRootTextBoxTextChanged?.Invoke(sender, e);
  }

  // Property accessor for the control
  public TextBox BalatroSaveRootTextBoxControl => BalatroSaveRootTextBox;
}
