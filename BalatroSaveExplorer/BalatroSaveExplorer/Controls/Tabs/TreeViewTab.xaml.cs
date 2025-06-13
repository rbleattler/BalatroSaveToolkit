using System.Collections.ObjectModel;
using System.Windows.Controls;
using BalatroSaveExplorer.Models;

namespace BalatroSaveExplorer.Controls.Tabs;

public partial class TreeViewTab : UserControl
{
	private readonly ObservableCollection<TreeNodeViewModel> _treeNodes;
	private string? _currentFilePath;
	private string? _currentDecompressedContent;

	public TreeViewTab()
	{
		InitializeComponent();
		_treeNodes = new ObservableCollection<TreeNodeViewModel>();
		DataTreeView.ItemsSource = _treeNodes;
	}

	/// <summary>
	/// Exposes the tree nodes collection for MainWindow to update
	/// </summary>
	public ObservableCollection<TreeNodeViewModel> TreeNodes => _treeNodes;

	/// <summary>
	/// Sets the current file information for the SaveAsLua functionality
	/// </summary>
	/// <param name="filePath">Current file path</param>
	/// <param name="decompressedContent">Current decompressed content</param>
	public void SetCurrentFile(string? filePath, string? decompressedContent)
	{
		_currentFilePath = filePath;
		_currentDecompressedContent = decompressedContent;
		DataTreeView.CurrentFilePath = filePath;
		DataTreeView.CurrentDecompressedContent = decompressedContent;
	}

	/// <summary>
	/// Clears the tree view data
	/// </summary>
	public void ClearData()
	{
		_treeNodes.Clear();
		SetCurrentFile(null, null);
	}
}