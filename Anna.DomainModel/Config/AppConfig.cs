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


    #region Culture

    private Cultures _Culture;

    public Cultures Culture
    {
        get => _Culture;
        set => SetProperty(ref _Culture, value);
    }

    #endregion


    #region Editor1

    private string _Editor1 = "";

    public string Editor1
    {
        get => _Editor1;
        set => SetProperty(ref _Editor1, value);
    }

    #endregion


    #region Editor1Options

    private string _editor1Options = "";

    public string Editor1Options
    {
        get => _editor1Options;
        set => SetProperty(ref _editor1Options, value);
    }

    #endregion


    #region Editor2

    private string _Editor2 = "";

    public string Editor2
    {
        get => _Editor2;
        set => SetProperty(ref _Editor2, value);
    }

    #endregion


    #region Editor2Options

    private string _editor2Options = "";

    public string Editor2Options
    {
        get => _editor2Options;
        set => SetProperty(ref _editor2Options, value);
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


    #region DestinationFolders

    private ObservableCollection<string> _DestinationFolders = new();

    public ObservableCollection<string> DestinationFolders
    {
        get => _DestinationFolders;
        set => SetProperty(ref _DestinationFolders, value);
    }

    #endregion


    public (string Editor, string Options) FindEditor(int index)
    {
        return index switch
        {
            1 => (Editor1, Editor1Options),
            2 => (Editor2, Editor2Options),
            _ => throw new IndexOutOfRangeException()
        };
    }

    public override void SetDefault()
    {
        var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        if (lang != "")
            Culture = CulturesExtensions.TryParse(lang, true, out var result) ? result : Cultures.En;
    }
}