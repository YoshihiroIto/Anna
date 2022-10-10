using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Anna.Gui.Views.Converters;

public enum AttributeBrushConverterBrushTypes
{
    Foreground,
    Background
}

public sealed class AttributeBrushConverter : AvaloniaObject, IMultiValueConverter
{
    public static readonly StyledProperty<IBrush> NoneAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(nameof(NoneAttributeBrush));

    public static readonly StyledProperty<IBrush> IsFolderAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(nameof(IsFolderAttributeBrush));

    public static readonly StyledProperty<IBrush> IsReadOnlyAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(nameof(IsReadOnlyAttributeBrush));

    public static readonly StyledProperty<IBrush> IsReparsePointAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(nameof(IsReparsePointAttributeBrush));

    public static readonly StyledProperty<IBrush> IsHiddenAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(nameof(IsHiddenAttributeBrush));

    public static readonly StyledProperty<IBrush> IsSystemAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(nameof(IsSystemAttributeBrush));

    public static readonly StyledProperty<IBrush> IsEncryptedAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(nameof(IsEncryptedAttributeBrush));

    public static readonly StyledProperty<IBrush> IsCompressedAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(nameof(IsCompressedAttributeBrush));

    public IBrush NoneAttributeBrush
    {
        get => GetValue(NoneAttributeBrushProperty);
        set => SetValue(NoneAttributeBrushProperty, value);
    }

    public IBrush IsFolderAttributeBrush
    {
        get => GetValue(IsFolderAttributeBrushProperty);
        set => SetValue(IsFolderAttributeBrushProperty, value);
    }

    public IBrush IsReadOnlyAttributeBrush
    {
        get => GetValue(IsReadOnlyAttributeBrushProperty);
        set => SetValue(IsReadOnlyAttributeBrushProperty, value);
    }

    public IBrush IsReparsePointAttributeBrush
    {
        get => GetValue(IsReparsePointAttributeBrushProperty);
        set => SetValue(IsReparsePointAttributeBrushProperty, value);
    }

    public IBrush IsHiddenAttributeBrush
    {
        get => GetValue(IsHiddenAttributeBrushProperty);
        set => SetValue(IsHiddenAttributeBrushProperty, value);
    }

    public IBrush IsSystemAttributeBrush
    {
        get => GetValue(IsSystemAttributeBrushProperty);
        set => SetValue(IsSystemAttributeBrushProperty, value);
    }

    public IBrush IsEncryptedAttributeBrush
    {
        get => GetValue(IsEncryptedAttributeBrushProperty);
        set => SetValue(IsEncryptedAttributeBrushProperty, value);
    }

    public IBrush IsCompressedAttributeBrush
    {
        get => GetValue(IsCompressedAttributeBrushProperty);
        set => SetValue(IsCompressedAttributeBrushProperty, value);
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var value0 = values.Count >= 1 ? values[0] : null;
        var value1 = values.Count >= 2 ? values[1] : null;

        if (value0 is not FileAttributes attributes)
            return null;

        if (value1 is not bool isOnCursor)
            isOnCursor = false;

        if (parameter is not AttributeBrushConverterBrushTypes brushType)
            return null;

        return brushType switch
        {
            AttributeBrushConverterBrushTypes.Foreground when isOnCursor => Brushes.Black,
            AttributeBrushConverterBrushTypes.Foreground when isOnCursor == false => FindBrush(attributes),
            AttributeBrushConverterBrushTypes.Background when isOnCursor => FindBrush(attributes),
            AttributeBrushConverterBrushTypes.Background when isOnCursor == false => Brushes.Transparent,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private IBrush FindBrush(FileAttributes attributes)
    {
        if ((attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
            return IsReparsePointAttributeBrush;

        if ((attributes & FileAttributes.System) == FileAttributes.System)
            return IsSystemAttributeBrush;

        if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            return IsHiddenAttributeBrush;

        if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            return IsReadOnlyAttributeBrush;

        if ((attributes & FileAttributes.Encrypted) == FileAttributes.Encrypted)
            return IsEncryptedAttributeBrush;

        if ((attributes & FileAttributes.Compressed) == FileAttributes.Compressed)
            return IsCompressedAttributeBrush;

        if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            return IsFolderAttributeBrush;

        return NoneAttributeBrush;
    }
}