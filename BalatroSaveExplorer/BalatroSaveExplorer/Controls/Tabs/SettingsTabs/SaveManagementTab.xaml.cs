using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using BalatroSaveExplorer.Services;

namespace BalatroSaveExplorer.Controls.Tabs.SettingsTabs;

public partial class SaveManagementTab : UserControl
{
  public SaveManagementService? SaveManagementService { get; set; }

  public SaveManagementTab()
  {
    InitializeComponent();

    // Initialize profile info
    UpdateCurrentProfileInfo(1, "");
  }

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
