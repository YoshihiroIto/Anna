using Anna.Foundation;
using Anna.Service.Services;
using ICSharpCode.SharpZipLib.Zip;
using Jewelry.Memory;
using System.Buffers;

namespace Anna.Service.Compression;

public sealed class CompressionService : ICompressionService
{
    public void Compress(IEnumerable<string> targetFilePaths, string destArchiveFilePath, Action<double> onProgress)
    {
        throw new NotImplementedException();
    }

    public void Decompress(IEnumerable<string> archiveFilePaths, string destFolderPath, Action<double> onProgress)
    {
        using var tempFilePaths = archiveFilePaths.ToPooledArray();

        var filePathsLengthInverse = 1d / tempFilePaths.Length;
        var count = 0;
        foreach (var filePath in tempFilePaths.AsSpan())
        {
            DecompressInternal(
                filePath,
                destFolderPath,
                p => onProgress(p * filePathsLengthInverse),
                count
            );

            ++count;
        }
        
        onProgress(1d);
    }

    private static void DecompressInternal(
        string archiveFilePath, string destFolderPath, Action<double> onProgress,
        double progressOffset)
    {
        Directory.CreateDirectory(destFolderPath);

        long fileCount;

        using (var zip = new ZipFile(archiveFilePath))
            fileCount = zip.Count;

        using var folders = new TempBuffer<(string Path, DateTime TimeStamp)>(512);

        // decompress files
        using var inputFile = File.OpenRead(archiveFilePath);
        using var zipInput = new ZipInputStream(inputFile);

        var processed = 0;

        while (zipInput.GetNextEntry() is {} entry)
        {
            if (entry.IsDirectory)
            {
                var folderPath = Path.GetDirectoryName(Path.Combine(destFolderPath, entry.Name)) ??
                                 throw new NullReferenceException();

                Directory.CreateDirectory(folderPath);

                folders.Add((Path: folderPath, TimeStamp: entry.DateTime));
            }
            else
            {
                var filePath = Path.Combine(destFolderPath, entry.Name);

                using (var file = File.OpenWrite(filePath))
                    zipInput.CopyTo(file);

                File.SetLastWriteTimeUtc(filePath, entry.DateTime);
            }

            ++processed;

            onProgress(progressOffset + (double)processed / fileCount);
        }

        // set folder timestamp
        foreach (var folder in folders.Buffer)
            Directory.SetLastWriteTimeUtc(folder.Path, folder.TimeStamp);
    }
}