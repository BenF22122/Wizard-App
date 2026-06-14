using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;

// -------------------------
// Console Window Wrapper (Fix for hidden dialogs)
// -------------------------
public class WindowWrapper : IWin32Window
{
    private readonly IntPtr _hwnd;
    public WindowWrapper(IntPtr handle) { _hwnd = handle; }
    public IntPtr Handle => _hwnd;
}

public static class ConsoleWindow
{
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();
}

// -------------------------
// MAIN FILE MANAGER
// -------------------------
public static class WizardFileManager
{
    public static void Start()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("========================================");
            Console.WriteLine("       📜 SCROLL & CHAMBER SPELLS       ");
            Console.WriteLine("========================================");
            Console.ResetColor();

            Console.WriteLine("\n[1] Create a new scroll");
            Console.WriteLine("[2] Seal a scroll with arcane protection");
            Console.WriteLine("[3] Open a sealed scroll");
            Console.WriteLine("[4] Edit a sealed scroll");
            Console.WriteLine("[5] Return to Main Sanctuary (Exit)");

            Console.Write("\nChoose your spell: ");
            string choice = Console.ReadLine()?.Trim();

            // Support both numbers and intuitive exit commands
            if (choice == "5" || choice?.ToLower() == "q" || choice?.ToLower() == "quit")
            {
                Console.WriteLine("\n✨ May your paths be safely guarded by ancient magic.");
                System.Threading.Thread.Sleep(1200);
                return;
            }

            switch (choice)
            {
                case "1": CreateScroll(); break;
                case "2": EncryptScroll(); break;
                case "3": OpenSealedScroll(); break;
                case "4": EditSealedScroll(); break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ That spell is unknown to this realm.");
                    Console.ResetColor();
                    Console.WriteLine("Press any key to clear your focus...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private static string BrowseForFile(string filter, string title)
    {
        using (var dialog = new OpenFileDialog())
        {
            dialog.Filter = filter;
            dialog.Title = title;
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;

            IntPtr consoleHandle = ConsoleWindow.GetConsoleWindow();
            var wrapper = new WindowWrapper(consoleHandle);

            return dialog.ShowDialog(wrapper) == DialogResult.OK ? dialog.FileName : null;
        }
    }

    private static string Ask(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()?.Trim();
    }

    // -------------------------
    // 1. Create Scroll
    // -------------------------
    private static void CreateScroll()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== ✨ Create a New Scroll ===\n");
        Console.ResetColor();
        
        string name = Ask("Name your scroll (e.g., notes.txt) or 'q' to go back: ");

        if (string.IsNullOrWhiteSpace(name) || name.ToLower() == "q")
            return;

        try
        {
            FileTools.CreateEmptyFile(name);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✨ Scroll '{name}' has been scribed into existence.");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Failed to scribe scroll: {ex.Message}");
        }
        
        Console.ResetColor();
        Console.WriteLine("\nPress any key to return...");
        Console.ReadKey();
    }

    // -------------------------
    // 2. Seal Scroll
    // -------------------------
    private static void EncryptScroll()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== 🔒 Seal a Scroll with Arcane Protection ===\n");
        Console.ResetColor();

        string browse = Ask("Browse for scroll? (y/n) [or 'q' to cancel]: ")?.ToLower();
        if (browse == "q" || browse == "quit") return;

        string path = browse == "y"
            ? BrowseForFile("Text Files (*.txt)|*.txt|All Files (*.*)|*.*", "Choose Scroll")
            : Ask("Enter scroll path: ");

        if (string.IsNullOrWhiteSpace(path)) return;

