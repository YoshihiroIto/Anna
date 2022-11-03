using Anna.Service.Services;
using ICSharpCode.SharpZipLib.Zip;
using Jewelry.Memory;
using Jewelry.Text;
using System.Runtime.CompilerServices;

namespace Anna.Service.Compression;

public sealed class CompressionService : ICompressionService
{
    public void Compress(IEnumerable<string> sourceEntryPaths, string destArchiveFilePath, Action onFileProcessed)
    {
        using var zipOutput = new ZipOutputStream(File.Create(destArchiveFilePath));

        var baseFolder = Path.GetDirectoryName(destArchiveFilePath) ?? throw new NullReferenceException();

        foreach (var targetEntryPath in sourceEntryPaths)
        {
            if (File.Exists(targetEntryPath))
                CompressEntry(zipOutput,
                    baseFolder,
                    targetEntryPath,
                    File.GetLastWriteTime(targetEntryPath),
                    true,
                    onFileProcessed);
            else
                CompressFolder(zipOutput, baseFolder, targetEntryPath, onFileProcessed);
        }

        zipOutput.Finish();
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
        using var zFile = new ZipFile(archiveFilePath);

        using var lsb = new LiteStringBuilder(stackalloc char[256 * 1024]);

        lsb.AppendLine(archiveFilePath);
        lsb.AppendLine($"{zFile.Count} entries");
        lsb.AppendLine("");
        lsb.AppendLine("    Original    Compressed   Ratio  Method      Timestamp             Name");
        lsb.AppendLine("------------  ------------  ------  ----------  --------------------  ----------");

        foreach (ZipEntry e in zFile)
        {
            var ratio = e.Size == 0 ? 0d : 100d * e.CompressedSize / e.Size;

            lsb.AppendLine(
                $"{e.Size,12:#,0}  {e.CompressedSize,12:#,0}  {ratio,5:0.0}%  {e.CompressionMethod,-10}  {e.DateTime,-20}  {e.Name}");
        }

        return lsb.ToString();
    }

    private static void CompressFolder(ZipOutputStream zipOutput, string baseFolder, string targetPath,
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

    private static void CompressEntry(ZipOutputStream zipOutput, string baseFolder, string targetPath,
        DateTime targetTimeStamp, bool isFile, Action onFileProcessed)
    {
        var zippedEntryPath = Path.GetRelativePath(baseFolder, targetPath);

        var entry = new ZipEntry(isFile ? zippedEntryPath : zippedEntryPath + "/") { DateTime = targetTimeStamp };

        if (isFile == false)
            entry.CompressionMethod = CompressionMethod.Stored;

        zipOutput.PutNextEntry(entry);

        if (isFile)
        {
            using var targetFile = File.OpenRead(targetPath);
            targetFile.CopyTo(zipOutput);

            onFileProcessed();
        }
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
                var folderPath = Path.GetDirectoryName(filePath) ?? throw new NullReferenceException();

                if (Directory.Exists(folderPath) == false)
                    Directory.CreateDirectory(folderPath);

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