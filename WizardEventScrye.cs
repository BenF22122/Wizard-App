using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

public static class WizardEventScryer
{
    private static readonly Dictionary<string, int[]> KeywordMap = new()
{
    // Authentication / Logon / Logoff
    { "logon", new[] {
        4624, 4625, 4634, 4647, 4648, 4672, 4778, 4779, 4800, 4801, 4802, 4803
    }},

    // Account Management
    { "account", new[] {
        4720, 4722, 4723, 4724, 4725, 4726, 4738, 4740, 4767
    }},

    // Group Membership Changes
    { "group", new[] {
        4732, 4733, 4735, 4737, 4756, 4757, 4764
    }},

    // Kerberos
    { "kerberos", new[] {
        4768, 4769, 4770, 4771, 4772, 4773, 4776
    }},

    // NTLM
    { "ntlm", new[] {
        4776
    }},

    // Service Control Manager
    { "service", new[] {
        7000, 7001, 7009, 7011, 7034, 7036, 7040, 7045
    }},

    // Startup / Shutdown / Boot
    { "startup", new[] {
        6005, 6006, 6008, 1074, 1076, 12, 13, 42, 41
    }},

    // Crash / Application Failures
    { "crash", new[] {
        1000, 1001, 1026, 1002, 1005, 1008
    }},

    // Error (generic)
    { "error", new[] {
        1000, 1001, 7000, 7001, 7034, 7031, 6008
    }},

    // USB / Removable Media
    { "usb", new[] {
        2003, 2100, 2102, 400, 410, 500, 501
    }},

    // Network Stack
    { "network", new[] {
        4201, 4202, 4226, 1014, 4000, 4001
    }},

    // DNS Client
    { "dns", new[] {
        3008, 3009, 4000, 4001, 4010, 4013, 1014
    }},

    // DHCP Client
    { "dhcp", new[] {
        1001, 1002, 1003, 1004
    }},

    // Firewall
    { "firewall", new[] {
        5152, 5153, 5155, 5157, 5031, 5025, 5027
    }},

    // RDP / Remote Desktop
    { "rdp", new[] {
        1149, 4624, 4625, 4778, 4779, 1024, 1026
    }},

    // Group Policy
    { "policy", new[] {
        4719, 4739, 1502, 1503, 1500
    }},

    // Malware / Defender
    { "malware", new[] {
        1116, 1117, 5007, 5001, 5004, 5010, 5012
    }},

    // PowerShell
    { "powershell", new[] {
        4103, 4104, 4105, 4106, 53504, 53505
    }},

    // Scheduled Tasks
    { "tasks", new[] {
        4698, 4699, 4700, 4701, 4702
    }},

    // BitLocker
    { "bitlocker", new[] {
        24576, 24577, 24578, 24620
    }},

    // LSASS / Credential Access
    { "lsass", new[] {
        4611, 4624, 4625, 4672
    }},

    // SAM / Registry Security
    { "sam", new[] {
        4657, 4660, 4663
    }},

    // Sysmon — Process Creation
    { "sysmon_process", new[] {
        1, 5, 6, 7, 8, 11, 12, 13, 14
    }},

    // Sysmon — Network Connections
    { "sysmon_network", new[] {
        3
    }},

    // Sysmon — File Events
    { "sysmon_file", new[] {
        11, 23, 24
    }},

    // Sysmon — Registry
    { "sysmon_registry", new[] {
        12, 13, 14
    }},

    // Sysmon — WMI
    { "sysmon_wmi", new[] {
        19, 20, 21
    }},

    // Sysmon — Pipe Events
    { "sysmon_pipe", new[] {
        17, 18
    }},

    // Sysmon — DNS Queries
    { "sysmon_dns", new[] {
        22
    }},

    // Sysmon — Clipboard
    { "sysmon_clipboard", new[] {
        24
    }}
} ; 


    private static List<EventLogEntry> _masterEventCache = new();

    public static void Start()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("🔮 Event Scrying Chamber & Threat Intel Suite");
            Console.WriteLine("1. Search by keyword (e.g., logon, usb, rdp)");
            Console.WriteLine("2. Search by event ID");
            Console.WriteLine("3. Cast Auto‑diagnostic spell (Scan Vital Signs)");
            Console.WriteLine("4. Start Live Event Watcher (Real‑time stream)");
            Console.WriteLine("5. Render Event Summary Dashboard");
            Console.WriteLine("6. Engage Forensic Correlation Engine");
            Console.WriteLine("7. Refresh the chronomantic cache (Reload Logs)");
            Console.WriteLine("8. Return to Wizard's Chamber");
            Console.Write("\nChoose your spell: ");

