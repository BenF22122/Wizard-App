using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using Microsoft.Win32;

public static class BootScryer
{
    public static void Start()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🧙‍♂️ BOOT SCRYER — SYSTEM AWAKENING SUMMARY");
            Console.ResetColor();
            Console.WriteLine("────────────────────────────────────────────");

            var summary = GetBootSummary();

            if (summary != null)
            {
                WriteMetric("🌑 BIOS / POST", summary.BiosPostSeconds);
                WriteMetric("🌕 Windows Boot", summary.BootDurationSeconds);
                WriteMetric("🌗 Logon Ritual", summary.LogonDurationSeconds);
                WriteMetric("🌘 Desktop Ready", summary.DesktopReadySeconds);

                if (summary.TotalSeconds.HasValue)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Total Awakening Time:     {summary.TotalSeconds:F1} seconds");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("The Wizard cannot read the Boot Scrying logs on this machine.");
                Console.WriteLine("Ensure you ran as Administrator and that the Performance log is active.");
                Console.ResetColor();
            }

            Console.WriteLine("────────────────────────────────────────────");
            Console.WriteLine("[1] View Startup Spells");
            Console.WriteLine("[2] View Slow Spells (Apps/Drivers/Services)");
            Console.WriteLine("[3] View Boot Timeline Details");
            Console.WriteLine("[4] Return to Wizard Chamber");
            Console.Write("\nYour incantation: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1": ShowStartupItems(); break;
                case "2": ShowSlowItems(); break;
                case "3": ShowBootTimelineDetails(); break;
                case "4": return;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nThe Boot Scryer does not understand that incantation.");
                    Console.ResetColor();
                    Pause();
                    break;
            }
        }
    }

    // ------------------------------------------------------------
    // SUMMARY
    // ------------------------------------------------------------

    private class BootSummary
    {
        public double? BiosPostSeconds { get; set; }
        public double? BootDurationSeconds { get; set; }
        public double? LogonDurationSeconds { get; set; }
        public double? DesktopReadySeconds { get; set; }
        public double? TotalSeconds { get; set; }
    }

    private static BootSummary? GetBootSummary()
    {
        try
        {
            var bootEvent = GetLatestEvent("Microsoft-Windows-Diagnostics-Performance/Operational", 100);
            var logonEvent = GetLatestEvent("Microsoft-Windows-Diagnostics-Performance/Operational", 200);

            if (bootEvent == null && logonEvent == null)
                return null;

            var summary = new BootSummary();

            if (bootEvent != null)
            {
                // Property [3] maps to MainPathBootTime (Core OS Load)
                summary.BootDurationSeconds = GetEventInt(bootEvent, 3) / 1000.0;
                
                // Property [4] maps to BootPostBootTime (Desktop Initializing)
                summary.DesktopReadySeconds = GetEventInt(bootEvent, 4) / 1000.0;
                
                // Fetch dedicated motherboard/UEFI execution time from Kernel-Boot log
                summary.BiosPostSeconds     = GetTrueBiosSeconds();
            }

            if (logonEvent != null)
            {
                // Property [2] maps to Total Logon Duration
                summary.LogonDurationSeconds = GetEventInt(logonEvent, 2) / 1000.0;
            }

            double total = 0;
            if (summary.BiosPostSeconds.HasValue) total += summary.BiosPostSeconds.Value;
            if (summary.BootDurationSeconds.HasValue) total += summary.BootDurationSeconds.Value;
            if (summary.LogonDurationSeconds.HasValue) total += summary.LogonDurationSeconds.Value;
            
            summary.TotalSeconds = total;
            return summary;
        }
        catch
        {
            return null;
        }
    }

    private static void WriteMetric(string label, double? value)
    {
        if (value.HasValue && value.Value > 0)
            Console.WriteLine($"{label,-24} {value:F1} seconds");
        else
            Console.WriteLine($"{label,-24} (not available)");
    }

    // ------------------------------------------------------------
    // EVENT LOG READER
    // ------------------------------------------------------------

    private static EventRecord? GetLatestEvent(string logName, int eventId)
    {
        try
        {
            var query = new EventLogQuery(logName, PathType.LogName, $"*[System/EventID={eventId}]")
            {
                ReverseDirection = true // Ensures reading the NEWEST records first
            };

            using var reader = new EventLogReader(query);
            return reader.ReadEvent();
        }
        catch
        {
            return null;
        }
    }

    private static double? GetTrueBiosSeconds()
    {
        try
        {
            // UEFI/BIOS firmware time is explicitly logged in the System log channel by Kernel-Boot
            var query = new EventLogQuery(
                "System", 
                PathType.LogName, 
                "*[System[Provider[@Name='Microsoft-Windows-Kernel-Boot'] and EventID=27]]")
            {
                ReverseDirection = true
            };

            using var reader = new EventLogReader(query);
            var record = reader.ReadEvent();
            
            if (record != null && record.Properties.Count > 1)
            {
                // Property [1] contains the FirmwareDuration field in milliseconds
                if (record.Properties[1].Value != null)
                {
                    return Convert.ToDouble(record.Properties[1].Value) / 1000.0;
                }
            }
        }
        catch { }
        return null;
    }

    private static int GetEventInt(EventRecord record, int index)
    {
        try
        {
            var props = record.Properties;
            if (index < props.Count && props[index].Value != null)
            {
                // Dynamic conversion prevents unboxing casting exceptions if the schema value acts as a uint/ulong
                return Convert.ToInt32(props[index].Value);
            }
        }
        catch { }
        return 0;
    }

    // ------------------------------------------------------------
    // STARTUP ITEMS
    // ------------------------------------------------------------

    private static void ShowStartupItems()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("🧙‍♂️ STARTUP SPELLS");
        Console.ResetColor();
        Console.WriteLine("────────────────────────────────────────────");

        var items = new List<string>();

        items.AddRange(ReadRunKey(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", "HKCU"));
        items.AddRange(ReadRunKey(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run", "HKLM"));
        items.AddRange(ReadRunKey(Registry.LocalMachine, @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", "HKLM (WOW64)"));

        var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        if (Directory.Exists(startupFolder))
        {
            foreach (var file in Directory.GetFiles(startupFolder))
                items.Add($"[Startup Folder] {Path.GetFileName(file)} -> {file}");
        }

        if (items.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No startup spells were found.");
            Console.ResetColor();
        }
        else
        {
            foreach (var line in items)
                Console.WriteLine(line);
        }

        Pause();
    }

    private static IEnumerable<string> ReadRunKey(RegistryKey root, string path, string label)
    {
        var list = new List<string>();
        try
        {
            using var key = root.OpenSubKey(path);
            if (key == null) return list;

            foreach (var name in key.GetValueNames())
            {
                var value = key.GetValue(name)?.ToString() ?? "";
                list.Add($"[{label} Run] {name} -> {value}");
            }
        }
        catch { }
        return list;
    }

    // ------------------------------------------------------------
    // SLOW ITEMS
    // ------------------------------------------------------------

    private static void ShowSlowItems()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("🧙‍♂️ SLOW SPELLS (APPS / SERVICES / DRIVERS)");
        Console.ResetColor();
        Console.WriteLine("────────────────────────────────────────────");

        var slowEvents = GetSlowEvents();

        if (slowEvents.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No slow spells were recorded in the recent boot rituals.");
            Console.ResetColor();
        }
        else
        {
            foreach (var e in slowEvents)
            {
                Console.WriteLine();
                Console.WriteLine($"[{e.TimeGenerated}] (ID {e.Id})");
                Console.WriteLine(e.Message);
                Console.WriteLine(new string('-', 44));
            }
        }

        Pause();
    }

    private static List<(int Id, DateTime TimeGenerated, string Message)> GetSlowEvents()
    {
        var list = new List<(int, DateTime, string)>();

        try
        {
            var query = new EventLogQuery(
                "Microsoft-Windows-Diagnostics-Performance/Operational",
                PathType.LogName,
                "*[System[(EventID >= 300 and EventID < 600)]]")
            {
                ReverseDirection = true
            };

            using var reader = new EventLogReader(query);
            EventRecord? rec;
            int count = 0;

            while ((rec = reader.ReadEvent()) != null && count < 15)
            {
                list.Add((rec.Id, rec.TimeCreated ?? DateTime.Now, rec.FormatDescription() ?? "No details available."));
                count++;
            }
        }
        catch { }

        return list;
    }

    // ------------------------------------------------------------
    // BOOT TIMELINE DETAILS
    // ------------------------------------------------------------

    private static void ShowBootTimelineDetails()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("🧙‍♂️ BOOT TIMELINE DETAILS");
        Console.ResetColor();
        Console.WriteLine("────────────────────────────────────────────");

        var bootEvent = GetLatestEvent("Microsoft-Windows-Diagnostics-Performance/Operational", 100);

        if (bootEvent == null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The Boot Scryer could not find a recent boot performance event.");
            Console.ResetColor();
            Pause();
            return;
        }

        Console.WriteLine($"Event Time: {bootEvent.TimeCreated}");
        Console.WriteLine();

        WriteMetric("Total Boot Duration", GetEventInt(bootEvent, 2) / 1000.0);
        WriteMetric("Main Path BootTime", GetEventInt(bootEvent, 3) / 1000.0);
        WriteMetric("Boot PostBoot Time", GetEventInt(bootEvent, 4) / 1000.0);

        Console.WriteLine();
        Console.WriteLine("Raw event message:");
        Console.WriteLine("------------------");
        Console.WriteLine(bootEvent.FormatDescription());

        Pause();
    }

    // ------------------------------------------------------------
    // UTIL
    // ------------------------------------------------------------

    private static void Pause()
    {
        Console.WriteLine();
        Console.Write("Press any key to return to the Boot Scryer...");
        Console.ReadKey(true);
    }
}