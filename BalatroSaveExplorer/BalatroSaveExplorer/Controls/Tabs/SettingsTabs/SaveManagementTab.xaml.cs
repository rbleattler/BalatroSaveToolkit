using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using BalatroSaveExplorer.Services;

namespace BalatroSaveExplorer.Controls.Tabs.SettingsTabs;

public partial class SaveManagementTab : UserControl
{
  public SaveManagementService? SaveManagementService { get; set; }

  // Save management event handlers
  public Action<object, RoutedEventArgs>? BrowseBackupDirectoryButtonClick { get; set; }
  public Action<object, RoutedEventArgs>? EnableAutoSaveCheckBoxCheckedChanged { get; set; }
  public Action<object, TextChangedEventArgs>? AutoSaveIntervalTextBoxTextChanged { get; set; }
  public Action<object, SelectionChangedEventArgs>? AutoSaveIntervalUnitComboBoxSelectionChanged { get; set; }
  public Action<object, RoutedEventArgs>? EnableSaveRetentionCheckBoxCheckedChanged { get; set; }
  public Action<object, TextChangedEventArgs>? SaveRetentionValueTextBoxTextChanged { get; set; }
  public Action<object, SelectionChangedEventArgs>? SaveRetentionUnitComboBoxSelectionChanged { get; set; }

  public SaveManagementTab()
  {
    InitializeComponent();

    // Initialize profile info
    UpdateCurrentProfileInfo(1, "");
  }

  // Save management event handlers
  private void BrowseBackupDirectoryButton_Click(object sender, RoutedEventArgs e)
  {
    BrowseBackupDirectoryButtonClick?.Invoke(sender, e);
  }

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

  // Save management control accessors
  public TextBox BackupDirectoryTextBoxControl => BackupDirectoryTextBox;
  public CheckBox EnableAutoSaveCheckBoxControl => EnableAutoSaveCheckBox;
  public TextBox AutoSaveIntervalTextBoxControl => AutoSaveIntervalTextBox;
  public ComboBox AutoSaveIntervalUnitComboBoxControl => AutoSaveIntervalUnitComboBox;
  public CheckBox EnableSaveRetentionCheckBoxControl => EnableSaveRetentionCheckBox;
  public TextBox SaveRetentionValueTextBoxControl => SaveRetentionValueTextBox;
  public ComboBox SaveRetentionUnitComboBoxControl => SaveRetentionUnitComboBox;

  #region Public Methods

  /// <summary>
  /// Updates the current profile information display
  /// </summary>
  public void UpdateCurrentProfileInfo(int profileNumber, string saveFilePath)
  {
    CurrentProfileTextBlock.Text = profileNumber.ToString();
    CurrentSaveFileTextBlock.Text = saveFilePath;

    if (File.Exists(saveFilePath))
    {
      var lastModified = File.GetLastWriteTime(saveFilePath);
      LastModifiedTextBlock.Text = lastModified.ToString("yyyy-MM-dd HH:mm:ss");
    }
    else
    {
      LastModifiedTextBlock.Text = "File not found";
    }
  }

  #endregion
}