            string choice = Console.ReadLine()?.Trim() ?? string.Empty;

            switch (choice)
            {
                case "1": SearchByKeyword(); break;
                case "2": SearchByEventId(); break;
                case "3": RunAutoDiagnostic(); break;
                case "4": StartLiveWatcher(); break;
                case "5": DisplaySummaryDashboard(); break;
                case "6": RunCorrelationEngine(); break;
                case "7":
                    LoadEventsToCache(forceRefresh: true);
                    Console.WriteLine("\n✨ Memory mirror synchronized with disk tracking systems.");
                    Console.ReadKey();
                    break;
                case "8": return;
                default:
                    Console.WriteLine("\n❌ Unrecognized menu selection.");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private static void SearchByKeyword()
    {
        Console.Write("\nSpeak your keyword: ");
        string keyword = Console.ReadLine()?.ToLower().Trim() ?? string.Empty;

        if (!KeywordMap.ContainsKey(keyword))
        {
            Console.WriteLine("❌ The spirits do not recognise that keyword.");
            Console.WriteLine("   Known runes: " + string.Join(", ", KeywordMap.Keys.OrderBy(k => k)));
            Console.ReadKey();
            return;
        }

        LoadEventsToCache(forceRefresh: false);
        int[] ids = KeywordMap[keyword];
        var filteredEvents = _masterEventCache.Where(e => ids.Contains(e.EventID)).ToList();
        DisplayEvents(filteredEvents, $"Results for keyword '{keyword}'");
    }

    private static void SearchByEventId()
    {
        Console.Write("\nSpeak the event ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("❌ That is not a valid event ID.");
            Console.ReadKey();
            return;
        }

        LoadEventsToCache(forceRefresh: false);
        var filteredEvents = _masterEventCache.Where(e => e.EventID == id).ToList();
        DisplayEvents(filteredEvents, $"Results for event ID {id}");
    }

    private static void RunAutoDiagnostic()
    {
        LoadEventsToCache(forceRefresh: false);
        Console.Clear();
        Console.WriteLine("🩺 Chronomantic Auto‑Diagnostic Assessment");
        Console.WriteLine("====================================================");

        PrintDiagnosticCategory("🚨 Boot Failures & Dirty Shutdowns", _masterEventCache.Where(e => new[] { 6008, 1074, 1076 }.Contains(e.EventID)).ToList());
        PrintDiagnosticCategory("🔐 Failed Authentication Attempts", _masterEventCache.Where(e => e.EventID == 4625).ToList());
        PrintDiagnosticCategory("🔌 USB Storage Device Interactivity", _masterEventCache.Where(e => new[] { 2003, 2100, 2102 }.Contains(e.EventID)).ToList());
        PrintDiagnosticCategory("🌐 Remote Desktop (RDP) Connections", _masterEventCache.Where(e => new[] { 1149, 4778, 4779 }.Contains(e.EventID)).ToList());
        PrintDiagnosticCategory("🛡️ Malicious Contamination Warnings", _masterEventCache.Where(e => new[] { 1116, 1117, 5007 }.Contains(e.EventID)).ToList());

        Console.WriteLine("====================================================");
        Console.WriteLine("✨ Diagnostic complete. Press any key to return.");
        Console.ReadKey();
    }

    private static void PrintDiagnosticCategory(string title, List<EventLogEntry> foundEvents)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n{title}");
        Console.ResetColor();

        if (foundEvents.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("   🟢 Clear. No anomalies detected.");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"   ⚠️ Found {foundEvents.Count} historical entry/entries.");
        Console.ResetColor();

        var recentTop3 = foundEvents.OrderByDescending(e => e.TimeGenerated).Take(3);
        foreach (var e in recentTop3)
        {
            string shortMsg = e.Message.Length > 80 ? e.Message.Substring(0, 77) + "..." : e.Message;
            shortMsg = shortMsg.Replace("\r", " ").Replace("\n", " ");
            Console.WriteLine($"   └─ [{e.TimeGenerated}] (ID: {e.EventID}) -> {shortMsg}");
        }
    }

