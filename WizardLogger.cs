using System;
using System.IO;
using System.Text;
using System.Threading;

public static class WizardLogger
{
    private static readonly string LogDir = Path.Combine(AppContext.BaseDirectory, WizardConfig.LogDirectory);
    private static readonly object _lock = new();
    private static bool _initialized = false;

    static WizardLogger()
    {
        Initialize();
    }

    private static void Initialize()
    {
        if (_initialized)
            return;

        try
        {
            if (!Directory.Exists(LogDir))
                Directory.CreateDirectory(LogDir);
            _initialized = true;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️ Warning: Could not initialize logging: {ex.Message}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Logs an error with exception details
    /// </summary>
    public static void LogError(string module, Exception ex, string? context = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ❌ ERROR in {module}");
        
        if (!string.IsNullOrWhiteSpace(context))
            sb.AppendLine($"Context: {context}");
        
        sb.AppendLine($"Message: {ex.Message}");
        sb.AppendLine("Stack:");
        sb.AppendLine(ex.StackTrace ?? "(no stack trace)");
        sb.AppendLine(new string('-', 60));

        Write(module, sb.ToString());
        Write("wizard", sb.ToString());

        if (WizardConfig.EnableDetailedLogging)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"[LOG] {module}: {ex.Message}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Logs an informational message
    /// </summary>
    public static void LogInfo(string module, string message)
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ℹ️ INFO in {module}: {message}{Environment.NewLine}";
        Write(module, line);
        Write("wizard", line);
    }

    /// <summary>
    /// Logs a warning message
    /// </summary>
    public static void LogWarning(string module, string message)
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ⚠️ WARNING in {module}: {message}{Environment.NewLine}";
        Write(module, line);
        Write("wizard", line);
    }

    /// <summary>
    /// Logs performance metrics
    /// </summary>
    public static void LogPerformance(string module, string operation, long elapsedMs)
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ⏱️ PERF in {module}: {operation} took {elapsedMs}ms{Environment.NewLine}";
        Write(module, line);
    }

    private static void Write(string module, string text)
    {
        if (!_initialized)
            return;

        try
        {
            string path = Path.Combine(LogDir, $"{Sanitize(module)}.log");
            lock (_lock)
            {
                // Check file size and rotate if needed
                if (File.Exists(path))
                {
                    var info = new FileInfo(path);
                    if (info.Length > (WizardConfig.MaxLogFileSizeKb * 1024))
                    {
                        string backupPath = $"{path}.{DateTime.Now:yyyyMMdd_HHmmss}";
                        File.Copy(path, backupPath, true);
                        File.WriteAllText(path, $"[Log rotated at {DateTime.Now}]\n");
                    }
                }

                File.AppendAllText(path, text);
            }
        }
        catch
        {
            // Silent fail – logging failures should not crash the application
        }
    }

    private static string Sanitize(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "wizard" : name;
    }

    /// <summary>
    /// Gets all log files
    /// </summary>
    public static string[] GetLogFiles()
    {
        try
        {
            if (!Directory.Exists(LogDir))
                return Array.Empty<string>();
            return Directory.GetFiles(LogDir, "*.log");
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Reads a specific log file
    /// </summary>
    public static string ReadLog(string module)
    {
        try
        {
            string path = Path.Combine(LogDir, $"{Sanitize(module)}.log");
            if (!File.Exists(path))
                return "";
            return File.ReadAllText(path);
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Reads a raw log file by path
    /// </summary>
    public static string ReadRawFile(string path)
    {
        try
        {
            if (!File.Exists(path))
                return "";
            return File.ReadAllText(path);
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Clears all log files
    /// </summary>
    public static void ClearAllLogs()
    {
        try
        {
            foreach (var file in GetLogFiles())
            {
                try { File.Delete(file); }
                catch { }
            }
        }
        catch { }
    }
}
