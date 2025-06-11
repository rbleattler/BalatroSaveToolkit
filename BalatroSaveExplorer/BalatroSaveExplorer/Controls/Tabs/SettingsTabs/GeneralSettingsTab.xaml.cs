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

  // Save management event handlers
  public Action<object, RoutedEventArgs>? EnableAutoSaveCheckBoxCheckedChanged { get; set; }
  public Action<object, TextChangedEventArgs>? AutoSaveIntervalTextBoxTextChanged { get; set; }
  public Action<object, SelectionChangedEventArgs>? AutoSaveIntervalUnitComboBoxSelectionChanged { get; set; }
  public Action<object, RoutedEventArgs>? EnableSaveRetentionCheckBoxCheckedChanged { get; set; }
  public Action<object, TextChangedEventArgs>? SaveRetentionValueTextBoxTextChanged { get; set; }
  public Action<object, SelectionChangedEventArgs>? SaveRetentionUnitComboBoxSelectionChanged { get; set; }

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

  // Save management event handlers
  private void EnableAutoSaveCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
  {
    EnableAutoSaveCheckBoxCheckedChanged?.Invoke(sender, e);
  }

  private void AutoSaveIntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    AutoSaveIntervalTextBoxTextChanged?.Invoke(sender, e);
  }

  private void AutoSaveIntervalUnitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    AutoSaveIntervalUnitComboBoxSelectionChanged?.Invoke(sender, e);
  }

  private void EnableSaveRetentionCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
  {
    EnableSaveRetentionCheckBoxCheckedChanged?.Invoke(sender, e);
  }

  private void SaveRetentionValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    SaveRetentionValueTextBoxTextChanged?.Invoke(sender, e);
  }

  private void SaveRetentionUnitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    SaveRetentionUnitComboBoxSelectionChanged?.Invoke(sender, e);
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

  // Save management control accessors
  public CheckBox EnableAutoSaveCheckBoxControl => EnableAutoSaveCheckBox;
  public TextBox AutoSaveIntervalTextBoxControl => AutoSaveIntervalTextBox;
  public ComboBox AutoSaveIntervalUnitComboBoxControl => AutoSaveIntervalUnitComboBox;
  public CheckBox EnableSaveRetentionCheckBoxControl => EnableSaveRetentionCheckBox;
  public TextBox SaveRetentionValueTextBoxControl => SaveRetentionValueTextBox;
  public ComboBox SaveRetentionUnitComboBoxControl => SaveRetentionUnitComboBox;
}
