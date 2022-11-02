namespace Anna.DomainModel.FileSystem.FileProcessable;

public interface IFileProcessable
{
    public event EventHandler? FileProcessed;

    void Invoke();
}