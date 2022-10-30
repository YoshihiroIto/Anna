namespace Anna.Service.Services;

public interface ICompressionService
{
    void Compress(IEnumerable<string> targetFilePaths, string destArchiveFilePath);
    void Decompress(string archiveFilePath, string destFolderPath);
}