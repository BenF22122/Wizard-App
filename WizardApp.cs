using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Collections.Generic;



public static class WizardApp

{private static readonly Random rand = new Random();
    public static void Run()
    {
        Console.Clear();
        DrawWizardIntro();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("✨ A puff of azure smoke fills the room. An ancient figure appears. ✨");
        Console.WriteLine("'Greetings, seeker of knowledge. I am the System Archmage. Which spell shall we cast today?'");
        Console.WriteLine("(Type 'help' to see the spellbook or 'begone' to dismiss the wizard)");
        Console.ResetColor();

        bool staying = true;

        while (staying)
        {
            Console.Write("\n🔮 > ");
            string? input = Console.ReadLine()?.ToLower().Trim();

            switch (input)
            {
                case "ping the dragon":
                case "cast ping":
                    CastPing("google.com");
                    break;

                case "ping of truth":
                    CastTruth();
                    break;
                
                case "check my brain":
                case "cast storage":
                    CastStorage();
                    break;

                case "check spirit":
                case "cast cpu":
                    CastCPU();
                    break;

                case "invoke chaos":
                    CastChaos();
                    break;

                case "cast runes":
                case "reveal runes":
                case "cast network":
                    CastNetwork();
                    break;

                case "summon public ip":
                case "summon ip":
                case "public ip":
                    CastPublicIP();
                    break;

                case "clean the clutter":
                case "cast clenup":
                case "cast disk clen up":
                    CastCleanMgr();
                    break;

                case "cast task manager":
                case "cast taskmanager":
                case "cast taskmgr":
                    CastTaskMgr();
                    break;

                case "i wish to draw my own scroll":
                case "cast paint":
                    CastPaint();
                    break;

                case "talk to a higher power":
                    CastCopilot();
                    break;                        

                case "help":
                case "open spellbook":
                    DrawSpellbook();
                    break;


                case "wise words":
                    WiseWords();
                    break;

                case "cast password":
                case "summon password":
                case "enchant word":
                    CastPassword();
                    break;
                   

                case "begone":
                    Console.WriteLine("'Farewell, traveler. May your latency be low and your uptime eternal.'");
                    staying = false;
                    break;

                default:
                    Console.WriteLine("'That incantation is unknown to me. Are you sure you've read the scrolls correctly?'");
                    break;
            }
        }
    }

    static void CastPing(string host)
    {
        Console.WriteLine($"'I cast my gaze toward the realm of {host}...'");

        try
        {
            using Process p = new Process();
            p.StartInfo.FileName = "ping";
            p.StartInfo.Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"-n 4 {host}" : $"-c 4 {host}";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            if (output.Contains("TTL") || output.Contains("time="))
                Console.WriteLine("'The stars are aligned! The connection is strong and true.'");
            else
                Console.WriteLine("'The mists are thick; I cannot reach that distant land.'");
        }
        catch
        {
            Console.WriteLine("'The magic fizzled. Check your permissions, mortal.'");
        }
    }

    private static void CastTruth()
{
    Console.Write("Speak the name of the distant realm (hostname or IP): ");
    string? host = Console.ReadLine();

    Console.WriteLine($"'I cast my gaze toward the realm of {host}...'");

    try
    {
        using Process p = new Process();
        p.StartInfo.FileName = "ping";
        p.StartInfo.Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? $"-n 4 {host}"
            : $"-c 4 {host}";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.Start();

        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        if (output.Contains("TTL") || output.Contains("time="))
            Console.WriteLine("'The stars are aligned! The connection is strong and true.'");
        else
            Console.WriteLine("'The mists are thick; I cannot reach that distant land.'");
    }
    catch
    {
        Console.WriteLine("'The magic fizzled. Check your permissions, mortal.'");
    }

    Console.WriteLine();
    Console.WriteLine("Press any key to return to the wizard’s chamber...");
    Console.ReadKey(true);
}


