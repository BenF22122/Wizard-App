using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;

public static class SystemMonitor
{
    enum MonitorMode { Normal, FullscreenGraphs }
    static MonitorMode mode = MonitorMode.Normal;

    // Rolling history buffers
    static readonly int GraphWidth = 50;
    static readonly double[] CpuHistory = new double[50];
    static readonly double[] RamHistory = new double[50];
    static readonly double[] NetDownHistory = new double[50];
    static readonly double[] NetUpHistory = new double[50];

    static int historyIndex = 0;

    public static void Run()
    {
        Console.Clear();
        Console.CursorVisible = false;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("✨ The Archmage conjures a vision of your machine’s inner workings…");
        Console.WriteLine("Press Q to return, G to toggle fullscreen graphs.");
        Console.ResetColor();

        Thread.Sleep(1200);
        Console.Clear();

        // Initial interface fetch
        var interfaces = GetActiveInterfaces();

        long lastBytesSent = SafeSumNetworkBytes(interfaces, true);
        long lastBytesReceived = SafeSumNetworkBytes(interfaces, false);

        // CPU timing
        long lastIdleTime = 0, lastKernelTime = 0, lastUserTime = 0;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            GetSystemTimes(out lastIdleTime, out lastKernelTime, out lastUserTime);
        }

        int logicalCores = Environment.ProcessorCount;
        DateTime bootTime = DateTime.Now - TimeSpan.FromMilliseconds(Environment.TickCount64);
        var os = Environment.OSVersion;
        string machineName = Environment.MachineName;

        const int intervalMs = 2000;

        while (true)
        {
            // Key handling
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.Q)
                    break;

