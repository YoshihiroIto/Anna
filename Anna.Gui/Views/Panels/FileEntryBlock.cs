using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Platform;
using System.Globalization;

namespace Anna.Gui.Views.Panels;

public sealed class FileEntryBlock : Control
{
    internal static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<FileEntryBlock>();

    internal static readonly StyledProperty<double> FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<FileEntryBlock>();

    internal static readonly StyledProperty<IBrush> BackgroundProperty =
        AvaloniaProperty.Register<FileEntryBlock, IBrush>(nameof(Background), defaultValue: Brushes.Transparent);

    internal static readonly StyledProperty<IBrush> ForegroundProperty =
        AvaloniaProperty.Register<FileEntryBlock, IBrush>(nameof(Foreground), defaultValue: Brushes.White);
    
    internal static readonly StyledProperty<string> EntryNameProperty =
        AvaloniaProperty.Register<FileEntryBlock, string>(nameof(EntryName), defaultValue: "");
    
    internal static readonly StyledProperty<string> EntryExtensionProperty =
        AvaloniaProperty.Register<FileEntryBlock, string>(nameof(EntryExtension), defaultValue: "");
    
    internal static readonly StyledProperty<string> EntrySizeProperty =
        AvaloniaProperty.Register<FileEntryBlock, string>(nameof(EntrySize), defaultValue: "");
    
    internal static readonly StyledProperty<string> EntryTimestampProperty =
        AvaloniaProperty.Register<FileEntryBlock, string>(nameof(EntryTimestamp), defaultValue: "");

    internal static readonly StyledProperty<double> EntryNameWidthProperty =
        AvaloniaProperty.Register<FileEntryBlock, double>(nameof(EntryNameWidth), defaultValue: 0d);
    
    internal static readonly StyledProperty<double> EntryExtensionWidthProperty =
        AvaloniaProperty.Register<FileEntryBlock, double>(nameof(EntryExtensionWidth), defaultValue: 0d);
    
    internal static readonly StyledProperty<double> EntrySizeWidthProperty =
        AvaloniaProperty.Register<FileEntryBlock, double>(nameof(EntrySizeWidth), defaultValue: 0d);
    
    internal static readonly StyledProperty<double> EntryTimestampWidthProperty =
        AvaloniaProperty.Register<FileEntryBlock, double>(nameof(EntryTimestampWidth), defaultValue: 0d);

    internal FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    internal double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    internal IBrush Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    internal IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }
    
    internal string EntryName
    {
        get => GetValue(EntryNameProperty);
        set => SetValue(EntryNameProperty, value);
    }
    
    internal string EntryExtension
    {
        get => GetValue(EntryExtensionProperty);
        set => SetValue(EntryExtensionProperty, value);
    }
    
    internal string EntrySize
    {
        get => GetValue(EntrySizeProperty);
        set => SetValue(EntrySizeProperty, value);
    }
    
    internal string EntryTimestamp
    {
        get => GetValue(EntryTimestampProperty);
        set => SetValue(EntryTimestampProperty, value);
    }

    internal double EntryNameWidth
    {
        get => GetValue(EntryNameWidthProperty);
        set => SetValue(EntryNameWidthProperty, value);
    }
    
    internal double EntryExtensionWidth
    {
        get => GetValue(EntryExtensionWidthProperty);
        set => SetValue(EntryExtensionWidthProperty, value);
    }
    
    internal double EntrySizeWidth
    {
        get => GetValue(EntrySizeWidthProperty);
        set => SetValue(EntrySizeWidthProperty, value);
    }
    
    internal double EntryTimestampWidth
    {
        get => GetValue(EntryTimestampWidthProperty);
        set => SetValue(EntryTimestampWidthProperty, value);
    }

    public override void Render(DrawingContext dc)
    {
        dc.FillRectangle(Background, new Rect(Bounds.Size));
        
        
        //TextBlock
        
        var ft = new FormattedText(
            EntryName,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily),
            FontSize,
            Foreground);

        dc.DrawText(ft, new Point(0, 0));
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(EntryNameWidth, 20);
    }
    
    protected override Size ArrangeOverride(Size finalSize)
    {
        return new Size(EntryNameWidth, 20);
    }
}