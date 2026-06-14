using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using Microsoft.Win32;

namespace WizService
{
    public class ServiceMatchDto
    {
        public string ServiceName { get; set; }
        public string DisplayName { get; set; }
        public ServiceControllerStatus Status { get; set; }
    }

    public static class WizService
    {
        enum ServiceAction { Start, Stop, Restart }

        // Entry point called from Program.cs
        public static void Start()
        {
            Console.Title = "Service Scryer - Wizard Module";
            ShowMainMenu();
        }

        static void ShowMainMenu()
        {
            while (true)
            {
                Console.Clear(); 
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("========================================");
                Console.WriteLine("       WIZARD SERVICE SCRYER CLOAK      ");
                Console.WriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine("\n[1] Start Service Scryer");
                Console.WriteLine("[2] Exit Wizard");
                Console.WriteLine("\n----------------------------------------");
                Console.Write("Select an option: ");

                var choice = Console.ReadLine()?.Trim();

                if (choice == "1")
                {
                    RunWizardLoop();
                }
                else if (choice == "2" || choice?.ToLower() == "q")
                {
                    Console.Clear();
                    Console.WriteLine("Closing wizard gates. Farewell.");
                    break;
                }
            }
        }

        static void RunWizardLoop()
        {
            while (true)
            {
                Console.Clear(); 
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("=== Service Scryer: Search Screen ===");
                Console.ResetColor();
                Console.WriteLine("Suggested terms: printer, update, network, audio, wifi, security");
                Console.WriteLine("----------------------------------------------------------------");
                // CHANGED: Prompt now indicates 'q' to go back
                Console.Write("\nEnter a search term or 'q' to go back to Main Menu: ");
                
                var query = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(query))
                    continue;

                // CHANGED: Evaluates 'q' or 'quit' to exit back to the main menu loop
                if (query.Equals("q", StringComparison.OrdinalIgnoreCase) ||
                    query.Equals("quit", StringComparison.OrdinalIgnoreCase))
                    break; 

                ExecuteSearchPhase(query);
            }
        }

        static void ExecuteSearchPhase(string query)
        {
            var matches = SearchServices(query);

            if (!matches.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nNo services matched your search.");
                Console.ResetColor();
                Console.WriteLine("Press any key to try another search...");
                Console.ReadKey();
                return;
            }

            while (true) 
            {
                Console.Clear(); 
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"=== Search Results for '{query}' ===");
                Console.ResetColor();
                Console.WriteLine($"Found {matches.Count} service(s):");
                Console.WriteLine("----------------------------------------------------------------");

                for (int i = 0; i < matches.Count; i++)
                {
                    var s = matches[i];
                    Console.WriteLine($"{i + 1}. {s.DisplayName,-40} [{s.ServiceName}] \n   Status: {s.Status}\n");
                }

                Console.WriteLine("----------------------------------------------------------------");
                Console.Write("Select a service number to manage (or press Enter to search again): ");
                var selection = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(selection))
                    return; 

