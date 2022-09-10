namespace Anna.DomainModel.FileSystem;

public class FileSystemDirectory : Directory
{
    public FileSystemDirectory(string path)
        : base(path)
    {
    }

    protected override void Update()
    {
        Directories.Clear();
        Files.Clear();
        Entries.Clear();

        foreach (var p in System.IO.Directory.EnumerateDirectories(Path))
        {
            var e = new Entry { Name = System.IO.Path.GetRelativePath(Path, p) };
            Directories.Add(e);
            Entries.Add(e);
        }

        foreach (var p in System.IO.Directory.EnumerateFiles(Path))
        {
            var e = new Entry { Name = System.IO.Path.GetRelativePath(Path, p) };
            Files.Add(e);
            Entries.Add(e);
        }
    }
}