using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;

namespace WizardUpdateSpell
{
    public static class UpdateSpell
    {
        // Entry point for the wizard to call
        public static void Start()
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (!IsRunningAsAdmin())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("🧙‍♂️ The Wizard senses you are but a mortal without elevated powers...");
                Console.WriteLine("   This spell requires ADMINISTRATOR privileges.");
                Console.WriteLine("   Right-click the Wizard and choose 'Run as administrator'.");
                Console.ResetColor();
                Pause();
                return;
            }

            ShowUpdateMenu();
        }

        // -----------------------------
        // ADMIN CHECK
        // -----------------------------
        static bool IsRunningAsAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        // -----------------------------
        // MAIN MENU
        // -----------------------------
        static void ShowUpdateMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("🧙‍♂️  THE WIZARD OF UPDATES");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("[1] Consult Windows Update scrolls");
                Console.WriteLine("[2] Consult Application scrolls (Winget)");
                Console.WriteLine("[3] Consult BOTH realms");
                Console.WriteLine("[4] Exorcise & Revive the Windows Update Service");
                Console.WriteLine("[5] Begone (return to the chamber)");
                Console.WriteLine();
                Console.Write("Speak your choice, mortal: ");

                string choice = Console.ReadLine()?.Trim() ?? "";

                switch (choice)
                {
                    case "1":
                        RunWindowsUpdateFlow();
                        break;
                    case "2":
                        RunAppUpdateFlow();
                        break;
                    case "3":
                        RunWindowsUpdateFlow();
                        RunAppUpdateFlow();
                        break;
                    case "4":
                        ManageUpdateServiceFlow();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("The Wizard does not understand that incantation...");
                        Pause();
                        break;
                }
            }
        }

        // -----------------------------
        // INTERNET CHECK
        // -----------------------------
        static bool HasInternet()
        {
            string result = RunCommand("ping", "8.8.8.8 -n 1", true);
            return result.Contains("TTL=");
        }

        // -----------------------------
        // WINGET CHECK
        // -----------------------------
        static bool WingetAvailable()
        {
            string result = RunCommand("winget", "--version", true);
            return !string.IsNullOrWhiteSpace(result) && result.Contains(".");
        }

        // -----------------------------
        // WINDOWS UPDATE FLOW
        // -----------------------------
        static void RunWindowsUpdateFlow()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🧙‍♂️ The Wizard peers into the Windows Update realm...");
            Console.ResetColor();
            Console.WriteLine();

            if (!HasInternet())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("🌩 The Wizard senses no connection to the outside realms.");
                Console.WriteLine("   Updates cannot be summoned without the internet.");
                Console.ResetColor();
                Pause();
                return;
            }

            Console.WriteLine("🔍 Invoking the Windows Update spirits (scan)...");
            RunCommand("cmd.exe", "/c UsoClient StartScan", false);

            Console.WriteLine("🔍 Asking Windows to download and prepare updates...");
            RunCommand("cmd.exe", "/c UsoClient StartDownload", false);

            Console.WriteLine("🔍 Requesting installation of available updates...");
            RunCommand("cmd.exe", "/c UsoClient StartInstall", false);

            Console.WriteLine();
            Console.WriteLine("✨ The ritual has been invoked. Windows will continue installations in the background.");
            Pause();
        }

        // -----------------------------
        // APPLICATION UPDATE FLOW
        // -----------------------------
        static void RunAppUpdateFlow()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("🧙‍♂️ The Wizard consults the Tome of Application Scrolls (Winget)...");
            Console.ResetColor();
            Console.WriteLine();

            if (!WingetAvailable())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("📕 The Wizard cannot find the Winget spellbook on this device.");
                Console.WriteLine("   Install or update 'App Installer' from the Microsoft Store.");
                Console.ResetColor();
                Pause();
                return;
            }

            if (!HasInternet())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("🌩 The Wizard senses no connection to the outside realms.");
                Console.WriteLine("   Application scrolls cannot be checked without the internet.");
                Console.ResetColor();
                Pause();
                return;
            }

            Console.WriteLine("🔍 Searching for outdated scrolls...");
            string output = RunCommand("winget", "upgrade", true);

            if (string.IsNullOrWhiteSpace(output) || !output.Contains("|"))
            {
                Console.WriteLine("✨ The Wizard found no scrolls requiring renewal.");
                Pause();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("📜 Winget reports the following outdated scrolls:");
            Console.WriteLine(output);

            Console.WriteLine();
            Console.Write("Would you like the Wizard to renew ALL scrolls? (y/n): ");
            string answer = Console.ReadLine()?.Trim().ToLower() ?? "n";

            if (answer == "y" || answer == "yes")
            {
                Console.WriteLine();
                Console.WriteLine("✨ The Wizard begins renewing all scrolls...");
                RunCommand("winget", "upgrade --all --accept-source-agreements --accept-package-agreements", false);
                Console.WriteLine("✅ The ritual is complete.");
            }
            else
            {
                Console.WriteLine("The Wizard stays his hand. No scrolls were renewed.");
            }

            Pause();
        }

        // -----------------------------
        // WINDOWS UPDATE SERVICE FLOW
        // -----------------------------
        static void ManageUpdateServiceFlow()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("🧙‍♂️ The Wizard tests the pulse of the Windows Update Service (wuauserv)...");
            Console.ResetColor();
            Console.WriteLine();

            string statusOutput = RunCommand("sc", "query wuauserv", true);

            if (statusOutput.Contains("RUNNING"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("🟢 The Update Service pulse is strong: RUNNING.");
                Console.ResetColor();
            }
            else if (statusOutput.Contains("STOPPED"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("🔴 The Update Service is dormant: STOPPED.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("🟡 The Update Service is fluctuating or elusive.");
                Console.ResetColor();
                Console.WriteLine(statusOutput);
            }

            Console.WriteLine();
            Console.Write("Do you wish to force a full banishment and revival (RESTART) of this service? (y/n): ");
            string answer = Console.ReadLine()?.Trim().ToLower() ?? "n";

            if (answer == "y" || answer == "yes")
            {
                Console.WriteLine();
                Console.WriteLine("⚡ Banishing the service (Stopping wuauserv)...");
                RunCommand("sc", "stop wuauserv", false);

                System.Threading.Thread.Sleep(1500);

                Console.WriteLine("⚡ Breathing life anew into the service (Starting wuauserv)...");
                RunCommand("sc", "start wuauserv", false);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✨ The Update Service magic has been refreshed!");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("The Wizard leaves the service to its current state.");
            }

            Pause();
        }

        // -----------------------------
        // COMMAND RUNNER
        // -----------------------------
        static string RunCommand(string fileName, string arguments, bool captureOutput)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = !captureOutput,
                    RedirectStandardOutput = captureOutput,
                    RedirectStandardError = captureOutput,
                    CreateNoWindow = captureOutput
                };

                using (Process proc = new Process { StartInfo = psi })
                {
                    proc.Start();

                    if (captureOutput)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(proc.StandardOutput.ReadToEnd());
                        sb.Append(proc.StandardError.ReadToEnd());
                        proc.WaitForExit();
                        return sb.ToString();
                    }
                    else
                    {
                        proc.WaitForExit();
                        return "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("💥 The Wizard encountered an error:");
                Console.ResetColor();
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        // -----------------------------
        // PAUSE
        // -----------------------------
        static void Pause()
        {
            Console.WriteLine();
            Console.Write("Press any key to return to the Wizard's menu...");
            Console.ReadKey(true);
        }
    }
}
