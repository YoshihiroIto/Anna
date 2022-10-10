using Avalonia.Data.Converters;
using Humanizer;
using System;
using System.Globalization;

namespace Anna.Gui.Views.Converters;

public class BytesValueToHumanizedConverter : IValueConverter
{
    public static readonly BytesValueToHumanizedConverter Default = new();

    public string Format { get; set; } = "#.##";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            long x => x.Bytes().Humanize(Format),
            int x => x.Bytes().Humanize(Format),
            _ => throw new NotSupportedException()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}