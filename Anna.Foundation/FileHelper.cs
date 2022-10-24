namespace Anna.Foundation;

public static class FileHelper
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
            (fi, _, subSize) =>
            {
                subSize += fi.Length;
                return subSize;
            },
            finalResult => Interlocked.Add(ref size, finalResult)
        );
        
        Parallel.ForEach(dirInfo.EnumerateDirectories(),
            () => 0L,
            (di, _, subSize) =>
            {
                subSize += MeasureFolderSize(di);
                return subSize;
            },
            finalResult => Interlocked.Add(ref size, finalResult)
        );
        
        return size;
    }
}