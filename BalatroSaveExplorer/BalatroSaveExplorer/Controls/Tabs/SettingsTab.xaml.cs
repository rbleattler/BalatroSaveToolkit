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
  }

  #region Event Handlers

  private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    // Only apply theme change if the selection is from user interaction
    // (not from programmatic loading)
    if (ThemeComboBox.SelectedItem is ComboBoxItem selectedItem && IsLoaded &&
        SettingsUIService != null && WorkingSettings != null)
    {
      SettingsUIService.HandleThemeChange(WorkingSettings, selectedItem, IsLoaded);
    }
  }

  private void BrowseJkrDirectoryButton_Click(object sender, RoutedEventArgs e)
  {
    SettingsUIService?.BrowseJkrDirectory(DefaultJkrDirectoryTextBox);
  }

  private void BrowseLuaExportDirectoryButton_Click(object sender, RoutedEventArgs e)
  {
    SettingsUIService?.BrowseLuaExportDirectory(DefaultLuaExportDirectoryTextBox);
  }

  private void BrowseBackupDirectoryButton_Click(object sender, RoutedEventArgs e)
  {
    SettingsUIService?.BrowseBackupDirectory(BackupDirectoryTextBox);
  }

  private void BrowseBalatroSaveRootButton_Click(object sender, RoutedEventArgs e)
  {
    if (BrowseBalatroSaveRootCallback != null && UpdateBalatroDerivedPathsCallback != null)
    {
      BrowseBalatroSaveRootCallback(BalatroSaveRootTextBox.Text, UpdateBalatroDerivedPathsCallback);
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
      var settingsPath = SettingsManager.Instance.GetSettingsFilePath();
      var settingsDirectory = System.IO.Path.GetDirectoryName(settingsPath);

      if (!string.IsNullOrEmpty(settingsDirectory) && System.IO.Directory.Exists(settingsDirectory))
      {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
          FileName = settingsDirectory,
          UseShellExecute = true,
          Verb = "open"
        });
      }
    }
    catch (Exception ex)
    {
      Logger?.Log($"Error opening settings folder: {ex.Message}");
      MessageBox.Show($"Could not open settings folder: {ex.Message}", "Error",
                     MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

  #endregion
  #region Public Properties

  /// <summary>
  /// Gets the BalatroSaveRootTextBox control for external access
  /// </summary>
  public TextBox BalatroSaveRootTextBoxControl => BalatroSaveRootTextBox;

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
          DefaultJkrDirectoryTextBox, DefaultLuaExportDirectoryTextBox,
          ShowLogsOnStartupCheckBox, AutoSaveDecompressedFilesCheckBox,
          ConfirmFileOverwritesCheckBox, EnableAutoBackupCheckBox,
          BackupDirectoryTextBox, BalatroSaveRootTextBox, MaxLogEntriesTextBox,
          EnableFileWatchingCheckBox, FlashTaskbarOnUpdateCheckBox,
          AutoRefreshOnFileChangeCheckBox, LogLevelComboBox, ThemeComboBox,
          UpdateBalatroDerivedPathsCallback);
    }
  }

  /// <summary>
  /// Saves the UI control values back to the working settings
  /// </summary>
  public void SaveControlsToSettings()
  {
    if (SettingsUIService != null && WorkingSettings != null)
    {
      SettingsUIService.SaveControlsToSettings(WorkingSettings,
          DefaultJkrDirectoryTextBox, DefaultLuaExportDirectoryTextBox,
          ShowLogsOnStartupCheckBox, AutoSaveDecompressedFilesCheckBox,
          ConfirmFileOverwritesCheckBox, EnableAutoBackupCheckBox,
          BackupDirectoryTextBox, BalatroSaveRootTextBox, MaxLogEntriesTextBox,
          EnableFileWatchingCheckBox, FlashTaskbarOnUpdateCheckBox,
          AutoRefreshOnFileChangeCheckBox, LogLevelComboBox, ThemeComboBox);
    }
  }

  /// <summary>
  /// Applies the working settings to the global settings manager
  /// </summary>
  public void ApplySettingsChanges()
  {
    if (WorkingSettings != null)
    {
      SettingsService.ApplySettingsChanges(WorkingSettings);
    }
  }

  /// <summary>
  /// Sets the settings file path for display
  /// </summary>
  public void SetSettingsFilePath(string filePath)
  {
    SettingsFilePathTextBox.Text = filePath;
  }

  /// <summary>
  /// Sets the checkbox state for showing logs on startup
  /// </summary>
  public void SetShowLogsCheckBox(bool isChecked)
  {
    ShowLogsCheckBox.IsChecked = isChecked;
  }

  #endregion
}
