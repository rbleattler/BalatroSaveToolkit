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
        MessageBox.Show("Save management service is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      var success = await SaveManagementService.CreateManualSaveBackupAsync();
      if (success)
      {
        MessageBox.Show("Backup created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        RefreshSaveBackupsList();
      }
      else
      {
        MessageBox.Show("Failed to create backup.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
    catch (Exception ex)
    {
      MessageBox.Show($"An error occurred while creating backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }
  private async void LoadButton_Click(object sender, RoutedEventArgs e)
  {
    try
    {
      var selectedBackup = GetSelectedBackup();
      if (selectedBackup == null)
      {
        MessageBox.Show("Please select a backup to load.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      if (SaveManagementService == null)
      {
        MessageBox.Show("Save management service is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
          MessageBox.Show("Backup loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
          UpdateCurrentProfileInfo(1, SaveManagementService.GetCurrentSaveFilePath());
        }
        else
        {
          MessageBox.Show("Failed to load backup.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }
    catch (Exception ex)
    {
      MessageBox.Show($"An error occurred while loading backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

  private void DeleteButton_Click(object sender, RoutedEventArgs e)
  {
    try
    {
      var selectedBackup = GetSelectedBackup();
      if (selectedBackup == null)
      {
        MessageBox.Show("Please select a backup to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
          MessageBox.Show("Backup deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
          RefreshSaveBackupsList();
        }
        else
        {
          MessageBox.Show("Backup file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }
    catch (Exception ex)
    {
      MessageBox.Show($"An error occurred while deleting backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

  private void OpenSavesFolderButton_Click(object sender, RoutedEventArgs e)
  {
    try
    {
      if (SaveManagementService == null)
      {
        MessageBox.Show("Save management service is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      var backupDirectory = SaveManagementService.GetBackupDirectory();
      if (Directory.Exists(backupDirectory))
      {
        Process.Start("explorer.exe", backupDirectory);
      }
      else
      {
        MessageBox.Show("Backup directory does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
    catch (Exception ex)
    {
      MessageBox.Show($"An error occurred while opening folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

  private void RefreshListButton_Click(object sender, RoutedEventArgs e)
  {
    RefreshSaveBackupsList();
    MessageBox.Show("Backup list refreshed.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
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

    TotalBackupsTextBlock.Text = SaveBackups.Count.ToString();
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
