using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using BalatroSaveExplorer.Models;

namespace BalatroSaveExplorer.Services;

/// <summary>
/// Service responsible for handling file change events and coordinating the appropriate responses.
/// Manages file watching notifications and auto-refresh functionality.
/// </summary>
public class FileChangeHandlerService
{
    private readonly Logger _logger;
    private readonly UIStateService _uiStateService;

    public FileChangeHandlerService(Logger logger, UIStateService uiStateService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _uiStateService = uiStateService ?? throw new ArgumentNullException(nameof(uiStateService));
    }

    /// <summary>
    /// Handles file change events from the file watching service
    /// </summary>
    public void HandleFileChange(string filePath, string? currentFilePath, Func<string, Task> loadFileFunc, Dispatcher dispatcher)
    {
        dispatcher.Invoke(() =>
        {
            var settings = SettingsManager.Instance.Settings;

            _logger.Log($"File changed: {filePath}");

            // Flash taskbar if enabled
            if (settings.FlashTaskbarOnUpdate)
            {
                _uiStateService.StartFlashNotification();
            }

            // Auto-refresh if enabled
            if (settings.AutoRefreshOnFileChange)
            {
                _uiStateService.RefreshCurrentFile(async () =>
                {
                    if (!string.IsNullOrEmpty(currentFilePath))
                    {
                        await loadFileFunc(currentFilePath);
                    }
                });
            }
        });
    }
}
