using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using BalatroSaveExplorer.Models;
using BalatroSaveExplorer.Services;

namespace BalatroSaveExplorer.Controls
{
  public partial class SearchableTreeView : UserControl
  {
    private ObservableCollection<TreeNodeViewModel>? _currentItemsSource;

    // Dependency properties for current file info (needed for SaveAsLua functionality)
    public static readonly DependencyProperty CurrentFilePathProperty =
        DependencyProperty.Register(
            nameof(CurrentFilePath),
            typeof(string),
            typeof(SearchableTreeView),
            new PropertyMetadata(null, OnCurrentFileInfoChanged));

    public static readonly DependencyProperty CurrentDecompressedContentProperty =
        DependencyProperty.Register(
            nameof(CurrentDecompressedContent),
            typeof(string),
            typeof(SearchableTreeView),
            new PropertyMetadata(null, OnCurrentFileInfoChanged));

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(ObservableCollection<TreeNodeViewModel>),
            typeof(SearchableTreeView),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public string? CurrentFilePath
    {
      get => (string?)GetValue(CurrentFilePathProperty);
      set => SetValue(CurrentFilePathProperty, value);
    }

    public string? CurrentDecompressedContent
    {
      get => (string?)GetValue(CurrentDecompressedContentProperty);
      set => SetValue(CurrentDecompressedContentProperty, value);
    }

    public ObservableCollection<TreeNodeViewModel>? ItemsSource
    {
      get => (ObservableCollection<TreeNodeViewModel>?)GetValue(ItemsSourceProperty);
      set => SetValue(ItemsSourceProperty, value);
    }

    private static void OnCurrentFileInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is SearchableTreeView control)
      {
        control.UpdateSaveAsLuaButtonState();
      }
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

    private void UpdateSaveAsLuaButtonState()
    {
      bool hasFileLoaded = !string.IsNullOrEmpty(CurrentFilePath) && !string.IsNullOrEmpty(CurrentDecompressedContent);
      SaveAsLuaButton.IsEnabled = hasFileLoaded;
    }

    private async void SaveAsLuaButton_Click(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(CurrentFilePath) || string.IsNullOrEmpty(CurrentDecompressedContent))
      {
        StatusBarService.Instance.SetActivity("No file is currently loaded.", true);
        return;
      }

      var settings = SettingsManager.Instance.Settings;
      var luaExportService = new LuaExportService(new Logger());

      // Use the LuaExportService to handle the export
      var success = await luaExportService.SaveAsLuaAsync(
          CurrentDecompressedContent,
          CurrentFilePath,
          settings.DefaultLuaExportDirectory,
          settings.ConfirmFileOverwrites);

      if (success)
      {
        StatusBarService.Instance.SetActivity($"Saved: {Path.GetFileName(CurrentFilePath)}.lua");
        // Update button state since the file now exists
        UpdateSaveAsLuaButtonState();
      }
    }
  }
}