                if (!int.TryParse(selection, out int index) || index < 1 || index > matches.Count)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid selection. Try again.");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(1000);
                    continue;
                }

                ExecuteManagementPhase(matches[index - 1]);
                break; 
            }
        }

        static void ExecuteManagementPhase(ServiceMatchDto selectedService)
        {
            Console.Clear(); 
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== Mana-Management Crucible ===");
            Console.ResetColor();
            Console.WriteLine($"Selected:       {selectedService.DisplayName}");
            Console.WriteLine($"Internal Name:  [{selectedService.ServiceName}]");
            Console.WriteLine($"Current status: {selectedService.Status}");
            Console.WriteLine("----------------------------------------------------------------");
            // CHANGED: UI action indicator swapped to [Q]uit/Back
            Console.WriteLine("Actions: [S]tart    S[t]op    [R]estart    [Q]uit/Back");
            Console.WriteLine("----------------------------------------------------------------");
            Console.Write("Choose an action: ");

            var actionInput = Console.ReadLine()?.Trim().ToLowerInvariant();
            Console.WriteLine();

            // CHANGED: Checks for 'q' or 'quit' to exit out of the service management screen
            if (string.IsNullOrWhiteSpace(actionInput) || actionInput == "q" || actionInput == "quit")
                return;

            if (actionInput == "s" || actionInput == "start")
                PerformServiceAction(selectedService.ServiceName, ServiceAction.Start);
            else if (actionInput == "t" || actionInput == "stop")
                PerformServiceAction(selectedService.ServiceName, ServiceAction.Stop);
            else if (actionInput == "r" || actionInput == "restart")
                PerformServiceAction(selectedService.ServiceName, ServiceAction.Restart);
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid action selected.");
                Console.ResetColor();
                System.Threading.Thread.Sleep(1500);
            }
        }

        static void PerformServiceAction(string serviceName, ServiceAction action)
        {
            if (!IsAdmin())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️ This action requires Administrator rights.");
                Console.WriteLine("Please relaunch the wizard as Administrator to control this service.");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to go back...");
                Console.ReadKey();
                return;
            }

            try
            {
                using (var service = new ServiceController(serviceName))
                {
                    TimeSpan timeout = TimeSpan.FromSeconds(30);

                    switch (action)
                    {
                        case ServiceAction.Start:
                            if (service.Status == ServiceControllerStatus.Running ||
                                service.Status == ServiceControllerStatus.StartPending)
                            {
                                Console.WriteLine("Service is already running or starting.");
                                break;
                            }
                            Console.WriteLine("Casting Start spell on service...");
                            service.Start();
                            service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Service started successfully.");
                            break;

                        case ServiceAction.Stop:
                            if (service.Status == ServiceControllerStatus.Stopped ||
                                service.Status == ServiceControllerStatus.StopPending)
                            {
                                Console.WriteLine("Service is already stopped or stopping.");
                                break;
                            }
                            Console.WriteLine("Casting Stop spell on service...");
                            service.Stop();
                            service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Service stopped successfully.");
                            break;

                        case ServiceAction.Restart:
                            Console.WriteLine("Recasting (Restarting) service...");
                            if (service.Status != ServiceControllerStatus.Stopped &&
                                service.Status != ServiceControllerStatus.StopPending)
                            {
                                service.Stop();
                                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                            }
                            service.Start();
                            service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Service restarted successfully.");
                            break;
                    }
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to perform action on service '{serviceName}'.");
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        static bool IsAdmin()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        static List<ServiceMatchDto> SearchServices(string query)
        {
            var searchTerms = ExpandSearchTerms(query);
            var matchingDtos = new List<ServiceMatchDto>();
            var allServices = ServiceController.GetServices();

            try
            {
                foreach (var s in allServices)
                {
                    string name = s.ServiceName.ToLowerInvariant();
                    string display = s.DisplayName.ToLowerInvariant();
                    string description = GetServiceDescription(s.ServiceName).ToLowerInvariant();

                    if (searchTerms.Any(term =>
                        name.Contains(term) ||
                        display.Contains(term) ||
                        description.Contains(term)))
                    {
                        matchingDtos.Add(new ServiceMatchDto
                        {
                            ServiceName = s.ServiceName,
                            DisplayName = s.DisplayName,
                            Status = s.Status
                        });
                    }

                    s.Dispose();
                }
            }
            catch
            {
                foreach (var s in allServices) { try { s.Dispose(); } catch { } }
                throw;
            }

            return matchingDtos;
        }

        static List<string> ExpandSearchTerms(string query)
        {
            var baseTerm = query.ToLowerInvariant().Trim();
            var terms = new HashSet<string> { baseTerm };

            var map = new Dictionary<string, string[]>
            {
                { "printer", new[] { "print", "spool", "printer", "printworkflow" } },
                { "print",   new[] { "print", "spool", "printer", "printworkflow" } },
                { "update",  new[] { "update", "wu", "orchestrator", "installer", "msiserver", "bits" } },
                { "windows update", new[] { "wuauserv", "windows update", "orchestrator" } },
                { "network", new[] { "network", "dhcp", "dns", "nla", "netlogon", "tcp/ip", "lanman" } },
                { "dhcp",    new[] { "dhcp" } },
                { "dns",     new[] { "dns" } },
                { "wifi",    new[] { "wlan", "wifi", "wireless", "autoconfig" } },
                { "wireless",new[] { "wlan", "wifi", "wireless" } },
                { "audio",   new[] { "audio", "audiosrv", "endpoint", "sound" } },
                { "sound",   new[] { "audio", "sound" } },
                { "security",new[] { "security", "antivirus", "defender", "firewall", "mpssvc" } },
                { "defender",new[] { "defender", "windefend" } },
                { "firewall",new[] { "firewall", "mpssvc" } },
                { "remote",  new[] { "remote", "remoteregistry", "remotedesktop", "termservice" } },
                { "rdp",     new[] { "termservice", "remote desktop" } },
                { "time",    new[] { "time", "w32time" } },
                { "print spooler", new[] { "spooler" } },
                { "backup",  new[] { "backup", "wbengine", "shadow", "vss" } },
                { "shadow",  new[] { "shadow", "vss" } },
                { "sql",     new[] { "sql", "mssql", "sql server" } },
                { "database",new[] { "sql", "database", "mssql" } },
                { "web",     new[] { "iis", "w3svc", "world wide web", "web" } },
                { "iis",     new[] { "iis", "w3svc" } }
            };

            foreach (var kvp in map)
            {
                if (baseTerm.Contains(kvp.Key))
                {
                    foreach (var t in kvp.Value)
                        terms.Add(t.ToLowerInvariant());
                }
            }

            return terms.ToList();
        }

        static string GetServiceDescription(string serviceName)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}"))
                {
                    var val = key?.GetValue("Description")?.ToString();
                    if (string.IsNullOrWhiteSpace(val) || val.StartsWith("@"))
                        return string.Empty;

                    return val;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}