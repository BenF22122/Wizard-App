using System;
using System.IO;
using System.Text;

public static class WizardLogger
{
    private static readonly string LogDir = Path.Combine(AppContext.BaseDirectory, "WizardLogs");
    private static readonly object _lock = new();

    static WizardLogger()
    {
        Directory.CreateDirectory(LogDir);
    }

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
        Write("wizard", sb.ToString()); // global log
    }

    public static void LogInfo(string module, string message)
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ℹ INFO in {module}: {message}{Environment.NewLine}";
        Write(module, line);
        Write("wizard", line);
    }

    public static void LogWarning(string module, string message)
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ⚠ WARNING in {module}: {message}{Environment.NewLine}";
        Write(module, line);
        Write("wizard", line);
    }

    private static void Write(string module, string text)
    {
        try
        {
            string path = Path.Combine(LogDir, $"{Sanitize(module)}.log");
            lock (_lock)
            {
                File.AppendAllText(path, text);
            }
        }
        catch
        {
            // Last resort: swallow logging failures
        }
    }

    private static string Sanitize(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    // For diagnostics spell
    public static string[] GetLogFiles()
    {
        try
        {
            return Directory.GetFiles(LogDir, "*.log");
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public static string ReadLog(string module)
    {
        try
        {
            string path = Path.Combine(LogDir, $"{Sanitize(module)}.log");
            if (!File.Exists(path)) return "";
            return File.ReadAllText(path);
        }
        catch
        {
            return "";
        }
    }

    public static string ReadRawFile(string path)
    {
        try
        {
            if (!File.Exists(path)) return "";
            return File.ReadAllText(path);
        }
        catch
        {
            return "";
        }
    }

    public static void ClearAllLogs()
    {
        try
        {
            foreach (var file in GetLogFiles())
            {
                File.Delete(file);
            }
        }
        catch { }
    }
}
