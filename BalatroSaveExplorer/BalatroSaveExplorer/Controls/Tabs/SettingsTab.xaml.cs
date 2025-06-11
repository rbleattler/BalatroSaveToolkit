using System;
using System.Windows;
using System.Windows.Controls;
using BalatroSaveExplorer.Models;
using BalatroSaveExplorer.Services;

namespace BalatroSaveExplorer.Controls.Tabs;

public partial class SettingsTab : UserControl
{
  // Delegates for callbacks to MainWindow
  public Action<AppSettings, Action, Action, Func<AppSettings>, Action<AppSettings>>? ApplySettingsCallback { get; set; }
  public Action<Action<AppSettings>, Action>? ResetToDefaultsCallback { get; set; }
  public Action<string, Action>? BrowseBalatroSaveRootCallback { get; set; }
  public Action? UpdateBalatroDerivedPathsCallback { get; set; }
  public Action<RowDefinition, Logger>? ShowLogPanelCallback { get; set; }
  public Action<RowDefinition, Logger>? HideLogPanelCallback { get; set; }
  // Service references (will be injected from MainWindow)
  public SettingsUIService? SettingsUIService { get; set; }
  public Logger? Logger { get; set; }
  public RowDefinition? LogPanelRow { get; set; }
  public SaveManagementService? SaveManagementService { get; set; }

  // Settings reference (will be injected from MainWindow)
  public AppSettings? WorkingSettings { get; set; }

  public SettingsTab()
  {
    InitializeComponent();

    // Connect sub-tab event handlers after InitializeComponent
    Loaded += (s, e) => ConnectSubTabEventHandlers();
  }
  private void ConnectSubTabEventHandlers()
  {
    // General Settings Tab
    GeneralSettingsTabControl.ThemeComboBoxSelectionChanged = ThemeComboBox_SelectionChanged;
    GeneralSettingsTabControl.BrowseJkrDirectoryButtonClick = BrowseJkrDirectoryButton_Click;
    GeneralSettingsTabControl.BrowseLuaExportDirectoryButtonClick = BrowseLuaExportDirectoryButton_Click;

    // Save Management Tab - wire save management events to this tab instead of General Settings
    SaveManagementTabControl.BrowseBackupDirectoryButtonClick = BrowseBackupDirectoryButton_Click;
    SaveManagementTabControl.EnableAutoSaveCheckBoxCheckedChanged = EnableAutoSaveCheckBox_CheckedChanged;
    SaveManagementTabControl.AutoSaveIntervalTextBoxTextChanged = AutoSaveIntervalTextBox_TextChanged;
    SaveManagementTabControl.AutoSaveIntervalUnitComboBoxSelectionChanged = AutoSaveIntervalUnitComboBox_SelectionChanged;
    SaveManagementTabControl.EnableSaveRetentionCheckBoxCheckedChanged = EnableSaveRetentionCheckBox_CheckedChanged;
    SaveManagementTabControl.SaveRetentionValueTextBoxTextChanged = SaveRetentionValueTextBox_TextChanged;
    SaveManagementTabControl.SaveRetentionUnitComboBoxSelectionChanged = SaveRetentionUnitComboBox_SelectionChanged;// Profile Manager Tab
    ProfileManagerTabControl.BrowseBalatroSaveRootButtonClick = BrowseBalatroSaveRootButton_Click;
    ProfileManagerTabControl.BalatroSaveRootTextBoxTextChanged = BalatroSaveRootTextBox_TextChanged;

    // Logging Tab
    LoggingTabControl.ShowLogsCheckBoxChecked = ShowLogsCheckBox_Checked;
    LoggingTabControl.ShowLogsCheckBoxUnchecked = ShowLogsCheckBox_Unchecked;

    // About Tab
    AboutTabControl.OpenSettingsFolderButtonClick = OpenSettingsFolderButton_Click;

    // Initialize save management service reference for status display only
    if (SaveManagementService != null)
    {
      SaveManagementTabControl.SaveManagementService = SaveManagementService;
    }
  }
  #region Event Handlers