    static void CastStorage()
    {
        Console.WriteLine("'Scanning the crystalline vaults of your machine...'");

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.IsReady)
            {
                long freeGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                Console.WriteLine($"'In the vault of {drive.Name}, there are {freeGB} gigabytes of empty scrolls remaining.'");
            }
        }
    }

    static void CastCPU()
    {
        Console.WriteLine("'I am peering into the very soul of the machine...'");

        var proc = Process.GetCurrentProcess();
        Console.WriteLine($"'The machine's spirit is currently burdened by {Environment.ProcessorCount} cores of raw power.'");
        Console.WriteLine($"'Your current ritual is consuming {proc.WorkingSet64 / 1024 / 1024} MB of ethereal memory.'");
    }

    static void CastNetwork()
    {
    Console.WriteLine("'I open the ancient tome of connectivity... stand back.'");

    try
    {
        using Process p = new Process();
        p.StartInfo.FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ipconfig" : "ifconfig";
        p.StartInfo.Arguments = "";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.Start();

        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("📜 'Behold, the runes of your network configuration:'");
        Console.ResetColor();

        Console.WriteLine(output);
    }
    catch
    {
        Console.WriteLine("'The spirits refuse to reveal the network runes. Something is amiss.'");
    }
}

    static async void CastPublicIP()
    {
    Console.WriteLine("'I reach beyond the veil... calling upon distant spirits to reveal your true sigil in the great web.'");

    try
    {
        using var client = new HttpClient();
        string ip = await client.GetStringAsync("https://api.ipify.org");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"📡 'The spirits whisper your public IP: {ip}'");
        Console.ResetColor();
    }
    catch
    {
        Console.WriteLine("'The ether resists my scrying. The public IP remains hidden from mortal eyes.'");
    }
}

    static void CastCleanMgr()
{
    Console.WriteLine("'I am summoning the spectral brooms to sweep away your digital cobwebs...'");
    try
    {
        Process.Start("cleanmgr.exe");
        Console.WriteLine("'The Purge has begun. Choose which illusions to banish from your sight.'");
    }
    catch
    {
        Console.WriteLine("'The brooms have failed to materialize. Are you perhaps not on a Windows-based realm?'");
    }
}

    static void CastTaskMgr()
{
    Console.WriteLine("'I am summoning the the orical...'");
    try
    {
        Process.Start("taskmgr.exe");
        Console.WriteLine("'The orical greats you and allows you access to this machines holy spirit'");
    }
    catch
    {
        Console.WriteLine("'The Orical does not venture outside of his Windows based home'");
    }
}

    static void CastPaint()
{
    Console.WriteLine("'A artist in the meaking i see...'");
    try
    {
        Process.Start("mspaint.exe");
        Console.WriteLine("'Behold the sacred doodleing ground young one'");
    }
    catch
    {
        Console.WriteLine("'The saced scroll making tome is not inviting, Are you sure you are in the correct universe?'");
    }
}

    static void CastCopilot()
    {
        Console.WriteLine("'Opening a portal to the Great Oracle... Prepare your questions.'");
        try
        {
            // This tells Windows to open the Copilot/Bing Chat interface
            Process.Start(new ProcessStartInfo
            {
                FileName = "microsoft-edge:?ux=copilot&tcp=1&source=taskbar",
                UseShellExecute = true
            });
        }
        catch
        {
            Console.WriteLine("'The Oracle is shielded by a powerful ward (or you are not on Windows 11).' ");
        }
    }

    static void CastChaos()
    {
        Console.WriteLine("'Brace yourself, mortal... chaos approaches.'");

        string[] chaosLines =
        {
            "✨ Reality shimmers like jelly...",
            "🐔 A spectral chicken materialises, judges you, and vanishes.",
            "⚡ Lightning erupts from the floorboards!",
            "📜 A scroll appears, reads YOU, and bursts into flames.",
            "🌀 The room rotates 90 degrees. Only you notice.",
            "👁 A giant floating eye blinks once and drifts away.",
            "💀 A skull laughs politely.",
            "🌈 Colours you’ve never seen before leak from the walls.",
            "🎲 The wizard rolls a d20. He does not tell you the result.",
            "🪄 Your keyboard briefly turns into a baguette."
        };

int events = rand.Next(3, 7);

for (int i = 0; i < events; i++)
{
    Console.ForegroundColor = (ConsoleColor)rand.Next(1, 16);
    Console.WriteLine(chaosLines[rand.Next(chaosLines.Length)]);
    System.Threading.Thread.Sleep(rand.Next(200, 700));
}


        Console.ResetColor();
        Console.WriteLine("'...and thus, the chaos subsides.'");
    }

   static void WiseWords()
    {
        Console.WriteLine("'A wise man once told me.'");

        string[] wisdomLines =
        {
            "💃 A women is always right",
            "👁️ Its all fun and games until you lose an eye, then its fun and games you no longer see",
            "🕯️ Fire is hot`",
            "📜 Dont let other ruin your day, ruin theres first",
            "🌀 it is alwys a good time to have a cookie",
            "👁 Its only wrong if they catch you",
            "🍫 Have a break have a Kitkat",
            "🌈 Dont be afraid to be yourself",
            "🤔 Just think what would a Dalek do?",
            "🤫 Would they notice if you sneak out early today?"
        };

int events = rand.Next(3, 7);

for (int i = 0; i < events; i++)
{
    Console.ForegroundColor = (ConsoleColor)rand.Next(1, 16);
    Console.WriteLine(wisdomLines[rand.Next(wisdomLines.Length)]);
    System.Threading.Thread.Sleep(rand.Next(200, 700));
}


        Console.ResetColor();
        Console.WriteLine("'...and thus ends this lesson.'");
    }

