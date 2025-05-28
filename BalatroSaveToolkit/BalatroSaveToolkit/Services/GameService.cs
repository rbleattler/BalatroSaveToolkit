using System.Diagnostics;

namespace BalatroSaveToolkit.Services;

public class GameService {
    public bool IsBalatroRunning => IsBalatroProcessRunning();

    bool IsBalatroProcessRunning() {
        try {
            // Check for a process named "balatro" (case-insensitive, without extension)
            return Process.GetProcessesByName("balatro").Length != 0;
        }
        catch { return false; }
    }
}
