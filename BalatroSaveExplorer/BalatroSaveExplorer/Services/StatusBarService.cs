using System;
using System.ComponentModel;
using System.Windows.Media;

namespace BalatroSaveExplorer.Services
{
    public enum AppReadinessState { Ready, Working, Error }
    public enum BalatroState { Running, NotRunning, Unknown }

    public class StatusBarService : INotifyPropertyChanged
    {
        private static StatusBarService? _instance;
        public static StatusBarService Instance => _instance ??= new StatusBarService();

        private AppReadinessState _readiness = AppReadinessState.Ready;
        private string _activityMessage = string.Empty;
        private bool _activityIsError = false;
        private BalatroState _balatroState = BalatroState.Unknown;
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? ActivityClicked;
        public event Action? ClearClicked;

        public AppReadinessState Readiness
        {
            get => _readiness;
            set { _readiness = value; OnPropertyChanged(nameof(Readiness)); OnPropertyChanged(nameof(IsError)); }
        }
        public string ActivityMessage
        {
            get => _activityMessage;
            set { _activityMessage = value; OnPropertyChanged(nameof(ActivityMessage)); }
        }
        public bool ActivityIsError
        {
            get => _activityIsError;
            set { _activityIsError = value; OnPropertyChanged(nameof(ActivityIsError)); OnPropertyChanged(nameof(IsError)); }
        }
        public BalatroState Balatro
        {
            get => _balatroState;
            set { _balatroState = value; OnPropertyChanged(nameof(Balatro)); }
        }
        public bool IsError => _readiness == AppReadinessState.Error || _activityIsError;

        public void SetReadiness(AppReadinessState state)
        {
            Readiness = state;
        }
        public void SetActivity(string message, bool isError = false)
        {
            ActivityMessage = message;
            ActivityIsError = isError;
            if (isError) Readiness = AppReadinessState.Error;
        }
        public void SetBalatroState(BalatroState state)
        {
            Balatro = state;
        }
        public void ClearMessages()
        {
            ActivityMessage = string.Empty;
            ActivityIsError = false;
            if (Readiness == AppReadinessState.Error) Readiness = AppReadinessState.Ready;
        }
        public void OnActivityClicked() => ActivityClicked?.Invoke();
        public void OnClearClicked() => ClearClicked?.Invoke();
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