    // 🔥 FEATURE 1: Asynchronous Real-Time Log Engine (-Wait behavior)
    private static void StartLiveWatcher()
    {
        Console.Clear();
        Console.WriteLine("🔥 Live Event Watcher Initiated.");
        Console.WriteLine("Listening asynchronously to core system channels... Press [ENTER] to exit telemetry link.");
        Console.WriteLine("=======================================================================================\n");

        string[] targets = { "Application", "System" };
        var structuredLogsList = new List<EventLog>();

        // Handler hook pattern
        EntryWrittenEventHandler listenerDelegate = (sender, e) =>
        {
            lock (Console.Out)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                switch (e.Entry.EntryType)
                {
                    case EventLogEntryType.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                    case EventLogEntryType.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                    default: Console.ForegroundColor = ConsoleColor.White; break;
                }
                Console.WriteLine($"[{e.Entry.TimeGenerated:HH:mm:ss}] | ID: {e.Entry.EventID} | [{e.Entry.Source}] -> {e.Entry.Message.Split('\n')[0].Trim()}");
                Console.ForegroundColor = originalColor;
            }
        };

        try
        {
            foreach (var name in targets)
            {
                if (EventLog.Exists(name))
                {
                    EventLog log = new(name);
                    log.EnableRaisingEvents = true;
                    log.EntryWritten += listenerDelegate;
                    structuredLogsList.Add(log);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Native subsystem linking anomaly: {ex.Message}");
        }

        Console.ReadLine(); // Block thread execution elegantly until user terminates session

        foreach (var log in structuredLogsList)
        {
            log.EnableRaisingEvents = false;
            log.Dispose();
        }
        Console.WriteLine("⚡ Live tracking grid dropped safely.");
    }

    // 📊 FEATURE 2: Metrics Summary Dashboard Aggregator
    private static void DisplaySummaryDashboard()
    {
        LoadEventsToCache(forceRefresh: false);
        Console.Clear();
        Console.WriteLine("📊 Chronomantic Diagnostic Metric Board");
        Console.WriteLine("====================================================");
        Console.WriteLine($"Total Monitored Event Load: {_masterEventCache.Count} Records\n");

        // Top Noise Generator Sources
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🔥 Top 5 High-Volume Component Sources:");
        Console.ResetColor();
        var topSources = _masterEventCache
            .Where(e => !string.IsNullOrEmpty(e.Source))
            .GroupBy(e => e.Source)
            .OrderByDescending(g => g.Count())
            .Take(5);
        foreach (var src in topSources)
            Console.WriteLine($"   ├─ {src.Key.PadRight(28)} : {src.Count()} events generated");

        // Top System Error Profiles
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n🚨 Top 5 Active System Error Fingerprints:");
        Console.ResetColor();
        var topErrors = _masterEventCache
            .Where(e => e.EntryType == EventLogEntryType.Error || e.EntryType == EventLogEntryType.FailureAudit)
            .GroupBy(e => e.EventID)
            .OrderByDescending(g => g.Count())
            .Take(5);
        foreach (var err in topErrors)
            Console.WriteLine($"   ├─ Event ID: {err.Key.ToString().PadRight(18)} : {err.Count()} instances recorded");

        // System Milestones
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\n⏱️ Chronological System Milestones:");
        Console.ResetColor();
        var lastBoot = _masterEventCache.FirstOrDefault(e => e.EventID == 6005);
        var lastCrash = _masterEventCache.FirstOrDefault(e => new[] { 1000, 1001, 6008 }.Contains(e.EventID));

        Console.WriteLine($"   ├─ Last Documented Startup Sequence : {(lastBoot != null ? lastBoot.TimeGenerated.ToString() : "No record available")}");
        Console.WriteLine($"   └─ Last Documented Crash Boundary   : {(lastCrash != null ? lastCrash.TimeGenerated.ToString() : "No record available")}");
        Console.WriteLine("====================================================");
        Console.Write("\nPress any key to return...");
        Console.ReadKey();
    }

    // 🧠 FEATURE 3: Forensic Chain Attack Path Engine
    private static void RunCorrelationEngine()
    {
        LoadEventsToCache(forceRefresh: false);
        Console.Clear();
        Console.WriteLine("🧠 Forensic Correlation Threat-Hunting Engine");
        Console.WriteLine("Scanning transaction histories for compound tactical maneuvers...");
        Console.WriteLine("====================================================================");

        // Sort ascending strictly inside evaluation space to step forward naturally through time
        var forwardTimeline = _masterEventCache.OrderBy(e => e.TimeGenerated).ToList();
        bool detectionsFound = false;

        for (int i = 0; i < forwardTimeline.Count; i++)
        {
            var currentEvent = forwardTimeline[i];

            // SCENARIO A: Intrusive Escalation Vector (Login -> Direct Privileged Component Modification)
            if (currentEvent.EventID == 4624) // Successful Authorization Hook
            {
                var subsequence = forwardTimeline.Skip(i + 1).TakeWhile(e => e.TimeGenerated <= currentEvent.TimeGenerated.AddMinutes(15));
                var groupEscalation = subsequence.FirstOrDefault(e => e.EventID == 4732 || e.EventID == 4720); // Account added to group or created

                if (groupEscalation != null)
                {
                    detectionsFound = true;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("⚠️ DETECTED: Rapid Privilege Escalation String");
                    Console.ResetColor();
                    Console.WriteLine($"   ├─ Step 1: Authentication [{currentEvent.TimeGenerated:HH:mm:ss}] - ID: 4624");
                    Console.WriteLine($"   └─ Step 2: Account Modification [{groupEscalation.TimeGenerated:HH:mm:ss}] - ID: {groupEscalation.EventID} (Source: {groupEscalation.Source})");
                    Console.WriteLine("--------------------------------------------------------------------");
                }
            }

            // SCENARIO B: Attack Persistence Vector (Malware Warning -> Drop Mitigation Execution/Bounces)
            if (currentEvent.EventID == 1116) // Threat Detection Triggered
            {
                var subsequence = forwardTimeline.Skip(i + 1).TakeWhile(e => e.TimeGenerated <= currentEvent.TimeGenerated.AddMinutes(30));
                var executionAnomaly = subsequence.FirstOrDefault(e => new[] { 7045, 1074, 6008 }.Contains(e.EventID)); // Service created or unexpected bounce

                if (executionAnomaly != null)
                {
                    detectionsFound = true;
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("⚠️ DETECTED: Suspicious Host Contamination Lifecycle");
                    Console.ResetColor();
                    Console.WriteLine($"   ├─ Step 1: Malicious Agent Signal [{currentEvent.TimeGenerated:HH:mm:ss}] - ID: 1116");
                    Console.WriteLine($"   └─ Step 2: System Control Intercept [{executionAnomaly.TimeGenerated:HH:mm:ss}] - ID: {executionAnomaly.EventID}");
                    Console.WriteLine("--------------------------------------------------------------------");
                }
            }
        }

        if (!detectionsFound)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🟢 Operational logs show complete alignment. No correlation warning indicators found.");
            Console.ResetColor();
        }

        Console.WriteLine("\n====================================================================");
        Console.Write("Analysis cycle complete. Press any key to drop tracing overlay...");
        Console.ReadKey();
    }

