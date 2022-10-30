using Anna.Foundation;
using Anna.Service.Services;
using ICSharpCode.SharpZipLib.Zip;
using Jewelry.Memory;
using System.Buffers;

namespace Anna.Service.Compression;

public sealed class CompressionService : ICompressionService
{
    public void Compress(IEnumerable<string> targetFilePaths, string destArchiveFilePath)
    {
        throw new NotImplementedException();
    }

    public void Decompress(string archiveFilePath, string destFolderPath)
    {
        Directory.CreateDirectory(destFolderPath);

        using var folders = new TempBuffer<(string Path, DateTime TimeStamp)>(512);

        // decompress files
        using var inputFileStream = File.OpenRead(archiveFilePath);
        using var zipInputStream = new ZipInputStream(inputFileStream);

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
                var buffer = ArrayPool<byte>.Shared.Rent((int)entry.Size);

                try
                {
                    _ = zipInputStream.Read(buffer, 0, buffer.Length);

                    var filePath = Path.Combine(destFolderPath, entry.Name);
                    FileSystemHelper.WriteSpan(filePath, buffer.AsSpan(0, (int)entry.Size));
                    File.SetLastWriteTimeUtc(filePath, entry.DateTime);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }

        // set folder timestamp
        foreach (var folder in folders.Buffer)
            Directory.SetLastWriteTimeUtc(folder.Path, folder.TimeStamp);
    }
}