                if (key == ConsoleKey.G)
                {
                    mode = (mode == MonitorMode.Normal) ? MonitorMode.FullscreenGraphs : MonitorMode.Normal;
                    Console.Clear();
                }
            }

            // Refresh interfaces every 10 cycles (20 seconds)
            if (historyIndex % 10 == 0)
            {
                interfaces = GetActiveInterfaces();

                // Reset counters to avoid negative deltas
                lastBytesSent = SafeSumNetworkBytes(interfaces, true);
                lastBytesReceived = SafeSumNetworkBytes(interfaces, false);
            }

            // CPU
            double cpuPercent = 0;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GetSystemTimes(out long currIdle, out long currKernel, out long currUser);
                long idleTicks = currIdle - lastIdleTime;
                long totalTicks = (currKernel - lastKernelTime) + (currUser - lastUserTime);

                if (totalTicks > 0)
                    cpuPercent = (1.0 - ((double)idleTicks / totalTicks)) * 100.0;

                lastIdleTime = currIdle;
                lastKernelTime = currKernel;
                lastUserTime = currUser;
            }

            // RAM
            var (totalMemGb, usedMemGb) = GetMemoryInfo();

            // Disk
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.Name.StartsWith("C"));
            double diskFree = drive?.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0 ?? 0;
            double diskTotal = drive?.TotalSize / 1024.0 / 1024.0 / 1024.0 ?? 0;

            // Network
            long newSent = SafeSumNetworkBytes(interfaces, true);
            long newReceived = SafeSumNetworkBytes(interfaces, false);

            double sentMbps = (newSent - lastBytesSent) * 8 / 1_000_000.0 / (intervalMs / 1000.0);
            double recvMbps = (newReceived - lastBytesReceived) * 8 / 1_000_000.0 / (intervalMs / 1000.0);

            // Clamp anomalies
            if (sentMbps < 0) sentMbps = 0;
            if (recvMbps < 0) recvMbps = 0;

            lastBytesSent = newSent;
            lastBytesReceived = newReceived;

            TimeSpan uptime = DateTime.Now - bootTime;

            // Top processes
            Process[] topProcesses;
            try
            {
                topProcesses = Process.GetProcesses()
                    .OrderByDescending(p => { try { return p.WorkingSet64; } catch { return 0; } })
                    .Take(5)
                    .ToArray();
            }
            catch
            {
                topProcesses = Array.Empty<Process>();
            }

            // Update history
            CpuHistory[historyIndex] = Math.Clamp(cpuPercent, 0, 100);
            RamHistory[historyIndex] = totalMemGb > 0 ? (usedMemGb / totalMemGb) * 100 : 0;
            NetDownHistory[historyIndex] = recvMbps;
            NetUpHistory[historyIndex] = sentMbps;

            historyIndex = (historyIndex + 1) % GraphWidth;

            // Draw
            if (mode == MonitorMode.Normal)
            {
                DrawNormalDashboard(cpuPercent, logicalCores, usedMemGb, totalMemGb,
                                    diskFree, diskTotal, recvMbps, sentMbps,
                                    uptime, machineName, os, topProcesses);
            }
            else
            {
                DrawFullscreenGraphs();
            }

            foreach (var proc in topProcesses)
                proc.Dispose();

            Thread.Sleep(intervalMs);
        }

        Console.CursorVisible = true;
        Console.Clear();
        Console.WriteLine("✨ The vision fades… returning you to the wizard’s chamber.");
    }

    // -----------------------------
    // SAFE NETWORK ADAPTER FILTER
    // -----------------------------
    static NetworkInterface[] GetActiveInterfaces()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(n =>
                n.OperationalStatus == OperationalStatus.Up &&
                n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                n.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                !n.Description.Contains("Hyper-V", StringComparison.OrdinalIgnoreCase) &&
                !n.Description.Contains("VMware", StringComparison.OrdinalIgnoreCase) &&
                !n.Description.Contains("VirtualBox", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    // -----------------------------
    // NORMAL MODE
    // -----------------------------
    static void DrawNormalDashboard(
        double cpuPercent, int logicalCores,
        double usedMemGb, double totalMemGb,
        double diskFree, double diskTotal,
        double recvMbps, double sentMbps,
        TimeSpan uptime, string machineName,
        OperatingSystem os, Process[] topProcesses)
    {
        Console.SetCursorPosition(0, 0);

        Console.ForegroundColor = ConsoleColor.Cyan;
        ClearLineWrite("✨ The Archmage conjures a vision of your machine’s inner workings…");
        ClearLineWrite("Press Q to return, G to toggle fullscreen graphs.");
        ClearLineWrite("");

        Console.ForegroundColor = ConsoleColor.Yellow;
        ClearLineWrite($"🧠 CPU Spirit Power:   {cpuPercent,6:0.0}%   (Cores: {logicalCores})");

        Console.ForegroundColor = ConsoleColor.Green;
        ClearLineWrite($"💾 Memory Crystals:    {usedMemGb:0.0} GB / {totalMemGb:0.0} GB");

        Console.ForegroundColor = ConsoleColor.Magenta;
        ClearLineWrite($"📀 Disk C: Essence:    {diskFree:0.0} GB free of {diskTotal:0.0} GB");

        Console.ForegroundColor = ConsoleColor.Cyan;
        ClearLineWrite($"🌐 Network Winds:      ↓ {recvMbps:0.0} Mbps   ↑ {sentMbps:0.0} Mbps");

        Console.ForegroundColor = ConsoleColor.Blue;
        ClearLineWrite($"⏱️  Uptime:            {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m");

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        ClearLineWrite($"🏰 Realm:              {machineName}  |  {os.VersionString}");

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        ClearLineWrite("");
        ClearLineWrite("👁️  Top memory‑hungry spirits:");

        foreach (var p in topProcesses)
        {
            try
            {
                string processName = p.ProcessName;
                long memoryUsageMb = p.WorkingSet64 / 1024 / 1024;
                ClearLineWrite($"   • {processName,-20} {memoryUsageMb,5} MB");
            }
            catch
            {
                ClearLineWrite("   • [Protected Spirit Resources Access Denied]");
            }
        }

        ClearLineWrite("");
        DrawGraph("CPU History", CpuHistory, ConsoleColor.Yellow);
        DrawGraph("RAM History", RamHistory, ConsoleColor.Green);
        DrawGraph("Net ↓ Mbps", NetDownHistory, ConsoleColor.Cyan);
        DrawGraph("Net ↑ Mbps", NetUpHistory, ConsoleColor.Magenta);

        Console.ResetColor();
    }

    // -----------------------------
    // FULLSCREEN MODE
    // -----------------------------
    static void DrawFullscreenGraphs()
    {
        Console.SetCursorPosition(0, 0);

        Console.ForegroundColor = ConsoleColor.Cyan;
        ClearLineWrite("✨ Fullscreen Scrying Pools ");
        ClearLineWrite("");

        DrawGraphFull("CPU    ", CpuHistory, ConsoleColor.Yellow);
        DrawGraphFull("RAM    ", RamHistory, ConsoleColor.Green);
        DrawGraphFull("Net ↓  ", NetDownHistory, ConsoleColor.Cyan);
        DrawGraphFull("Net ↑  ", NetUpHistory, ConsoleColor.Magenta);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        ClearLineWrite("");
        ClearLineWrite("Press G to return to normal view.");
        Console.ResetColor();
    }

    // -----------------------------
    // GRAPH DRAWING
    // -----------------------------
    static void DrawGraph(string label, double[] history, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        string prefix = $"{label,-15}: ";
        Console.Write(prefix);

        int maxAvailableWidth = Math.Max(0, Math.Min(Console.WindowWidth - prefix.Length - 1, GraphWidth));

        for (int i = 0; i < maxAvailableWidth; i++)
        {
            int index = (historyIndex + i) % GraphWidth;
            Console.Write(GetGraphChar(history[index]));
        }

        int currentLeft = Console.CursorLeft;
        int remainingSpaces = Math.Max(0, Console.WindowWidth - currentLeft - 1);
        Console.WriteLine(new string(' ', remainingSpaces));
    }

    static void DrawGraphFull(string label, double[] history, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        string prefix = $"{label,-10}: ";
        Console.Write(prefix);

        int maxAvailableWidth = Math.Max(0, Math.Min(Console.WindowWidth - prefix.Length - 1, GraphWidth));

        for (int i = 0; i < maxAvailableWidth; i++)
        {
            int index = (historyIndex + i) % GraphWidth;
            Console.Write(GetGraphChar(history[index]));
        }

        int currentLeft = Console.CursorLeft;
        int remainingSpaces = Math.Max(0, Console.WindowWidth - currentLeft - 1);
        Console.WriteLine(new string(' ', remainingSpaces));
    }

    static char GetGraphChar(double value)
    {
        if (value < 12.5) return ' ';
        if (value < 25) return '▂';
        if (value < 37.5) return '▃';
        if (value < 50) return '▄';
        if (value < 62.5) return '▅';
        if (value < 75) return '▆';
        if (value < 87.5) return '▇';
        return '█';
    }

    // Helpers
    static void ClearLineWrite(string text)
    {
        int maxWidth = Console.WindowWidth - 1;
        if (maxWidth < 0) maxWidth = 0;

        if (text.Length > maxWidth)
            text = text.Substring(0, maxWidth);

        Console.WriteLine(text.PadRight(maxWidth));
    }

    static long SafeSumNetworkBytes(NetworkInterface[] interfaces, bool send)
    {
        long sum = 0;
        foreach (var i in interfaces)
        {
            try
            {
                var stats = i.GetIPv4Statistics();
                sum += send ? stats.BytesSent : stats.BytesReceived;
            }
            catch { }
        }
        return sum;
    }

    // CPU API
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetSystemTimes(out long lpIdleTime, out long lpKernelTime, out long lpUserTime);

    // RAM API
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    static (double totalGb, double usedGb) GetMemoryInfo()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            MEMORYSTATUSEX mem = new MEMORYSTATUSEX();
            mem.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));

            if (GlobalMemoryStatusEx(ref mem))
            {
                double total = mem.ullTotalPhys / 1024.0 / 1024.0 / 1024.0;
                double free = mem.ullAvailPhys / 1024.0 / 1024.0 / 1024.0;
                return (total, total - free);
            }
        }
        return (0.0, 0.0);
    }
}
