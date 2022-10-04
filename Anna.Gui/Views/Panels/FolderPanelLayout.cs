using Anna.Foundation;
using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using FormattedText=Avalonia.Media.FormattedText;

namespace Anna.Gui.Views.Panels;

public class FolderPanelLayout : NotificationObject
{
    #region FontFamily

    private FontFamily _FontFamily =
        new(new Uri("avares://Anna.Gui/Assets/UDEVGothicNF-Regular.ttf"), "UDEV Gothic NF");

    public FontFamily FontFamily
    {
        get => _FontFamily;
        set
        {
            if (SetProperty(ref _FontFamily, value) == false)
                return;

            UpdateItemSize();
        }
    }

    #endregion


    #region FontSize

    private double _FontSize = 14;

    public double FontSize
    {
        get => _FontSize;
        set
        {
            if (SetProperty(ref _FontSize, value))
                return;

            UpdateItemSize();
        }
    }

    #endregion


    #region NameCount

    private int _NameCount = 16;

    public int NameCount
    {
        get => _NameCount;
        set
        {
            if (SetProperty(ref _NameCount, value))
                return;

            UpdateItemSize();
        }
    }

    #endregion


    #region ExtensionCount

    private int _ExtensionCount = 5;

    public int ExtensionCount
    {
        get => _ExtensionCount;
        set
        {
            if (SetProperty(ref _ExtensionCount, value))
                return;

            UpdateItemSize();
        }
    }

    #endregion


    #region ItemWidth

    private double _ItemWidth;

    public double ItemWidth
    {
        get => _ItemWidth;
        private set => SetProperty(ref _ItemWidth, value);
    }

    #endregion


    #region ItemHeight

    private double _ItemHeight;

    public double ItemHeight
    {
        get => _ItemHeight;
        private set => SetProperty(ref _ItemHeight, value);
    }

    #endregion


    #region NameWidth

    private double _NameWidth = 220;

    public double NameWidth
    {
        get => _NameWidth;
        private set => SetProperty(ref _NameWidth, value);
    }

    #endregion


    #region ExtensionWidth

    private double _ExtensionWidth = 40;

    public double ExtensionWidth
    {
        get => _ExtensionWidth;
        private set => SetProperty(ref _ExtensionWidth, value);
    }

    #endregion


    #region NameWithExtensionWidth

    // Must be NameWidth + ExtensionWidth
    private double _NameWithExtensionWidth = 260;

    public double NameWithExtensionWidth
    {
        get => _NameWithExtensionWidth;
        private set => SetProperty(ref _NameWithExtensionWidth, value);
    }

    #endregion

    
    private double _charaWidth;

    public static Thickness ItemMargin { get; } = new(0, 0, 32, 0);
    public static Thickness SelectedMarkMargin { get; } = new(0, 0, 2, 0);

    public FolderPanelLayout()
    {
        UpdateItemSize();
    }

    private void UpdateItemSize()
    {
        var key = HashCode.Combine(FontFamily, FontSize);

        if (ItemHeightCache.TryGetValue(key, out var value) == false)
        {
            var typeface = new Typeface(FontFamily);
            var ft = new FormattedText("A",
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                typeface,
                FontSize,
                Brushes.Black);

            value = new Size(
                Math.Ceiling(ft.WidthIncludingTrailingWhitespace),
                Math.Ceiling(ft.Height));

            ItemHeightCache.Add(key, value);
        }

        _charaWidth = value.Width;

        NameWidth = NameCount * _charaWidth;
        ExtensionWidth = ExtensionCount * _charaWidth;
        NameWithExtensionWidth = NameWidth + ExtensionWidth;

        ItemWidth = value.Height +// SelectedMark width
                    SelectedMarkMargin.Left +
                    SelectedMarkMargin.Right +
                    ItemMargin.Left +
                    ItemMargin.Right +
                    NameWithExtensionWidth;
        ItemHeight = value.Height;
    }

    private static readonly Dictionary<int, Size> ItemHeightCache = new();
}