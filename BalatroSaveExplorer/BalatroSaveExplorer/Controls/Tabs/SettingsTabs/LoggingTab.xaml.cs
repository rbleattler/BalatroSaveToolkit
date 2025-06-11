using System;
using System.Windows;
using System.Windows.Controls;

namespace BalatroSaveExplorer.Controls.Tabs.SettingsTabs;

public partial class LoggingTab : UserControl
{
  // Event handlers exposed as actions that the parent SettingsTab can connect to
  public Action<object, RoutedEventArgs>? ShowLogsCheckBoxChecked { get; set; }
  public Action<object, RoutedEventArgs>? ShowLogsCheckBoxUnchecked { get; set; }

  public LoggingTab()
  {
    InitializeComponent();
  }

  private void ShowLogsCheckBox_Checked(object sender, RoutedEventArgs e)
  {
    ShowLogsCheckBoxChecked?.Invoke(sender, e);
  }

  private void ShowLogsCheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    ShowLogsCheckBoxUnchecked?.Invoke(sender, e);
  }

  // Property accessors for the controls
  public CheckBox ShowLogsCheckBoxControl => ShowLogsCheckBox;
  public CheckBox ShowLogsOnStartupCheckBoxControl => ShowLogsOnStartupCheckBox;
  public ComboBox LogLevelComboBoxControl => LogLevelComboBox;
  public TextBox MaxLogEntriesTextBoxControl => MaxLogEntriesTextBox;
}