        if (!FileTools.FileExists(path))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Manifestation error: Scroll file not found.");
            Console.ResetColor();
            Console.ReadKey();
            return;
        }

        string password = Ask("Create a master password to seal this scroll: ");
        if (string.IsNullOrEmpty(password)) return;

        try
        {
            string contents = File.ReadAllText(path);
            EncryptionTools.EncryptTextToFile(contents, path + ".wizlock", password);
            File.Delete(path); // Safely remove original plain text

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n🔒 The scroll has been successfully sealed with an arcane lock.");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Sealing spell failed: {ex.Message}");
        }

        Console.ResetColor();
        Console.ReadKey();
    }

    // -------------------------
    // 3. Open Sealed Scroll
    // -------------------------
    private static void OpenSealedScroll()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== ✨ Open a Sealed Scroll ===\n");
        Console.ResetColor();

        string browse = Ask("Browse for sealed scroll? (y/n) [or 'q' to cancel]: ")?.ToLower();
        if (browse == "q" || browse == "quit") return;

        string path = browse == "y"
            ? BrowseForFile("Sealed Scrolls (*.wizlock)|*.wizlock|All Files (*.*)|*.*", "Choose Sealed Scroll")
            : Ask("Enter sealed scroll path: ");

        if (string.IsNullOrWhiteSpace(path)) return;

        if (!FileTools.FileExists(path))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ The parchment you seek does not exist.");
            Console.ResetColor();
            Console.ReadKey();
            return;
        }

        string password = Ask("Provide the key to break the seal: ");

        try
        {
            string contents = EncryptionTools.DecryptFile(path, password);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n--- 📜 The Scroll Reads ---");
            Console.ResetColor();
            Console.WriteLine(string.IsNullOrWhiteSpace(contents) ? "[The scroll is completely blank]" : contents);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--------------------------");
        }
        catch
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n❌ The protective barrier rejects your password.");
        }

        Console.ResetColor();
        Console.WriteLine("\nPress any key to close the scroll...");
        Console.ReadKey();
    }

    // -------------------------
    // 4. Edit Sealed Scroll
    // -------------------------
    private static void EditSealedScroll()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== ✨ Edit a Sealed Scroll ===\n");
        Console.ResetColor();

        string browse = Ask("Browse for sealed scroll? (y/n) [or 'q' to cancel]: ")?.ToLower();
        if (browse == "q" || browse == "quit") return;

        string path = browse == "y"
            ? BrowseForFile("Sealed Scrolls (*.wizlock)|*.wizlock|All Files (*.*)|*.*", "Choose Sealed Scroll")
            : Ask("Enter sealed scroll path: ");

        if (string.IsNullOrWhiteSpace(path)) return;

        if (!FileTools.FileExists(path))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ No such sealed scroll exists.");
            Console.ResetColor();
            Console.ReadKey();
            return;
        }

        string password = Ask("Enter password to break the seal: ");
        string contents;

        try
        {
            contents = EncryptionTools.DecryptFile(path, password);
        }
        catch
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Incorrect password. Access denied.");
            Console.ResetColor();
            Console.ReadKey();
            return;
        }

        // Standardize newlines safely split into a manipulation list
        List<string> lines = contents.Replace("\r", "").Split('\n').ToList();
        
        // Fix split behavior adding an empty string on fresh empty files
        if (lines.Count == 1 && string.IsNullOrEmpty(lines[0])) 
            lines.Clear();

        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== 🛠️ Transmuting Scroll Contents ===");
            Console.ResetColor();
            
            ShowLines(lines);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n--- Arcane Commands ---");
            Console.WriteLine(":add <text>            -> Add new line to bottom");
            Console.WriteLine(":set <number> <text>   -> Replace content of specific line");
            Console.WriteLine(":del <number>          -> Wipe a line out of existence");
            Console.WriteLine(":save                  -> Seal changes and write to disk");
            Console.WriteLine(":q                     -> Discard changes and walk away");
            Console.ResetColor();

            Console.Write("\n⚡ > ");
            string input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input)) continue;

            if (input.Equals(":q", StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️ Alterations abandoned.");
                Console.ResetColor();
                System.Threading.Thread.Sleep(1200);
                return;
            }
            else if (input.Equals(":save", StringComparison.OrdinalIgnoreCase))
            {
                SaveEditedScroll(path, password, lines);
                return;
            }
            else if (input.StartsWith(":add ", StringComparison.OrdinalIgnoreCase))
            {
                if (input.Length > 5)
                    lines.Add(input.Substring(5));
            }
            // Defensively managed parsing block for line updates
            else if (input.StartsWith(":set ", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = input.Split(new[] { ' ' }, 3);
                if (parts.Length == 3 && int.TryParse(parts[1], out int ln) && ln >= 1 && ln <= lines.Count)
                {
                    lines[ln - 1] = parts[2];
                }
                else
                {
                    ShowError("Invalid line selection or format missing replacement text.");
                }
            }
            // Defensively managed parsing block for line deletions
            else if (input.StartsWith(":del ", StringComparison.OrdinalIgnoreCase))
            {
                string indexPart = input.Substring(5).Trim();
                if (int.TryParse(indexPart, out int ln) && ln >= 1 && ln <= lines.Count)
                {
                    lines.RemoveAt(ln - 1);
                }
                else
                {
                    ShowError("Invalid line index provided.");
                }
            }
            else
            {
                ShowError("Unknown incantation command.");
            }
        }
    }

    private static void ShowLines(List<string> lines)
    {
        if (lines.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("[ This scroll holds no incantations yet. use :add to begin ]");
            Console.ResetColor();
            return;
        }

        for (int i = 0; i < lines.Count; i++)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($"[{i + 1}] ");
            Console.ResetColor();
            Console.WriteLine(lines[i]);
        }
    }

    private static void ShowError(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ {msg}");
        Console.ResetColor();
        System.Threading.Thread.Sleep(1500);
    }

    private static void SaveEditedScroll(string encryptedPath, string password, List<string> lines)
    {
        try
        {
            string newText = string.Join(Environment.NewLine, lines);
            string temp = encryptedPath + ".tmp";

            EncryptionTools.EncryptTextToFile(newText, temp, password);

            if (File.Exists(encryptedPath)) File.Delete(encryptedPath);
            File.Move(temp, encryptedPath);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✨ The scroll has been completely resealed into security.");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Failed to encrypt changes safely: {ex.Message}");
        }

        Console.ResetColor();
        Console.ReadKey();
    }
}