  private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    // Only apply theme change if the selection is from user interaction
    // (not from programmatic loading)
    if (GeneralSettingsTabControl.ThemeComboBoxControl.SelectedItem is ComboBoxItem selectedItem && IsLoaded &&
        SettingsUIService != null && WorkingSettings != null)
    {
      SettingsUIService.HandleThemeChange(WorkingSettings, selectedItem, IsLoaded);
    }
  }

  private void BrowseJkrDirectoryButton_Click(object sender, RoutedEventArgs e)
  {
    SettingsUIService?.BrowseJkrDirectory(GeneralSettingsTabControl.DefaultJkrDirectoryTextBoxControl);
  }

  private void BrowseLuaExportDirectoryButton_Click(object sender, RoutedEventArgs e)
  {
    SettingsUIService?.BrowseLuaExportDirectory(GeneralSettingsTabControl.DefaultLuaExportDirectoryTextBoxControl);
  }
  private void BrowseBackupDirectoryButton_Click(object sender, RoutedEventArgs e)
  {
    SettingsUIService?.BrowseBackupDirectory(SaveManagementTabControl.BackupDirectoryTextBoxControl);
  }

  private void BrowseBalatroSaveRootButton_Click(object sender, RoutedEventArgs e)
  {
    if (BrowseBalatroSaveRootCallback != null && UpdateBalatroDerivedPathsCallback != null)
    {
      BrowseBalatroSaveRootCallback(ProfileManagerTabControl.BalatroSaveRootTextBoxControl.Text, UpdateBalatroDerivedPathsCallback);
    }
  }
  private void BalatroSaveRootTextBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    UpdateBalatroDerivedPathsCallback?.Invoke();
  }
  private void DebugCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
  {
    // Debug mode toggled - could be used to enable additional logging
    var isChecked = sender is CheckBox checkBox && (checkBox.IsChecked ?? false);
    Logger?.Log($"Debug mode {(isChecked ? "enabled" : "disabled")}");
  }  // Save management event handlers
  private void EnableAutoSaveCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
  {
    var isEnabled = SaveManagementTabControl.EnableAutoSaveCheckBoxControl.IsChecked ?? false;
    Logger?.Log($"Auto-save {(isEnabled ? "enabled" : "disabled")} - settings will be applied when Apply is clicked");

    // Update the SaveManagementService immediately if available to reflect the change
    if (SaveManagementService != null && WorkingSettings != null)
    {
      // Preview the change without saving to settings yet
      var previewInterval = int.TryParse(SaveManagementTabControl.AutoSaveIntervalTextBoxControl.Text, out int interval) ? interval : 5;
      var previewUnit = (SaveManagementTabControl.AutoSaveIntervalUnitComboBoxControl.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Minutes";

      if (isEnabled && previewInterval > 0)
      {
        Logger?.Log($"Auto-save will activate every {previewInterval} {previewUnit.ToLower()} when settings are applied");
      }
    }
  }
  private void AutoSaveIntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    var textBox = sender as TextBox;
    if (textBox != null && int.TryParse(textBox.Text, out int interval))
    {
      if (interval <= 0)
      {
        Logger?.Log("WARNING: Auto-save interval must be greater than 0");
      }
      else if (interval > 0 && SaveManagementTabControl.EnableAutoSaveCheckBoxControl.IsChecked == true)
      {
        var unit = (SaveManagementTabControl.AutoSaveIntervalUnitComboBoxControl.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Minutes";
        Logger?.Log($"Auto-save interval updated to {interval} {unit.ToLower()}");
      }
    }
    else if (!string.IsNullOrEmpty(textBox?.Text))
    {
      Logger?.Log("WARNING: Auto-save interval must be a valid number");
    }
  }
  private void AutoSaveIntervalUnitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (SaveManagementTabControl.EnableAutoSaveCheckBoxControl.IsChecked == true)
    {
      var interval = SaveManagementTabControl.AutoSaveIntervalTextBoxControl.Text;
      var unit = (sender as ComboBox)?.SelectedItem as ComboBoxItem;

      if (int.TryParse(interval, out int intervalValue) && intervalValue > 0 && unit != null)
      {
        Logger?.Log($"Auto-save interval unit changed to {unit.Content}");
      }
    }
  }
  private void EnableSaveRetentionCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
  {
    var isEnabled = SaveManagementTabControl.EnableSaveRetentionCheckBoxControl.IsChecked ?? false;
    Logger?.Log($"Save retention {(isEnabled ? "enabled" : "disabled")} - settings will be applied when Apply is clicked");

    if (isEnabled)
    {
      var retentionValue = SaveManagementTabControl.SaveRetentionValueTextBoxControl.Text;
      var retentionUnit = (SaveManagementTabControl.SaveRetentionUnitComboBoxControl.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Days";

      if (int.TryParse(retentionValue, out int value) && value > 0)
      {
        Logger?.Log($"Old saves will be deleted after {value} {retentionUnit.ToLower()} when settings are applied");
      }
    }
  }
  private void SaveRetentionValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    var textBox = sender as TextBox;
    if (textBox != null && int.TryParse(textBox.Text, out int value))
    {
      if (value <= 0)
      {
        Logger?.Log("WARNING: Save retention value must be greater than 0");
      }
      else if (value > 0 && SaveManagementTabControl.EnableSaveRetentionCheckBoxControl.IsChecked == true)
      {
        var unit = (SaveManagementTabControl.SaveRetentionUnitComboBoxControl.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Days";
        Logger?.Log($"Save retention period updated to {value} {unit.ToLower()}");
      }
    }
    else if (!string.IsNullOrEmpty(textBox?.Text))
    {
      Logger?.Log("WARNING: Save retention value must be a valid number");
    }
  }
  private void SaveRetentionUnitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (SaveManagementTabControl.EnableSaveRetentionCheckBoxControl.IsChecked == true)
    {
      var value = SaveManagementTabControl.SaveRetentionValueTextBoxControl.Text;
      var unit = (sender as ComboBox)?.SelectedItem as ComboBoxItem;

      if (int.TryParse(value, out int retentionValue) && retentionValue > 0 && unit != null)
      {
        Logger?.Log($"Save retention unit changed to {unit.Content}");
      }
    }
  }


  private void ApplySettingsButton_Click(object sender, RoutedEventArgs e)
  {
    if (ApplySettingsCallback != null && WorkingSettings != null)
    {
      ApplySettingsCallback(
          WorkingSettings,
          SaveControlsToSettings,
          ApplySettingsChanges,
          () => WorkingSettings,
          (settings) => WorkingSettings = settings);
    }
  }

  private void ResetToDefaultsButton_Click(object sender, RoutedEventArgs e)
  {
    if (ResetToDefaultsCallback != null)
    {
      ResetToDefaultsCallback(
          (settings) => WorkingSettings = settings,
          LoadSettingsIntoControls);
    }
  }

  private void ShowLogsCheckBox_Checked(object sender, RoutedEventArgs e)
  {
    if (ShowLogPanelCallback != null && LogPanelRow != null && Logger != null)
    {
      ShowLogPanelCallback(LogPanelRow, Logger);
    }
  }

  private void ShowLogsCheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    if (HideLogPanelCallback != null && LogPanelRow != null && Logger != null)
    {
      HideLogPanelCallback(LogPanelRow, Logger);
    }
  }

  private void OpenSettingsFolderButton_Click(object sender, RoutedEventArgs e)
  {
    try
    {
      var settingsFolder = System.IO.Path.GetDirectoryName(SettingsManager.Instance.GetSettingsFilePath());
      if (!string.IsNullOrEmpty(settingsFolder))
      {
        System.Diagnostics.Process.Start("explorer.exe", settingsFolder);
      }
    }
    catch (Exception ex)
    {
      Logger?.Log($"Error opening settings folder: {ex.Message}");
    }
  }

  #endregion
  #region Public Properties

  /// <summary>
  /// Provides access to the BalatroSaveRootTextBox from the ProfileManager sub-tab
  /// </summary>
  public TextBox BalatroSaveRootTextBoxControl => ProfileManagerTabControl.BalatroSaveRootTextBoxControl;

  #endregion

  #region Public Methods
  /// <summary>
  /// Loads the working settings into the UI controls
  /// </summary>
  public void LoadSettingsIntoControls()
  {
    if (SettingsUIService != null && WorkingSettings != null && UpdateBalatroDerivedPathsCallback != null)
    {
      SettingsUIService.LoadSettingsIntoControls(WorkingSettings,
          GeneralSettingsTabControl.DefaultJkrDirectoryTextBoxControl,
          GeneralSettingsTabControl.DefaultLuaExportDirectoryTextBoxControl,
          LoggingTabControl.ShowLogsOnStartupCheckBoxControl,
          GeneralSettingsTabControl.AutoSaveDecompressedFilesCheckBoxControl,
          GeneralSettingsTabControl.ConfirmFileOverwritesCheckBoxControl, GeneralSettingsTabControl.EnableAutoBackupCheckBoxControl,
          SaveManagementTabControl.BackupDirectoryTextBoxControl,
          ProfileManagerTabControl.BalatroSaveRootTextBoxControl,
          LoggingTabControl.MaxLogEntriesTextBoxControl,
          GeneralSettingsTabControl.EnableFileWatchingCheckBoxControl,
          GeneralSettingsTabControl.FlashTaskbarOnUpdateCheckBoxControl,
          GeneralSettingsTabControl.AutoRefreshOnFileChangeCheckBoxControl,
          LoggingTabControl.LogLevelComboBoxControl,
          GeneralSettingsTabControl.ThemeComboBoxControl,
          UpdateBalatroDerivedPathsCallback,
          // Save management controls
          SaveManagementTabControl.EnableAutoSaveCheckBoxControl,
          SaveManagementTabControl.AutoSaveIntervalTextBoxControl,
          SaveManagementTabControl.AutoSaveIntervalUnitComboBoxControl,
          SaveManagementTabControl.EnableSaveRetentionCheckBoxControl,
          SaveManagementTabControl.SaveRetentionValueTextBoxControl,
          SaveManagementTabControl.SaveRetentionUnitComboBoxControl);// Update save management tab with current profile info
      SaveManagementTabControl.UpdateCurrentProfileInfo(WorkingSettings.CurrentProfileNumber, WorkingSettings.CurrentProfileSavePath);
    }
  }
  /// <summary>
  /// Saves the UI control values back to the working settings
  /// </summary>
  private void SaveControlsToSettings()
  {
    if (SettingsUIService != null && WorkingSettings != null)
    {
      SettingsUIService.SaveControlsToSettings(WorkingSettings,
          GeneralSettingsTabControl.DefaultJkrDirectoryTextBoxControl,
          GeneralSettingsTabControl.DefaultLuaExportDirectoryTextBoxControl,
          LoggingTabControl.ShowLogsOnStartupCheckBoxControl,
          GeneralSettingsTabControl.AutoSaveDecompressedFilesCheckBoxControl,
          GeneralSettingsTabControl.ConfirmFileOverwritesCheckBoxControl, GeneralSettingsTabControl.EnableAutoBackupCheckBoxControl,
          SaveManagementTabControl.BackupDirectoryTextBoxControl,
          ProfileManagerTabControl.BalatroSaveRootTextBoxControl,
          LoggingTabControl.MaxLogEntriesTextBoxControl,
          GeneralSettingsTabControl.EnableFileWatchingCheckBoxControl,
          GeneralSettingsTabControl.FlashTaskbarOnUpdateCheckBoxControl,
          GeneralSettingsTabControl.AutoRefreshOnFileChangeCheckBoxControl,
          LoggingTabControl.LogLevelComboBoxControl,
          GeneralSettingsTabControl.ThemeComboBoxControl,
          // Save management controls
          SaveManagementTabControl.EnableAutoSaveCheckBoxControl,
          SaveManagementTabControl.AutoSaveIntervalTextBoxControl,
          SaveManagementTabControl.AutoSaveIntervalUnitComboBoxControl,
          SaveManagementTabControl.EnableSaveRetentionCheckBoxControl,
          SaveManagementTabControl.SaveRetentionValueTextBoxControl,
          SaveManagementTabControl.SaveRetentionUnitComboBoxControl);
    }
  }

  /// <summary>
  /// Applies the settings changes by calling the SettingsService
  /// </summary>
  private void ApplySettingsChanges()
  {
    if (WorkingSettings != null)
    {
      SettingsService.ApplySettingsChanges(WorkingSettings);
    }
  }

  /// <summary>
  /// Sets the settings file path for display in the About tab
  /// </summary>
  public void SetSettingsFilePath(string path)
  {
    AboutTabControl.SetSettingsFilePath(path);
  }

  /// <summary>
  /// Sets the show logs checkbox state in the Logging tab
  /// </summary>
  public void SetShowLogsCheckBoxState(bool isChecked)
  {
    LoggingTabControl.ShowLogsCheckBoxControl.IsChecked = isChecked;
  }

  #endregion
}
