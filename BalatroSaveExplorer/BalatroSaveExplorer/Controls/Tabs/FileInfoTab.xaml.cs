using System.Windows.Controls;

namespace BalatroSaveExplorer.Controls.Tabs;

public partial class FileInfoTab : UserControl
{
  public FileInfoTab()
  {
    InitializeComponent();
  }

  /// <summary>
  /// Clears all file information
  /// </summary>
  public void ClearFileInfo()
  {
    FilePathTextBox.Text = "";
    FileSizeTextBox.Text = "";
    LastModifiedTextBox.Text = "";
    CompressionInfoTextBox.Text = "";
    ContentTypeTextBox.Text = "";
    EntriesCountTextBox.Text = "";
    RawContentTextBox.Text = "";
  }

  /// <summary>
  /// Updates the file information display
  /// </summary>
  public void UpdateFileInfo(string filePath, string fileSize, string lastModified,
                            string compressionInfo, string contentType, string entriesCount)
  {
    FilePathTextBox.Text = filePath;
    FileSizeTextBox.Text = fileSize;
    LastModifiedTextBox.Text = lastModified;
    CompressionInfoTextBox.Text = compressionInfo;
    ContentTypeTextBox.Text = contentType;
    EntriesCountTextBox.Text = entriesCount;
  }

  /// <summary>
  /// Updates the raw content display
  /// </summary>
  public void UpdateRawContent(string content)
  {
    RawContentTextBox.Text = content;
  }

  /// <summary>
  /// Provides access to the individual TextBoxes for FileLoadingService
  /// </summary>
  public TextBox GetFilePathTextBox() => FilePathTextBox;
  public TextBox GetFileSizeTextBox() => FileSizeTextBox;
  public TextBox GetLastModifiedTextBox() => LastModifiedTextBox;
  public TextBox GetCompressionInfoTextBox() => CompressionInfoTextBox;
  public TextBox GetContentTypeTextBox() => ContentTypeTextBox;
  public TextBox GetEntriesCountTextBox() => EntriesCountTextBox;
  public TextBox GetRawContentTextBox() => RawContentTextBox;
}
