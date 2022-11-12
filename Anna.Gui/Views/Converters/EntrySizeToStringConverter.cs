using Anna.Foundation;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Anna.Gui.Views.Converters;

public sealed class EntrySizeToStringConverter : IMultiValueConverter
{
    public static readonly EntrySizeToStringConverter Default = new();

    private const int LeftPadding = 1;
    
    [SkipLocalsInit]
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not int count)
            return "";

        if (values[1] is not long size)
            return "";

        const int bufferSize = 256;
        const int bufferCenter = bufferSize/2;
        Span<char> buffer = stackalloc char[bufferSize];

        if (size.TryFormat(buffer[bufferCenter..], out var charsWritten, "#,0") == false)
            return "";

        var bodyCount = count - LeftPadding;

        if (charsWritten > bodyCount)
        {
            var simpled = StringHelper.MakeSimpleSizeString(size);

            return simpled.Length < bodyCount
                ? string.Create(count,
                    (simpled, count),
                    (b, s) =>
                    {
                        var start = s.count - s.simpled.Length;

                        b[..start].Fill(' ');
                        s.simpled.CopyTo(b[start..]);
                    })
                : simpled;
        }

        var startIndex = bufferCenter - (bodyCount - charsWritten);
        buffer.Slice(startIndex - LeftPadding, bodyCount - charsWritten + LeftPadding).Fill(' ');

        return buffer.Slice(startIndex - LeftPadding, count).ToString();
    }
}

public sealed class FolderEntrySizeToStringConverter : IValueConverter
{
    public static readonly FolderEntrySizeToStringConverter Default = new();

    private static readonly Dictionary<int, string> PlaceHolders = new();
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int count)
            return "";

        if (PlaceHolders.TryGetValue(count, out var placeHolder) == false)
        {
            placeHolder = string.Create(count,
                0,
                (b, _) => b.Fill(' ')
            );

            PlaceHolders.Add(count, placeHolder);
        }

        return placeHolder;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}