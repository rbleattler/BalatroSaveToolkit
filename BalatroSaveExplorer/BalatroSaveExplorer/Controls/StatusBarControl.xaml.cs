using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BalatroSaveExplorer.Services;

namespace BalatroSaveExplorer.Controls
{
    public partial class StatusBarControl : UserControl
    {
        public StatusBarControl()
        {
            InitializeComponent();
        }

        private void Activity_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StatusBarService.Instance.OnActivityClicked();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            StatusBarService.Instance.OnClearClicked();
            StatusBarService.Instance.ClearMessages();
        }
    }
}
