using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace BalatroSaveExplorer.Controls
{
  public class SearchableTreeViewViewModel : INotifyPropertyChanged
  {
    private string _searchText = string.Empty;
    private ObservableCollection<TreeNodeViewModel>? _originalItems;
    private ObservableCollection<TreeNodeViewModel> _filteredItems = new();
    private readonly DispatcherTimer _searchTimer;

    public string SearchText
    {
      get => _searchText;
      set
      {
        if (_searchText != value)
        {
          _searchText = value;
          OnPropertyChanged(nameof(SearchText));
          OnPropertyChanged(nameof(HasResults));

          // Debounce search to avoid too frequent filtering
          _searchTimer.Stop();
          _searchTimer.Start();
        }
      }
    }
    public ObservableCollection<TreeNodeViewModel>? OriginalItems
    {
      get => _originalItems;
      set
      {
        if (_originalItems != value)
        {
          _originalItems = value;
          OnPropertyChanged(nameof(OriginalItems));
          FilterItems();
        }
      }
    }

    public ObservableCollection<TreeNodeViewModel> FilteredItems
    {
      get => _filteredItems;
      private set
      {
        _filteredItems = value;
        OnPropertyChanged(nameof(FilteredItems));
        OnPropertyChanged(nameof(HasResults));
      }
    }

    public bool HasResults => FilteredItems?.Any() == true || string.IsNullOrWhiteSpace(SearchText);

    public ICommand ClearSearchCommand { get; }

    public SearchableTreeViewViewModel()
    {
      ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);

      // Set up debounce timer for search
      _searchTimer = new DispatcherTimer
      {
        Interval = TimeSpan.FromMilliseconds(300)
      };
      _searchTimer.Tick += (s, e) =>
      {
        _searchTimer.Stop();
        FilterItems();
      };
    }

    private void FilterItems()
    {
      if (OriginalItems == null)
      {
        FilteredItems = new ObservableCollection<TreeNodeViewModel>();
        return;
      }

      if (string.IsNullOrWhiteSpace(SearchText))
      {
        // Show all items when no search text
        FilteredItems = new ObservableCollection<TreeNodeViewModel>(OriginalItems);
        return;
      }

      var filtered = new ObservableCollection<TreeNodeViewModel>();
      var searchLower = SearchText.ToLower();

      foreach (var item in OriginalItems)
      {
        var filteredItem = FilterItem(item, searchLower);
        if (filteredItem != null)
        {
          filtered.Add(filteredItem);
        }
      }

      FilteredItems = filtered;
    }

    private TreeNodeViewModel? FilterItem(TreeNodeViewModel item, string searchText)
    {
      // Check if current item matches
      bool currentMatches = item.DisplayName.ToLower().Contains(searchText) ||
                            item.ValueDisplay.ToLower().Contains(searchText);

      // Check children recursively
      var filteredChildren = new ObservableCollection<TreeNodeViewModel>();
      foreach (var child in item.Children)
      {
        var filteredChild = FilterItem(child, searchText);
        if (filteredChild != null)
        {
          filteredChildren.Add(filteredChild);
        }
      }

      // Include item if it matches or has matching children
      if (currentMatches || filteredChildren.Any())
      {
        var filteredItem = new TreeNodeViewModel
        {
          DisplayName = item.DisplayName,
          ValueDisplay = item.ValueDisplay
        };

        foreach (var child in filteredChildren)
        {
          filteredItem.Children.Add(child);
        }

        return filteredItem;
      }

      return null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public class RelayCommand : ICommand
  {
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
      _execute = execute ?? throw new ArgumentNullException(nameof(execute));
      _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();
  }
}
