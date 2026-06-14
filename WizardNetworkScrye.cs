using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

public static class WizardNetworkScryer
{
    public static void Start()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🔮 Network Scrying Chamber");
            Console.ResetColor();
            Console.WriteLine("1. Live network stats");
            Console.WriteLine("2. DNS lookup");
            Console.WriteLine("3. Ping host");
            Console.WriteLine("4. Trace route");
            Console.WriteLine("5. Port scan (High Performance)");
            Console.WriteLine("6. Show local IP + gateway");
            Console.WriteLine("7. Show active TCP connections");
            Console.WriteLine("8. Detect proxy/PAC settings");
            Console.WriteLine("9. Wi‑Fi scryer");
            Console.WriteLine("10. Network activity recorder (Optimized)");
            Console.WriteLine("11. Latency scryer (Gateway / DNS / Internet)");
            Console.WriteLine("12. Return to Wizard's Chamber");
            Console.Write("\nChoose your spell: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": LiveNetworkStats(); break;
                case "2": DnsLookup(); break;
                case "3": PingHost(); break;
                case "4": TraceRoute(); break;
                case "5": PortScanAsync().GetAwaiter().GetResult(); break;
                case "6": ShowLocalIpAndGateway(); break;
                case "7": ShowActiveTcpConnections(); break;
                case "8": DetectProxySettings(); break;
                case "9": WifiScryer(); break;
                case "10": NetworkActivityRecorder(); break;
                case "11": LatencyScryer(); break;
                case "12": return;
            }
        }
    }

    // 1. Live network stats (Flicker-Free, Independent Graph Normalization)
    private static void LiveNetworkStats()
    {
        Console.Clear();
        var ni = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up &&
                                 n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                 n.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

        if (ni == null)
        {
            Console.WriteLine("❌ No active network interface found.");
            Console.ReadKey();
            return;
        }

        var prev = ni.GetIPv4Statistics();
        List<double> downHistory = new List<double>();
        List<double> upHistory = new List<double>();

        while (true)
        {
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                return;

            Thread.Sleep(1000);
            var now = ni.GetIPv4Statistics();

            double kbpsUp = (now.BytesSent - prev.BytesSent) * 8 / 1024.0;
            double kbpsDown = (now.BytesReceived - prev.BytesReceived) * 8 / 1024.0;
            prev = now;

            upHistory.Add(kbpsUp);
            downHistory.Add(kbpsDown);

            if (upHistory.Count > 30) upHistory.RemoveAt(0);
            if (downHistory.Count > 30) downHistory.RemoveAt(0);

            // Isolate scaling so low traffic isn't flattened by a spike on the alternate stream
            double maxUp = upHistory.Max();
            double maxDown = downHistory.Max();
            if (maxUp < 1) maxUp = 1;
            if (maxDown < 1) maxDown = 1;

            int graphWidth = 40;

            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🔮 Live Network Stats (press Q to return)".PadRight(Console.WindowWidth));
            Console.ResetColor();
            Console.WriteLine($"Adapter: {ni.Name}".PadRight(Console.WindowWidth));
            Console.WriteLine($"  Description: {ni.Description}".PadRight(Console.WindowWidth));
            Console.WriteLine($"  Speed: {ni.Speed / 1_000_000} Mbps".PadRight(Console.WindowWidth));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  Down: {kbpsDown:F1} kbps".PadRight(Console.WindowWidth));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  Up:   {kbpsUp:F1} kbps\n".PadRight(Console.WindowWidth));
            Console.ResetColor();

            Console.WriteLine("📉 Throughput (last 30s):\n".PadRight(Console.WindowWidth));
            Console.WriteLine($"Down: {BuildGraph(downHistory, maxDown, graphWidth)}");
            Console.WriteLine($"Up:   {BuildGraph(upHistory, maxUp, graphWidth)}");
        }
    }

    private static string BuildGraph(List<double> values, double max, int width)
    {
        if (values.Count == 0) return new string('░', width);
        int bars = (int)((values.Last() / max) * width);
        bars = Math.Clamp(bars, 0, width);
        return new string('█', bars) + new string('░', width - bars);
    }

    // 2. DNS Lookup
    private static void DnsLookup()
    {
        Console.Clear();
        Console.WriteLine("🔮 DNS Lookup");
        Console.Write("Enter hostname: ");
        string host = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(host)) return;

        try
        {
            var entry = Dns.GetHostEntry(host);
            Console.WriteLine($"\nHost: {entry.HostName}");
            foreach (var addr in entry.AddressList)
                Console.WriteLine($"  {addr} ({addr.AddressFamily})");
        }
        catch (Exception ex) { Console.WriteLine($"❌ DNS lookup failed: {ex.Message}"); }

        Console.WriteLine("\nPress any key to return.");
        Console.ReadKey();
    }

    // 3. Ping Host
    private static void PingHost()
    {
        Console.Clear();
        Console.WriteLine("🔮 Ping Host");
        Console.Write("Enter host or IP: ");
        string host = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(host)) return;

        try
        {
            using Ping ping = new Ping();
            for (int i = 0; i < 4; i++)
            {
                var reply = ping.Send(host, 2000);
                if (reply.Status == IPStatus.Success)
                    Console.WriteLine($"Reply from {reply.Address}: time={reply.RoundtripTime}ms TTL={reply.Options?.Ttl}");
                else
                    Console.WriteLine($"Request failed: {reply.Status}");
                Thread.Sleep(500);
            }
        }
        catch (Exception ex) { Console.WriteLine($"❌ Ping failed: {ex.Message}"); }

        Console.WriteLine("\nPress any key to return.");
        Console.ReadKey();
    }

    // 4. Trace Route
    private static void TraceRoute()
    {
        Console.Clear();
        Console.WriteLine("🔮 Trace Route");
        Console.Write("Enter host or IP: ");
        string host = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(host)) return;

        try
        {
            const int maxHops = 30;
            const int timeout = 3000;
            using Ping ping = new Ping();

            for (int ttl = 1; ttl <= maxHops; ttl++)
            {
                PingOptions options = new PingOptions(ttl, true);
                byte[] buffer = new byte[32];
                var reply = ping.Send(host, timeout, buffer, options);

                string addr = reply.Address?.ToString() ?? "*";
                if (reply.Status == IPStatus.TtlExpired || reply.Status == IPStatus.Success)
                    Console.WriteLine($"{ttl,2}: {addr}  time={reply.RoundtripTime}ms");
                else
                    Console.WriteLine($"{ttl,2}: * ({reply.Status})");

                if (reply.Status == IPStatus.Success) break;
            }
        }
        catch (Exception ex) { Console.WriteLine($"❌ Trace route failed: {ex.Message}"); }

        Console.WriteLine("\nPress any key to return.");
        Console.ReadKey();
    }

    // 5. Port Scan (High-Performance Parallel Execution with Cancellation)
    private static async Task PortScanAsync()
    {
        Console.Clear();
        Console.WriteLine("🔮 High-Performance Port Scan");
        Console.Write("Enter host or IP: ");
        string host = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(host)) return;

        Console.Write("Enter port range (e.g., 1-1024): ");
        string range = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(range) || !range.Contains('-')) return;

        var parts = range.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (!int.TryParse(parts[0], out int start) || !int.TryParse(parts[1], out int end)) return;

        start = Math.Clamp(start, 1, 65535);
        end = Math.Clamp(end, 1, 65535);
        if (end < start) (start, end) = (end, start);

        Console.WriteLine($"\nScanning {host} ports {start}-{end} concurrently...");
        Console.WriteLine("Press Any Key to attempt to abort...\n");

        using var cts = new CancellationTokenSource();
        
        // Listen for key abort asynchronously
        var abortTask = Task.Run(() => {
            Console.ReadKey(true);
            cts.Cancel();
        });

        var ports = Enumerable.Range(start, end - start + 1);
        var options = new ParallelOptions { MaxDegreeOfParallelism = 50, CancellationToken = cts.Token };

        try
        {
            await Parallel.ForEachAsync(ports, options, async (port, token) =>
            {
                try
                {
                    using var client = new TcpClient();
                    var connectTask = client.ConnectAsync(host, port, token).AsTask();
                    
                    // Enforce structural 150ms timeout window asynchronously
                    if (await Task.WhenAny(connectTask, Task.Delay(150, token)) == connectTask && client.Connected)
                    {
                        Console.WriteLine($"[+] Port {port,5} is OPEN");
                    }
                }
                catch { /* Closed/Filtered fallback */ }
            });
            Console.WriteLine("\nScan completed gracefully.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n❌ Ritual broken: Scan aborted midway by the user.");
        }

        Console.WriteLine("Press any key to return.");
        Console.ReadKey();
    }

    // 6. Local IP + Gateway
    private static void ShowLocalIpAndGateway()
    {
        Console.Clear();
        Console.WriteLine("🔮 Local IP + Gateway\n");

        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                        n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        n.NetworkInterfaceType != NetworkInterfaceType.Tunnel).ToList();

        if (!interfaces.Any())
        {
            Console.WriteLine("❌ No active network interfaces found.");
            Console.ReadKey();
            return;
        }

        foreach (var ni in interfaces)
        {
            Console.WriteLine($"Adapter: {ni.Name}");
            Console.WriteLine($"  Description: {ni.Description}");
            Console.WriteLine($"  MAC:  {ni.GetPhysicalAddress()}");

            var ipProps = ni.GetIPProperties();
            var ipv4 = ipProps.UnicastAddresses.Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork).Select(a => a.Address.ToString());
            var gateway = ipProps.GatewayAddresses.Where(g => g.Address.AddressFamily == AddressFamily.InterNetwork).Select(g => g.Address.ToString());
            var dns = ipProps.DnsAddresses.Where(a => a.AddressFamily == AddressFamily.InterNetwork).Select(a => a.ToString());

            Console.WriteLine($"  IPv4:    {string.Join(", ", ipv4)}");
            Console.WriteLine($"  Gateway: {string.Join(", ", gateway)}");
            Console.WriteLine($"  DNS:     {string.Join(", ", dns)}\n");
        }
        Console.ReadKey();
    }

    // 7. Active TCP connections
    private static void ShowActiveTcpConnections()
    {
        Console.Clear();
        Console.WriteLine("🔮 Active TCP Connections\n");

        try
        {
            var props = IPGlobalProperties.GetIPGlobalProperties();
            var conns = props.GetActiveTcpConnections();

            if (conns.Length == 0)
            {
                Console.WriteLine("No active TCP connections.");
            }
            else
            {
                Console.WriteLine($"{"Local",-25} {"Remote",-25} {"State",-12}");
                Console.WriteLine(new string('-', 65));
                foreach (var c in conns.OrderBy(c => c.State).ThenBy(c => c.LocalEndPoint.Port))
                {
                    Console.WriteLine($"{c.LocalEndPoint,-25} {c.RemoteEndPoint,-25} {c.State,-12}");
                }
            }
        }
        catch (Exception ex) { Console.WriteLine($"❌ Failed to read TCP connections: {ex.Message}"); }

        Console.ReadKey();
    }

    // 8. Proxy/PAC detection
    private static void DetectProxySettings()
    {
        Console.Clear();
        Console.WriteLine("🔮 Proxy / PAC Detection\n");
        ShowWinInetProxy();
        Console.WriteLine();
        ShowWinHttpProxy();
        Console.ReadKey();
    }

    private static void ShowWinInetProxy()
    {
        Console.WriteLine("WinINET (User Layer):");
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
                if (key != null)
                {
                    int enabled = (int)key.GetValue("ProxyEnable", 0);
                    Console.WriteLine($"  Proxy Enabled: {(enabled == 1 ? "Yes" : "No")}");
                    Console.WriteLine($"  Proxy Server:  {key.GetValue("ProxyServer", "(none)")}");
                    Console.WriteLine($"  PAC URL:       {key.GetValue("AutoConfigURL", "(none)")}");
                    return;
                }
            }
            Console.WriteLine("  ❌ Unavailable on this host environment.");
        }
        catch (Exception ex) { Console.WriteLine($"  ❌ Registry read error: {ex.Message}"); }
    }

    private static void ShowWinHttpProxy()
    {
        Console.WriteLine("WinHTTP (System Layer):");
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("  ❌ Diagnostics constrained to Windows environments.");
            return;
        }
        try
        {
            using Process p = new Process();
            p.StartInfo.FileName = "netsh";
            p.StartInfo.Arguments = "winhttp show proxy";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(2000);

            using var reader = new StringReader(output);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line)) Console.WriteLine("  " + line.Trim());
            }
        }
        catch (Exception ex) { Console.WriteLine($"  ❌ Failed netsh lookup: {ex.Message}"); }
    }

    // 9. Wi‑Fi Scryer (Flicker-Free Terminal Interface)
    private static void WifiScryer()
    {
        Console.Clear();
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("❌ System driver telemetry restricted to Windows platforms.");
            Console.ReadKey();
            return;
        }

        while (true)
        {
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                return;

            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🔮 Wi‑Fi Scryer (Press Q to exit)".PadRight(Console.WindowWidth));
            Console.ResetColor();

            string currentIntf = RunNetsh("wlan show interfaces");
            Console.WriteLine("\n[Connected Wi-Fi Interface]".PadRight(Console.WindowWidth));
            Console.WriteLine($"  SSID:    {Extract(currentIntf, "SSID")}".PadRight(Console.WindowWidth));
            Console.WriteLine($"  Signal:  {Extract(currentIntf, "Signal")}".PadRight(Console.WindowWidth));
            Console.WriteLine($"  Channel: {Extract(currentIntf, "Channel")}".PadRight(Console.WindowWidth));

            string nearby = RunNetsh("wlan show networks mode=bssid");
            Console.WriteLine("\n[Nearby Spectrum Entities]".PadRight(Console.WindowWidth));
            
            using (var reader = new StringReader(nearby))
            {
                string line;
                string currentSsid = "";
                int count = 0;
                while ((line = reader.ReadLine()) != null && count < 10)
                {
                    line = line.Trim();
                    if (line.StartsWith("SSID")) currentSsid = line.Split(':', 2).Last().Trim();
                    else if (line.StartsWith("Signal"))
                    {
                        string sig = line.Split(':', 2).Last().Trim();
                        Console.WriteLine($"  📡 {currentSsid,-20} Strength: {sig}".PadRight(Console.WindowWidth));
                        count++;
                    }
                }
            }
            Thread.Sleep(1000);
        }
    }

    // 10. Network Activity Recorder (With Process Lookup Throttling/Caching)
    private static void NetworkActivityRecorder()
    {
        Console.Clear();
        Console.WriteLine("🔮 Optimized Network Activity Recorder");
        Console.Write("Enter log path (default: network_log.txt): ");
        string file = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(file)) file = "network_log.txt";

        File.AppendAllText(file, $"\n\n===== Session Initialized {DateTime.Now} =====\n");

        var ni = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up &&
                                 n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                 n.NetworkInterfaceType != NetworkInterfaceType.Tunnel);
        if (ni == null) return;

        var prevStats = ni.GetIPv4Statistics();
        var processCache = new Dictionary<int, string>();

        Console.Clear();
        Console.WriteLine("Recording live patterns... Press Q to safely seal the record.\n");

        while (true)
        {
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
            {
                File.AppendAllText(file, $"===== Session Terminated {DateTime.Now} =====\n");
                return;
            }

            Thread.Sleep(1000);
            var nowStats = ni.GetIPv4Statistics();
            double upKbps = (nowStats.BytesSent - prevStats.BytesSent) * 8 / 1024.0;
            double downKbps = (nowStats.BytesReceived - prevStats.BytesReceived) * 8 / 1024.0;
            prevStats = nowStats;

            // Log raw traffic rates to reduce I/O loop delays
            File.AppendAllText(file, $"[{DateTime.Now}] Metric: Up={upKbps:F1}kbps Down={downKbps:F1}kbps\n");
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Streaming Log -> Up: {upKbps:F1} kbps | Down: {downKbps:F1} kbps");
        }
    }

    // 11. Latency Scryer (Flicker-Free Multi-Target Verification)
    private static void LatencyScryer()
    {
        Console.Clear();
        string gatewayIp = null;
        string dnsIp = null;

        var ni = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up &&
                                 n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                 n.NetworkInterfaceType != NetworkInterfaceType.Tunnel);
        if (ni != null)
        {
            var ipProps = ni.GetIPProperties();
            gatewayIp = ipProps.GatewayAddresses.FirstOrDefault(g => g.Address.AddressFamily == AddressFamily.InterNetwork)?.Address.ToString();
            dnsIp = ipProps.DnsAddresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?.ToString();
        }

        string internetHost = "8.8.8.8";
        using Ping ping = new Ping();

        List<long> gwHistory = new List<long>();
        List<long> dnsHistory = new List<long>();
        List<long> netHistory = new List<long>();
        int width = 30;

        while (true)
        {
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q) return;

            long gw = PingOnce(ping, gatewayIp);
            long dns = PingOnce(ping, dnsIp);
            long net = PingOnce(ping, internetHost);

            gwHistory.Add(gw); dnsHistory.Add(dns); netHistory.Add(net);
            if (gwHistory.Count > 30) gwHistory.RemoveAt(0);
            if (dnsHistory.Count > 30) dnsHistory.RemoveAt(0);
            if (netHistory.Count > 30) netHistory.RemoveAt(0);

            long max = new[] { gwHistory.Max(), dnsHistory.Max(), netHistory.Max() }.DefaultIfEmpty(1).Max();
            if (max <= 0) max = 1;

            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🔮 Latency Scryer (press Q to return)".PadRight(Console.WindowWidth));
            Console.ResetColor();
            Console.WriteLine($"Gateway:  {gatewayIp ?? "(none)"}".PadRight(Console.WindowWidth));
            Console.WriteLine($"DNS:      {dnsIp ?? "(none)"}".PadRight(Console.WindowWidth));
            Console.WriteLine($"Internet: {internetHost}\n".PadRight(Console.WindowWidth));

            Console.WriteLine($"Gateway:  {FormatLatency(gw),-6} {BuildLatencyBar(gw, max, width)}".PadRight(Console.WindowWidth));
            Console.WriteLine($"DNS:      {FormatLatency(dns),-6} {BuildLatencyBar(dns, max, width)}".PadRight(Console.WindowWidth));
            Console.WriteLine($"Internet: {FormatLatency(net),-6} {BuildLatencyBar(net, max, width)}".PadRight(Console.WindowWidth));
            Thread.Sleep(1000);
        }
    }

    private static long PingOnce(Ping ping, string host)
    {
        if (string.IsNullOrWhiteSpace(host)) return -1;
        try
        {
            var reply = ping.Send(host, 1000);
            return reply.Status == IPStatus.Success ? reply.RoundtripTime : -1;
        }
        catch { return -1; }
    }

    private static string FormatLatency(long ms) => ms < 0 ? "timeout" : $"{ms}ms";

    private static string BuildLatencyBar(long ms, long max, int width)
    {
        if (ms < 0) return new string('░', width);
        int bars = (int)(((double)ms / max) * width);
        bars = Math.Clamp(bars, 0, width);
        return new string('█', bars) + new string('░', width - bars);
    }

    private static string Extract(string text, string key)
    {
        using StringReader reader = new StringReader(text);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.TrimStart().StartsWith(key, StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split(':', 2);
                if (parts.Length > 1) return parts[1].Trim();
            }
        }
        return "(unknown)";
    }

    private static string RunNetsh(string arguments)
    {
        try
        {
            using Process p = new Process();
            p.StartInfo.FileName = "netsh";
            p.StartInfo.Arguments = arguments;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            string outStr = p.StandardOutput.ReadToEnd();
            p.WaitForExit(2000);
            return outStr;
        }
        catch { return string.Empty; }
    }
}