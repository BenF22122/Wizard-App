/// <summary>
/// Centralized configuration for the Wizard System Scryer
/// </summary>
public static class WizardConfig
{
    // Logging Configuration
    public const bool EnableDetailedLogging = true;
    public const string LogDirectory = "WizardLogs";
    public const int MaxLogFileSizeKb = 10240; // 10 MB

    // Network Configuration
    public const int DefaultNetworkTimeout = 5000; // 5 seconds
    public const int PingTimeout = 2000; // 2 seconds
    public const int DnsTimeout = 3000; // 3 seconds

    // Performance Configuration
    public const int MaxParallelThreads = 32;
    public const int MinParallelThreads = 4;
    public const int DefaultParallelThreads = 16;

    // Retry Configuration
    public const int MaxRetries = 3;
    public const int RetryDelayMs = 500;

    // Resource Limits
    public const int MaxConcurrentPings = 50;
    public const int MaxOpenConnections = 100;
    public const int PortScanTimeout = 150; // ms per port

    // UI Configuration
    public const int GraphHistorySize = 50;
    public const int MaxConsoleLineLength = 256;
    public const int PauseDelayMs = 400;

    // Data Collection
    public const int EventLogMaxEntries = 1000;
    public const int NetworkHistorySize = 30;
    public const int DiskScanMaxResults = 50;
}
