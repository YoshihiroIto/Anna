namespace Anna.DomainModel.FileSystem
{
    public interface IFileProcessable
    {
        public event EventHandler? FileProcessed;
    }
}