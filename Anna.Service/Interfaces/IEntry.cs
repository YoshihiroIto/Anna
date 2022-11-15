namespace Anna.Service.Interfaces;

public interface IEntry
{
    string Path { get; }
    long Size { get; }
    bool IsFolder { get; }
    bool IsSelectable { get; }

    public static IEntry Create(string path)
    {
        var fi = new FileInfo(path);

        var isFolder = fi.Exists == false;
        var size = fi.Exists ? fi.Length : 0;

        return new DtEntry(path, isFolder, size, true);
    }
    
    private sealed record DtEntry(string Path, bool IsFolder, long Size, bool IsSelectable) : IEntry;
}