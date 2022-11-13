using Anna.Constants;
using Anna.Service.Services;
using Avalonia.Media;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Anna.DomainModel.Config;

public sealed class AppConfig : ConfigBase<AppConfigData>
{
    public AppConfig(IObjectSerializerService objectSerializer)
        : base(objectSerializer)
    {
    }
}

public sealed class AppConfigData : ConfigData
{
    public static readonly FontFamily DefaultViewerFontFamily =
        new(new Uri("avares://Anna.Gui/Assets/UDEVGothicNF-Regular.ttf"), "UDEV Gothic NF");

    public const double DefaultViewerFontSize = 14;

    public enum ExternalApp
    {
        Terminal,
        App1,
        App2,
    }

    #region Culture

    private Cultures _Culture;

    public Cultures Culture
    {
        get => _Culture;
        set => SetProperty(ref _Culture, value);
    }

    #endregion


    #region TerminalApp

    private string _TerminalApp = "";

    public string TerminalApp
    {
        get => _TerminalApp;
        set => SetProperty(ref _TerminalApp, value);
    }

    #endregion


    #region TerminalAppOptions

    private string _TerminalAppOptions = "";

    public string TerminalAppOptions
    {
        get => _TerminalAppOptions;
        set => SetProperty(ref _TerminalAppOptions, value);
    }

    #endregion


    #region ExternalApp1

    private string _ExternalApp1 = "";

    public string ExternalApp1
    {
        get => _ExternalApp1;
        set => SetProperty(ref _ExternalApp1, value);
    }

    #endregion


    #region ExternalApp1Options

    private string _ExternalApp1Options = "";

    public string ExternalApp1Options
    {
        get => _ExternalApp1Options;
        set => SetProperty(ref _ExternalApp1Options, value);
    }

    #endregion


    #region ExternalApp2

    private string _ExternalApp2 = "";

    public string ExternalApp2
    {
        get => _ExternalApp2;
        set => SetProperty(ref _ExternalApp2, value);
    }

    #endregion


    #region ExternalApp2Options

    private string _ExternalApp2Options = "";

    public string ExternalApp2Options
    {
        get => _ExternalApp2Options;
        set => SetProperty(ref _ExternalApp2Options, value);
    }

    #endregion


    #region TextViewerMaxBufferSize

    private int _textViewerMaxBufferSize = 2 * 1024 * 1024;

    public int TextViewerMaxBufferSize
    {
        get => _textViewerMaxBufferSize;
        set => SetProperty(ref _textViewerMaxBufferSize, value);
    }

    #endregion


    #region ViewerFontFamily

    private FontFamily _ViewerFontFamily = DefaultViewerFontFamily;

    [JsonIgnore]
    public FontFamily ViewerFontFamily
    {
        get => _ViewerFontFamily;
        set => SetProperty(ref _ViewerFontFamily, value);
    }

    #endregion


    #region ViewerFontSize

    private double _ViewerFontSize = DefaultViewerFontSize;

    public double ViewerFontSize
    {
        get => _ViewerFontSize;
        set => SetProperty(ref _ViewerFontSize, value);
    }

    #endregion


    #region TimestampFormat

    private string _TimestampFormat = "";

    public string TimestampFormat
    {
        get => _TimestampFormat;
        set => SetProperty(ref _TimestampFormat, value);
    }

    #endregion


    #region FolderWindows

    private ListModeLayout[] _ListModeLayouts = Array.Empty<ListModeLayout>();

    public ListModeLayout[] ListModeLayouts
    {
        get => _ListModeLayouts;
        set => SetProperty(ref _ListModeLayouts, value);
    }

    #endregion


    #region FolderWindows

    private ObservableCollection<FolderWindowConfigData> _FolderWindows = new();

    public ObservableCollection<FolderWindowConfigData> FolderWindows
    {
        get => _FolderWindows;
        set => SetProperty(ref _FolderWindows, value);
    }

    #endregion


    #region DestinationFolders

    private ObservableCollection<string> _DestinationFolders = new();

    public ObservableCollection<string> DestinationFolders
    {
        get => _DestinationFolders;
        set => SetProperty(ref _DestinationFolders, value);
    }

    #endregion


    public (string ExternalApp, string Options) FindExternalApp(ExternalApp externalApp)
    {
        return externalApp switch
        {
            ExternalApp.App1 => (ExternalApp1, ExternalApp1Options),
            ExternalApp.App2 => (ExternalApp2, ExternalApp2Options),
            ExternalApp.Terminal => (TerminalApp, TerminalAppOptions),
            _ => throw new IndexOutOfRangeException()
        };
    }

    public override void SetDefault()
    {
        var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        if (lang != "")
            Culture = CulturesExtensions.TryParse(lang, true, out var result) ? result : Cultures.En;

        ListModeLayouts = new ListModeLayout[]
        {
            new(16, 5, 12, 20), new(16, 5, 12, 0), new(16, 5, 0, 0), new(8, 4, 0, 0),
        };

        if (Culture == Cultures.Ja)
            TimestampFormat = "yyyy/MM/dd HH:mm:ss";

        if (OperatingSystem.IsWindows())
        {
            TerminalApp = "cmd";
            TerminalAppOptions = "/K \"cd /d %CurrentFolder%\"";
        }
    }
}

public sealed record FolderWindowConfigData(int Id, int X, int Y, double Width, double Height);

public sealed record ListModeLayout(int Name, int Extension, int Size, int Timestamp);