    private static void LoadEventsToCache(bool forceRefresh)
    {
        if (_masterEventCache.Count > 0 && !forceRefresh) return;

        Console.Write("\n🔮 Accessing local chronological matrix... Please wait...");
        var logs = new List<EventLog>();
        var entries = new List<EventLogEntry>();
        string[] standardLogs = { "Application", "System", "Setup", "ForwardedEvents" };

        foreach (var name in standardLogs)
        {
            try { if (EventLog.Exists(name)) logs.Add(new EventLog(name)); } catch { }
        }
        try { if (EventLog.Exists("Security")) logs.Add(new EventLog("Security")); }
        catch { Console.WriteLine("\n⚠️ Security log is sealed to non‑archmages (run as admin to access)."); }

        foreach (var log in logs)
        {
            try { entries.AddRange(log.Entries.Cast<EventLogEntry>()); } catch { }
        }

        _masterEventCache = entries.OrderByDescending(e => e.TimeGenerated).ToList();
    }

    private static void DisplayEvents(List<EventLogEntry> activeWorkingList, string title)
    {
        if (activeWorkingList.Count == 0)
        {
            Console.WriteLine("\n❌ No events found.");
            Console.ReadKey();
            return;
        }

        var originalFilteredSet = activeWorkingList.ToList();
        int page = 0;
        const int pageSize = 10;

        while (true)
        {
            Console.Clear();
            Console.WriteLine($"🔮 {title}");
            Console.WriteLine($"📄 Page {page + 1}/{(activeWorkingList.Count + pageSize - 1) / pageSize} ({activeWorkingList.Count} Total Events)\n");

            var pageEvents = activeWorkingList.Skip(page * pageSize).Take(pageSize);
            foreach (var e in pageEvents)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                switch (e.EntryType)
                {
                    case EventLogEntryType.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                    case EventLogEntryType.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                    case EventLogEntryType.Information: Console.ForegroundColor = ConsoleColor.White; break;
                    case EventLogEntryType.SuccessAudit: Console.ForegroundColor = ConsoleColor.Green; break;
                    case EventLogEntryType.FailureAudit: Console.ForegroundColor = ConsoleColor.DarkRed; break;
                    default: Console.ForegroundColor = ConsoleColor.Cyan; break;
                }

                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"🕒 Time:   {e.TimeGenerated}");
                Console.WriteLine($"🔢 ID:     {e.EventID}"); 
                Console.WriteLine($"📌 Source: {e.Source}");
                Console.WriteLine($"📜 Message:\n{e.Message}");
                Console.ForegroundColor = originalColor;
            }

