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

    public void Decompress(string archiveFilePath, string destFolderPath, Action<double> onProgress)
    {
        Directory.CreateDirectory(destFolderPath);

        long fileCount;

        using (var zip = new ZipFile(archiveFilePath))
        {
            fileCount = zip.Count;
        }

        using var folders = new TempBuffer<(string Path, DateTime TimeStamp)>(512);

        var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);

        try
        {
            // decompress files
            using var inputFileStream = File.OpenRead(archiveFilePath);
            using var zipInputStream = new ZipInputStream(inputFileStream);

            var processed = 0;

            while (zipInputStream.GetNextEntry() is {} entry)
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
                    if (buffer.Length < entry.Size)
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                        buffer = ArrayPool<byte>.Shared.Rent((int)entry.Size);
                    }

                    _ = zipInputStream.Read(buffer, 0, buffer.Length);

                    var filePath = Path.Combine(destFolderPath, entry.Name);
                    FileSystemHelper.WriteSpan(filePath, buffer.AsSpan(0, (int)entry.Size));
                    File.SetLastWriteTimeUtc(filePath, entry.DateTime);
                }

                ++processed;

                onProgress((double)processed / fileCount);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        // set folder timestamp
        foreach (var folder in folders.Buffer)
            Directory.SetLastWriteTimeUtc(folder.Path, folder.TimeStamp);

        onProgress(1d);
    }
}