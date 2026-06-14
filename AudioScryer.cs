using System;
using System.Diagnostics;
using NAudio.CoreAudioApi;

public static class AudioScryer
{
    private static MMDeviceEnumerator? _enumerator;

    public static void Start()
    {
        EnsureInit();

        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🔊 AUDIO SCRYER — SOUND REALM INSPECTION");
            Console.ResetColor();
            Console.WriteLine("────────────────────────────────────────────");
            Console.WriteLine("1. Show default output device and master volume");
            Console.WriteLine("2. Real-time output levels (speakers/headphones)");
            Console.WriteLine("3. Real-time microphone levels (input)");
            Console.WriteLine("4. Combined levels (output + input)");
            Console.WriteLine("5. List microphone sessions (apps using the mic)");
            Console.WriteLine("6. Show which app is using the mic right now");
            Console.WriteLine("7. List active audio sessions (output apps)");
            Console.WriteLine("8. Change per-app volume");
            Console.WriteLine("9. Mute/unmute an app");
            Console.WriteLine("10. Change master volume");
            Console.WriteLine("11. Return to the wizard chamber");
            Console.WriteLine("────────────────────────────────────────────");
            Console.Write("Choose your incantation: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1": ShowDefaultDeviceInfo(); break;
                case "2": ShowRealtimeOutputLevels(); break;
                case "3": ShowRealtimeMicLevels(); break;
                case "4": ShowRealtimeCombinedLevels(); break;
                case "5": ListMicSessions(); break;
                case "6": ShowMicUsageNow(); break;
                case "7": ListAudioSessions(); break;
                case "8": ChangePerAppVolume(); break;
                case "9": ToggleAppMute(); break;
                case "10": ChangeMasterVolume(); break;
                case "11": return;
                default: break;
            }
        }
    }

    private static void EnsureInit()
    {
        if (_enumerator == null)
            _enumerator = new MMDeviceEnumerator();
    }

    private static MMDevice? GetDefaultRenderDevice()
    {
        try
        {
            EnsureInit();
            return _enumerator!.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }
        catch { return null; }
    }

    private static MMDevice? GetDefaultCaptureDevice()
    {
        try
        {
            EnsureInit();
            return _enumerator!.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
        }
        catch { return null; }
    }

    // ------------------------------------------------------------
    // 1. Default device info
    // ------------------------------------------------------------

    private static void ShowDefaultDeviceInfo()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🔊 Default Output Device");
        Console.ResetColor();
        Console.WriteLine();

        var device = GetDefaultRenderDevice();
        if (device == null)
        {
            Console.WriteLine("No default audio device could be found.");
            Pause();
            return;
        }

        float vol = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100f;
        bool muted = device.AudioEndpointVolume.Mute;

        Console.WriteLine($"Name      : {device.FriendlyName}");
        Console.WriteLine($"Volume    : {vol:F0}%");
        Console.WriteLine($"Muted     : {(muted ? "Yes" : "No")}");
        Console.WriteLine($"State     : {device.State}");
        Console.WriteLine();

        Pause();
    }

    // ------------------------------------------------------------
    // 2. Real-time OUTPUT levels
    // ------------------------------------------------------------

    private static void ShowRealtimeOutputLevels()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("📈 Real-time Output Levels");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Press Q to return to the wizard chamber.");
        Console.WriteLine();

        var device = GetDefaultRenderDevice();
        if (device == null)
        {
            Console.WriteLine("No output device could be found.");
            Pause();
            return;
        }

        Console.CursorVisible = false;
        try
        {
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                    break;

                float peak = device.AudioMeterInformation.MasterPeakValue;
                var peaks = device.AudioMeterInformation.PeakValues;

                Console.SetCursorPosition(0, 5);
                DrawBar("Out L", peaks.Count > 0 ? peaks[0] : peak);
                DrawBar("Out R", peaks.Count > 1 ? peaks[1] : peak);
                DrawBar("Master", peak);

                System.Threading.Thread.Sleep(80);
            }
        }
        finally { Console.CursorVisible = true; }
    }

    // ------------------------------------------------------------
    // 3. Real-time MICROPHONE levels
    // ------------------------------------------------------------

    private static void ShowRealtimeMicLevels()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("🎤 Real-time Microphone Levels");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Press Q to return to the wizard chamber.");
        Console.WriteLine();

        var mic = GetDefaultCaptureDevice();
        if (mic == null)
        {
            Console.WriteLine("No microphone device could be found.");
            Pause();
            return;
        }

        Console.CursorVisible = false;
        try
        {
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                    break;

                float peak = mic.AudioMeterInformation.MasterPeakValue;
                var peaks = mic.AudioMeterInformation.PeakValues;

                Console.SetCursorPosition(0, 5);
                DrawBar("Mic L", peaks.Count > 0 ? peaks[0] : peak);
                DrawBar("Mic R", peaks.Count > 1 ? peaks[1] : peak);
                DrawBar("Master", peak);

                System.Threading.Thread.Sleep(80);
            }
        }
        finally { Console.CursorVisible = true; }
    }

    // ------------------------------------------------------------
    // 4. Combined OUTPUT + INPUT levels
    // ------------------------------------------------------------

    private static void ShowRealtimeCombinedLevels()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("🔊🎤 Combined Audio Levels (Output + Input)");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Press Q to return to the wizard chamber.");
        Console.WriteLine();

        var output = GetDefaultRenderDevice();
        var mic = GetDefaultCaptureDevice();

        if (output == null && mic == null)
        {
            Console.WriteLine("No audio devices could be found.");
            Pause();
            return;
        }

        Console.CursorVisible = false;
        try
        {
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                    break;

                Console.SetCursorPosition(0, 5);

                if (output != null)
                {
                    float oPeak = output.AudioMeterInformation.MasterPeakValue;
                    var oPeaks = output.AudioMeterInformation.PeakValues;
                    DrawBar("Out L", oPeaks.Count > 0 ? oPeaks[0] : oPeak);
                    DrawBar("Out R", oPeaks.Count > 1 ? oPeaks[1] : oPeak);
                }
                else
                {
                    Console.WriteLine("Out L : (no device)");
                    Console.WriteLine("Out R : (no device)");
                }

                Console.WriteLine();

                if (mic != null)
                {
                    float mPeak = mic.AudioMeterInformation.MasterPeakValue;
                    var mPeaks = mic.AudioMeterInformation.PeakValues;
                    DrawBar("Mic L", mPeaks.Count > 0 ? mPeaks[0] : mPeak);
                    DrawBar("Mic R", mPeaks.Count > 1 ? mPeaks[1] : mPeak);
                }
                else
                {
                    Console.WriteLine("Mic L : (no device)");
                    Console.WriteLine("Mic R : (no device)");
                }

                System.Threading.Thread.Sleep(80);
            }
        }
        finally { Console.CursorVisible = true; }
    }

    private static void DrawBar(string label, float value)
    {
        int width = 40;
        int filled = (int)(value * width);
        if (filled < 0) filled = 0;
        if (filled > width) filled = width;

        string bar = new string('█', filled) + new string('░', width - filled);
        Console.WriteLine($"{label,-6}: {bar}  {value * 100:F0}%   ");
    }

    // ------------------------------------------------------------
    // 5. Microphone Session Viewer
    // ------------------------------------------------------------

    private static void ListMicSessions()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("🎤 Active Microphone Sessions");
        Console.ResetColor();
        Console.WriteLine();

        var mic = GetDefaultCaptureDevice();
        if (mic == null)
        {
            Console.WriteLine("No microphone device could be found.");
            Pause();
            return;
        }

        var sessions = mic.AudioSessionManager.Sessions;
        if (sessions.Count == 0)
        {
            Console.WriteLine("No applications are currently using the microphone.");
            Pause();
            return;
        }

        for (int i = 0; i < sessions.Count; i++)
        {
            var s = sessions[i];
            string name = SafeSessionName(s);

            float peak = 0f;
            try { peak = s.AudioMeterInformation.MasterPeakValue * 100f; } catch { }

            Console.WriteLine($"{i + 1}. {name}");
            Console.WriteLine($"    Peak Level: {peak:F0}%");
        }

        Console.WriteLine();
        Pause();
    }

    // ------------------------------------------------------------
    // 6. Show which app is using the mic RIGHT NOW
    // ------------------------------------------------------------

    private static void ShowMicUsageNow()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🎤 Who Is Using The Microphone Right Now?");
        Console.ResetColor();
        Console.WriteLine();

        var mic = GetDefaultCaptureDevice();
        if (mic == null)
        {
            Console.WriteLine("No microphone device could be found.");
            Pause();
            return;
        }

        var sessions = mic.AudioSessionManager.Sessions;
        if (sessions.Count == 0)
        {
            Console.WriteLine("No application is currently using the microphone.");
            Pause();
            return;
        }

        Console.WriteLine("The following applications are using the microphone:");
        Console.WriteLine();

        for (int i = 0; i < sessions.Count; i++)
        {
            var s = sessions[i];
            string name = SafeSessionName(s);
            Console.WriteLine($"• {name}");
        }

        Console.WriteLine();
        Pause();
    }

    // ------------------------------------------------------------
    // 7. Output audio sessions
    // ------------------------------------------------------------

    private static void ListAudioSessions()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("🎧 Active Output Audio Sessions");
        Console.ResetColor();
        Console.WriteLine();

        var device = GetDefaultRenderDevice();
        if (device == null)
        {
            Console.WriteLine("No default audio device could be found.");
            Pause();
            return;
        }

        var sessions = device.AudioSessionManager.Sessions;
        if (sessions.Count == 0)
        {
            Console.WriteLine("No active audio sessions were found.");
            Pause();
            return;
        }

        for (int i = 0; i < sessions.Count; i++)
        {
            var s = sessions[i];
            string id = SafeSessionName(s);
            float vol = s.SimpleAudioVolume.Volume * 100f;
            bool mute = s.SimpleAudioVolume.Mute;
            float peak = 0f;
            try { peak = s.AudioMeterInformation.MasterPeakValue * 100f; } catch { }

            Console.WriteLine($"{i + 1}. {id}");
            Console.WriteLine($"    Volume: {vol:F0}%   Muted: {(mute ? "Yes" : "No")}   Peak: {peak:F0}%");
        }

        Console.WriteLine();
        Pause();
    }

    private static string SafeSessionName(AudioSessionControl s)
    {
        try
        {
            if (s.IsSystemSoundsSession)
                return "System Sounds (Windows)";

            string dn = s.DisplayName;
            if (!string.IsNullOrWhiteSpace(dn))
                return dn;

            uint pid = s.GetProcessID;
            if (pid != 0)
            {
                using var proc = Process.GetProcessById((int)pid);
                return $"{proc.ProcessName}.exe";
            }
        }
        catch { }

        return "(Unnamed Application Session)";
    }

    // ------------------------------------------------------------
    // 8. Change per-app volume
    // ------------------------------------------------------------

    private static void ChangePerAppVolume()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🎚 Change Per-App Volume");
        Console.ResetColor();
        Console.WriteLine();

        var device = GetDefaultRenderDevice();
        if (device == null)
        {
            Console.WriteLine("No default audio device could be found.");
            Pause();
            return;
        }

        var sessions = device.AudioSessionManager.Sessions;
        if (sessions.Count == 0)
        {
            Console.WriteLine("No active audio sessions were found.");
            Pause();
            return;
        }

        for (int i = 0; i < sessions.Count; i++)
            Console.WriteLine($"{i + 1}. {SafeSessionName(sessions[i])}");

        Console.WriteLine();
        Console.Write("Choose a session to adjust (or press Enter to cancel): ");
        var choice = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(choice)) return;

        if (!int.TryParse(choice, out int index) || index < 1 || index > sessions.Count)
        {
            Console.WriteLine("The chosen session does not exist.");
            Pause();
            return;
        }

        var session = sessions[index - 1];

        Console.Write("Enter new volume (0–100): ");
        var volText = Console.ReadLine()?.Trim();
        if (!int.TryParse(volText, out int volPercent) || volPercent < 0 || volPercent > 100)
        {
            Console.WriteLine("The volume incantation is invalid.");
            Pause();
            return;
        }

        try
        {
            session.SimpleAudioVolume.Volume = volPercent / 100f;
            Console.WriteLine($"Volume set to {volPercent}% for {SafeSessionName(session)}.");
        }
        catch
        {
            Console.WriteLine("The spell failed to bind to that session.");
        }

        Pause();
    }

    // ------------------------------------------------------------
    // 9. Mute/unmute an app
    // ------------------------------------------------------------

    private static void ToggleAppMute()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🔇 Mute / Unmute App");
        Console.ResetColor();
        Console.WriteLine();

        var device = GetDefaultRenderDevice();
        if (device == null)
        {
            Console.WriteLine("No default audio device could be found.");
            Pause();
            return;
        }

        var sessions = device.AudioSessionManager.Sessions;
        if (sessions.Count == 0)
        {
            Console.WriteLine("No active audio sessions were found.");
            Pause();
            return;
        }

        for (int i = 0; i < sessions.Count; i++)
            Console.WriteLine($"{i + 1}. {SafeSessionName(sessions[i])} (Muted: {(sessions[i].SimpleAudioVolume.Mute ? "Yes" : "No")})");

        Console.WriteLine();
        Console.Write("Choose a session to toggle mute (or press Enter to cancel): ");
        var choice = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(choice)) return;

        if (!int.TryParse(choice, out int index) || index < 1 || index > sessions.Count)
        {
            Console.WriteLine("The chosen session does not exist.");
            Pause();
            return;
        }

        var session = sessions[index - 1];

        try
        {
            session.SimpleAudioVolume.Mute = !session.SimpleAudioVolume.Mute;
            Console.WriteLine($"Mute state for {SafeSessionName(session)} is now: {(session.SimpleAudioVolume.Mute ? "Muted" : "Unmuted")}");
        }
        catch
        {
            Console.WriteLine("The mute incantation fizzled.");
        }

        Pause();
    }

    // ------------------------------------------------------------
    // 10. Change master volume
    // ------------------------------------------------------------

    private static void ChangeMasterVolume()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🎚 Change Master Volume");
        Console.ResetColor();
        Console.WriteLine();

        var device = GetDefaultRenderDevice();
        if (device == null)
        {
            Console.WriteLine("No default audio device could be found.");
            Pause();
            return;
        }

        float current = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100f;
        Console.WriteLine($"Current master volume: {current:F0}%");
        Console.Write("Enter new volume (0–100): ");

        var text = Console.ReadLine()?.Trim();
        if (!int.TryParse(text, out int volPercent) || volPercent < 0 || volPercent > 100)
        {
            Console.WriteLine("The volume incantation is invalid.");
            Pause();
            return;
        }

        try
        {
            device.AudioEndpointVolume.MasterVolumeLevelScalar = volPercent / 100f;
            Console.WriteLine($"Master volume set to {volPercent}%.");
        }
        catch
        {
            Console.WriteLine("The spell failed to bind to the master channel.");
        }

        Pause();
    }

    // ------------------------------------------------------------
    // UTIL
    // ------------------------------------------------------------

    private static void Pause()
    {
        Console.WriteLine();
        Console.Write("Press any key to return...");
        Console.ReadKey(true);
    }
}
