namespace Anna.Service.Services;

public interface ICompressionService
{
    void Compress(IEnumerable<string> sourceEntryPaths, string destArchiveFilePath, Action onFileProcessed);
    
    void Decompress(IEnumerable<string> archiveFilePaths,  string destFolderPath, Action<double> onProgress);
}