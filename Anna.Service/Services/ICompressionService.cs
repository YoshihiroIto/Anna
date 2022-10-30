namespace Anna.Service.Services;

public interface ICompressionService
{
    void Compress(IEnumerable<string> targetFilePaths, string destArchiveFilePath, Action<double> onProgress);
    
    void Decompress(string archiveFilePath, string destFolderPath, Action<double> onProgress);
}