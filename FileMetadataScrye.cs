using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DocumentFormat.OpenXml.Packaging;          
using MetadataExtractor;                         
using TagLib;                                    
using UglyToad.PdfPig;

public static class FileMetadataScryer
{
    // ============================================================
    //  START METHOD — OPENS ITS OWN SCREEN LIKE ALL OTHER SPELLS
    // ============================================================
    public static void Start()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=====================================================");
        Console.WriteLine("🔍 FILE METADATA SCRYER — REVEALING HIDDEN SECRETS");
        Console.WriteLine("=====================================================");
        Console.ResetColor();

        Console.Write("\nEnter the full path to the file you wish to inspect: ");
        string? filePath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n❌ That file does not exist in this realm.");
            Console.ResetColor();
            Console.WriteLine("\nPress any key to return to the Wizard...");
            Console.ReadKey();
            return;
        }

        var results = Scry(filePath);

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("=====================================================");
        Console.WriteLine("📜 METADATA RESULTS");
        Console.WriteLine("=====================================================");
        Console.ResetColor();

        foreach (var kv in results)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{kv.Key}: ");
            Console.ResetColor();
            Console.WriteLine(kv.Value);
        }

        Console.WriteLine("\n-----------------------------------------------------");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("✨ Press any key to return to the Wizard...");
        Console.ResetColor();
        Console.ReadKey();
    }

    // ============================================================
    //  MAIN SCRY ENGINE
    // ============================================================
    public static Dictionary<string, string> Scry(string path)
    {
        var ext = Path.GetExtension(path).ToLower();
        var data = new Dictionary<string, string>();

        AddBasicFileInfo(path, data);

        switch (ext)
        {
            case ".exe":
            case ".dll":
                AddExeMetadata(path, data);
                break;

            case ".msi":
                AddMsiMetadata(path, data);
                break;

            case ".docx":
            case ".xlsx":
            case ".pptx":
                AddOfficeMetadata(path, data);
                break;

            case ".pdf":
                AddPdfMetadata(path, data);
                break;

            case ".jpg":
            case ".jpeg":
            case ".png":
            case ".tiff":
                AddImageMetadata(path, data);
                break;

            case ".mp3":
            case ".wav":
            case ".flac":
                AddAudioMetadata(path, data);
                break;

            case ".mp4":
            case ".mkv":
            case ".avi":
                AddVideoMetadata(path, data);
                break;

            default:
                data["Note"] = "No specialised metadata reader for this file type.";
                break;
        }

        return data;
    }

    // ============================================================
    //  BASIC FILE INFO
    // ============================================================
    private static void AddBasicFileInfo(string path, Dictionary<string, string> data)
    {
        var info = new FileInfo(path);

        data["File Name"] = info.Name;
        data["Extension"] = info.Extension;
        data["Size"] = $"{info.Length / 1024.0:F2} KB";
        data["Created"] = info.CreationTime.ToString();
        data["Modified"] = info.LastWriteTime.ToString();
        data["Accessed"] = info.LastAccessTime.ToString();
        data["Attributes"] = info.Attributes.ToString();
    }

    // ============================================================
    //  EXE / DLL METADATA
    // ============================================================
    private static void AddExeMetadata(string path, Dictionary<string, string> data)
    {
        var info = FileVersionInfo.GetVersionInfo(path);

        data["File Version"] = info.FileVersion;
        data["Product Version"] = info.ProductVersion;
        data["Company"] = info.CompanyName;
        data["Description"] = info.FileDescription;
        data["Original Filename"] = info.OriginalFilename;
        data["Internal Name"] = info.InternalName;
        data["Language"] = info.Language;
    }

    // ============================================================
    //  MSI METADATA (NO WIX REQUIRED)
    // ============================================================
    private static void AddMsiMetadata(string path, Dictionary<string, string> data)
    {
        try
        {
            Type installerType = Type.GetTypeFromProgID("WindowsInstaller.Installer");
            dynamic installer = Activator.CreateInstance(installerType);

            dynamic database = installer.OpenDatabase(path, 0);
            dynamic summary = database.SummaryInformation(0);

            data["Title"] = summary.Property[2];
            data["Subject"] = summary.Property[3];
            data["Author"] = summary.Property[4];
            data["Keywords"] = summary.Property[5];
            data["Comments"] = summary.Property[6];
            data["Created"] = summary.Property[12];
            data["Last Saved"] = summary.Property[13];

            dynamic view = database.OpenView("SELECT Property, Value FROM Property");
            view.Execute();

            dynamic record;
            while ((record = view.Fetch()) != null)
            {
                string key = record.StringData[1];
                string value = record.StringData[2];
                data[$"MSI Property: {key}"] = value;
            }

            try
            {
                var proc = new Process();
                proc.StartInfo.FileName = "msiexec";
                proc.StartInfo.Arguments = $"/i \"{path}\" /?";
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd();
                data["MSI Help Output"] = output;
            }
            catch
            {
                data["MSI Help Output"] = "Unable to capture msiexec help text.";
            }
        }
        catch (Exception ex)
        {
            data["MSI Error"] = "Unable to read MSI metadata: " + ex.Message;
        }
    }

    // ============================================================
    //  OFFICE METADATA
    // ============================================================
    private static void AddOfficeMetadata(string path, Dictionary<string, string> data)
    {
        try
        {
            using (var doc = WordprocessingDocument.Open(path, false))
            {
                var props = doc.PackageProperties;

                data["Title"] = props.Title;
                data["Author"] = props.Creator;
                data["Last Modified By"] = props.LastModifiedBy;
                data["Created"] = props.Created?.ToString();
                data["Modified"] = props.Modified?.ToString();
                data["Description"] = props.Description;
                data["Keywords"] = props.Keywords;
            }
        }
        catch
        {
            data["Office Error"] = "Unable to read Office metadata.";
        }
    }

    // ============================================================
    //  PDF METADATA
    // ============================================================
    private static void AddPdfMetadata(string path, Dictionary<string, string> data)
    {
        try
        {
            var pdf = PdfDocument.Open(path);
            var info = pdf.Information;

            data["Title"] = info.Title;
            data["Author"] = info.Author;
            data["Creator"] = info.Creator;
            data["Producer"] = info.Producer;
            data["Creation Date"] = info.CreationDate?.ToString();
            data["Pages"] = pdf.NumberOfPages.ToString();
        }
        catch
        {
            data["PDF Error"] = "Unable to read PDF metadata.";
        }
    }

    // ============================================================
    //  IMAGE METADATA
    // ============================================================
    private static void AddImageMetadata(string path, Dictionary<string, string> data)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(path);

            foreach (var dir in directories)
            {
                foreach (var tag in dir.Tags)
                {
                    data[$"{dir.Name}: {tag.Name}"] = tag.Description;
                }
            }
        }
        catch
        {
            data["Image Error"] = "Unable to read image metadata.";
        }
    }

    // ============================================================
    //  AUDIO METADATA
    // ============================================================
    private static void AddAudioMetadata(string path, Dictionary<string, string> data)
    {
        try
        {
            var file = TagLib.File.Create(path);

            data["Title"] = file.Tag.Title;
            data["Artist"] = string.Join(", ", file.Tag.Performers);
            data["Album"] = file.Tag.Album;
            data["Year"] = file.Tag.Year.ToString();
            data["Genre"] = string.Join(", ", file.Tag.Genres);
            data["Duration"] = file.Properties.Duration.ToString();
            data["Bitrate"] = file.Properties.AudioBitrate.ToString();
        }
        catch
        {
            data["Audio Error"] = "Unable to read audio metadata.";
        }
    }

    // ============================================================
    //  VIDEO METADATA
    // ============================================================
    private static void AddVideoMetadata(string path, Dictionary<string, string> data)
    {
        try
        {
            var file = TagLib.File.Create(path);

            data["Duration"] = file.Properties.Duration.ToString();

            if (file.Properties.VideoWidth > 0 && file.Properties.VideoHeight > 0)
                data["Resolution"] = $"{file.Properties.VideoWidth}x{file.Properties.VideoHeight}";
            else
                data["Resolution"] = "Unknown";

            data["Audio Sample Rate"] = file.Properties.AudioSampleRate.ToString();
            data["Audio Channels"] = file.Properties.AudioChannels.ToString();
        }
        catch
        {
            data["Video Error"] = "Unable to read video metadata.";
        }
    }
}