private static void CastPassword()
{
    Console.Write("Speak the base word you wish to enchant: ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("'The spell fizzles… you must provide a word.'");
        return;
    }

    // Define possible rune substitutions
    var runes = new Dictionary<char, string[]>
    {
        { 'a', new[] { "@", "4" } },
        { 'e', new[] { "3" } },
        { 'i', new[] { "1", "!" } },
        { 'o', new[] { "0" } },
        { 's', new[] { "$", "5" } },
        { 't', new[] { "+" } },
        { 'b', new[] { "8" } }
    };

    string result = "";

    foreach (char c in input)
    {
        char lower = char.ToLower(c);

        // If this letter has rune replacements…
        if (runes.ContainsKey(lower))
        {
            // 50% chance to replace it
            if (rand.NextDouble() < 0.5)
            {
                string[] options = runes[lower];
                string chosen = options[rand.Next(options.Length)];

                // Preserve uppercase if needed
                if (char.IsUpper(c))
                    chosen = chosen.ToUpper();

                result += chosen;
                continue;
            }
        }

        // Otherwise keep the original character
        result += c;
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n🔐 Your enchanted password is: {result}");
    Console.ResetColor();

    Console.WriteLine("\nPress any key to return to the wizard’s chamber...");
    Console.ReadKey(true);
}

static void DrawSpellbook()
{
    Console.ForegroundColor = ConsoleColor.Magenta;

    string[] book =
    {
        "  _____________________________________________",
        " /                                             \\",
        "|   📖  *THE ARCANE SPELLBOOK OF SYSTEM MAGIC*  |",
        "|                                               |",
        "|   ✨ ping the dragon      – Ping google.com    |",
        "|   ✨ ping of truth        – Ping any host      |",
        "|   ✨ check my brain       – Disk space         |",
        "|   ✨ check spirit         – CPU info           |",
        "|   ✨ cast network         – Network config     |",
        "|   ✨ summon public ip     – Public IP lookup   |",
        "|   ✨ clean the clutter    – Disk cleanup       |",
        "|   ✨ cast taskmanager     – Task Manager       |",
        "|   ✨ cast paint           – MSPaint            |",
        "|   ✨ wise words           – Random wisdom      |",
        "|   ✨ cast password        – Enchant a word     |",
        "|   ✨ invoke chaos         – Pure nonsense      |",
        "|   ✨ begone               – Exit the wizard    |",
        "|                                               |",
        " \\_____________________________________________/"
    };

    foreach (string line in book)
    {
        Console.WriteLine(line);
        System.Threading.Thread.Sleep(20);
    }

    Console.ResetColor();
}

static void DrawWizardIntro()
{
    Console.ForegroundColor = ConsoleColor.Yellow;

    string[] wizardArt =
    {
        "                 ____",
        "               .'* *.'",
        "            __/_*_*(_",
        "           / _______ \\",
        "          _\\_)/___\\(_/_",
        "         / _((\\- -/))_ \\",
        "         \\ \\())(-)(()/ /",
        "          ' \\(((()))/ '",
        "         / ' \\)).))/ ' \\",
        "        / _ \\ - | - /_  \\",
        "       (   ( .;''';. .'  )",
        "       _\\\"__ /    )\\ __\"/_",
        "         \\/  \\   ' /  \\/",
        "          .'  '...' ' )",
        "           / /  |  \\ \\",
        "          / .   .   . \\",
        "         /   .     .   \\",
        "        /   /   |   \\   \\",
        "      .'   /    b    '.  '.",
        "   _.'    /     bb     '._ '.",
        "  (______/      bb        \\_____)"
    };

    foreach (string line in wizardArt)
    {
        Console.WriteLine(line);
        System.Threading.Thread.Sleep(25);
    }

    Console.ResetColor();
}


}