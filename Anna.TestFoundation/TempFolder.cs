namespace Anna.TestFoundation;

public class TempFolder : IDisposable
{
    public string AppConfigFilePath => $"{RootPath}/AppConfig.json";
    public string LogFilePath => Path.Combine(RootPath, "logs", "log" + DateTime.Today.ToString("yyyyMMdd") + ".txt");

    public string[] ReadLogLines() => File.ReadAllLines(LogFilePath);

    private readonly string _workDir;

    public TempFolder(string workDir = "")
    {
        _workDir = workDir;

        RootPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(RootPath);
        DirectoryInfo = new DirectoryInfo(RootPath);
    }

    public string RootPath { get; }

    public DirectoryInfo DirectoryInfo { get; }

    public TempFolder CreateFolder(string path)
    {
        Directory.CreateDirectory(Path.Combine(RootPath, _workDir, path));
        return this;
    }

    public TempFolder CreateDirectories(params string[] paths)
    {
        foreach (var path in paths)
        {
            var fullPath = Path.Combine(RootPath, _workDir, path);
            Directory.CreateDirectory(fullPath);
        }

        return this;
    }

    public TempFolder CreateFile(string path, string text = "temp", FileAttributes attributes = 0)
    {
        var filePath = Path.Combine(RootPath, _workDir, path);

        File.WriteAllText(filePath, text);

        if (attributes != 0)
            File.SetAttributes(filePath, attributes);

        return this;
    }

    public TempFolder CreateFiles(params string[] fileRelativePaths)
    {
        foreach (var path in fileRelativePaths)
        {
            var fullPath = Path.Combine(RootPath, _workDir, path);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? throw new NullReferenceException());

            File.WriteAllText(
                fullPath,
                string.Format("Automatically generated for testing on {0:yyyy}/{0:MM}/{0:dd} {0:hh}:{0:mm}:{0:ss}",
                    DateTime.UtcNow));
        }

        return this;
    }

    public TempFolder CreateWorkFolder()
    {
        Directory.CreateDirectory(Path.Combine(RootPath, _workDir));

        return this;
    }

    public TempFolder DeleteWorkFolder()
    {
        //Directory.Delete(Path.Combine(RootPath, _workDir), true);

        var di = new DirectoryInfo(Path.Combine(RootPath, _workDir));

        foreach (var d in di.EnumerateDirectories())
            Directory.Delete(d.FullName);

        foreach (var f in di.EnumerateFiles())
            File.Delete(f.FullName);

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