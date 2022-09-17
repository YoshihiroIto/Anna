﻿using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Anna.Views.Converters;

public enum AttributeBrushConverterBrushTypes
{
    Foreground,
    Background
}

public class AttributeBrushConverter : AvaloniaObject, IMultiValueConverter
{
    public static readonly StyledProperty<IBrush> EmptyAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(nameof(EmptyAttributeBrush));

    public static readonly StyledProperty<IBrush> IsDirectoryAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(
        nameof(IsDirectoryAttributeBrush));

    public static readonly StyledProperty<IBrush> IsReadOnlyAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(
        nameof(IsReadOnlyAttributeBrush));

    public static readonly StyledProperty<IBrush> IsReparsePointAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(
        nameof(IsReparsePointAttributeBrush));

    public static readonly StyledProperty<IBrush> IsHiddenAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(nameof(IsHiddenAttributeBrush));

    public static readonly StyledProperty<IBrush> IsSystemAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(nameof(IsSystemAttributeBrush));

    public static readonly StyledProperty<IBrush> IsEncryptedAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(
        nameof(IsEncryptedAttributeBrush));

    public static readonly StyledProperty<IBrush> IsCompressedAttributeBrushProperty =
        AvaloniaProperty.Register<AttributeBrushConverter, IBrush>(
        nameof(IsCompressedAttributeBrush));

    public IBrush EmptyAttributeBrush
    {
        get => GetValue(EmptyAttributeBrushProperty);
        set => SetValue(EmptyAttributeBrushProperty, value);
    }

    public IBrush IsDirectoryAttributeBrush
    {
        get => GetValue(IsDirectoryAttributeBrushProperty);
        set => SetValue(IsDirectoryAttributeBrushProperty, value);
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

    public object? Convert(
        IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2)
            return null;

        if (values[0] is not FileAttributes attributes)
            return null;

        if (values[1] is not bool isOnCursor)
            return null;

        if (parameter is not AttributeBrushConverterBrushTypes brushType)
            return null;

        return brushType switch
        {
            AttributeBrushConverterBrushTypes.Foreground when isOnCursor =>
                Brushes.Black,
            AttributeBrushConverterBrushTypes.Foreground when isOnCursor == false =>
                FindBrush(attributes),
            AttributeBrushConverterBrushTypes.Background when isOnCursor =>
                FindBrush(attributes),
            AttributeBrushConverterBrushTypes.Background when isOnCursor == false =>
                Brushes.Transparent,
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
            return IsDirectoryAttributeBrush;

        return EmptyAttributeBrush;
    }
}