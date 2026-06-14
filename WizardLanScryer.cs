using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public static class WizardLanScryer
{
    private class LanDevice
    {
        public string Ip { get; set; } = "";
        public string Hostname { get; set; } = "";
        public string Mac { get; set; } = "";
        public long PingMs { get; set; }
    }

    public static void Start()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("🔮 LAN Scryer");
            Console.WriteLine("1. Scan a subnet (e.g. 192.168.1.x)");
            Console.WriteLine("2. Return to Wizard's Chamber");
            Console.Write("\nChoose your spell: ");

            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    ScanSubnet();
                    break;
                case "2":
                    return;
            }
        }
    }

    private static void ScanSubnet()
    {
        Console.Clear();
        Console.WriteLine("🔮 LAN Scryer - Subnet Scan\n");
        Console.Write("Enter subnet (e.g. 192.168.1.x or 192.168.0.x): ");
        string subnetInput = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(subnetInput))
            return;

        string[] parts = subnetInput.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || !parts[0].All(char.IsDigit) || !parts[1].All(char.IsDigit) || !parts[2].All(char.IsDigit))
        {
            Console.WriteLine("\n❌ That doesn't look like a valid subnet pattern.");
            Pause();
            return;
        }

        string basePrefix = $"{parts[0]}.{parts[1]}.{parts[2]}.";

        Console.Write("\nEnter start host (default 1): ");
        string startStr = Console.ReadLine() ?? "";
        Console.Write("Enter end host (default 254): ");
        string endStr = Console.ReadLine() ?? "";

        int start = 1;
        int end = 254;

        if (!string.IsNullOrWhiteSpace(startStr) && int.TryParse(startStr, out int s)) start = Math.Clamp(s, 1, 254);
        if (!string.IsNullOrWhiteSpace(endStr) && int.TryParse(endStr, out int e)) end = Math.Clamp(e, 1, 254);
        if (end < start) (start, end) = (end, start);

        Console.WriteLine($"\nScanning {basePrefix}{start} to {basePrefix}{end}...");
        Console.WriteLine("Press Q to cancel.\n");

        string? gatewayIp = GetGatewayIp();
        int maxDegree = GetPingAwareParallelism(gatewayIp);

        Console.WriteLine($"Using up to {maxDegree} parallel probes (based on gateway latency).\n");

        var devices = new List<LanDevice>();
        object lockObj = new();

        var cts = new CancellationTokenSource();

        // Background key watcher for Q to cancel
        Task.Run(() =>
        {
            while (!cts.IsCancellationRequested)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                {
                    cts.Cancel();
                    break;
                }
                Thread.Sleep(100);
            }
        });

        var hosts = Enumerable.Range(start, end - start + 1)
                             .Select(i => basePrefix + i)
                             .ToList();

        Parallel.ForEach(hosts,
            new ParallelOptions { MaxDegreeOfParallelism = maxDegree, CancellationToken = cts.Token },
            ip =>
            {
                if (cts.IsCancellationRequested)
                    return;

                try
                {
                    // Fix 1: Each thread MUST have its own isolated Ping component instance
                    using var pingInstance = new Ping();
                    
                    var reply = pingInstance.Send(ip, 400);
                    if (reply.Status != IPStatus.Success)
                        return;

                    long rtt = reply.RoundtripTime;
                    string hostname = "";
                    
                    try
                    {
                        var entry = Dns.GetHostEntry(ip);
                        hostname = entry.HostName;
                    }
                    catch
                    {
                        // ignore DNS resolution failures safely
                    }

                    lock (lockObj)
                    {
                        devices.Add(new LanDevice
                        {
                            Ip = ip,
                            Hostname = hostname,
                            Mac = "", // We will populate this efficiently in a batch later
                            PingMs = rtt
                        });
                    }
                }
                catch
                {
                    // ignore individual unreachable probe exceptions
                }
            });

        cts.Cancel(); // stop background key watcher

        Console.Clear();
        Console.WriteLine("🔮 LAN Scryer - Results\n");

        if (devices.Count == 0)
        {
            Console.WriteLine("No responsive devices found in that range.");
            Pause();
            return;
        }

        // Fix 2: Fetch the system ARP table ONCE instead of spawning a system process per-device
        var arpTable = GetAllMacsFromArp();

        foreach (var device in devices)
        {
            if (arpTable.TryGetValue(device.Ip, out string? collectedMac))
            {
                device.Mac = collectedMac;
            }
        }

        // Fix 3: uint handles high range local bits safely without falling backward due to negative values
        var ordered = devices.OrderBy(d => IPToSortable(d.Ip)).ToList();

        Console.WriteLine($"{"IP",-16} {"Ping",-6} {"Hostname",-40} {"MAC",-20}");
        Console.WriteLine(new string('-', 90));

        foreach (var d in ordered)
        {
            string pingStr = d.PingMs > 0 ? $"{d.PingMs}ms" : "0ms";
            string hostStr = string.IsNullOrWhiteSpace(d.Hostname) ? "(unknown)" : d.Hostname;
            string macStr = string.IsNullOrWhiteSpace(d.Mac) ? "(unknown)" : d.Mac;

            if (hostStr.Length > 38) hostStr = hostStr.Substring(0, 35) + "...";

            Console.WriteLine($"{d.Ip,-16} {pingStr,-6} {hostStr,-40} {macStr,-20}");
        }

        Console.WriteLine();
        Pause();
    }

    private static string? GetGatewayIp()
    {
        try
        {
            var ni = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up &&
                                     n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                     n.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

            if (ni == null) return null;

            var ipProps = ni.GetIPProperties();
            return ipProps.GatewayAddresses
                .FirstOrDefault(g => g.Address.AddressFamily == AddressFamily.InterNetwork)?
                .Address.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static int GetPingAwareParallelism(string? gatewayIp)
    {
        const int minThreads = 8;
        const int maxThreads = 32;

        if (string.IsNullOrWhiteSpace(gatewayIp))
            return 16;

        try
        {
            using var ping = new Ping();
            long total = 0;
            int samples = 0;

            for (int i = 0; i < 5; i++)
            {
                var reply = ping.Send(gatewayIp, 400);
                if (reply.Status == IPStatus.Success)
                {
                    total += reply.RoundtripTime;
                    samples++;
                }
            }

            if (samples == 0)
                return 16;

            double avg = total / (double)samples;

            if (avg < 5)   return maxThreads; 
            if (avg < 20)  return 24;        
            if (avg < 50)  return 16;        
            return minThreads;               
        }
        catch
        {
            return 16;
        }
    }

    // High performance optimization: Batch parse the entire system ARP cache once
    private static Dictionary<string, string> GetAllMacsFromArp()
    {
        var resultTable = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using Process p = new Process();
            p.StartInfo.FileName = "arp";
            p.StartInfo.Arguments = "-a";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;

            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(2000);

            using StringReader reader = new StringReader(output);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    string potentialIp = parts[0];
                    string potentialMac = parts[1];

                    // Quick basic validation of format structural match
                    if (potentialIp.Contains('.') && potentialMac.Contains('-'))
                    {
                        resultTable[potentialIp] = potentialMac;
                    }
                }
            }
        }
        catch
        {
            // Fallback gracefully with an empty tracking map if system features are restricted
        }
        return resultTable;
    }

    private static uint IPToSortable(string ip)
    {
        try
        {
            var parts = ip.Split('.');
            if (parts.Length != 4) return uint.MaxValue;
            return ((uint)int.Parse(parts[0]) << 24) |
                   ((uint)int.Parse(parts[1]) << 16) |
                   ((uint)int.Parse(parts[2]) << 8) |
                   (uint)int.Parse(parts[3]);
        }
        catch
        {
            return uint.MaxValue;
        }
    }

    private static void Pause()
    {
        Console.WriteLine("Press any key to return...");
        Console.ReadKey(true);
    }
}