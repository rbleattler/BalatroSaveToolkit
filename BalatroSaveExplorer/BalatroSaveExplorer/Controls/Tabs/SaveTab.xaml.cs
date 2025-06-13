using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BalatroSaveExplorer.Services;

namespace BalatroSaveExplorer.Controls.Tabs;

public partial class SaveTab : UserControl
{
  public SaveManagementService? SaveManagementService { get; set; }
  public ObservableCollection<SaveBackupInfo> SaveBackups { get; } = new ObservableCollection<SaveBackupInfo>();

  public SaveTab()
  {
    InitializeComponent();
    SaveBackupsListView.ItemsSource = SaveBackups;

    // Initialize profile info
    UpdateCurrentProfileInfo(1, "");

    // Initial refresh of backups list
    RefreshSaveBackupsList();
  }

  #region Event Handlers
  private async void SaveButton_Click(object sender, RoutedEventArgs e)
  {
    try
    {
      if (SaveManagementService == null)
      {
        StatusBarService.Instance.SetActivity("Save management service is not available.", true);
        return;
      }

      var success = await SaveManagementService.CreateManualSaveBackupAsync();
      if (success)
      {
        StatusBarService.Instance.SetActivity("Backup created successfully!");
        RefreshSaveBackupsList();
      }
      else
      {
        StatusBarService.Instance.SetActivity("Failed to create backup.", true);
      }
    }
    catch (Exception ex)
    {
      StatusBarService.Instance.SetActivity($"An error occurred while creating backup: {ex.Message}", true);
    }
  }
  private async void LoadButton_Click(object sender, RoutedEventArgs e)
  {
    try
    {
      var selectedBackup = GetSelectedBackup();
      if (selectedBackup == null)
      {
        StatusBarService.Instance.SetActivity("Please select a backup to load.", true);
        return;
      }

      if (SaveManagementService == null)
      {
        StatusBarService.Instance.SetActivity("Save management service is not available.", true);
        return;
      }

      var confirmResult = MessageBox.Show(
        $"Are you sure you want to load backup '{selectedBackup.FileName}'?\n\nThis will overwrite your current save file.",
        "Confirm Load",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);

      if (confirmResult == MessageBoxResult.Yes)
      {
        var success = await SaveManagementService.RestoreSaveBackupAsync(selectedBackup.FilePath);
        if (success)
        {
          StatusBarService.Instance.SetActivity("Backup loaded successfully!");
          UpdateCurrentProfileInfo(1, SaveManagementService.GetCurrentSaveFilePath());
        }
        else
        {
          StatusBarService.Instance.SetActivity("Failed to load backup.", true);
        }
      }
    }
    catch (Exception ex)
    {
      StatusBarService.Instance.SetActivity($"An error occurred while loading backup: {ex.Message}", true);
    }
  }

  private void DeleteButton_Click(object sender, RoutedEventArgs e)
  {
    try
    {
      var selectedBackup = GetSelectedBackup();
      if (selectedBackup == null)
      {
        StatusBarService.Instance.SetActivity("Please select a backup to delete.", true);
        return;
      }

      var confirmResult = MessageBox.Show(
        $"Are you sure you want to delete backup '{selectedBackup.FileName}'?\n\nThis action cannot be undone.",
        "Confirm Delete",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);

      if (confirmResult == MessageBoxResult.Yes)
      {
        if (File.Exists(selectedBackup.FilePath))
        {
          File.Delete(selectedBackup.FilePath);
          StatusBarService.Instance.SetActivity("Backup deleted successfully!");
          RefreshSaveBackupsList();
        }
        else
        {
          StatusBarService.Instance.SetActivity("Backup file not found.", true);
        }
      }
    }
    catch (Exception ex)
    {
      StatusBarService.Instance.SetActivity($"An error occurred while deleting backup: {ex.Message}", true);
    }
  }

  private void OpenSavesFolderButton_Click(object sender, RoutedEventArgs e)
  {
    try
    {
      if (SaveManagementService == null)
      {
        StatusBarService.Instance.SetActivity("Save management service is not available.", true);
        return;
      }

      var backupDirectory = SaveManagementService.GetBackupDirectory();
      if (Directory.Exists(backupDirectory))
      {
        Process.Start("explorer.exe", backupDirectory);
      }
      else
      {
        StatusBarService.Instance.SetActivity("Backup directory does not exist.", true);
      }
    }
    catch (Exception ex)
    {
      StatusBarService.Instance.SetActivity($"An error occurred while opening folder: {ex.Message}", true);
    }
  }

  private void RefreshListButton_Click(object sender, RoutedEventArgs e)
  {
    RefreshSaveBackupsList();
    StatusBarService.Instance.SetActivity("Backup list refreshed.");
  }

  private void DebugCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
  {
    if (SaveManagementService != null)
    {
      SaveManagementService.IsDebugEnabled = DebugCheckBox.IsChecked ?? false;
    }
  }

  private void SaveBackupsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    var hasSelection = SaveBackupsListView.SelectedItem != null;
    LoadButton.IsEnabled = hasSelection;
    DeleteButton.IsEnabled = hasSelection;
  }

  #endregion

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

  /// <summary>
  /// Refreshes the save backups list
  /// </summary>
  public void RefreshSaveBackupsList()
  {
    if (SaveManagementService == null) return;

    SaveBackups.Clear();
    var backups = SaveManagementService.GetSaveBackups();

    foreach (var backup in backups)
    {
      SaveBackups.Add(backup);
    }
  }

  /// <summary>
  /// Gets the currently selected save backup
  /// </summary>
  public SaveBackupInfo? GetSelectedBackup()
  {
    return SaveBackupsListView.SelectedItem as SaveBackupInfo;
  }

  /// <summary>
  /// Sets the debug checkbox state
  /// </summary>
  public void SetDebugState(bool isDebugEnabled)
  {
    DebugCheckBox.IsChecked = isDebugEnabled;
  }

  #endregion
}
