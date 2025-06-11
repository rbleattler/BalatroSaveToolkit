using System;
using System.Windows;
using System.Windows.Controls;

namespace BalatroSaveExplorer.Controls.Tabs.SettingsTabs;

public partial class GeneralSettingsTab : UserControl
{
  // Event handlers exposed as actions that the parent SettingsTab can connect to
  public Action<object, SelectionChangedEventArgs>? ThemeComboBoxSelectionChanged { get; set; }
  public Action<object, RoutedEventArgs>? BrowseJkrDirectoryButtonClick { get; set; }
  public Action<object, RoutedEventArgs>? BrowseLuaExportDirectoryButtonClick { get; set; }
  public Action<object, RoutedEventArgs>? BrowseBackupDirectoryButtonClick { get; set; }

  public GeneralSettingsTab()
  {
    InitializeComponent();
  }

  private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    ThemeComboBoxSelectionChanged?.Invoke(sender, e);
  }

  private void BrowseJkrDirectoryButton_Click(object sender, RoutedEventArgs e)
  {
    BrowseJkrDirectoryButtonClick?.Invoke(sender, e);
  }

  private void BrowseLuaExportDirectoryButton_Click(object sender, RoutedEventArgs e)
  {
    BrowseLuaExportDirectoryButtonClick?.Invoke(sender, e);
  }

  private void BrowseBackupDirectoryButton_Click(object sender, RoutedEventArgs e)
  {
    BrowseBackupDirectoryButtonClick?.Invoke(sender, e);
  }

  // Property accessors for the controls
  public ComboBox ThemeComboBoxControl => ThemeComboBox;
  public TextBox DefaultJkrDirectoryTextBoxControl => DefaultJkrDirectoryTextBox;
  public TextBox DefaultLuaExportDirectoryTextBoxControl => DefaultLuaExportDirectoryTextBox;
  public CheckBox AutoSaveDecompressedFilesCheckBoxControl => AutoSaveDecompressedFilesCheckBox;
  public CheckBox ConfirmFileOverwritesCheckBoxControl => ConfirmFileOverwritesCheckBox;
  public CheckBox EnableAutoBackupCheckBoxControl => EnableAutoBackupCheckBox;
  public CheckBox EnableFileWatchingCheckBoxControl => EnableFileWatchingCheckBox;
  public CheckBox FlashTaskbarOnUpdateCheckBoxControl => FlashTaskbarOnUpdateCheckBox;
  public CheckBox AutoRefreshOnFileChangeCheckBoxControl => AutoRefreshOnFileChangeCheckBox;
  public TextBox BackupDirectoryTextBoxControl => BackupDirectoryTextBox;
}