            Console.WriteLine("\nCommands:");
            Console.WriteLine("  :next           → next page | :prev           → previous page");
            Console.WriteLine("  :export <file>  → export current filter layout to disk");
            Console.WriteLine("  :last24 / :last7 / :today");
            Console.WriteLine("  :find <text>    → search inside message fields");
            Console.WriteLine("  :reset          → instant memory cache filter reset");
            Console.WriteLine("  :quit or Q      → exit to selection dashboard");
            Console.Write("> ");

            string raw = Console.ReadLine() ?? string.Empty;
            string cmd = raw.Trim();

            if (cmd.Equals(":next", StringComparison.OrdinalIgnoreCase) && (page + 1) * pageSize < activeWorkingList.Count) page++;
            else if (cmd.Equals(":prev", StringComparison.OrdinalIgnoreCase) && page > 0) page--;
            else if (cmd.StartsWith(":export ", StringComparison.OrdinalIgnoreCase))
            {
                string filename = cmd.Substring(8).Trim();
                if (!string.IsNullOrWhiteSpace(filename)) ExportEvents(activeWorkingList, filename);
            }
            else if (cmd.Equals(":last24", StringComparison.OrdinalIgnoreCase))
            {
                activeWorkingList = activeWorkingList.Where(e => e.TimeGenerated >= DateTime.Now.AddHours(-24)).ToList();
                page = 0;
            }
            else if (cmd.Equals(":last7", StringComparison.OrdinalIgnoreCase))
            {
                activeWorkingList = activeWorkingList.Where(e => e.TimeGenerated >= DateTime.Now.AddDays(-7)).ToList();
                page = 0;
            }
            else if (cmd.Equals(":today", StringComparison.OrdinalIgnoreCase))
            {
                activeWorkingList = activeWorkingList.Where(e => e.TimeGenerated >= DateTime.Today).ToList();
                page = 0;
            }
            else if (cmd.StartsWith(":find ", StringComparison.OrdinalIgnoreCase))
            {
                string term = cmd.Substring(6).Trim();
                if (!string.IsNullOrWhiteSpace(term))
                {
                    activeWorkingList = activeWorkingList.Where(e =>
                        (e.Source != null && e.Source.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (e.Message != null && e.Message.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                    ).ToList();
                    page = 0;
                }
            }
            else if (cmd.Equals(":reset", StringComparison.OrdinalIgnoreCase))
            {
                activeWorkingList = originalFilteredSet.ToList();
                page = 0;
                Console.WriteLine("✨ Reset complete via memory mirror.");
                System.Threading.Thread.Sleep(400);
            }
            else if (cmd.Equals(":quit", StringComparison.OrdinalIgnoreCase) || cmd.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }
    }

    private static void ExportEvents(List<EventLogEntry> events, string filename)
    {
        try
        {
            using StreamWriter writer = new(filename);
            foreach (var e in events)
            {
                writer.WriteLine("----------------------------------------");
                writer.WriteLine($"Time:   {e.TimeGenerated} | ID: {e.EventID} | Source: {e.Source} | Type: {e.EntryType}\nMessage:\n{e.Message}");
            }
            Console.WriteLine($"✨ Events exported to '{filename}'.");
        }
        catch { Console.WriteLine("❌ Failed to export events."); }
        Console.ReadKey();
    }
}