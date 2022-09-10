using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Anna.DomainModel.Operator")]

namespace Anna.DomainModel.FileSystem;

public sealed class FileSystemDirectory : Directory
{
    internal FileSystemDirectory(string path)
        : base(path)
    {
    }

    protected override void Update()
    {
        Entries.Clear();

        foreach (var p in System.IO.Directory.EnumerateDirectories(Path))
        {
            Entries.Add(
            new Entry { Name = System.IO.Path.GetRelativePath(Path, p) }
            );
        }

        foreach (var p in System.IO.Directory.EnumerateFiles(Path))
        {
            Entries.Add(
            new Entry { Name = System.IO.Path.GetRelativePath(Path, p) }
            );
        }
    }
}