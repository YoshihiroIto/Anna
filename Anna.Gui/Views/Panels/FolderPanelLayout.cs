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


    #region TimestampWidth

    private double _TimestampWidth;

    public double TimestampWidth
    {
        get => _TimestampWidth;
        private set => SetProperty(ref _TimestampWidth, value);
    }

    #endregion


    public FolderPanelViewModel? ViewModel
    {
        get => _ViewModel;
        set
        {
            if (_ViewModel == value)
                return;

            if (_ViewModel is not null)
                _ViewModel.ListModeChanged -= OnListModeChanged;

            _ViewModel = value;

            if (_ViewModel is not null)
                _ViewModel.ListModeChanged += OnListModeChanged;

            UpdateItemSize();

            //////////////////////////////////////////////////////////////
            void OnListModeChanged(object? sender, EventArgs e)
                => UpdateItemSize();
        }
    }

    private FolderPanelViewModel? _ViewModel;


    public bool IsVisibleSize
    {
        get => (_flags & FlagIsVisibleSize) != 0;
        set => SetFlagProperty(ref _flags, FlagIsVisibleSize, value);
    }

    public bool IsVisibleTimestamp
    {
        get => (_flags & FlagIsVisibleTimestamp) != 0;
        set => SetFlagProperty(ref _flags, FlagIsVisibleTimestamp, value);
    }
    
    internal double ItemMargin { get; private set; }

    private uint _flags = FlagIsVisibleSize | FlagIsVisibleTimestamp;

    private const uint FlagIsVisibleSize = 1 << 0;
    private const uint FlagIsVisibleTimestamp = 1 << 1;

    public static Thickness SelectedMarkMargin { get; } = new(0, 0, 2, 0);

    private static readonly Dictionary<int, Size> ItemHeightCache = new();

    public int NameCount => _ViewModel?.NameCount ?? 16;
    public int ExtensionCount => _ViewModel?.ExtensionCount ?? 5;
    public int SizeCount => _ViewModel?.SizeCount ?? 12;
    public int TimestampCount => _ViewModel?.TimestampCount ?? 12;

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

        var charaWidth = value.Width;

        NameWidth = NameCount * charaWidth;
        ExtensionWidth = ExtensionCount * charaWidth;
        NameWithExtensionWidth = NameWidth + ExtensionWidth;

        SizeWidth = SizeCount * charaWidth;
        IsVisibleSize = SizeCount > 0;

        TimestampWidth = TimestampCount * charaWidth;
        IsVisibleTimestamp = TimestampCount > 0;

        ItemMargin = charaWidth * 4;

        ItemWidth = value.Height +// SelectedMark width
                    SelectedMarkMargin.Left +
                    SelectedMarkMargin.Right +
                    (NameCount + ExtensionCount) * charaWidth +
                    (IsVisibleSize ? SizeCount * charaWidth : 0) +
                    (IsVisibleTimestamp ? TimestampCount * charaWidth : 0) +
                    ItemMargin;

        ItemHeight = value.Height;

        RaisePropertyChanged(nameof(NameCount));
        RaisePropertyChanged(nameof(ExtensionCount));
        RaisePropertyChanged(nameof(SizeCount));
        RaisePropertyChanged(nameof(TimestampCount));
    }
}