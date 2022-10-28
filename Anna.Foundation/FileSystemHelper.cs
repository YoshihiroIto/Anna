
namespace Anna.Foundation;

public static class FileSystemHelper
{
    public static void WriteSpan(string filePath, ReadOnlySpan<byte> bytes)
    {
        using var file = File.OpenWrite(filePath);

        file.Write(bytes);
    }

    public static long MeasureFolderSize(DirectoryInfo dirInfo)
    {
        var size = 0L;

        Parallel.ForEach(dirInfo.EnumerateFiles(),
            () => 0L,
            (fi, _, subSize) => subSize + fi.Length,
            finalResult => Interlocked.Add(ref size, finalResult)
        );

        Parallel.ForEach(dirInfo.EnumerateDirectories(),
            () => 0L,
            (di, _, subSize) => subSize + MeasureFolderSize(di),
            finalResult => Interlocked.Add(ref size, finalResult)
        );

        return size;
    }

    public static string MakeNewEntryName(string folderPath, string baseName)
    {
        for (var i = 1;; ++i)
        {
            var entryName = i == 1
                ? baseName
                : $"{baseName} ({i})";

            var path = Path.Combine(folderPath, entryName);

            if (File.Exists(path) == false && Directory.Exists(path) == false)
                return entryName;
        }
    }
}