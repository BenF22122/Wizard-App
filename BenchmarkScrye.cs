using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.Versioning;
using System.Threading.Tasks;

[SupportedOSPlatform("windows")]
public static class BenchmarkScryer
{
    public static void Start()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🧪 LIGHTWEIGHT BENCHMARK — TRIAL OF STRENGTH");
            Console.ResetColor();
            Console.WriteLine("────────────────────────────────────────────");
            Console.WriteLine("1. Run full benchmark (CPU / RAM / GPU / Disk / RNG)");
            Console.WriteLine("2. Return to the wizard chamber");
            Console.WriteLine("────────────────────────────────────────────");
            Console.Write("Choose your incantation: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    RunFullBenchmark();
                    break;
                case "2":
                    return;
                default:
                    break;
            }
        }
    }

    private static void RunFullBenchmark()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🧪 Summoning the Trial of Strength…");
        Console.ResetColor();
        
        var machineType = DetectMachineType();
        bool isLaptop = IsLaptop(machineType);
        int threadCount = Environment.ProcessorCount;
        var gpuInfo = GetGpuInfo();

        // Adjust test execution times based on hardware constraints to manage thermal throttling
        double targetSecondsCpu = isLaptop ? 1.5 : 3.0;
        double targetSecondsMem = isLaptop ? 1.0 : 2.0;
        double targetSecondsGpu = isLaptop ? 1.5 : 3.0;
        double targetSecondsDisk = isLaptop ? 1.5 : 3.0;
        double targetSecondsRng = isLaptop ? 1.0 : 2.0;

        if (isLaptop)
        {
            Console.WriteLine($"✨ Laptop realm detected ({threadCount} Ley-Lines). Adapting burst trials.");
        }
        else
        {
            Console.WriteLine($"🏰 Desktop realm detected ({threadCount} Ley-Lines). Sustained stress trials engaged.");
        }
        Console.WriteLine("────────────────────────────────────────────");

        var cpuInt = CpuIntegerMultiThreaded(targetSecondsCpu, threadCount);
        var cpuFloat = CpuFloatMultiThreaded(targetSecondsCpu, threadCount);
        var mem = MemoryBenchmark(targetSecondsMem);
        var gpu = GpuBenchmark(targetSecondsGpu, gpuInfo.IsDedicated);
        var disk = DiskBenchmark(targetSecondsDisk);
        var rng = RngBenchmark(targetSecondsRng);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("📊 Benchmark Results");
        Console.ResetColor();
        Console.WriteLine("────────────────────────────────────────────");
        Console.WriteLine($"🏷 Machine Type       : {machineType} ({threadCount} Threads)");
        Console.WriteLine($"🔮 GPU Detected       : {gpuInfo.Name} {(gpuInfo.IsDedicated ? "[Dedicated]" : "[Integrated]")}");
        Console.WriteLine($"⚔ CPU Integer Ops     : {cpuInt.OpsPerSec / 1_000_000_000d,8:0.00} billion ops/sec");
        Console.WriteLine($"🔥 CPU Float Ops       : {cpuFloat.OpsPerSec / 1_000_000_000d,8:0.00} billion ops/sec");
        Console.WriteLine($"📚 Memory Throughput   : {mem.GbPerSec,8:0.00} GB/sec");
        Console.WriteLine($"👁 GPU Compute Speed   : {gpu.OpsPerSec / 1_000_000_000d,8:0.00} billion GFLOPS/sec");
        Console.WriteLine($"💾 Disk Write Speed    : {disk.WriteMbPerSec,8:0.00} MB/sec");
        Console.WriteLine($"💾 Disk Read Speed     : {disk.ReadMbPerSec,8:0.00} MB/sec");
        Console.WriteLine($"🎲 RNG Speed           : {rng.OpsPerSec / 1_000_000_000d,8:0.00} billion numbers/sec");
        Console.WriteLine("────────────────────────────────────────────");

        int score = ComputeScore(isLaptop, threadCount, gpuInfo.IsDedicated, cpuInt, cpuFloat, mem, gpu, disk, rng);
        int score100 = Math.Clamp(score / 10, 0, 100);

        string verdict;
        ConsoleColor verdictColor;

        if (score100 >= 85)
        {
            verdict = "⭐ The machine is strong. Its spirit burns bright.";
            verdictColor = ConsoleColor.Green;
        }
        else if (score100 >= 70)
        {
            verdict = "✨ The machine is capable. Its power is steady.";
            verdictColor = ConsoleColor.DarkGreen;
        }
        else if (score100 >= 50)
        {
            verdict = "⚠ The machine is adequate. It may struggle under heavy rituals.";
            verdictColor = ConsoleColor.Yellow;
        }
        else if (score100 >= 30)
        {
            verdict = "☠ The machine is weary. Demanding magic is not advised.";
            verdictColor = ConsoleColor.Red;
        }
        else
        {
            verdict = "💀 The machine is ancient. Only the simplest spells are safe.";
            verdictColor = ConsoleColor.DarkRed;
        }

        Console.ForegroundColor = verdictColor;
        Console.WriteLine($"Wizard Benchmark Score: {score100} / 100 ({machineType} Mixed-Compute Tuning)");
        Console.ResetColor();
        Console.WriteLine(verdict);
        Console.WriteLine("────────────────────────────────────────────");
        Pause();
    }

    // ------------------------------------------------------------
    // Models
    // ------------------------------------------------------------

    private record CpuResult(long Operations, double Seconds)
    {
        public double OpsPerSec => Seconds > 0 ? Operations / Seconds : 0;
    }

    private record MemResult(long BytesProcessed, double Seconds)
    {
        public double GbPerSec => Seconds > 0 ? (BytesProcessed / (1024d * 1024d * 1024d)) / Seconds : 0;
    }

    private record GpuResult(long Operations, double Seconds)
    {
        public double OpsPerSec => Seconds > 0 ? Operations / Seconds : 0;
    }

    private record DiskResult(double WriteMbPerSec, double ReadMbPerSec);

    private record RngResult(long Operations, double Seconds)
    {
        public double OpsPerSec => Seconds > 0 ? Operations / Seconds : 0;
    }

    private record GpuHardwareInfo(string Name, bool IsDedicated);

    // ------------------------------------------------------------
    // Machine & GPU Hardware Detection
    // ------------------------------------------------------------

    private static string DetectMachineType()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SystemEnclosure");
            foreach (ManagementObject obj in searcher.Get())
            {
                var types = obj["ChassisTypes"] as ushort[];
                if (types == null || types.Length == 0)
                    continue;

                foreach (var t in types)
                {
                    return t switch
                    {
                        8  => "Portable",
                        9  => "Laptop",
                        10 => "Notebook",
                        14 => "Sub-Notebook",
                        30 => "Tablet",
                        31 => "Convertible",
                        32 => "Detachable",
                        3  => "Desktop",
                        4  => "Desktop",
                        5  => "Desktop",
                        6  => "Desktop Tower",
                        7  => "Desktop Tower",
                        15 => "Desktop",
                        16 => "Portable Desktop",
                        35 => "All-in-One",
                        _  => "Unknown"
                    };
                }
            }
        }
        catch { }

        return "Unknown";
    }

    private static bool IsLaptop(string machineType)
    {
        if (string.IsNullOrWhiteSpace(machineType)) return false;

        machineType = machineType.ToLowerInvariant();
        return machineType.Contains("laptop")
            || machineType.Contains("notebook")
            || machineType.Contains("portable")
            || machineType.Contains("sub-notebook")
            || machineType.Contains("tablet")
            || machineType.Contains("convertible")
            || machineType.Contains("detachable");
    }

    private static GpuHardwareInfo GetGpuInfo()
    {
        string gpuName = "Unknown Graphics";
        bool isDedicated = false;

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (ManagementObject obj in searcher.Get())
            {
                gpuName = (obj["Name"] as string) ?? gpuName;
                
                // Read adapter RAM configuration to predict architecture limits
                if (obj["AdapterRAM"] != null)
                {
                    long ramBytes = Convert.ToInt64(obj["AdapterRAM"]);
                    // Integrated cards rarely declare over 2GB of hardware-dedicated static memory allocation pools
                    if (ramBytes > 2_147_483_648L)
                    {
                        isDedicated = true;
                    }
                }

                // Fallback checks via popular labeling strings
                string lowerName = gpuName.ToLowerInvariant();
                if (lowerName.Contains("nvidia") || lowerName.Contains("radeon") || lowerName.Contains("rtx") || lowerName.Contains("gtx"))
                {
                    if (!lowerName.Contains("graphics")) // filter clean integrated labels
                        isDedicated = true;
                }
                break; 
            }
        }
        catch { }

        return new GpuHardwareInfo(gpuName, isDedicated);
    }

    // ------------------------------------------------------------
    // CPU Multi-Threaded Integer Benchmark
    // ------------------------------------------------------------

    private static CpuResult CpuIntegerMultiThreaded(double targetSeconds, int threadCount)
    {
        Console.WriteLine("⚔ CPU Integer Trial (Multi-Threaded) starting…");

        long totalOps = 0;
        var sw = Stopwatch.StartNew();

        Parallel.For(0, threadCount, i =>
        {
            long localOps = 0;
            var localSw = Stopwatch.StartNew();

            while (localSw.Elapsed.TotalSeconds < targetSeconds)
            {
                for (int j = 0; j < 5_000_000; j++)
                {
                    localOps++;
                }
            }
            localSw.Stop();
            System.Threading.Interlocked.Add(ref totalOps, localOps);
        });

        sw.Stop();
        return new CpuResult(totalOps, sw.Elapsed.TotalSeconds);
    }

    // ------------------------------------------------------------
    // CPU Multi-Threaded Floating-Point Benchmark
    // ------------------------------------------------------------

    private static CpuResult CpuFloatMultiThreaded(double targetSeconds, int threadCount)
    {
        Console.WriteLine("🔥 CPU Floating-Point Trial (Multi-Threaded) starting…");

        long totalOps = 0;
        var sw = Stopwatch.StartNew();

        Parallel.For(0, threadCount, i =>
        {
            long localOps = 0;
            double x = 0.0001;
            var localSw = Stopwatch.StartNew();

            while (localSw.Elapsed.TotalSeconds < targetSeconds)
            {
                for (int j = 0; j < 2_000_000; j++)
                {
                    x = Math.Sqrt(x + 1.000001);
                    x = Math.Sin(x);
                    x = Math.Cos(x);
                    localOps += 3;
                }
            }
            localSw.Stop();
            System.Threading.Interlocked.Add(ref totalOps, localOps);
        });

        sw.Stop();
        return new CpuResult(totalOps, sw.Elapsed.TotalSeconds);
    }

    // ------------------------------------------------------------
    // Memory benchmark
    // ------------------------------------------------------------

    private static MemResult MemoryBenchmark(double targetSeconds)
    {
        Console.WriteLine("📚 Memory Trial starting…");

        int size = 64 * 1024 * 1024;
        byte[] buffer = new byte[size];
        long totalBytes = 0;
        var sw = Stopwatch.StartNew();
        var rnd = new Random(1234);

        while (sw.Elapsed.TotalSeconds < targetSeconds)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)rnd.Next(0, 256);
            }
            totalBytes += buffer.Length;

            long sum = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                sum += buffer[i];
            }
            totalBytes += buffer.Length;
        }

        sw.Stop();
        return new MemResult(totalBytes, sw.Elapsed.TotalSeconds);
    }

    // ------------------------------------------------------------
    // GPU Benchmark (High-Density Vector Transformation Simulation)
    // ------------------------------------------------------------

    private static GpuResult GpuBenchmark(double targetSeconds, bool isDedicated)
    {
        Console.WriteLine("🔮 GPU Compute Sight-Trial starting…");

        long totalOps = 0;
        var sw = Stopwatch.StartNew();

        // Dedicated pipelines use high-density optimization thread limits to simulate shader parallelism
        int computationalScale = isDedicated ? 32 : 8;

        Parallel.For(0, computationalScale, i =>
        {
            long localOps = 0;
            float x = 1.05f;
            float y = 0.95f;
            float z = 1.12f;

            var localSw = Stopwatch.StartNew();
            while (localSw.Elapsed.TotalSeconds < targetSeconds)
            {
                // Matrix 3D geometric transformation replication loop
                for (int j = 0; j < 1_000_000; j++)
                {
                    x = (x * 0.99f) + (y * 0.01f);
                    y = (y * 0.98f) - (z * 0.02f);
                    z = (z * 1.01f) + (x * 0.005f);
                    localOps += 6; // 6 floating point math steps per loop
                }
            }
            localSw.Stop();
            System.Threading.Interlocked.Add(ref totalOps, localOps);
        });

        sw.Stop();
        return new GpuResult(totalOps, sw.Elapsed.TotalSeconds);
    }

    // ------------------------------------------------------------
    // Disk benchmark (safe)
    // ------------------------------------------------------------

    private static DiskResult DiskBenchmark(double targetSeconds)
    {
        Console.WriteLine("💾 Disk Trial starting…");

        string tempFile = Path.Combine(Path.GetTempPath(), "wizard_bench.tmp");
        byte[] buffer = new byte[8 * 1024 * 1024];
        new Random(42).NextBytes(buffer);

        double writeSeconds = 0;
        double readSeconds = 0;
        long totalWritten = 0;
        long totalRead = 0;

        try
        {
            var swWrite = Stopwatch.StartNew();
            using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, FileOptions.SequentialScan))
            {
                while (swWrite.Elapsed.TotalSeconds < targetSeconds)
                {
                    fs.Write(buffer, 0, buffer.Length);
                    totalWritten += buffer.Length;
                }
            }
            swWrite.Stop();
            writeSeconds = swWrite.Elapsed.TotalSeconds;
        }
        catch { }

        try
        {
            var swRead = Stopwatch.StartNew();
            using (var fs = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.SequentialScan))
            {
                int read;
                while (swRead.Elapsed.TotalSeconds < targetSeconds &&
                       (read = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    totalRead += read;
                }
            }
            swRead.Stop();
            readSeconds = swRead.Elapsed.TotalSeconds;
        }
        catch { }

        try
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
        catch { }

        double writeMbSec = (writeSeconds > 0) ? (totalWritten / (1024d * 1024d)) / writeSeconds : 0;
        double readMbSec = (readSeconds > 0) ? (totalRead / (1024d * 1024d)) / readSeconds : 0;

        return new DiskResult(writeMbSec, readMbSec);
    }

    // ------------------------------------------------------------
    // RNG benchmark
    // ------------------------------------------------------------

    private static RngResult RngBenchmark(double targetSeconds)
    {
        Console.WriteLine("🎲 RNG Trial starting…");

        var rnd = new Random(999);
        long ops = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < targetSeconds)
        {
            for (int i = 0; i < 10_000_000; i++)
            {
                rnd.Next();
                ops++;
            }
        }

        sw.Stop();
        return new RngResult(ops, sw.Elapsed.TotalSeconds);
    }

    // ------------------------------------------------------------
    // Scoring (type, thread, and GPU-aware)
    // ------------------------------------------------------------

    private static int ComputeScore(bool isLaptop, int threads, bool isDedicatedGpu, CpuResult cpuInt, CpuResult cpuFloat, MemResult mem, GpuResult gpu, DiskResult disk, RngResult rng)
    {
        double coreScale = threads * 0.75; 
        double baselineCpuInt   = (isLaptop ? 0.8e9 : 1.2e9) * coreScale;
        double baselineCpuFloat = (isLaptop ? 0.4e9 : 0.8e9) * coreScale;
        
        double baselineMemGb    = isLaptop ? 3.0   : 10.0;
        double baselineDiskWrite= isLaptop ? 250   : 300;
        double baselineDiskRead = isLaptop ? 350   : 400;
        double baselineRng      = isLaptop ? 0.7e9 : 1.0e9;

        // Dynamic GPU baseline setting based on dedicated or integrated hardware design architecture
        double baselineGpuCompute = isDedicatedGpu ? 50.0e9 : 10.0e9; 
        if (isLaptop && !isDedicatedGpu) baselineGpuCompute = 6.0e9;

        int score = 0;

        // Balanced weight distribution totaling 1000 Max points
        score += ScoreComponent(cpuInt.OpsPerSec,   baselineCpuInt,   150);
        score += ScoreComponent(cpuFloat.OpsPerSec, baselineCpuFloat, 150);
        score += ScoreComponent(mem.GbPerSec,       baselineMemGb,    150);
        score += ScoreComponent(gpu.OpsPerSec,      baselineGpuCompute,200); // 200 Points for visual compute processing
        score += ScoreComponent(disk.WriteMbPerSec, baselineDiskWrite,125);
        score += ScoreComponent(disk.ReadMbPerSec,  baselineDiskRead, 125);
        score += ScoreComponent(rng.OpsPerSec,      baselineRng,      100);

        return Math.Clamp(score, 0, 1000);
    }

    private static int ScoreComponent(double value, double baseline, int maxPoints)
    {
        if (baseline <= 0) return 0;
        if (value <= 0) return 0;

        double ratio = value / baseline;
        double normalized = Math.Clamp((ratio - 0.3) / 1.7, 0.0, 1.0);
        return (int)(normalized * maxPoints);
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.Write("Press any key to return...");
        Console.ReadKey(true);
    }
}