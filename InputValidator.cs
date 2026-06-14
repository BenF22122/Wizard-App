using System;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Utility class for safe user input validation and sanitization
/// </summary>
public static class InputValidator
{
    /// <summary>
    /// Safely reads a line from console with length limit and error handling
    /// </summary>
    public static string? SafeReadLine(int maxLength = 256)
    {
        try
        {
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return null;

            return input.Length > maxLength
                ? input.Substring(0, maxLength)
                : input;
        }
        catch (Exception ex)
        {
            WizardLogger.LogWarning("InputValidator", $"Failed to read input: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Validates if a string is a valid file path
    /// </summary>
    public static bool IsValidPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            var fullPath = Path.GetFullPath(path);
            var invalidChars = Path.GetInvalidPathChars();
            return !invalidChars.Any(fullPath.Contains);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if a string is a valid file name
    /// </summary>
    public static bool IsValidFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        try
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return !invalidChars.Any(fileName.Contains) && !fileName.StartsWith(".");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Sanitizes a file name by removing invalid characters
    /// </summary>
    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "document";

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new StringBuilder();

        foreach (char c in fileName)
        {
            if (!invalidChars.Contains(c))
                sanitized.Append(c);
        }

        return sanitized.ToString().TrimEnd('.') is string result && !string.IsNullOrWhiteSpace(result)
            ? result
            : "document";
    }

    /// <summary>
    /// Safely parses an integer with bounds checking
    /// </summary>
    public static bool TryParseInt(string? input, out int value, int minValue = int.MinValue, int maxValue = int.MaxValue)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        if (!int.TryParse(input, out int parsed))
            return false;

        if (parsed < minValue || parsed > maxValue)
            return false;

        value = parsed;
        return true;
    }

    /// <summary>
    /// Validates if string is a valid IP address
    /// </summary>
    public static bool IsValidIpAddress(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return false;

        return System.Net.IPAddress.TryParse(ip, out _);
    }

    /// <summary>
    /// Validates if string is a valid hostname
    /// </summary>
    public static bool IsValidHostname(string? hostname)
    {
        if (string.IsNullOrWhiteSpace(hostname) || hostname.Length > 253)
            return false;

        var parts = hostname.Split('.');
        return parts.All(p => p.Length > 0 && p.Length <= 63 && char.IsLetterOrDigit(p[0]));
    }

    /// <summary>
    /// Validates if string is a valid port number
    /// </summary>
    public static bool IsValidPort(string? port)
    {
        return TryParseInt(port, out int p, 1, 65535);
    }

    /// <summary>
    /// Safely confirms a destructive action with user
    /// </summary>
    public static bool ConfirmDestructiveAction(string action)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n⚠️ WARNING: {action}");
        Console.WriteLine("This action cannot be easily undone.");
        Console.ResetColor();
        Console.Write("Are you absolutely certain? (yes/no): ");

        string? response = SafeReadLine();
        return response?.Equals("yes", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
