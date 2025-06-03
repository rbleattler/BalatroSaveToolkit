using System.Text;

namespace BalatroSaveExplorer;

public class Logger
{
    private readonly List<string> _logs;
    private readonly string _logFilePath;

    public event EventHandler<string>? LogAdded;

    public Logger()
    {
        _logs = new List<string>();

        // Create log file path in temp directory
        string tempDir = System.IO.Path.GetTempPath();
        string appTempDir = System.IO.Path.Combine(tempDir, "BalatroSaveExplorer");
        System.IO.Directory.CreateDirectory(appTempDir);
        _logFilePath = System.IO.Path.Combine(appTempDir, "application.log");

        // Clear log file on startup
        if (System.IO.File.Exists(_logFilePath))
        {
            System.IO.File.Delete(_logFilePath);
        }
    }

    public void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logEntry = $"[{timestamp}] {message}";

        _logs.Add(logEntry);
        LogAdded?.Invoke(this, logEntry);

        // Append to file immediately
        try
        {
            System.IO.File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }
        catch
        {
            // Ignore file write errors to prevent crashes
        }
    }

    public void ClearLogs()
    {
        _logs.Clear();
        Log("Logs cleared");
    }

    public void SaveToFile()
    {
        try
        {
            System.IO.File.WriteAllLines(_logFilePath, _logs);
        }
        catch
        {
            // Ignore file write errors
        }
    }

    public IReadOnlyList<string> GetLogs() => _logs.AsReadOnly();
}
