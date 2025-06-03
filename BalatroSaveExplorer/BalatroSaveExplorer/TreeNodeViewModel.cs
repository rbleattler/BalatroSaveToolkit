using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BalatroSaveExplorer;

public class TreeNodeViewModel : INotifyPropertyChanged
{
    private string _displayName = string.Empty;
    private string _valueDisplay = string.Empty;

    public string DisplayName
    {
        get => _displayName;
        set
        {
            _displayName = value;
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public string ValueDisplay
    {
        get => _valueDisplay;
        set
        {
            _valueDisplay = value;
            OnPropertyChanged(nameof(ValueDisplay));
        }
    }

    public ObservableCollection<TreeNodeViewModel> Children { get; }

    public TreeNodeViewModel()
    {
        Children = new ObservableCollection<TreeNodeViewModel>();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
