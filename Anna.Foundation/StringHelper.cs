using System.Buffers;
using UtfUnknown;

namespace Anna.Foundation;

public static class StringHelper
{
    public static async ValueTask<string> BuildString(Stream source, int bufferLength, string maxiLengthInExceededMessage)
    {
        var buf = ArrayPool<byte>.Shared.Rent(bufferLength);

        try
        {
            var readLength = await source.ReadAsync(buf);
            var encoding = CharsetDetector.DetectFromBytes(buf, 0, Math.Min(readLength, 4096));

            if (encoding is null)
                return "";

            var s = encoding.Detected.Encoding.GetString(buf, 0, readLength);

            if (readLength == bufferLength)
                return s + maxiLengthInExceededMessage;

            return s;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buf);
        }
    }
}