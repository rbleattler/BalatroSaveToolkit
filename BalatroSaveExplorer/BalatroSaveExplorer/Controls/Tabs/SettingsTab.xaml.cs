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
    GeneralSettingsTabControl.BrowseBackupDirectoryButtonClick = BrowseBackupDirectoryButton_Click;

    // Profile Manager Tab
    ProfileManagerTabControl.BrowseBalatroSaveRootButtonClick = BrowseBalatroSaveRootButton_Click;
    ProfileManagerTabControl.BalatroSaveRootTextBoxTextChanged = BalatroSaveRootTextBox_TextChanged;

    // Logging Tab
    LoggingTabControl.ShowLogsCheckBoxChecked = ShowLogsCheckBox_Checked;
    LoggingTabControl.ShowLogsCheckBoxUnchecked = ShowLogsCheckBox_Unchecked;

    // About Tab
    AboutTabControl.OpenSettingsFolderButtonClick = OpenSettingsFolderButton_Click;
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
    SettingsUIService?.BrowseBackupDirectory(GeneralSettingsTabControl.BackupDirectoryTextBoxControl);
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
          GeneralSettingsTabControl.ConfirmFileOverwritesCheckBoxControl,
          GeneralSettingsTabControl.EnableAutoBackupCheckBoxControl,
          GeneralSettingsTabControl.BackupDirectoryTextBoxControl,
          ProfileManagerTabControl.BalatroSaveRootTextBoxControl,
          LoggingTabControl.MaxLogEntriesTextBoxControl,
          GeneralSettingsTabControl.EnableFileWatchingCheckBoxControl,
          GeneralSettingsTabControl.FlashTaskbarOnUpdateCheckBoxControl,
          GeneralSettingsTabControl.AutoRefreshOnFileChangeCheckBoxControl,
          LoggingTabControl.LogLevelComboBoxControl,
          GeneralSettingsTabControl.ThemeComboBoxControl,
          UpdateBalatroDerivedPathsCallback);
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
          GeneralSettingsTabControl.ConfirmFileOverwritesCheckBoxControl,
          GeneralSettingsTabControl.EnableAutoBackupCheckBoxControl,
          GeneralSettingsTabControl.BackupDirectoryTextBoxControl,
          ProfileManagerTabControl.BalatroSaveRootTextBoxControl,
          LoggingTabControl.MaxLogEntriesTextBoxControl,
          GeneralSettingsTabControl.EnableFileWatchingCheckBoxControl,
          GeneralSettingsTabControl.FlashTaskbarOnUpdateCheckBoxControl,
          GeneralSettingsTabControl.AutoRefreshOnFileChangeCheckBoxControl,
          LoggingTabControl.LogLevelComboBoxControl,
          GeneralSettingsTabControl.ThemeComboBoxControl);
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
