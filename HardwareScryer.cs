using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

public static class HardwareScryer
{
    public static void Start()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🔧 Hardware Scryer");
            Console.ResetColor();

            Console.WriteLine("1. CPU Info");
            Console.WriteLine("2. RAM Info");
            Console.WriteLine("3. Storage Info");
            Console.WriteLine("4. USB Devices");
            Console.WriteLine("5. Display & Video Info");
            Console.WriteLine("6. Network Adapter Hardware Info");
            Console.WriteLine("7. Motherboard Info");
            Console.WriteLine("8. BIOS Info");
            Console.WriteLine("9. Battery Info");
            Console.WriteLine("10. Run ALL diagnostics");
            Console.WriteLine("11. Return to Wizard's Chamber");
            Console.Write("\nChoose your spell: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": Console.Clear(); CpuInfo(); Pause(); break;
                case "2": Console.Clear(); RamInfo(); Pause(); break;
                case "3": Console.Clear(); StorageInfo(); Pause(); break;
                case "4": Console.Clear(); UsbDevices(); Pause(); break;
                case "5": Console.Clear(); DisplayInfo(); Pause(); break;
                case "6": Console.Clear(); NicHardwareInfo(); Pause(); break;
                case "7": Console.Clear(); MotherboardInfo(); Pause(); break;
                case "8": Console.Clear(); BiosInfo(); Pause(); break;
                case "9": Console.Clear(); BatteryInfo(); Pause(); break;
                case "10": RunAllDiagnostics(); break;
                case "11": return;
            }
        }
    }

    // -------------------------
    // 1. CPU INFO
    // -------------------------
    private static void CpuInfo()
    {
        Console.WriteLine("🧠 CPU Information\n");
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                Console.WriteLine($"Name:                 {obj["Name"]?.ToString().Trim()}");
                Console.WriteLine($"Manufacturer:         {obj["Manufacturer"]}");
                Console.WriteLine($"Cores:                {obj["NumberOfCores"]}");
                Console.WriteLine($"Logical Processors:   {obj["NumberOfLogicalProcessors"]}");
                Console.WriteLine($"Max Clock Speed:      {obj["MaxClockSpeed"]} MHz");
                Console.WriteLine($"Current Clock Speed:  {obj["CurrentClockSpeed"]} MHz");
                Console.WriteLine($"L2 Cache Size:        {obj["L2CacheSize"]} KB");
                Console.WriteLine($"L3 Cache Size:        {obj["L3CacheSize"]} KB");
                Console.WriteLine($"Architecture Mode:    {DecodeArchitecture(obj["Architecture"])}");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to read CPU info: {ex.Message}");
        }
    }

    private static string DecodeArchitecture(object arch)
    {
        if (arch == null) return "Unknown";
        return Convert.ToInt32(arch) switch
        {
            0 => "x86 (32-bit)",
            5 => "ARM",
            9 => "x64 (64-bit)",
            12 => "ARM64",
            _ => $"Unknown Enum ({arch})"
        };
    }

    // -------------------------
    // 2. RAM INFO
    // -------------------------
    private static void RamInfo()
    {
        Console.WriteLine("💾 RAM Information\n");
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            ulong total = 0;
            int slot = 1;

            foreach (var obj in searcher.Get())
            {
                ulong cap = obj["Capacity"] != null ? Convert.ToUInt64(obj["Capacity"]) : 0;
                total += cap;

                Console.WriteLine($"Slot {slot++}:");
                Console.WriteLine($"  Capacity:     {cap / (1024 * 1024 * 1024)} GB");
                Console.WriteLine($"  Speed:        {obj["Speed"]} MHz");
                Console.WriteLine($"  Manufacturer: {obj["Manufacturer"]?.ToString().Trim()}");
                Console.WriteLine($"  Serial Num:   {obj["SerialNumber"]?.ToString().Trim()}");
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Total Installed Physical Memory: {total / (1024 * 1024 * 1024)} GB");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to read RAM info: {ex.Message}");
        }
    }

    // -------------------------
    // 3. STORAGE INFO
    // -------------------------
    private static void StorageInfo()
    {
        Console.WriteLine("🗄 Storage Information\n");
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

            foreach (var obj in searcher.Get())
            {
                ulong sizeBytes = obj["Size"] != null ? Convert.ToUInt64(obj["Size"]) : 0;
                Console.WriteLine($"Drive Hardware: {obj["Model"]}");
                Console.WriteLine($"  Interface:    {obj["InterfaceType"]}");
                Console.WriteLine($"  Total Size:   {sizeBytes / (1024 * 1024 * 1024)} GB");
                Console.WriteLine($"  System ID:    {obj["DeviceID"]}");
                Console.WriteLine();
            }

            Console.WriteLine("Logical Partition Mappings:\n");

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady) continue;

                Console.WriteLine($"Volume: [{drive.Name}]");
                Console.WriteLine($"  File System:  {drive.DriveFormat}");
                Console.WriteLine($"  Type:         {drive.DriveType}");
                Console.WriteLine($"  Capacity:     {drive.TotalSize / (1024 * 1024 * 1024)} GB");
                Console.WriteLine($"  Available:    {drive.AvailableFreeSpace / (1024 * 1024 * 1024)} GB free");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to read storage info: {ex.Message}");
        }
    }

    // -------------------------
    // 4. USB DEVICES
    // -------------------------
    private static void UsbDevices()
    {
        Console.WriteLine("🔌 USB Controller Stack & Connected Hardware\n");
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT * FROM Win32_PnPEntity WHERE PNPClass = 'USB' OR Service = 'USBSTOR'");

            int counted = 0;
            foreach (var obj in searcher.Get())
            {
                string name = obj["Name"]?.ToString();
                if (string.IsNullOrWhiteSpace(name)) continue;

                Console.WriteLine($"Device Node: {name}");
                Console.WriteLine($"  Vendor:    {obj["Manufacturer"] ?? "(Generic)"}");
                Console.WriteLine($"  PNP ID:    {obj["DeviceID"]}");
                Console.WriteLine();
                counted++;
            }

            if (counted == 0) Console.WriteLine("No operational USB nodes intercepted.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to read USB devices: {ex.Message}");
        }
    }

    // -------------------------
    // 5. DISPLAY & VIDEO INFO
    // -------------------------
    private static void DisplayInfo()
    {
        Console.WriteLine("🖥 Display & Graphics Hardware\n");
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (var obj in searcher.Get())
            {
                Console.WriteLine($"GPU Adapter:  {obj["Name"]}");
                Console.WriteLine($"  Processor:  {obj["VideoProcessor"]}");
                Console.WriteLine($"  Driver Ver: {obj["DriverVersion"]}");

                if (obj["CurrentHorizontalResolution"] != null)
                {
                    Console.WriteLine($"  Resolution: {obj["CurrentHorizontalResolution"]} x {obj["CurrentVerticalResolution"]} @ {obj["CurrentRefreshRate"]}Hz");
                }
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to intercept video telemetry: {ex.Message}");
        }
    }

    // -------------------------
    // 6. NIC HARDWARE INFO
    // -------------------------
    private static void NicHardwareInfo()
    {
        Console.WriteLine("🌐 Network Adapter Hardware Info\n");
        try
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var ni in adapters)
            {
                Console.WriteLine($"Adapter Alias: {ni.Name}");
                Console.WriteLine($"  Description: {ni.Description}");
                Console.WriteLine($"  Physical MAC: {ni.GetPhysicalAddress()}");
                Console.WriteLine($"  Media Type:   {ni.NetworkInterfaceType}");
                Console.WriteLine($"  Link Speed:   {ni.Speed / 1_000_000} Mbps");
                Console.WriteLine($"  Link Status:  {ni.OperationalStatus}");

                var ipProps = ni.GetIPProperties();
                try
                {
                    var v4Props = ipProps?.GetIPv4Properties();
                    if (v4Props != null)
                    {
                        Console.WriteLine($"  MTU Range:    {v4Props.Mtu}");
                    }
                }
                catch { }
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to read NIC info: {ex.Message}");
        }
    }

    // -------------------------
    // 7. MOTHERBOARD INFO
    // -------------------------
    private static void MotherboardInfo()
    {
        Console.WriteLine("🧩 Motherboard Information\n");
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            foreach (var obj in searcher.Get())
            {
                Console.WriteLine($"Manufacturer:   {obj["Manufacturer"]}");
                Console.WriteLine($"Product:        {obj["Product"]}");
                Console.WriteLine($"Model:          {obj["Model"] ?? "(not provided)"}");
                Console.WriteLine($"Serial Number:  {obj["SerialNumber"]}");
                Console.WriteLine($"Version:        {obj["Version"]}");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to read motherboard info: {ex.Message}");
        }
    }

    // -------------------------
    // 8. BIOS INFO
    // -------------------------
    private static void BiosInfo()
    {
        Console.WriteLine("📜 BIOS / Firmware Information\n");
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
            foreach (var obj in searcher.Get())
            {
                Console.WriteLine($"Vendor:          {obj["Manufacturer"]}");
                Console.WriteLine($"Version:         {obj["SMBIOSBIOSVersion"]}");
                Console.WriteLine($"Release Date:    {obj["ReleaseDate"]}");
                Console.WriteLine($"BIOS Caption:    {obj["Caption"]}");
                Console.WriteLine($"SMBIOS Version:  {obj["SMBIOSMajorVersion"]}.{obj["SMBIOSMinorVersion"]}");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to read BIOS info: {ex.Message}");
        }
    }

    // -------------------------
    // 9. BATTERY INFO (Safe Memory Padding & Translated UX)
    // -------------------------
    private static void BatteryInfo()
    {
        Console.WriteLine("🔋 Battery / Power Information\n");

        // Sealed WMI safe block wrapper guarding non-mobile systems
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            var results = searcher.Get();

            if (results.Count == 0)
            {
                Console.WriteLine("No telemetry from Win32_Battery topology (Desktop PC or virtualized system).");
            }
            else
            {
                foreach (var obj in results)
                {
                    Console.WriteLine($"Status:            {DecodeBatteryStatus(obj["BatteryStatus"])}");
                    Console.WriteLine($"Estimated Charge:  {obj["EstimatedChargeRemaining"]}%");
                    Console.WriteLine($"Estimated Runtime: {(obj["EstimatedRunTime"] != null ? obj["EstimatedRunTime"] + " minutes" : "Unknown")}");
                    Console.WriteLine($"Chemistry:         {DecodeChemistry(obj["Chemistry"])}");
                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Win32_Battery query rejected by OS: {ex.Message}");
        }

        Console.WriteLine("\nSystem Power Status (kernel32 Architecture):\n");

        if (GetSystemPowerStatus(out SYSTEM_POWER_STATUS status))
        {
            Console.WriteLine($"AC Line Status:    {DecodeACLine(status.ACLineStatus)}");
            Console.WriteLine($"Battery Life %:    {(status.BatteryLifePercent == 255 ? "Unknown" : status.BatteryLifePercent + "%")}");
            Console.WriteLine($"Battery Life Time: {(status.BatteryLifeTime == -1 ? "Unlimited / AC Connected" : status.BatteryLifeTime + " seconds")}");
            Console.WriteLine($"Battery Full Time: {(status.BatteryFullLifeTime == -1 ? "Unknown / AC Connected" : status.BatteryFullLifeTime + " seconds")}");
        }
        else
        {
            Console.WriteLine("❌ Unable to retrieve kernel32 power status.");
        }
    }

    private static string DecodeBatteryStatus(object status)
    {
        if (status == null) return "Unknown";
        return Convert.ToInt32(status) switch
        {
            1 => "Discharging (Battery Power)",
            2 => "AC Connected (Fully Charged / Connected)",
            3 => "Fully Charged",
            4 => "Low Battery",
            5 => "Critical Battery",
            6 => "Charging",
            7 => "Charging and High",
            8 => "Charging and Low",
            9 => "Charging and Critical",
            10 => "Undefined / Initialization State",
            11 => "Partially Charged",
            _ => $"Unknown Flag ({status})"
        };
    }

    private static string DecodeACLine(byte ac)
    {
        return ac switch
        {
            0 => "Offline (Running on Battery Power)",
            1 => "Online (Wall Outlet Power Connected)",
            255 => "Unknown Status",
            _ => $"Invalid code ({ac})"
        };
    }

    private static string DecodeChemistry(object chem)
    {
        if (chem == null) return "Unknown";
        return Convert.ToInt32(chem) switch
        {
            1 => "Other",
            2 => "Unknown",
            3 => "Lead Acid",
            4 => "Nickel Cadmium",
            5 => "Nickel Metal Hydride",
            6 => "Lithium Ion",
            7 => "Zinc Air",
            8 => "Lithium Polymer",
            _ => $"Unknown ({chem})"
        };
    }

    // Explicitly aligned and padded Win32 memory struct blueprint
    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEM_POWER_STATUS
    {
        public byte ACLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte SystemStatus;        // Maps 1:1 to 'BatterySaver' byte parameter padding slot
        public int BatteryLifeTime;
        public int BatteryFullLifeTime;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS sps);

    // -------------------------
    // 10. RUN ALL DIAGNOSTICS
    // -------------------------
    private static void RunAllDiagnostics()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("🔮 Running All Hardware Diagnostics Sequentially...\n");
        Console.ResetColor();

        CpuInfo(); Separator();
        RamInfo(); Separator();
        StorageInfo(); Separator();
        UsbDevices(); Separator();
        DisplayInfo(); Separator();
        NicHardwareInfo(); Separator();
        MotherboardInfo(); Separator();
        BiosInfo(); Separator();
        BatteryInfo();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✨ All system diagnostics metrics aggregated successfully.");
        Console.ResetColor();
        Pause();
    }

    private static void Separator()
    {
        Console.WriteLine(new string('-', Math.Max(10, Console.WindowWidth - 1)));
    }

    private static void Pause()
    {
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
    }
}