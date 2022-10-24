namespace Anna.Service.Interfaces;

public interface IEntry
{
    string Path { get; }
    long Size { get; }
    bool IsFolder { get; }
}