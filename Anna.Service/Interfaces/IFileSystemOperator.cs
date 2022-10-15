namespace Anna.Service.Interfaces;

public interface IFileSystemOperator
{
    event EventHandler FileCopied;
    
    void Copy(IEnumerable<IEntry> sourceEntries, string destPath);
}