using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;

public static class SpellbookScryer
{
    private record Spell(string Name, string Command);

    private static readonly string SpellFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WizardScryer");

    private static readonly string SpellFile =
        Path.Combine(SpellFolder, "spells.json");

    private static List<Spell> _spells = new();

    public static void Start()
    {
        LoadSpells();

        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("📜 THE GRAND ARCHMAGE'S WORKBENCH");
            Console.ResetColor();
            Console.WriteLine("────────────────────────────────────────────");
            Console.WriteLine("1. Inspect the Mana Reservoir (Live RAM Tracker)");
            Console.WriteLine("2. Peer into the Planar Gate Ledger (Netstat TCP Check)");
            Console.WriteLine("3. Teach a new spell (Create Custom Ritual)");
            Console.WriteLine("4. Reshape a spell (Edit Custom Ritual)");
            Console.WriteLine("5. Unleash a custom spell (Cast)");
            Console.WriteLine("6. Purge a spell from memory (Delete)");
            Console.WriteLine("7. Return to the wizard chamber (Exit)");
            Console.WriteLine("────────────────────────────────────────────");
            Console.Write("Choose your incantation: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    ScryManaReservoir();
                    break;
                case "2":
                    ReadPlanarGateLedger();
                    break;
                case "3":
                    LearnNewSpell();
                    break;
                case "4":
                    EditSpell();
                    break;
                case "5":
                    CastSpell();
                    break;
                case "6":
                    DeleteSpell();
                    break;
                case "7":
                    SaveSpells();
                    Console.WriteLine("\n✨ May the wards protect you on your journey.");
                    return;
                default:
                    break;
            }
        }
    }

    // -------------------- Core Diagnostic Spells --------------------

    private static void ScryManaReservoir()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("🔮 SCRYING THE MANA RESERVOIR (RESOURCE CHECK)");
        Console.ResetColor();
        Console.WriteLine("────────────────────────────────────────────");

        try
        {
            // Fetch total memory allocations using modern cross-platform safe metrics
            long totalAllocatedMemory = GC.GetTotalMemory(false);
            double allocatedMb = totalAllocatedMemory / 1024.0 / 1024.0;

            using (var currentProc = Process.GetCurrentProcess())
            {
                double privateWorkingSetMb = currentProc.PrivateMemorySize64 / 1024.0 / 1024.0;
                
                Console.WriteLine($"• App Mana Consumption: {allocatedMb:F2} MB");
                Console.WriteLine($"• System Working Set:     {privateWorkingSetMb:F2} MB physical memory claimed.");
                Console.WriteLine($"• Thread Phantoms:       {currentProc.Threads.Count} active spirits handling tasks.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ The mana metrics could not be stabilized: {ex.Message}");
        }

        Console.WriteLine("────────────────────────────────────────────");
        Pause("The Ley Lines have been evaluated.");
    }

    private static void ReadPlanarGateLedger()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("🌐 THE PLANAR GATE LEDGER (ACTIVE CONNECTIONS)");
        Console.ResetColor();
        Console.WriteLine("────────────────────────────────────────────");
        Console.WriteLine($"{"[Local Realm Endpoint]",-30} {"[Foreign Realm Endpoint]",-30} {"[Gate State]"}");
        Console.WriteLine("────────────────────────────────────────────");

        try
        {
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnections = ipProperties.GetActiveTcpConnections();

            // Display the top 15 connections to avoid flooding the scroll
            foreach (var conn in tcpConnections.Take(15))
            {
                Console.WriteLine($"{conn.LocalEndPoint,-30} {conn.RemoteEndPoint,-30} {conn.State}");
            }

            if (tcpConnections.Length > 15)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"...and {tcpConnections.Length - 15} more hidden planar links.");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Unable to peer through the planar veil: {ex.Message}");
        }

        Console.WriteLine("────────────────────────────────────────────");
        Pause("The gate monitoring lattice has settled.");
    }

    // -------------------- Storage & Persistence --------------------

    private static void LoadSpells()
    {
        try
        {
            if (!Directory.Exists(SpellFolder))
                Directory.CreateDirectory(SpellFolder);

            if (!File.Exists(SpellFile))
            {
                _spells = new List<Spell>();
                return;
            }

            var json = File.ReadAllText(SpellFile);
            var loaded = JsonSerializer.Deserialize<List<Spell>>(json);
            _spells = loaded ?? new List<Spell>();
        }
        catch
        {
            _spells = new List<Spell>();
        }
    }

    private static void SaveSpells()
    {
        try
        {
            if (!Directory.Exists(SpellFolder))
                Directory.CreateDirectory(SpellFolder);

            var json = JsonSerializer.Serialize(_spells, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(SpellFile, json);
        }
        catch
        {
            // Silent fail – wizard keeps calm if scroll ink spills
        }
    }

    // -------------------- Custom Script Management --------------------

    private static void LearnNewSpell()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("✨ TEACH THE WIZARD A NEW SPELL");
        Console.ResetColor();

        Console.Write("Spell name: ");
        var name = (Console.ReadLine() ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            Pause("The winds carry no name. Spell abandoned.");
            return;
        }

        Console.Write("Command to cast (e.g., ping google.com): ");
        var command = (Console.ReadLine() ?? "").Trim();

        if (string.IsNullOrWhiteSpace(command))
        {
            Pause("A spell with no words cannot be spoken.");
            return;
        }

        // Prevent duplicate names gracefully
        _spells.RemoveAll(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        _spells.Add(new Spell(name, command));
        SaveSpells();

        Pause($"The spell \"{name}\" has been etched into the spellbook.");
    }

    private static void EditSpell()
    {
        if (!EnsureAnySpells()) return;

        int index = ChooseSpell("Which spell shall be reshaped?");
        if (index < 0) return;

        var spell = _spells[index];

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"🛠 EDITING SPELL: {spell.Name}");
        Console.ResetColor();

        Console.Write($"New name (leave blank to keep \"{spell.Name}\"): ");
        var newName = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(newName))
            newName = spell.Name;

        Console.Write($"New command (leave blank to keep \"{spell.Command}\"): ");
        var newCommand = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(newCommand))
            newCommand = spell.Command;

        _spells[index] = new Spell(newName, newCommand);
        SaveSpells();

        Pause($"The spell \"{newName}\" has been reforged.");
    }

    private static void CastSpell()
    {
        if (!EnsureAnySpells()) return;

        int index = ChooseSpell("Which spell shall be cast?");
        if (index < 0) return;

        var spell = _spells[index];

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"🔮 UNLEASHING RITUAL: {spell.Name}");
        Console.ResetColor();
        Console.WriteLine($"Chanting: {spell.Command}");
        Console.WriteLine("────────────────────────────────────────────");

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"{spell.Command}\"", // Wrapped in escaped quotes to secure sub-arguments
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                if (process == null)
                {
                    Pause("🔮 The planar portal failed to open. (Process initialization error)");
                    return;
                }

                // Synchronously ingest the text streams so nothing gets clipped out
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(output))
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(output);
                }

                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("⚠️ The spell feedback encountered an anomaly:");
                    Console.WriteLine(error);
                }
            }

            Console.ResetColor();
            Console.WriteLine("────────────────────────────────────────────");
            Pause("✨ The echoes of the spell have faded back into the ether.");
        }
        catch (Exception ex)
        {
            Console.ResetColor();
            Pause($"💥 The spell fizzled violently: {ex.Message}");
        }
    }

    private static void DeleteSpell()
    {
        if (!EnsureAnySpells()) return;

        int index = ChooseSpell("Which spell shall be forgotten?");
        if (index < 0) return;

        var spell = _spells[index];

        Console.Write($"Are you sure you wish to erase \"{spell.Name}\"? (y/N): ");
        var confirm = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
        if (confirm == "y" || confirm == "yes")
        {
            _spells.RemoveAt(index);
            SaveSpells(); // Fixed: Ensure data is saved immediately to disk upon purge
            Pause($"The spell \"{spell.Name}\" has faded from the tome.");
        }
    }

    // -------------------- Internal Helpers --------------------

    private static bool EnsureAnySpells()
    {
        if (_spells.Count == 0)
        {
            Pause("The spellbook is empty. Teach the wizard a custom ritual first.");
            return false;
        }
        return true;
    }

    private static int ChooseSpell(string prompt)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("📜 SPELLBOOK CONTENTS");
        Console.ResetColor();
        Console.WriteLine("────────────────────────────────────────────");

        for (int i = 0; i < _spells.Count; i++)
        {
            Console.WriteLine($"[{i + 1}] { _spells[i].Name }  →  { _spells[i].Command }");
        }

        Console.WriteLine("────────────────────────────────────────────");
        Console.Write(prompt + " (or press Enter to cancel): ");

        var input = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(input))
            return -1;

        if (!int.TryParse(input, out int choice))
        {
            Pause("The runes are unclear. No spell chosen.");
            return -1;
        }

        choice -= 1; // Translate back to structural 0-based array indexing
        if (choice < 0 || choice >= _spells.Count)
        {
            Pause("That spell does not exist in this tome.");
            return -1;
        }

        return choice;
    }

    private static void Pause(string message)
    {
        Console.WriteLine();
        Console.WriteLine(message);
        Console.WriteLine();
        Console.Write("Press any key to return...");
        Console.ReadKey(true);
    }
}