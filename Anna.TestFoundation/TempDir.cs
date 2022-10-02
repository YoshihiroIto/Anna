﻿namespace Anna.TestFoundation;

public class TempDir : IDisposable
{
    public string AppConfigFilePath => $"{RootPath}/AppConfig.json";
    public string LogFilePath => Path.Combine(RootPath, "logs", "log" + DateTime.Today.ToString("yyyyMMdd") + ".txt");

    public string[] ReadLogLines() => File.ReadAllLines(LogFilePath);

    public TempDir()
    {
        RootPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(RootPath);
        DirectoryInfo = new DirectoryInfo(RootPath);
    }

    public string RootPath { get; }

    public DirectoryInfo DirectoryInfo { get; }

    public TempDir CreateDirectory(string path)
    {
        Directory.CreateDirectory(Path.Combine(RootPath, path));
        return this;
    }

    public TempDir CreateFolders(params string[] paths)
    {
        foreach (var path in paths)
        {
            var fullPath = Path.Combine(RootPath, path);
            Directory.CreateDirectory(fullPath);
        }

        return this;
    }

    public TempDir CreateFile(string path)
    {
        File.WriteAllText(Path.Combine(RootPath, path), "temp");
        return this;
    }

    public TempDir CreateFiles(params string[] fileRelativePaths)
    {
        foreach (var path in fileRelativePaths)
        {
            var fullPath = Path.Combine(RootPath, path);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? throw new NullReferenceException());

            File.WriteAllText(
                fullPath,
                string.Format("Automatically generated for testing on {0:yyyy}/{0:MM}/{0:dd} {0:hh}:{0:mm}:{0:ss}",
                    DateTime.UtcNow));
        }

        return this;
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(RootPath, true);
        }
        catch
        {
            // ignored
        }
    }
}