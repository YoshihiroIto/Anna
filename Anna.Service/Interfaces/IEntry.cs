namespace Anna.Service.Interfaces;

public interface IEntry
{
    string Path { get; }
    bool IsFolder { get; }
}