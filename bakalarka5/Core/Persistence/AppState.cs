using System;
using System.IO;

namespace bakalarka5.Core.Persistence;


public static class AppState
{
    private static readonly string Folder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bakalarka5");

    private static readonly string FilePath =
        Path.Combine(Folder, "last.txt");

    public static void SaveLastFile(string path)
    {
        Directory.CreateDirectory(Folder);
        File.WriteAllText(FilePath, path);
    }

    public static string? LoadLastFile()
    {
        return !File.Exists(FilePath) ? null : File.ReadAllText(FilePath);
    }
}