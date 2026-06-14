using System.IO;

public static class FileTools
{
    public static void CreateEmptyFile(string path)
    {
        File.WriteAllText(path, string.Empty);
    }

    public static bool FileExists(string path)
    {
        return File.Exists(path);
    }
}
