using System.Windows;
using System.Windows.Controls;

namespace BalatroSaveExplorer.Utilities;

/// <summary>
/// Utility class for managing log panel UI state and operations.
/// Provides centralized methods for showing, hiding, and clearing the log panel.
/// </summary>
public static class LogPanelManager
{
  /// <summary>
  /// Shows the log panel with a specified height.
  /// </summary>
  /// <param name="logPanelRow">The row definition for the log panel</param>
  /// <param name="logger">The logger instance</param>
  /// <param name="height">The height to set for the log panel (default: 200)</param>
  public static void ShowLogPanel(RowDefinition logPanelRow, Logger logger, double height = 200)
  {
    if (logPanelRow == null)
    {
      logger?.Log("Cannot show log panel - logPanelRow is null");
      return;
    }

    logPanelRow.Height = new GridLength(height);
    logger?.Log("Log panel shown");
  }

  /// <summary>
  /// Hides the log panel by setting its height to 0.
  /// </summary>
  /// <param name="logPanelRow">The row definition for the log panel</param>
  /// <param name="logger">The logger instance</param>
  public static void HideLogPanel(RowDefinition logPanelRow, Logger logger)
  {
    if (logPanelRow == null)
    {
      logger?.Log("Cannot hide log panel - logPanelRow is null");
      return;
    }

    logPanelRow.Height = new GridLength(0);
    logger?.Log("Log panel hidden");
  }

  /// <summary>
  /// Clears the logs from both the logger and the log text box.
  /// </summary>
  /// <param name="logger">The logger instance</param>
  /// <param name="logTextBox">The log text box to clear</param>
  public static void ClearLogs(Logger logger, TextBox logTextBox)
  {
    logger?.ClearLogs();

    if (logTextBox != null)
    {
      logTextBox.Text = "";
    }
  }

  /// <summary>
  /// Toggles the log panel visibility based on a checkbox state.
  /// </summary>
  /// <param name="logPanelRow">The row definition for the log panel</param>
  /// <param name="logger">The logger instance</param>
  /// <param name="isChecked">Whether the checkbox is checked</param>
  /// <param name="height">The height to set when showing the panel (default: 200)</param>
  public static void ToggleLogPanel(RowDefinition logPanelRow, Logger logger, bool isChecked, double height = 200)
  {
    if (isChecked)
    {
      ShowLogPanel(logPanelRow, logger, height);
    }
    else
    {
      HideLogPanel(logPanelRow, logger);
    }
  }

  /// <summary>
  /// Handles log message addition by updating the log text box on the UI thread.
  /// </summary>
  /// <param name="logTextBox">The log text box to update</param>
  /// <param name="logMessage">The log message to add</param>
  /// <param name="dispatcher">The dispatcher for UI thread operations</param>
  public static void HandleLogAdded(TextBox logTextBox, string logMessage, System.Windows.Threading.Dispatcher dispatcher)
  {
    if (logTextBox == null || dispatcher == null) return;

    // Update UI on main thread
    dispatcher.Invoke(() =>
    {
      logTextBox.AppendText(logMessage + System.Environment.NewLine);
      logTextBox.ScrollToEnd();
    });
  }
}
