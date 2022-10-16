namespace Anna.Service.Interfaces;

public interface IFileSystemCopyOperator
{
    event EventHandler FileCopied;

    void Invoke(IEnumerable<IEntry> sourceEntries, string destPath);
}
