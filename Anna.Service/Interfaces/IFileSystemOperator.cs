namespace Anna.Service.Interfaces;

public interface IFileSystemOperator
{
    event EventHandler FileCopied;

    Task CopyAsync(IEnumerable<IEntry> sourceEntries, string destPath);
}
