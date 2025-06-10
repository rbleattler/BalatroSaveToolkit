using System.Collections.ObjectModel;
using System.Windows.Controls;
using BalatroSaveExplorer.Models;

namespace BalatroSaveExplorer.Controls.Tabs;

public partial class TreeViewTab : UserControl
{
	private readonly ObservableCollection<TreeNodeViewModel> _treeNodes;

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
	/// Clears the tree view data
	/// </summary>
	public void ClearData()
	{
		_treeNodes.Clear();
	}
}