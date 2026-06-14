using System;
using System.IO;
using System.Linq;

public static class WizardDiagnosticScryer
{
    public static void Start()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("🔮 WIZARD DIAGNOSTIC SCRYER");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("The wizard peers into the echoes of past rituals...");
            Console.WriteLine();
            Console.WriteLine("1. View last wizard disturbance (last error)");
            Console.WriteLine("2. View logs by module");
            Console.WriteLine("3. View full raw log files");
            Console.WriteLine("4. Clear all logs");
            Console.WriteLine("5. Return to the tower");
            Console.WriteLine();
            Console.Write("Choose your scrying focus: ");

            string? choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1": ShowLastError(); break;
                case "2": ShowLogsByModule(); break;
                case "3": ShowRawLogFiles(); break;
                case "4": ClearLogs(); break;
                case "5": return;
                default: break;
            }
        }
    }

    // ------------------------------------------------------------
    // LAST ERROR
    // ------------------------------------------------------------

    private static void ShowLastError()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("✨ Last Magical Disturbance");
        Console.ResetColor();
        Console.WriteLine();

        var files = WizardLogger.GetLogFiles();
        if (files.Length == 0)
        {
            Console.WriteLine("The aether is calm. No disturbances have been recorded.");
            Pause();
            return;
        }

        // Prefer global wizard.log if present
        string? wizardLog = files.FirstOrDefault(f =>
            string.Equals(Path.GetFileName(f), "wizard.log", StringComparison.OrdinalIgnoreCase));

        // FIXED: Safe file selection
        string targetFile = wizardLog ??
            files.Select(f => new { File = f, Time = SafeGetLastWriteTime(f) })
                 .OrderByDescending(x => x.Time)
                 .First().File;

        string content = WizardLogger.ReadRawFile(targetFile);
        if (string.IsNullOrWhiteSpace(content))
        {
            Console.WriteLine("The visions are cloudy. No readable disturbances found.");
            Pause();
            return;
        }

        // Take last 40 lines
        var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        var lastLines = lines.Reverse().Take(40).Reverse().ToArray();

        Console.WriteLine($"From: {Path.GetFileName(targetFile)}");
        Console.WriteLine(new string('─', 60));
        foreach (var line in lastLines)
            Console.WriteLine(line);
        Console.WriteLine(new string('─', 60));

        Pause();
    }

    private static DateTime SafeGetLastWriteTime(string path)
    {
        try { return File.GetLastWriteTime(path); }
        catch { return DateTime.MinValue; }
    }

    // ------------------------------------------------------------
    // LOGS BY MODULE
    // ------------------------------------------------------------

    private static void ShowLogsByModule()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("📜 Logs by Module");
        Console.ResetColor();
        Console.WriteLine();

        var files = WizardLogger.GetLogFiles();
        if (files.Length == 0)
        {
            Console.WriteLine("No scrolls have been written yet.");
            Pause();
            return;
        }

        var modules = files
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .OrderBy(n => n)
            .ToArray();

        for (int i = 0; i < modules.Length; i++)
            Console.WriteLine($"{i + 1}. {modules[i]}");

        Console.WriteLine();
        Console.Write("Choose a module to inspect (or press Enter to return): ");
        string? choice = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(choice)) return;

        if (!int.TryParse(choice, out int index) || index < 1 || index > modules.Length)
        {
            Console.WriteLine("The runes do not match any known module.");
            Pause();
            return;
        }

        string module = modules[index - 1];
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"📜 Log for {module}");
        Console.ResetColor();
        Console.WriteLine();

        string content = WizardLogger.ReadLog(module);
        if (string.IsNullOrWhiteSpace(content))
        {
            Console.WriteLine("This scroll is blank or unreadable.");
        }
        else
        {
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var lastLines = lines.Reverse().Take(80).Reverse().ToArray();
            foreach (var line in lastLines)
                Console.WriteLine(line);
        }

        Console.WriteLine();
        Console.WriteLine("(Only the tail of the scroll is shown.)");
        Pause();
    }

    // ------------------------------------------------------------
    // RAW LOG FILES
    // ------------------------------------------------------------

    private static void ShowRawLogFiles()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("📂 Raw Log Scrolls");
        Console.ResetColor();
        Console.WriteLine();

        var files = WizardLogger.GetLogFiles();
        if (files.Length == 0)
        {
            Console.WriteLine("No scrolls have been inscribed yet.");
            Pause();
            return;
        }

        for (int i = 0; i < files.Length; i++)
            Console.WriteLine($"{i + 1}. {Path.GetFileName(files[i])}");

        Console.WriteLine();
        Console.Write("Choose a scroll to read (or press Enter to return): ");
        string? choice = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(choice)) return;

        if (!int.TryParse(choice, out int index) || index < 1 || index > files.Length)
        {
            Console.WriteLine("The chosen scroll does not exist in this archive.");
            Pause();
            return;
        }

        string file = files[index - 1];
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"📜 Raw contents of {Path.GetFileName(file)}");
        Console.ResetColor();
        Console.WriteLine();

        string content = WizardLogger.ReadRawFile(file);
        if (string.IsNullOrWhiteSpace(content))
        {
            Console.WriteLine("This scroll appears to be empty.");
        }
        else
        {
            if (content.Length > 2000)
            {
                Console.WriteLine(content.Substring(0, 2000));
                Console.WriteLine();
                Console.WriteLine("... (scroll continues, but the vision fades here)");
            }
            else
            {
                Console.WriteLine(content);
            }
        }

        Pause();
    }

    // ------------------------------------------------------------
    // CLEAR LOGS
    // ------------------------------------------------------------

    private static void ClearLogs()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("⚠ CLEAR ALL LOGS");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("This will burn all existing scrolls of wizard history.");
        Console.Write("Are you sure? (Y/N): ");
        string? answer = Console.ReadLine()?.Trim();

        if (answer.Equals("Y", StringComparison.OrdinalIgnoreCase))
        {
            WizardLogger.ClearAllLogs();
            Console.WriteLine("The scrolls crumble into arcane dust.");
        }
        else
        {
            Console.WriteLine("The scrolls remain safely in the archive.");
        }

        Pause();
    }

    // ------------------------------------------------------------
    // UTIL
    // ------------------------------------------------------------

    private static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to return...");
        Console.ReadKey(true);
    }
}
