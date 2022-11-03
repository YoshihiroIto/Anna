namespace Anna.DomainModel.FileSystem.FileProcessable;

public interface IFileProcessable
{
    public event EventHandler? FileProcessed;

    void Invoke();
}

public sealed class FileProcessedDirectEventArgs : EventArgs
{
    public readonly double Progress;

    public FileProcessedDirectEventArgs(double progress)
    {
        Progress = progress;
    }
}