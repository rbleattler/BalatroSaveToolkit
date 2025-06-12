using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BalatroSaveExplorer.Models;

namespace BalatroSaveExplorer.Controls
{
  public partial class SearchableTreeView : UserControl
  {
    private ObservableCollection<TreeNodeViewModel>? _currentItemsSource;

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
      if (d is SearchableTreeView control)
      {
        control.UpdateItemsSource(e.OldValue as ObservableCollection<TreeNodeViewModel>,
                                  e.NewValue as ObservableCollection<TreeNodeViewModel>);
      }
    }

    private void UpdateItemsSource(ObservableCollection<TreeNodeViewModel>? oldCollection,
                                  ObservableCollection<TreeNodeViewModel>? newCollection)
    {
      // Unsubscribe from old collection changes
      if (_currentItemsSource != null)
      {
        _currentItemsSource.CollectionChanged -= OnItemsSourceCollectionChanged;
      }

      _currentItemsSource = newCollection;

      // Subscribe to new collection changes
      if (_currentItemsSource != null)
      {
        _currentItemsSource.CollectionChanged += OnItemsSourceCollectionChanged;
      }

      // Update the ViewModel
      if (DataContext is SearchableTreeViewViewModel viewModel)
      {
        viewModel.OriginalItems = newCollection;
      }
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
      // When the collection changes, update the ViewModel to refresh the display
      if (DataContext is SearchableTreeViewViewModel viewModel)
      {
        viewModel.RefreshItems();
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
        UpdateItemsSource(null, ItemsSource);
      }

      Loaded += UserControl_Loaded;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      if (DataContext is SearchableTreeViewViewModel viewModel && ItemsSource != null)
      {
        viewModel.OriginalItems = ItemsSource;
      }
    }

    private void FilteredTreeView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      // Find the parent ScrollViewer
      var scrollViewer = FindParentScrollViewer(FilteredTreeView);
      if (scrollViewer != null)
      {
        // Scroll vertically
        if (e.Delta != 0)
        {
          scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
          e.Handled = true;
        }
      }
    }

    private ScrollViewer? FindParentScrollViewer(DependencyObject child)
    {
      DependencyObject? parent = VisualTreeHelper.GetParent(child);
      while (parent != null && parent is not ScrollViewer)
      {
        parent = VisualTreeHelper.GetParent(parent);
      }
      return parent as ScrollViewer;
    }
  }
}
