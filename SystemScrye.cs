using System;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using System.Globalization;
using System.Threading.Tasks;

[SupportedOSPlatform("windows")]
public static class SystemScryer
{
    // CACHE: Fetched once, lazily, when first requested.
    private static readonly Lazy<OsInfo> CachedOs = new(FetchOsInfo);
    private static readonly Lazy<DeviceInfo> CachedDevice = new(FetchDeviceInfo);
    private static readonly Lazy<HardwareInfo> CachedHardware = new(FetchHardwareInfo);

    // Make the entry point async to support modern asynchronous UI rendering
    public static async Task StartAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🧙 SYSTEM SCRYER — REALM OVERVIEW");
            Console.ResetColor();
            Console.WriteLine("────────────────────────────────────────────");
            Console.WriteLine("1. Show system overview");
            Console.WriteLine("2. Show Windows & activation details");
            Console.WriteLine("3. Show hardware details");
            Console.WriteLine("4. Show security status (TPM / Secure Boot)");
            Console.WriteLine("5. Show last 5 installed updates");
            Console.WriteLine("6. Show system health score");
            Console.WriteLine("7. Return to the wizard chamber");
            Console.WriteLine("────────────────────────────────────────────");
            Console.Write("Choose your incantation: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1": await ShowSystemOverviewAsync(); break;
                case "2": ShowWindowsAndActivation(); break;
                case "3": ShowHardwareDetails(); break;
                case "4": ShowSecurityStatus(); break;
                case "5": ShowRecentUpdates(); break;
                case "6": await ShowHealthScoreAsync(); break;
                case "7": return;
                default: break;
            }
        }
    }

    // ------------------------------------------------------------
    //  Parallel Dashboard Views
    // ------------------------------------------------------------

    private static async Task ShowSystemOverviewAsync()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🧙 SYSTEM SCRYER — REALM OVERVIEW (PARALLEL)");
        Console.ResetColor();
        Console.WriteLine("────────────────────────────────────────────");
        Console.WriteLine("🔮 Conjuring system attributes concurrently...");
        Console.WriteLine();

        // PARALLEL STEP: Spin up dynamic WMI tasks simultaneously
        var securityTask = Task.Run(() => GetSecurityInfo());
        var updatesTask = Task.Run(() => GetRecentUpdates(5));

        // Pull cached values instantly while background tasks run
        var os = CachedOs.Value;
        var device = CachedDevice.Value;
        var hw = CachedHardware.Value;

        // Wait for both concurrent WMI calls to finish up
        await Task.WhenAll(securityTask, updatesTask);

        var sec = securityTask.Result;
        var updates = updatesTask.Result;

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🧙 SYSTEM SCRYER — REALM OVERVIEW");
        Console.ResetColor();
        Console.WriteLine("────────────────────────────────────────────");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🪪 Windows");
        Console.ResetColor();
        Console.WriteLine($"  Edition   : {os.Edition}");
        Console.WriteLine($"  Version   : {os.Version} (Build {os.Build})");
        Console.WriteLine($"  Installed : {os.InstallDate}");
        Console.WriteLine($"  Activated : {os.ActivationStatus}");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("💻 Device");
        Console.ResetColor();
        Console.WriteLine($"  Manufacturer : {device.Manufacturer}");
        Console.WriteLine($"  Model        : {device.Model}");
        Console.WriteLine($"  Serial       : {device.SerialNumber}");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🔧 Hardware");
        Console.ResetColor();
        Console.WriteLine($"  CPU   : {hw.CpuName}");
        Console.WriteLine($"  RAM   : {hw.RamGb} GB");
        Console.WriteLine($"  Board : {hw.BoardManufacturer} {hw.BoardProduct}");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🔐 Security");
        Console.ResetColor();
        Console.WriteLine($"  Secure Boot : {sec.SecureBoot}");
        Console.WriteLine($"  TPM         : {sec.Tpm}");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🕒 Recent Updates");
        Console.ResetColor();
        if (updates.Length == 0)
        {
            Console.WriteLine("  No updates found.");
        }
        else
        {
            foreach (var u in updates)
                Console.WriteLine($"  {u}");
        }

        Console.WriteLine("────────────────────────────────────────────");
        Pause();
    }

    private static async Task ShowHealthScoreAsync()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("💚 System Health Score (PARALLEL)");
        Console.ResetColor();
        Console.WriteLine("────────────────────────────────────────────");
        Console.WriteLine("🔮 Measuring realm vitality metrics concurrently...");
        Console.WriteLine();

        // 🚀 PARALLEL STEP: Boot up calculations across multiple threads
        var secTask = Task.Run(() => GetSecurityInfo());
        var updatesTask = Task.Run(() => GetRecentUpdates(20));
        var diskTask = Task.Run(() => GetDiskInfo()); // Fast, but runs alongside smoothly

        var os = CachedOs.Value;
        var hw = CachedHardware.Value;
        var uptime = GetUptime(); // Instant Native

        await Task.WhenAll(secTask, updatesTask, diskTask);

        var sec = secTask.Result;
        var updates = updatesTask.Result;
        var disk = diskTask.Result;

        // --- Rest of your original Scoring Heuristics Logic ---
        var score = 0;
        var maxScore = 100;

        if (os.ActivationStatus == "Licensed") score += 10;
        if (!string.IsNullOrWhiteSpace(os.Version) && (os.Version.StartsWith("10.") || os.Version.StartsWith("6.3"))) score += 5;

        var lastUpdateDate = GetMostRecentUpdateDate(updates);
        if (lastUpdateDate.HasValue)
        {
            var days = (DateTime.UtcNow.Date - lastUpdateDate.Value.Date).TotalDays;
            if (days <= 7) score += 10;
            else if (days <= 30) score += 7;
        }

        if (DateTime.TryParse(os.InstallDate, out var installDt) && (DateTime.UtcNow - installDt).TotalDays / 365.0 <= 5) score += 5;
        if (hw.RamGb >= 16) score += 10; else if (hw.RamGb >= 8) score += 7;
        if (hw.Cores >= 8) score += 10; else if (hw.Cores >= 4) score += 7;

        if (disk.TotalGb > 0)
        {
            var freePercent = (double)disk.FreeGb / disk.TotalGb * 100.0;
            if (freePercent >= 40) score += 10; else if (freePercent >= 20) score += 5;
        }

        if (sec.SecureBoot.StartsWith("Enabled", StringComparison.OrdinalIgnoreCase)) score += 10;
        if (sec.Tpm.StartsWith("Present", StringComparison.OrdinalIgnoreCase)) score += 10;

        if (uptime.HasValue)
        {
            var days = uptime.Value.TotalDays;
            if (days <= 3) score += 10; else if (days <= 7) score += 7;
        }

        score = Math.Clamp(score, 0, maxScore);

        Console.Clear();
        Console.ForegroundColor = score >= 70 ? ConsoleColor.Green : ConsoleColor.Yellow;
        Console.WriteLine($"Realm Vitality: {score} / {maxScore}");
        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("📊 Breakdown");
        Console.ResetColor();
        Console.WriteLine($"  Activation : {os.ActivationStatus}");
        Console.WriteLine($"  Last Update: {(lastUpdateDate.HasValue ? lastUpdateDate.Value.ToString("yyyy-MM-dd") : "Unknown")}");
        Console.WriteLine($"  Disk C:    : {disk.FreeGb} GB free / {disk.TotalGb} GB total");
        Console.WriteLine($"  Secure Boot: {sec.SecureBoot}");
        Console.WriteLine($"  TPM        : {sec.Tpm}");
        Console.WriteLine("────────────────────────────────────────────");
        Pause();
    }

    // ------------------------------------------------------------
    // Standard Views (Cached Data Loading)
    // ------------------------------------------------------------

    private static void ShowWindowsAndActivation()
    {
        Console.Clear();
        var os = CachedOs.Value;
        Console.WriteLine("🪪 Windows & Activation Details");
        Console.WriteLine("────────────────────────────────────────────");
        Console.WriteLine($"Edition        : {os.Edition}");
        Console.WriteLine($"Version        : {os.Version}");
        Console.WriteLine($"Activation     : {os.ActivationStatus}");
        Pause();
    }

    private static void ShowHardwareDetails()
    {
        Console.Clear();
        var device = CachedDevice.Value;
        var hw = CachedHardware.Value;
        Console.WriteLine("🔧 Hardware Details");
        Console.WriteLine("────────────────────────────────────────────");
        Console.WriteLine($"CPU            : {hw.CpuName}");
        Console.WriteLine($"RAM            : {hw.RamGb} GB");
        Console.WriteLine($"Model          : {device.Model}");
        Pause();
    }

    private static void ShowSecurityStatus()
    {
        Console.Clear();
        var sec = GetSecurityInfo();
        Console.WriteLine("🔐 Security Status");
        Console.WriteLine("────────────────────────────────────────────");
        Console.WriteLine($"Secure Boot : {sec.SecureBoot}");
        Console.WriteLine($"TPM         : {sec.Tpm}");
        Pause();
    }

    private static void ShowRecentUpdates()
    {
        Console.Clear();
        var updates = GetRecentUpdates(5);
        Console.WriteLine("🕒 Last 5 Installed Updates");
        Console.WriteLine("────────────────────────────────────────────");
        foreach (var u in updates) Console.WriteLine(u);
        Pause();
    }

    // ------------------------------------------------------------
    // Data structures & Worker Engines
    // ------------------------------------------------------------

    private record OsInfo(string Edition, string Version, string Build, string InstallDate, string ActivationStatus, string PartialProductKey);
    private record DeviceInfo(string Manufacturer, string Model, string SerialNumber);
    private record HardwareInfo(string CpuName, int Cores, int LogicalProcessors, long RamGb, string BoardManufacturer, string BoardProduct, string BoardSerial);
    private record SecurityInfo(string SecureBoot, string Tpm);
    private record DiskInfo(long TotalGb, long FreeGb);

    private static OsInfo FetchOsInfo()
    {
        string edition = "Unknown", version = "Unknown", build = "Unknown", installDate = "Unknown", activation = "Unknown", partialKey = "N/A";
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Caption, Version, BuildNumber, InstallDate FROM Win32_OperatingSystem");
            using var results = searcher.Get();
            foreach (ManagementObject os in results)
            {
                edition = os["Caption"]?.ToString() ?? edition;
                version = os["Version"]?.ToString() ?? version;
                build = os["BuildNumber"]?.ToString() ?? build;
                break;
            }
        } catch { }
        return new OsInfo(edition, version, build, installDate, activation, partialKey);
    }

    private static DeviceInfo FetchDeviceInfo()
    {
        string man = "Unknown", mod = "Unknown", ser = "Unknown";
        try
        {
            using var csSearcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem");
            using var csResults = csSearcher.Get();
            foreach (ManagementObject cs in csResults)
            {
                man = cs["Manufacturer"]?.ToString() ?? man;
                mod = cs["Model"]?.ToString() ?? mod;
                break;
            }
        } catch { }
        return new DeviceInfo(man, mod, ser);
    }

    private static HardwareInfo FetchHardwareInfo()
    {
        string cpuName = "Unknown"; int cores = 0, logical = 0; long ramGb = 0;
        try
        {
            using var cpuSearcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor");
            using var cpuResults = cpuSearcher.Get();
            foreach (ManagementObject cpu in cpuResults)
            {
                cpuName = cpu["Name"]?.ToString() ?? cpuName;
                cores = Convert.ToInt32(cpu["NumberOfCores"] ?? cores);
                break;
            }
        } catch { }
        return new HardwareInfo(cpuName, cores, logical, ramGb, "Unknown", "Unknown", "Unknown");
    }

    private static SecurityInfo GetSecurityInfo()
    {
        string sbStr = "Unknown", tpmStr = "Unknown";
        try
        {
            using var sbSearcher = new ManagementObjectSearcher(@"root\Microsoft\Windows\HardwareManagement", "SELECT SecureBootEnabled FROM MS_SecureBoot");
            using var sbResults = sbSearcher.Get();
            foreach (ManagementObject sb in sbResults)
            {
                sbStr = Convert.ToBoolean(sb["SecureBootEnabled"]) ? "Enabled" : "Disabled";
                break;
            }
        } catch { sbStr = "Not Available / No Admin"; }
        return new SecurityInfo(sbStr, tpmStr);
    }

    private static string[] GetRecentUpdates(int count)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT HotFixID, InstalledOn, Description FROM Win32_QuickFixEngineering");
            using var results = searcher.Get();
            return results.Cast<ManagementObject>()
                .Select(mo => new { 
                    id = mo["HotFixID"]?.ToString() ?? "Unknown", 
                    date = mo["InstalledOn"]?.ToString() ?? "" 
                })
                .Take(count)
                .Select(x => $"{x.id} — {x.date}")
                .ToArray();
        } catch { return Array.Empty<string>(); }
    }

    private static DateTime? GetMostRecentUpdateDate(string[] updates)
    {
        if (updates.Length == 0) return null;
        if (DateTime.TryParse(updates[0].Split('—').LastOrDefault(), out var dt)) return dt;
        return null;
    }

    private static TimeSpan? GetUptime() => TimeSpan.FromMilliseconds(Environment.TickCount64);

    private static DiskInfo GetDiskInfo()
    {
        var drive = new System.IO.DriveInfo("C");
        return drive.IsReady ? new DiskInfo(drive.TotalSize / 1073741824, drive.AvailableFreeSpace / 1073741824) : new DiskInfo(0, 0);
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.Write("Press any key to return...");
        Console.ReadKey(true);
    }
}