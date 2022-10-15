namespace Anna.Service.Interfaces;

public interface IFileSystemOperator
{
    void Copy(IEnumerable<IEntry> sourceEntries, string destPath, Action? fileCopied);
}