using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace BalatroSaveExplorer.Controls
{
  public partial class SearchableTreeView : UserControl
  {
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(ObservableCollection<TreeNodeViewModel>),
            typeof(SearchableTreeView),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public ObservableCollection<TreeNodeViewModel>? ItemsSource
    {
      get => (ObservableCollection<TreeNodeViewModel>?)GetValue(ItemsSourceProperty);
      set => SetValue(ItemsSourceProperty, value);
    }
    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is SearchableTreeView control && control.DataContext is SearchableTreeViewViewModel viewModel)
      {
        viewModel.OriginalItems = e.NewValue as ObservableCollection<TreeNodeViewModel>;
      }
    }
    public SearchableTreeView()
    {
      InitializeComponent();

      // Create and set the ViewModel immediately
      var viewModel = new SearchableTreeViewViewModel();
      DataContext = viewModel;

      // If ItemsSource is already set (unlikely but possible), update the ViewModel
      if (ItemsSource != null)
      {
        viewModel.OriginalItems = ItemsSource;
      }

      Loaded += UserControl_Loaded;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      if (DataContext is SearchableTreeViewViewModel viewModel)
      {
        viewModel.OriginalItems = ItemsSource;
      }
    }
  }
}
