using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Win32;

public static class ReportScryer
{
    private static readonly string DefaultRoot =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WizardScryer", "Reports");

    // Unified Arcane Theme (CSS Style)
    private static readonly string RunicStyle = @"
        body { font-family: 'Consolas', 'Courier New', monospace; background: #07070d; color: #e2e8f0; padding: 20px; }
        h1 { color: #38bdf8; border-bottom: 2px dashed #0284c7; padding-bottom: 8px; margin-bottom: 5px; font-size: 2.2em; }
        h2 { color: #c084fc; border-bottom: 1px solid #3b0764; padding-bottom: 4px; margin-top: 25px; font-size: 1.5em; }
        h3 { color: #f43f5e; font-size: 1.1em; margin-top: 20px; }
        .section { margin-bottom: 30px; padding: 20px; background: #11131e; border: 1px solid #1e1b4b; border-radius: 8px; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.5); }
        .meta { font-size: 0.85em; color: #64748b; font-style: italic; margin-bottom: 20px; }
        table { border-collapse: collapse; width: 100%; margin-top: 10px; background: #0f172a; }
        th, td { border: 1px solid #334155; padding: 8px 12px; font-size: 0.9em; text-align: left; }
        th { background: #1e293b; color: #38bdf8; font-weight: bold; }
        tr:hover { background: #1e1b4b; }
        p { color: #94a3b8; font-size: 0.95em; }
        code { color:#e5e7eb; background:#020617; padding:2px 4px; border-radius:3px; }
    ";

    private static string _customFolder = string.Empty;

    public static void Start()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("📜 REPORT SCRYER — ARCANE LEDGER FORGE");
            Console.ResetColor();
            Console.WriteLine("────────────────────────────────────────────");
            Console.WriteLine(" 1. Export system overview report");
            Console.WriteLine(" 2. Export hardware report");
            Console.WriteLine(" 3. Export network report");
            Console.WriteLine(" 4. Export disk & storage report");
            Console.WriteLine(" 5. Export uptime & session report");
            Console.WriteLine(" 6. Export update & patch report");
            Console.WriteLine(" 7. Export spellbook summary report");
            Console.WriteLine(" 8. Export battery report");
            Console.WriteLine(" 9. Export startup programs report");
            Console.WriteLine("10. Export ALL (mega report)");
            Console.WriteLine("11. Choose save location (current shown below)");
            Console.WriteLine("12. Return to the wizard chamber");
            Console.WriteLine("────────────────────────────────────────────");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Current report sanctum: {GetCurrentFolder()}");
            Console.ResetColor();
            Console.Write("\nChoose your inscription: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1": ExportSingle("system_overview.html", BuildSystemOverviewHtml()); break;
                case "2": ExportSingle("hardware_report.html", BuildHardwareHtml()); break;
                case "3": ExportSingle("network_report.html", BuildNetworkHtml()); break;
                case "4": ExportSingle("disk_report.html", BuildDiskHtml()); break;
                case "5": ExportSingle("uptime_session_report.html", BuildUptimeHtml()); break;
                case "6": ExportSingle("updates_report.html", BuildUpdatesHtml()); break;
                case "7": ExportSingle("spellbook_report.html", BuildSpellbookHtml()); break;
                case "8": ExportSingle("battery_report.html", BuildBatteryHtml()); break;
                case "9": ExportSingle("startup_programs_report.html", BuildStartupHtml()); break;
                case "10": ExportMegaReport(); break;
                case "11": ChangeFolder(); break;
                case "12": return;
                default: break;
            }
        }
    }

    private static string GetCurrentFolder()
    {
        if (!string.IsNullOrWhiteSpace(_customFolder))
            return _customFolder;
        return DefaultRoot;
    }

    private static void EnsureFolderExists(string folder)
    {
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
    }

    private static void ChangeFolder()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("📂 Where shall the scrolls be stored, traveler?");
        Console.ResetColor();
        Console.WriteLine("Enter a full folder path, or press Enter to restore the default sanctum:");
        Console.Write("> ");

        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            _customFolder = string.Empty;
            Console.WriteLine("\n✨ The reports shall return to the default arcane archive.");
        }
        else
        {
            try
            {
                Directory.CreateDirectory(input);
                _customFolder = input;
                Console.WriteLine($"\n✨ The reports shall now be etched into: {input}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n💥 The path rejected the magic: {ex.Message}");
                Console.ResetColor();
            }
        }
        Pause();
    }

    private static void ExportSingle(string fileName, string html)
    {
        try
        {
            var folder = GetCurrentFolder();
            EnsureFolderExists(folder);
            var path = Path.Combine(folder, fileName);
            File.WriteAllText(path, html, Encoding.UTF8);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✨ The scroll has been inscribed: {path}");
            Console.ResetColor();

            AskOpen(path);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n💥 The inscription ritual failed: {ex.Message}");
            Console.ResetColor();
            Pause();
        }
    }

    private static void ExportMegaReport()
    {
        try
        {
            var folder = GetCurrentFolder();
            EnsureFolderExists(folder);
            var path = Path.Combine(folder, $"wizard_mega_report_{DateTime.Now:yyyyMMdd_HHmmss}.html");

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\" />");
            sb.AppendLine("<title>Wizard Mega Report</title>");
            sb.AppendLine($"<style>{RunicStyle}</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<h1>🧙 Grand Compendium Mega Report</h1>");
            sb.AppendLine($"<div class='meta'>Forged at {DateTime.Now:yyyy-MM-dd HH:mm:ss} on core array {Environment.MachineName}</div>");
            sb.AppendLine("<hr style='border: 1px dashed #1e1b4b;' />");

            sb.AppendLine(BuildSection("System Overview", BuildSystemOverviewInner()));
            sb.AppendLine(BuildSection("Hardware Construction", BuildHardwareInner()));
            sb.AppendLine(BuildSection("Planar Networks", BuildNetworkInner()));
            sb.AppendLine(BuildSection("Disk Vaults & Storage", BuildDiskInner()));
            sb.AppendLine(BuildSection("Uptime & Planar Sessions", BuildUptimeInner()));
            sb.AppendLine(BuildSection("Applied Runes & Patches", BuildUpdatesInner()));
            sb.AppendLine(BuildSection("Spellbook Manifest", BuildSpellbookInner()));
            sb.AppendLine(BuildSection("Portable Mana Core (Battery)", BuildBatteryInner()));
            sb.AppendLine(BuildSection("Startup Rituals (Autostart Programs)", BuildStartupInner()));

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✨ The grand compendium has been forged: {path}");
            Console.ResetColor();

            AskOpen(path);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n💥 The mega‑report ritual failed: {ex.Message}");
            Console.ResetColor();
            Pause();
        }
    }

    private static string BuildSection(string title, string innerHtml)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<div class='section'>");
        sb.AppendLine($"<h2>{title}</h2>");
        sb.AppendLine(innerHtml);
        sb.AppendLine("</div>");
        return sb.ToString();
    }

    private static void AskOpen(string path)
    {
        Console.Write("\nShall I project this scroll inside your web mirror? (y/N): ");
        var ans = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
        if (ans == "y" || ans == "yes")
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch
            {
                Console.WriteLine("🔮 The projection window resisted the visualization loop.");
            }
        }
        Pause();
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.Write("Press any key to return to the report sanctum...");
        Console.ReadKey(true);
    }

    private static string BuildSystemOverviewHtml() => WrapStandalone("System Overview Report", BuildSystemOverviewInner());
    private static string BuildHardwareHtml() => WrapStandalone("Hardware Analytics Report", BuildHardwareInner());
    private static string BuildNetworkHtml() => WrapStandalone("Planar Grid Network Report", BuildNetworkInner());
    private static string BuildDiskHtml() => WrapStandalone("Storage Vault Report", BuildDiskInner());
    private static string BuildUptimeHtml() => WrapStandalone("Temporal Uptime Report", BuildUptimeInner());
    private static string BuildUpdatesHtml() => WrapStandalone("Applied System Runes Report", BuildUpdatesInner());
    private static string BuildSpellbookHtml() => WrapStandalone("Arcane Ritual Summary", BuildSpellbookInner());
    private static string BuildBatteryHtml() => WrapStandalone("Portable Mana Core (Battery) Report", BuildBatteryInner());
    private static string BuildStartupHtml() => WrapStandalone("Startup Rituals (Autostart Programs)", BuildStartupInner());

    private static string WrapStandalone(string title, string inner)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\" />");
        sb.AppendLine($"<title>{title}</title>");
        sb.AppendLine($"<style>{RunicStyle}</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine($"<h1>🧙 {title}</h1>");
        sb.AppendLine($"<div class='meta'>Inscribed at {DateTime.Now:yyyy-MM-dd HH:mm:ss} on node {Environment.MachineName}</div>");
        sb.AppendLine("<div class='section'>");
        sb.AppendLine(inner);
        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }

    private static string BuildSystemOverviewInner()
    {
        var sb = new StringBuilder();
        string osCaption = "Unknown Realm";
        string osVersion = Environment.OSVersion.ToString();
        string osBuild = "Unknown Build";
        string installDate = "Unknown Timeline";

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get())
            {
                osCaption = (os["Caption"] as string) ?? osCaption;
                osVersion = (os["Version"] as string) ?? osVersion;
                osBuild = (os["BuildNumber"] as string) ?? osBuild;

                if (os["InstallDate"] is string raw && raw.Length >= 8)
                {
                    if (DateTime.TryParseExact(raw.Substring(0, 8), "yyyyMMdd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var dt))
                    {
                        installDate = dt.ToString("yyyy-MM-dd");
                    }
                }
                break;
            }
        }
        catch { }

        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Arcane Registry Node</th><th>State Signature</th></tr>");
        sb.AppendLine($"<tr><td>Machine Ident</td><td>{EscapeHtml(Environment.MachineName)}</td></tr>");
        sb.AppendLine($"<tr><td>Operating Persona</td><td>{EscapeHtml(Environment.UserName)}</td></tr>");
        sb.AppendLine($"<tr><td>Core OS Overlay</td><td>{EscapeHtml(osCaption)}</td></tr>");
        sb.AppendLine($"<tr><td>Runic Architecture</td><td>{EscapeHtml(osVersion)} (Build {EscapeHtml(osBuild)})</td></tr>");
        sb.AppendLine($"<tr><td>Inception Date</td><td>{EscapeHtml(installDate)}</td></tr>");
        sb.AppendLine($"<tr><td>64-Bit Astral Frame</td><td>{Environment.Is64BitOperatingSystem}</td></tr>");
        sb.AppendLine("</table>");
        return sb.ToString();
    }

    private static string BuildHardwareInner()
    {
        var sb = new StringBuilder();
        string cpuName = "Unknown Engine";
        int cores = 0, logical = 0;
        long ramGb = 0;
        string boardMan = "Generic", boardProd = "Altar";
        string gpuName = "Illusory Matrix Adapter";

        try
        {
            using var cpuSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject cpu in cpuSearcher.Get())
            {
                cpuName = (cpu["Name"] as string) ?? cpuName;
                cores = Convert.ToInt32(cpu["NumberOfCores"] ?? cores);
                logical = Convert.ToInt32(cpu["NumberOfLogicalProcessors"] ?? logical);
                break;
            }
        }
        catch { }

        try
        {
            using var memSearcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
            long totalBytes = 0;
            foreach (ManagementObject mem in memSearcher.Get())
            {
                if (mem["Capacity"] != null)
                    totalBytes += Convert.ToInt64(mem["Capacity"]);
            }
            ramGb = totalBytes / (1024 * 1024 * 1024);
        }
        catch { }

        try
        {
            using var boardSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            foreach (ManagementObject board in boardSearcher.Get())
            {
                boardMan = (board["Manufacturer"] as string) ?? boardMan;
                boardProd = (board["Product"] as string) ?? boardProd;
                break;
            }
        }
        catch { }

        try
        {
            using var gpuSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (ManagementObject gpu in gpuSearcher.Get())
            {
                gpuName = (gpu["Name"] as string) ?? gpuName;
                break;
            }
        }
        catch { }

        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Hardware Matrix Component</th><th>Runic Capacity Details</th></tr>");
        sb.AppendLine($"<tr><td>Thinking Matrix (CPU)</td><td>{EscapeHtml(cpuName)} ({cores} Physical Cores / {logical} Threads)</td></tr>");
        sb.AppendLine($"<tr><td>Mana Core Size (RAM)</td><td>{ramGb} GB Allocated</td></tr>");
        sb.AppendLine($"<tr><td>Altar Foundation (Motherboard)</td><td>{EscapeHtml(boardMan)} — {EscapeHtml(boardProd)}</td></tr>");
        sb.AppendLine($"<tr><td>Vision Loom (GPU)</td><td>{EscapeHtml(gpuName)}</td></tr>");
        sb.AppendLine("</table>");
        return sb.ToString();
    }

    private static string BuildNetworkInner()
    {
        var sb = new StringBuilder();
        try
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .ToArray();

            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Conduit Name</th><th>Aether Interface Type</th><th>Physical Signet (MAC)</th><th>IPv4 Link</th><th>IPv6 Vector</th></tr>");

            foreach (var nic in nics)
            {
                var ipProps = nic.GetIPProperties();
                var ipv4 = ipProps.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.Address.ToString() ?? "Disconnected";
                var ipv6 = ipProps.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)?.Address.ToString() ?? "None";

                sb.AppendLine($"<tr><td>{EscapeHtml(nic.Name)}</td><td>{nic.NetworkInterfaceType}</td><td>{nic.GetPhysicalAddress()}</td><td>{ipv4}</td><td>{EscapeHtml(ipv6)}</td></tr>");
            }
            sb.AppendLine("</table>");
        }
        catch
        {
            sb.AppendLine("<p>⚠️ The network crystal structures are currently occluded from alignment checks.</p>");
        }

        try
        {
            var ipProps = IPGlobalProperties.GetIPGlobalProperties();
            var tcp = ipProps.GetActiveTcpConnections();
            sb.AppendLine("<h3>🔮 Active Planar Gateways (Top 20 Outbound Tracks)</h3>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Local Portal Endpoint</th><th>Remote Realm Target</th><th>Gateway State Matrix</th></tr>");
            foreach (var c in tcp.Take(20))
            {
                sb.AppendLine($"<tr><td>{c.LocalEndPoint}</td><td>{c.RemoteEndPoint}</td><td><strong>{c.State}</strong></td></tr>");
            }
            sb.AppendLine("</table>");
        }
        catch
        {
            sb.AppendLine("<p>⚠️ Planar gate metrics failed validation. Gateways may be locked down.</p>");
        }

        return sb.ToString();
    }

    private static string BuildDiskInner()
    {
        var sb = new StringBuilder();
        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady && (d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Removable)).ToArray();

            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Storage Vault</th><th>Sigil Label</th><th>Device Class</th><th>Format Inscription</th><th>Available Mana Space</th><th>Total Vault Depth</th></tr>");

            foreach (var d in drives)
            {
                double totalGb = d.TotalSize / (1024d * 1024d * 1024d);
                double freeGb = d.AvailableFreeSpace / (1024d * 1024d * 1024d);
                sb.AppendLine($"<tr><td><strong>{EscapeHtml(d.Name)}</strong></td><td>{EscapeHtml(d.VolumeLabel)}</td><td>{d.DriveType}</td><td>{EscapeHtml(d.DriveFormat)}</td><td>{freeGb:0.00} GB</td><td>{totalGb:0.00} GB</td></tr>");
            }
            sb.AppendLine("</table>");
        }
        catch
        {
            sb.AppendLine("<p>⚠️ Vault logs are protected. Storage information withheld by core lockouts.</p>");
        }
        return sb.ToString();
    }

    private static string BuildUptimeInner()
    {
        var sb = new StringBuilder();
        TimeSpan? uptime = null;
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get())
            {
                if (os["LastBootUpTime"] is string raw && raw.Length >= 14)
                {
                    if (DateTime.TryParseExact(raw.Substring(0, 14), "yyyyMMddHHmmss",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.AssumeLocal, out var boot))
                    {
                        uptime = DateTime.Now - boot;
                    }
                }
                break;
            }
        }
        catch { }

        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Temporal Axis Metric</th><th>Duration Metric Signature</th></tr>");
        sb.AppendLine($"<tr><td>Realm Standard Time</td><td>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</td></tr>");
        sb.AppendLine($"<tr><td>Continuous Ignition (Uptime)</td><td>{(uptime.HasValue ? $"{uptime.Value.Days}d {uptime.Value.Hours}h {uptime.Value.Minutes}m" : "Unknown Chronomancy State")}</td></tr>");
        sb.AppendLine($"<tr><td>Active Realm Session ID</td><td>{EscapeHtml(Environment.UserDomainName)}\\{EscapeHtml(Environment.UserName)}</td></tr>");
        sb.AppendLine("</table>");
        return sb.ToString();
    }

    private static string BuildUpdatesInner()
    {
        var sb = new StringBuilder();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT HotFixID, InstalledOn, Description FROM Win32_QuickFixEngineering");
            var list = searcher.Get()
                .Cast<ManagementObject>()
                .Select(mo =>
                {
                    string id = (mo["HotFixID"] as string) ?? "Unknown Runic Patch";
                    string desc = (mo["Description"] as string) ?? "Standard System Ward Adjustment";
                    string date = (mo["InstalledOn"] as string) ?? "";
                    return new { id, desc, date };
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.id))
                .OrderByDescending(x =>
                {
                    string cleanDate = x.date.Replace("'", "").Trim();
                    if (DateTime.TryParse(cleanDate, out var dt)) return dt;
                    return DateTime.MinValue;
                })
                .Take(30)
                .ToArray();

            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Grand Ward Codex (KB ID)</th><th>Inscription Timestamp</th><th>Ward Specialization Context</th></tr>");

            foreach (var u in list)
            {
                sb.AppendLine($"<tr><td><strong>{EscapeHtml(u.id)}</strong></td><td>{(string.IsNullOrWhiteSpace(u.date) ? "Timeline Sealed" : EscapeHtml(u.date))}</td><td>{EscapeHtml(u.desc)}</td></tr>");
            }
            sb.AppendLine("</table>");
        }
        catch
        {
            sb.AppendLine("<p>⚠️ QuickFix ledger tracking was inaccessible. Protection history obscured.</p>");
        }
        return sb.ToString();
    }

    private static string BuildSpellbookInner()
    {
        var sb = new StringBuilder();
        try
        {
            var spellFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WizardScryer");
            var spellFile = Path.Combine(spellFolder, "spells.json");

            if (!File.Exists(spellFile))
            {
                sb.AppendLine("<p>🍃 Your customizable tome contains no inscribed rituals yet.</p>");
                return sb.ToString();
            }

            var json = File.ReadAllText(spellFile, Encoding.UTF8);
            var spells = System.Text.Json.JsonSerializer.Deserialize<SpellRecord[]>(json) ?? Array.Empty<SpellRecord>();

            if (spells.Length == 0)
            {
                sb.AppendLine("<p>🍃 The book is blank. No rituals have been committed to code sheets.</p>");
                return sb.ToString();
            }

            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Custom Spell Designation</th><th>Incantation Payload Pattern</th></tr>");
            foreach (var s in spells)
            {
                sb.AppendLine($"<tr><td>🔮 <code>{EscapeHtml(s.Name)}</code></td><td><code>{EscapeHtml(s.Command)}</code></td></tr>");
            }
            sb.AppendLine("</table>");
        }
        catch
        {
            sb.AppendLine("<p>⚠️ The custom script data matrix is fractured and cannot be currently decrypted.</p>");
        }
        return sb.ToString();
    }

    private static string BuildBatteryInner()
    {
        var sb = new StringBuilder();
        bool anyBattery = false;

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            var list = searcher.Get().Cast<ManagementObject>().ToArray();

            if (list.Length == 0)
            {
                sb.AppendLine("<p>🍃 This realm has no portable mana core (battery not detected).</p>");
            }
            else
            {
                anyBattery = true;
                sb.AppendLine("<table>");
                sb.AppendLine("<tr><th>Battery Element</th><th>Value</th></tr>");

                foreach (var b in list)
                {
                    string name = (b["Name"] as string) ?? "Unknown Battery";
                    string status = (b["BatteryStatus"]?.ToString()) ?? "Unknown";
                    string chem = (b["Chemistry"]?.ToString()) ?? "Unknown";
                    string designCap = (b["DesignCapacity"]?.ToString()) ?? "Unknown";
                    string fullCap = (b["FullChargeCapacity"]?.ToString()) ?? "Unknown";
                    string estRun = (b["EstimatedRunTime"]?.ToString()) ?? "Unknown";

                    sb.AppendLine($"<tr><td>Battery Name</td><td>{EscapeHtml(name)}</td></tr>");
                    sb.AppendLine($"<tr><td>Status Code</td><td>{status}</td></tr>");
                    sb.AppendLine($"<tr><td>Chemistry Code</td><td>{chem}</td></tr>");
                    sb.AppendLine($"<tr><td>Design Capacity</td><td>{designCap}</td></tr>");
                    sb.AppendLine($"<tr><td>Full Charge Capacity</td><td>{fullCap}</td></tr>");
                    sb.AppendLine($"<tr><td>Estimated Runtime (minutes)</td><td>{estRun}</td></tr>");
                }
                sb.AppendLine("</table>");
            }
        }
        catch
        {
            sb.AppendLine("<p>⚠️ Battery telemetry is occluded. The mana core refuses to reveal its state.</p>");
        }

        try
        {
            var folder = GetCurrentFolder();
            EnsureFolderExists(folder);
            var batteryReportPath = Path.Combine(folder, "powercfg_battery_report.html");

            var psi = new ProcessStartInfo
            {
                FileName = "powercfg",
                Arguments = $"/batteryreport /output \"{batteryReportPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var proc = Process.Start(psi);
            proc?.WaitForExit(5000);

            if (File.Exists(batteryReportPath))
            {
                sb.AppendLine("<h3>🔗 Linked Mortal Battery Report</h3>");
                sb.AppendLine($"<p>A detailed mortal-format battery report has been etched by <code>powercfg</code>:<br/>");
                sb.AppendLine($"<code>{EscapeHtml(batteryReportPath)}</code></p>");
            }
            else if (anyBattery)
            {
                sb.AppendLine("<p>⚠️ Attempted to invoke <code>powercfg /batteryreport</code>, but no external report was produced.</p>");
            }
        }
        catch
        {
            if (anyBattery)
                sb.AppendLine("<p>⚠️ The <code>powercfg</code> ritual could not be invoked. Battery report link unavailable.</p>");
        }

        return sb.ToString();
    }

    private static string BuildStartupInner()
    {
        var sb = new StringBuilder();
        var entries = new List<StartupEntry>();

        try
        {
            var userStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);

            CollectStartupFolder(entries, userStartup, "User Startup Folder");
            CollectStartupFolder(entries, commonStartup, "Common Startup Folder");
        }
        catch { }

        try
        {
            CollectRunKey(entries, Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", "HKCU Run");
            CollectRunKey(entries, Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run", "HKLM Run");
        }
        catch { }

        try
        {
            CollectScheduledTasks(entries);
        }
        catch { }

        if (entries.Count == 0)
        {
            sb.AppendLine("<p>🍃 No startup rituals were detected. This realm awakens cleanly.</p>");
            return sb.ToString();
        }

        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Source</th><th>Name</th><th>Command / Path</th></tr>");
        foreach (var e in entries.OrderBy(e => e.Source).ThenBy(e => e.Name))
        {
            sb.AppendLine($"<tr><td>{EscapeHtml(e.Source)}</td><td>{EscapeHtml(e.Name)}</td><td><code>{EscapeHtml(e.Command)}</code></td></tr>");
        }
        sb.AppendLine("</table>");

        return sb.ToString();
    }

    private static void CollectStartupFolder(List<StartupEntry> list, string folder, string sourceLabel)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            return;

        foreach (var file in Directory.GetFiles(folder))
        {
            var name = Path.GetFileName(file);
            list.Add(new StartupEntry(sourceLabel, name, file));
        }
    }

    private static void CollectRunKey(List<StartupEntry> list, RegistryKey root, string subKey, string sourceLabel)
    {
        using var key = root.OpenSubKey(subKey);
        if (key == null) return;

        foreach (var name in key.GetValueNames())
        {
            var value = key.GetValue(name)?.ToString() ?? "";
            list.Add(new StartupEntry(sourceLabel, name, value));
        }
    }

    private static void CollectScheduledTasks(List<StartupEntry> list)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = "/query /fo CSV /v",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null) return;

            using var reader = proc.StandardOutput;
            // Skip CSV header
            reader.ReadLine();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Simple comma splitting handling common output variations
                var parts = line.Split(new[] { "\",\"" }, StringSplitOptions.None)
                                .Select(p => p.Trim('"')).ToArray();

                if (parts.Length > 1)
                {
                    string taskName = parts[0];
                    // Look for tasks designed to run when a user logs on
                    bool isLogonTask = line.IndexOf("Logon", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (isLogonTask)
                    {
                        string commandPath = parts.Length > 8 ? parts[8] : "Scheduled Execution Trigger";
                        list.Add(new StartupEntry("Scheduled Task (Logon)", taskName, commandPath));
                    }
                }
            }
        }
        catch
        {
            // Fallback silently if schtasks is restricted by administration policy
        }
    }

    private static string EscapeHtml(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input.Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;")
                    .Replace("'", "&#39;");
    }
}

// ─────────────────────────────────────────────────────────────
// Required Core Data Models
// ─────────────────────────────────────────────────────────────

public class SpellRecord
{
    public string Name { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
}

public class StartupEntry
{
    public string Source { get; set; }
    public string Name { get; set; }
    public string Command { get; set; }

    public StartupEntry(string source, string name, string command)
    {
        Source = source;
        Name = name;
        Command = command;
    }
}