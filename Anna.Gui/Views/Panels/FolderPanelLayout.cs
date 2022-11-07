using Anna.DomainModel.Config;
using Anna.Foundation;
using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using FormattedText=Avalonia.Media.FormattedText;

namespace Anna.Gui.Views.Panels;

public sealed class FolderPanelLayout : NotificationObject
{
    #region FontFamily

    private FontFamily _FontFamily = AppConfigData.DefaultViewerFontFamily;

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

    private double _FontSize = AppConfigData.DefaultViewerFontSize;

    public double FontSize
    {
        get => _FontSize;
        set
        {
            if (SetProperty(ref _FontSize, value) == false)
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

    private double _NameWidth;

    public double NameWidth
    {
        get => _NameWidth;
        private set => SetProperty(ref _NameWidth, value);
    }

    #endregion


    #region ExtensionWidth

    private double _ExtensionWidth;

    public double ExtensionWidth
    {
        get => _ExtensionWidth;
        private set => SetProperty(ref _ExtensionWidth, value);
    }

    #endregion


    #region NameWithExtensionWidth

    // Must be NameWidth + ExtensionWidth
    private double _NameWithExtensionWidth;

    public double NameWithExtensionWidth
    {
        get => _NameWithExtensionWidth;
        private set => SetProperty(ref _NameWithExtensionWidth, value);
    }

    #endregion


    #region SizeWidth

    private double _SizeWidth;

    public double SizeWidth
    {
        get => _SizeWidth;
        private set => SetProperty(ref _SizeWidth, value);
    }

    #endregion


    public bool IsVisibleSize
    {
        get => (_flags & FlagIsVisibleSize) != 0;
        set => SetFlagProperty(ref _flags, FlagIsVisibleSize, value);
    }

    public bool IsVisibleTimeStamp
    {
        get => (_flags & FlagIsVisibleTimeStamp) != 0;
        set => SetFlagProperty(ref _flags, FlagIsVisibleTimeStamp, value);
    }

    private uint _flags = FlagIsVisibleSize | FlagIsVisibleTimeStamp;

    private const uint FlagIsVisibleSize = 1 << 0;
    private const uint FlagIsVisibleTimeStamp = 1 << 1;

    private double _charaWidth;

    public static Thickness ItemMargin { get; } = new(0, 0, 32, 0);
    public static Thickness SelectedMarkMargin { get; } = new(0, 0, 2, 0);

    private static readonly Dictionary<int, Size> ItemHeightCache = new();

    private const int NameCount = 16;
    private const int ExtensionCount = 5;
    private const int SizeCount = 10;

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
        SizeWidth = SizeCount * _charaWidth;

        ItemWidth = value.Height +// SelectedMark width
                    SelectedMarkMargin.Left +
                    SelectedMarkMargin.Right +
                    ItemMargin.Left +
                    (NameCount + ExtensionCount) * _charaWidth +
                    (IsVisibleSize ? SizeCount * _charaWidth : 0) +
                    ItemMargin.Right;

        ItemHeight = value.Height;
    }
}