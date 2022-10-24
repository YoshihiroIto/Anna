namespace Anna.Foundation;

public static class FileHelper
{
    public static void WriteSpan(string filePath, ReadOnlySpan<byte> bytes)
    {
        using var file = File.OpenWrite(filePath);

        file.Write(bytes);
    }
}