using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;
using System.Threading;

public static class DiskScryer
{
    private class FileCategoryInfo
    {
        public string Name { get; set; } = "";
        public long TotalBytes { get; set; }
    }

    private class FileEntry
    {
        public string Path { get; set; } = "";
        public long Size { get; set; }
    }

    private static readonly Dictionary<string, string> ExtensionCategoryMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".mp4"] = "Video", [".mkv"] = "Video", [".avi"] = "Video",
            [".mov"] = "Video", [".wmv"] = "Video", [".flv"] = "Video",

            [".mp3"] = "Music", [".wav"] = "Music", [".flac"] = "Music",
            [".aac"] = "Music", [".ogg"] = "Music", [".m4a"] = "Music",

            [".jpg"] = "Image", [".jpeg"] = "Image", [".png"] = "Image",
            [".gif"] = "Image", [".bmp"] = "Image", [".tiff"] = "Image",
            [".webp"] = "Image",

            [".pdf"] = "Document", [".doc"] = "Document", [".docx"] = "Document",
            [".xls"] = "Document", [".xlsx"] = "Document", [".ppt"] = "Document",
            [".pptx"] = "Document", [".txt"] = "Document", [".rtf"] = "Document",

            [".zip"] = "Archive", [".rar"] = "Archive", [".7z"] = "Archive",
            [".tar"] = "Archive", [".gz"] = "Archive", [".iso"] = "Archive",

            [".exe"] = "Executable", [".dll"] = "Executable", [".msi"] = "Executable",
            [".bat"] = "Executable", [".cmd"] = "Executable", [".ps1"] = "Executable"
        };

    private static long _filesScanned = 0;
    private static bool _scanRunning = false;
    private static Task? _statusTask = null; // Track status thread
    private static readonly object _consoleLock = new(); // Protect the console output

    public static void Start()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("💿 Disk Scryer");
            Console.WriteLine("1. Analyse a folder or drive");
            Console.WriteLine("2. Return to Wizard's Chamber");
            Console.Write("\nChoose your spell: ");

            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1": AnalysePath(); break;
                case "2": return;
            }
        }
    }

    private static void AnalysePath()
    {
        Console.Clear();
        Console.WriteLine("💿 Disk Scryer - Path Selection\n");
        Console.Write("Enter a folder or drive to analyse: ");
        string path = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(path)) return;
        if (!Directory.Exists(path))
        {
            Console.WriteLine("\n❌ That folder/drive does not exist.");
            Pause();
            return;
        }

        Console.WriteLine("\nChoose scan mode:");
        Console.WriteLine("1. Full recursive scan");
        Console.WriteLine("2. Quick scan");
        Console.Write("\nYour choice: ");
        string mode = Console.ReadLine() ?? "";

        bool recursive = mode == "1";

        int maxDegree = GetCpuAwareParallelism(path);
        var drive = GetDriveInfo(path);
        string driveType = drive?.DriveType.ToString() ?? "Unknown";

        Console.Clear();
        Console.WriteLine("💿 Disk Scryer");
        Console.WriteLine($"Target: {path}");
        Console.WriteLine($"Mode: {(recursive ? "Full recursive" : "Quick scan")}");
        Console.WriteLine($"Threads: {maxDegree}\n");
        Console.WriteLine("Scanning...\n");

        var categories = new Dictionary<string, FileCategoryInfo>(StringComparer.OrdinalIgnoreCase);
        var largestFiles = new List<FileEntry>();
        var folderSizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

        object categoryLock = new();
        object largestLock = new();
        object folderLock = new();

        StartStatusPanel(maxDegree, driveType);

        try
        {
            ScanPathParallel(path, recursive, maxDegree,
                categories, largestFiles, folderSizes,
                categoryLock, largestLock, folderLock);
        }
        catch (Exception ex)
        {
            StopStatusPanel();
            Console.WriteLine($"\n❌ Scan failed: {ex.Message}");
            Pause();
            return;
        }

        StopStatusPanel(); // This now blocks until the status thread fully stops cleanly
        
        lock (_consoleLock)
        {
            Console.Clear();
            Console.WriteLine("💿 Disk Scryer - Results\n");

            ShowCategoryAndFolderSummary(categories, folderSizes, path);
            Console.WriteLine();
            ShowLargestFiles(largestFiles);
        }

        Pause();
    }

    private static int GetCpuAwareParallelism(string path)
    {
        int logical = Environment.ProcessorCount;
        int physical = GetPhysicalCoreCount();
        float load = GetCpuLoad();

        int threads = physical > 0 ? physical : logical;

        if (load > 80) threads = Math.Max(1, threads / 2);
        else if (load > 60) threads = Math.Max(1, threads - 2);

        var drive = GetDriveInfo(path);
        if (drive != null)
        {
            switch (drive.DriveType)
            {
                case DriveType.Network: threads = Math.Min(threads, 2); break;
                case DriveType.Removable: threads = Math.Min(threads, 2); break;
                case DriveType.CDRom: threads = 1; break;
                case DriveType.Fixed:
                    if (IsLikelyHdd(drive)) threads = Math.Min(threads, 4);
                    break;
            }
        }

        return Math.Max(1, Math.Min(threads, logical));
    }

    private static int GetPhysicalCoreCount()
    {
        try
        {
            int cores = 0;
            using var searcher = new ManagementObjectSearcher("select NumberOfCores from Win32_Processor");
            foreach (var item in searcher.Get())
                cores += Convert.ToInt32(item["NumberOfCores"]);
            return cores;
        }
        catch { return 0; }
    }

    private static float GetCpuLoad()
    {
        try
        {
            using var cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpu.NextValue();
            Thread.Sleep(200);
            return cpu.NextValue();
        }
        catch { return 0; }
    }

    private static DriveInfo? GetDriveInfo(string path)
    {
        try
        {
            string root = Path.GetPathRoot(path) ?? path;
            return DriveInfo.GetDrives().FirstOrDefault(d =>
                d.Name.Equals(root, StringComparison.OrdinalIgnoreCase));
        }
        catch { return null; }
    }

    private static bool IsLikelyHdd(DriveInfo drive)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT MediaType FROM Win32_DiskDrive");
            foreach (var item in searcher.Get())
            {
                string type = item["MediaType"]?.ToString() ?? "";
                if (type.Contains("SSD", StringComparison.OrdinalIgnoreCase))
                    return false;
            }
        }
        catch { }
        return true;
    }

    private static void StartStatusPanel(int threadCount, string driveType)
    {
        _scanRunning = true;
        _filesScanned = 0;

        var spinner = new[] { "—", "\\", "|", "/" };
        int spin = 0;

        var sw = Stopwatch.StartNew();

        _statusTask = Task.Run(async () =>
        {
            while (_scanRunning)
            {
                double sec = sw.Elapsed.TotalSeconds;
                double speed = sec > 0 ? _filesScanned / sec : 0;

                lock (_consoleLock)
                {
                    if (!_scanRunning) break; // Double-check inside lock
                    
                    Console.SetCursorPosition(0, 5);
                    Console.WriteLine("📡 Live Status Panel               ");
                    Console.WriteLine($"Threads:       {threadCount}       ");
                    Console.WriteLine($"Drive Type:    {driveType}        ");
                    Console.WriteLine($"Files Scanned: {_filesScanned:N0} ");
                    Console.WriteLine($"Speed:         {speed:N0} files/sec   ");
                    Console.WriteLine($"Elapsed:       {sw.Elapsed:hh\\:mm\\:ss}   ");
                    Console.WriteLine($"Spinner:       [{spinner[spin]}]   ");
                }

                spin = (spin + 1) % spinner.Length;
                await Task.Delay(150);
            }
        });
    }

    private static void StopStatusPanel()
    {
        _scanRunning = false;
        _statusTask?.Wait(); // Block until background output is explicitly dead
    }

    private static void ScanPathParallel(
        string rootPath, bool recursive, int maxDegree,
        Dictionary<string, FileCategoryInfo> categories,
        List<FileEntry> largestFiles,
        Dictionary<string, long> folderSizes,
        object categoryLock, object largestLock, object folderLock)
    {
        var files = EnumerateFilesSafe(rootPath, recursive);

        Parallel.ForEach(files,
            new ParallelOptions { MaxDegreeOfParallelism = maxDegree },
            file =>
            {
                long size;
                try { size = new FileInfo(file).Length; }
                catch { return; }

                Interlocked.Increment(ref _filesScanned);

                string ext = Path.GetExtension(file);
                string category = ExtensionCategoryMap.TryGetValue(ext, out string mapped)
                    ? mapped : "Other";

                lock (categoryLock)
                {
                    if (!categories.TryGetValue(category, out var info))
                        categories[category] = info = new FileCategoryInfo { Name = category };
                    info.TotalBytes += size;
                }

                string topFolder = GetTopLevelFolder(rootPath, file);
                lock (folderLock)
                {
                    folderSizes[topFolder] =
                        folderSizes.TryGetValue(topFolder, out long cur)
                        ? cur + size : size;
                }

                lock (largestLock)
                {
                    if (largestFiles.Count < 50)
                    {
                        largestFiles.Add(new FileEntry { Path = file, Size = size });
                        if (largestFiles.Count == 50)
                            largestFiles.Sort((a, b) => b.Size.CompareTo(a.Size));
                    }
                    else if (size > largestFiles[^1].Size)
                    {
                        largestFiles[^1] = new FileEntry { Path = file, Size = size };
                        largestFiles.Sort((a, b) => b.Size.CompareTo(a.Size));
                    }
                }
            });
    }

    private static string GetTopLevelFolder(string rootPath, string filePath)
    {
        try
        {
            string fullRoot = Path.GetFullPath(rootPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            string fullFile = Path.GetFullPath(filePath);

            if (!fullFile.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
                return "(Other)";

            string relative = fullFile.Substring(fullRoot.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (string.IsNullOrEmpty(relative))
                return "(root)";

            string[] parts = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (parts.Length == 1)
                return "(root)";

            return parts[0];
        }
        catch { return "(Unknown)"; }
    }

    private static IEnumerable<string> EnumerateFilesSafe(string rootPath, bool recursive)
    {
        var dirs = new Stack<string>();
        dirs.Push(rootPath);

        while (dirs.Count > 0)
        {
            string current = dirs.Pop();

            IEnumerable<string> files;
            try { files = Directory.EnumerateFiles(current); }
            catch { continue; }

            foreach (var f in files) yield return f;

            if (!recursive) continue;

            IEnumerable<string> subdirs;
            try { subdirs = Directory.EnumerateDirectories(current); }
            catch { continue; }

            foreach (var d in subdirs) dirs.Push(d);
        }
    }

    private static void ShowCategoryAndFolderSummary(
        Dictionary<string, FileCategoryInfo> categories,
        Dictionary<string, long> folderSizes,
        string rootPath)
    {
        Console.WriteLine("📊 Space by Category (left) and by Folder (right):\n");

        var orderedCats = categories.Values.OrderByDescending(c => c.TotalBytes).ToList();
        var orderedFolders = folderSizes.OrderByDescending(f => f.Value).Take(15).ToList();

        // Fix: Prevent sequence exception on empty folders
        long maxCat = orderedCats.Count > 0 ? orderedCats.Max(c => c.TotalBytes) : 1;
        long maxFolder = orderedFolders.Count > 0 ? orderedFolders.Max(f => f.Value) : 1;

        int totalWidth = Console.WindowWidth < 40 ? 80 : Console.WindowWidth;
        int halfWidth = totalWidth / 2;

        // Give data strings room to breathe so they don't get truncated early
        int leftBarWidth = Math.Max(5, halfWidth - 25);
        int rightBarWidth = Math.Max(5, halfWidth - 25);

        string headerLeft = PadRight("File Categories", halfWidth);
        string headerRight = "Folder Sizes".PadLeft(halfWidth); 
        Console.WriteLine(headerLeft + headerRight);    

        int rows = Math.Max(orderedCats.Count, orderedFolders.Count);

        for (int i = 0; i < rows; i++)
        {
            string left = "";
            string right = "";

            if (i < orderedCats.Count)
            {
                var cat = orderedCats[i];
                string sizeStr = FormatSize(cat.TotalBytes);
                int bars = (int)Math.Round((double)cat.TotalBytes / maxCat * leftBarWidth);
                bars = Math.Max(0, Math.Min(bars, leftBarWidth));
                string bar = new string('█', bars) + new string('░', leftBarWidth - bars);
                left = $"{cat.Name,-12} {bar} {sizeStr,8}";
            }

            if (i < orderedFolders.Count)
            {
                var f = orderedFolders[i];
                string folderName = f.Key == rootPath ? "(root)" : f.Key;
                // Cap folder character displaying layout to avoid wrapping bugs
                if (folderName.Length > 12) folderName = folderName.Substring(0, 9) + "...";
                
                string sizeStr = FormatSize(f.Value);
                int bars = (int)Math.Round((double)f.Value / maxFolder * rightBarWidth);
                bars = Math.Max(0, Math.Min(bars, rightBarWidth));
                string bar = new string('█', bars) + new string('░', rightBarWidth - bars);
                right = $"{folderName,-12} {bar} {sizeStr,8}";
            }

            string leftPadded = PadRight(left, halfWidth);
            Console.WriteLine(leftPadded + right);
        }
    }

    private static string PadRight(string text, int width)
    {
        if (text.Length >= width) return text.Substring(0, width - 1);
        return text.PadRight(width);
    }

    private static void ShowLargestFiles(List<FileEntry> largestFiles)
    {
        Console.WriteLine("📂 Largest Files (Top 20):\n");

        int count = Math.Min(20, largestFiles.Count);
        for (int i = 0; i < count; i++)
        {
            var f = largestFiles[i];
            Console.WriteLine($"{i + 1,2}. {FormatSize(f.Size),8}  {f.Path}");
        }
    }

    private static string FormatSize(long bytes)
    {
        const double KB = 1024.0, MB = KB * 1024, GB = MB * 1024, TB = GB * 1024;
        if (bytes >= TB) return $"{bytes / TB:0.0} TB";
        if (bytes >= GB) return $"{bytes / GB:0.0} GB";
        if (bytes >= MB) return $"{bytes / MB:0.0} MB";
        if (bytes >= KB) return $"{bytes / KB:0.0} KB";
        return $"{bytes} B";
    }

    private static void Pause()
    {
        Console.WriteLine("\nPress any key to return...");
        Console.ReadKey();
    }
}