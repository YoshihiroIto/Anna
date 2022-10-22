using Humanizer;
using System.Buffers;
using UtfUnknown;

namespace Anna.Foundation;

public static class StringHelper
{
    public static async ValueTask<(string Result, bool IsText)> BuildString(Stream source, int bufferLength,
        string maxiLengthInExceededMessage)
    {
        var buf = ArrayPool<byte>.Shared.Rent(bufferLength);

        try
        {
            var readLength = await source.ReadAsync(buf).ConfigureAwait(false);
            if (readLength == 0)
                return ("", true);
            
            var encoding = CharsetDetector.DetectFromBytes(buf, 0, Math.Min(readLength, 4096));
            if (encoding?.Detected is null)
                return ("", false);

            var s = encoding.Detected.Encoding.GetString(buf, 0, readLength);

            return (readLength == bufferLength ? s + maxiLengthInExceededMessage : s, true);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buf);
        }
    }
    
    public static string MakeSizeString(long size)
    {
        var h = size.Bytes().Humanize("#.##");
        var b = $"{size:#,0} B";
        
        return b == h ? b : $"{h} ({b})";
    }
}