using System;
using System.Diagnostics;
using System.IO;
using System.Linq;                 // Needed for .Any()
using System.Runtime.InteropServices;
using WizardUpdateSpell;
using Wizard.Modules;

public class Program
{
    // MAIN IS NOW ASYNC ✔
    public static async Task Main()
    {
        // Enable full Unicode + emoji support in ALL terminals (fixes installer issue)
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        while (true)
        {
            Console.Clear();
            
            // Grand Header Alignment
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=====================================================================");
            Console.WriteLine("🔮 WELCOME MORTAL TO MY HUMBLE ABODE. WHAT DO YOU SEEK?");
            Console.WriteLine("=====================================================================");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  [1]  Summon admin tools        [6]  Summon hardware        [11] Summon updates     [16] Summon custom spell");
            Console.WriteLine("  [2]  Summon system monitor     [7]  Summon disk scan       [12] Summon boot        [17] Summon export spell");
            Console.WriteLine("  [3]  Summon scroll spells      [8]  Summon LAN scan        [13] Summon sound       [18] Summon services");
            Console.WriteLine("  [4]  Summon event scrolls      [9]  Summon wixard error    [14] Summon system      [19] Summon file spy");
            Console.WriteLine("  [5]  Summon network            [10] Summon program spy     [15] Summon benchmark   [20] Summon grimoire");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("✨ Type a [Number], a keyword, or 'Begone' to leave the chamber.");
            Console.ResetColor();
            Console.WriteLine("---------------------------------------------------------------------");

            Console.Write("\nYour incantation: ");
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            // Normalize input to allow robust, flexible choice lookups
            if (IsMatch(input, "Begone", "exit", "quit"))
            {
                break;
            }
            else if (IsMatch(input, "1", "admin", "Summon admin tools"))
            {
                WizardApp.Run(); 
            }
            else if (IsMatch(input, "2", "monitor", "system", "Summon system monitor"))
            {
                SystemMonitor.Run();
            }
            else if (IsMatch(input, "3", "scroll", "spells", "files", "Summon scroll spells"))
            {
                WizardFileManager.Start();
            }
            else if (IsMatch(input, "4", "event", "events", "Summon event scrolls"))
            {
                WizardEventScryer.Start();
            }   
            else if (IsMatch(input, "5", "network", "net", "Summon network"))
            {
                WizardNetworkScryer.Start();
            } 
            else if (IsMatch(input, "6", "hardware", "specs", "Summon hardware"))
            {
                HardwareScryer.Start();  
            } 
            else if (IsMatch(input, "7", "disk", "scan", "Summon disk scan"))
            {
                DiskScryer.Start();
            }
            else if (IsMatch(input, "8", "lan", "local", "Summon LAN scan"))
            {
                WizardLanScryer.Start();
            }
            else if (IsMatch(input, "10", "program", "spy", "Summon program spy"))
            {
                WizardProgramScryer.Start();
            }
            else if (IsMatch(input, "11", "update", "Summon update"))
            {
                UpdateSpell.Start();
            }
            else if (IsMatch(input, "12", "boot", "Summon boot"))
            {
                BootScryer.Start();
            }
            else if (IsMatch(input, "13", "audio", "sound", "Summond sound"))
            {
                AudioScryer.Start();
            }
            else if (IsMatch(input, "14", "windows", "overview", "Summon system"))
            {
                // ✔ async System Scryer call
                await SystemScryer.StartAsync();
            }
            else if (IsMatch(input, "15", "stress", "benchmark", "Summon benchmark"))
            {
               BenchmarkScryer.Start();
            }
            else if (IsMatch(input, "16", "custom", "Summon custom spell"))
            {
               SpellbookScryer.Start();
            }
            else if (IsMatch(input, "17", "export", "Summon export spell"))
            {
               ReportScryer.Start();
            }
            else if (IsMatch(input, "18", "service", "Summon services"))
            {
                WizService.WizService.Start();
            }
            else if (IsMatch(input, "19", "metadata", "Summon file spy"))
            {
               FileMetadataScryer.Start();
            }
            else if (IsMatch(input, "9", "error", "diagnostic", "Summon wizard error"))
            {
                 WizardDiagnosticScryer.Start();
            }
                        else if (IsMatch(input, "20", "logbook", "Summon grimoir"))
            {
                new Grimoire().Run();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n💨 The silence remains unbroken. (Incorrect summoning phrase).");
                Console.ResetColor();
                Console.WriteLine("Press any key to clear the miscast spell.");
                Console.ReadKey();
            }
        }

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("✨ The doors of the sanctuary slam shut. Farewell, traveler.");
        Console.ResetColor();
    }

    // High performance case-insensitive flexible token scanner
    private static bool IsMatch(string input, params string[] options)
    {
        return options.Any(opt => string.Equals(input, opt, StringComparison.OrdinalIgnoreCase));
    }
}
