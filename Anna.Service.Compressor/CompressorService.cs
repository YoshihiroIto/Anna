using Anna.Service.Services;
using Jewelry.Memory;
using Jewelry.Text;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers;
using System.Runtime.CompilerServices;

namespace Anna.Service.Compressor;

public sealed class CompressorService : ICompressorService
{
    public void Compress(IEnumerable<string> sourceEntryPaths, string destArchiveFilePath, Action onFileProcessed)
    {
        using var zip = File.OpenWrite(destArchiveFilePath);
        using var zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, CompressionType.Deflate);

        var baseFolder = Path.GetDirectoryName(destArchiveFilePath) ?? throw new NullReferenceException();

        foreach (var targetEntryPath in sourceEntryPaths)
        {
            if (File.Exists(targetEntryPath))
                CompressEntry(zipWriter,
                    baseFolder,
                    targetEntryPath,
                    File.GetLastWriteTime(targetEntryPath),
                    true,
                    onFileProcessed);
            else
                CompressFolder(zipWriter, baseFolder, targetEntryPath, onFileProcessed);
        }
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

    [SkipLocalsInit]
    public string ReadMetaString(string archiveFilePath)
    {
        using var zip = ArchiveFactory.Open(archiveFilePath);

        using var lsb = new LiteStringBuilder(stackalloc char[256 * 1024]);
        lsb.AppendLine(archiveFilePath);
        lsb.AppendLine($"{zip.Entries.Count()} entries");
        lsb.AppendLine("");
        lsb.AppendLine("    Original    Compressed   Ratio  Method      Timestamp             Name");
        lsb.AppendLine("------------  ------------  ------  ----------  --------------------  ----------");

        foreach (var e in zip.Entries)
        {
            var ratio = e.Size == 0 ? 0d : 100d * e.CompressedSize / e.Size;

            lsb.AppendLine(
                $"{e.Size,12:#,0}  {e.CompressedSize,12:#,0}  {ratio,5:0.0}%  {e.CompressionType,-10}  {e.LastModifiedTime,-20}  {e.Key}");
        }

        return lsb.ToString();
    }

    private static void CompressFolder(IWriter zipOutput, string baseFolder, string targetPath,
        Action onFileProcessed)
    {
        var di = new DirectoryInfo(targetPath);

        CompressEntry(zipOutput, baseFolder, di.FullName, di.LastWriteTime, false, onFileProcessed);

        foreach (var fi in di.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).OrderBy(x => x.FullName))
        {
            var isFile = fi is FileInfo;
            CompressEntry(zipOutput, baseFolder, fi.FullName, fi.LastWriteTime, isFile, onFileProcessed);
        }
    }

    private static void CompressEntry(IWriter zipOutput, string baseFolder, string targetPath,
        DateTime targetTimeStamp, bool isFile, Action onFileProcessed)
    {
        var zippedEntryPath = Path.GetRelativePath(baseFolder, targetPath);
        
        if (isFile == false)
            zipOutput.Write(zippedEntryPath + "/", ForFolder, targetTimeStamp);
        else
        {
            using var file = File.OpenRead(targetPath);
            zipOutput.Write(zippedEntryPath, file, targetTimeStamp);
        }

        onFileProcessed();
    }

    private static void DecompressInternal(
        string archiveFilePath, string destFolderPath, Action<double> onProgress,
        double progressOffset)
    {
        Directory.CreateDirectory(destFolderPath);

        using var zip = ArchiveFactory.Open(archiveFilePath);
        var options = new ExtractionOptions { Overwrite = true, ExtractFullPath = true, PreserveFileTime = true };

        var processed = 0;
        var fileCount = zip.Entries.Count();

        foreach (var entry in zip.Entries)
        {
            if (entry.IsDirectory == false)
            {
                entry.WriteToDirectory(destFolderPath, options);
            }

            ++processed;
            onProgress(progressOffset + (double)processed / fileCount);
        }
    }

    private static readonly MemoryStream ForFolder = new();
}