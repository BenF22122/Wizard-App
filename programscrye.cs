using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

public static class WizardProgramScryer
{
    public static void Start()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("========================================");
            Console.WriteLine("          🔮 PROGRAM SCRYER             ");
            Console.WriteLine("========================================");
            Console.ResetColor();
            Console.WriteLine("\nSpeak the name of the program you wish to observe.");
            Console.WriteLine("Example: mspaint.exe, notepad.exe, chrome.exe");
            Console.WriteLine("Type 'q' or 'quit' to return to the tower.\n");

            Console.Write("Program name: ");
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input.Equals("q", StringComparison.OrdinalIgnoreCase) || 
                input.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("back", StringComparison.OrdinalIgnoreCase))
                return;

            // Strip .exe if present
            string procName = input.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                ? input[..^4]
                : input;

            var processes = Process.GetProcessesByName(procName)
                                   .OrderBy(p => p.Id) // Sorted by Id for presentation consistency
                                   .ToList();

            if (processes.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ No running process found with name '{procName}'.");
                Console.ResetColor();
                Console.WriteLine("Press any key to try again...");
                Console.ReadKey(true);
                continue;
            }

            Process target;

            if (processes.Count == 1)
            {
                target = processes[0];
            }
            else
            {
                Console.WriteLine($"\n✨ Multiple instances of '{procName}' found:");
                for (int i = 0; i < processes.Count; i++)
                {
                    var p = processes[i];
                    string title = string.IsNullOrWhiteSpace(p.MainWindowTitle)
                        ? "(no window title)"
                        : p.MainWindowTitle;
                    Console.WriteLine($"[{i + 1}] PID {p.Id} - {title}");
                }

                Console.Write("\nChoose which instance to scry (number), or 'c' to cancel: ");
                string? choice = Console.ReadLine()?.Trim();

                if (choice?.Equals("c", StringComparison.OrdinalIgnoreCase) == true || 
                    choice?.Equals("q", StringComparison.OrdinalIgnoreCase) == true)
                    continue;

                if (!int.TryParse(choice, out int index) || index < 1 || index > processes.Count)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ That is not a valid instance.");
                    Console.ResetColor();
                    Console.WriteLine("Press any key to try again...");
                    Console.ReadKey(true);
                    continue;
                }

                target = processes[index - 1];
            }

            try
            {
                MonitorProcess(target);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ The scrying spell failed: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("Press any key to return...");
                Console.ReadKey(true);
            }
        }
    }

    private static void MonitorProcess(Process process)
    {
        if (process.HasExited)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n❌ The process has already exited.");
            Console.ResetColor();
            Console.WriteLine("Press any key to return...");
            Console.ReadKey(true);
            return;
        }

        TimeSpan lastTotalProcessorTime = process.TotalProcessorTime;
        DateTime lastSampleTime = DateTime.UtcNow;
        int processorCount = Environment.ProcessorCount;

        // Clear once right before entering loop to build our persistent UI canvas
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("========================================");
        Console.WriteLine("          🔮 PROGRAM SCRYER             ");
        Console.WriteLine("========================================");
        Console.ResetColor();
        Console.WriteLine($"\nScrying: {process.ProcessName}.exe (PID {process.Id})");
        Console.WriteLine(new string('─', 55));

        // CAPTURE BASE REFRESH ROW: Lines below this will be rewritten in place
        int metricsStartRow = Console.CursorTop;

        while (true)
        {
            if (process.HasExited)
            {
                Console.SetCursorPosition(0, metricsStartRow);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n⚠️  The observed manifestation has vanished (Process Exited).");
                Console.ResetColor();
                // Clear out trailing leftover layout lines cleanly
                for (int i = 0; i < 8; i++) Console.WriteLine(new string(' ', 60));
                
                Console.SetCursorPosition(0, metricsStartRow + 2);
                Console.WriteLine("Press any key to return to the tower sanctuary...");
                Console.ReadKey(true);
                return;
            }

            process.Refresh();

            DateTime now = DateTime.UtcNow;
            TimeSpan currentTotalProcessorTime = process.TotalProcessorTime;

            double cpuPercent = 0.0;
            double elapsedMs = (now - lastSampleTime).TotalMilliseconds;
            if (elapsedMs > 0)
            {
                double cpuMs = (currentTotalProcessorTime - lastTotalProcessorTime).TotalMilliseconds;
                cpuPercent = (cpuMs / elapsedMs) * 100.0 / processorCount;
            }

            lastSampleTime = now;
            lastTotalProcessorTime = currentTotalProcessorTime;

            long workingSet = process.WorkingSet64;
            int threads = process.Threads.Count;
            int handles = process.HandleCount;
            string title = string.IsNullOrWhiteSpace(process.MainWindowTitle)
                ? "(no window title)"
                : process.MainWindowTitle;

            TimeSpan uptime = DateTime.Now - process.StartTime;

            // ANTI-FLICKER ROUTINE: Snap terminal context back to the target index row
            Console.SetCursorPosition(0, metricsStartRow);

            // Print with padding widths (-X) to ensure character shrinkage wraps cleanly without fragments
            Console.WriteLine($"CPU Usage:      {cpuPercent,6:F2} %" + new string(' ', 10));
            Console.WriteLine($"RAM Usage:      {FormatBytes(workingSet),12}" + new string(' ', 10));
            Console.WriteLine($"Threads:        {threads,-15}");
            Console.WriteLine($"Handles:        {handles,-15}");
            Console.WriteLine($"Uptime:         {uptime:dd\\.hh\\:mm\\:ss}");
            
            // Limit long terminal headers from breaking text row lines
            if (title.Length > 35) title = title.Substring(0, 32) + "...";
            Console.WriteLine($"Main Window:    {title,-40}");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string('─', 55));
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("✨ Press [Q] to sever connection and return home.");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("✨ Visual alignment updates automatically every second...");
            Console.ResetColor();

            // Dynamic non-blocking keystroke collection loop
            for (int i = 0; i < 10; i++)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Q)
                        return;
                }
                Thread.Sleep(100);
            }
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double value = bytes;
        int unit = 0;

        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:F2} {units[unit]}";
    }
}