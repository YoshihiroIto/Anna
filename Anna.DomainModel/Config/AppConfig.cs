using Anna.Constants;
using Anna.UseCase;
using System.Globalization;

namespace Anna.DomainModel.Config;

public class AppConfig : ConfigBase<AppConfigData>
{
    public AppConfig(IObjectSerializerUseCase objectSerializer)
        : base(objectSerializer)
    {
    }
}

public class AppConfigData : ConfigData
{
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


    #region Editor1Option

    private string _Editor1Option = "";

    public string Editor1Option
    {
        get => _Editor1Option;
        set => SetProperty(ref _Editor1Option, value);
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


    #region Editor2Option

    private string _Editor2Option = "";

    public string Editor2Option
    {
        get => _Editor2Option;
        set => SetProperty(ref _Editor2Option, value);
    }

    #endregion
    
    
    public (string Editor, string Option) FindEditor(int index)
    {
        return index switch
        {
            1 => (Editor1, Editor1Option),
            2 => (Editor2, Editor2Option),
            _ => throw new IndexOutOfRangeException()
        };
    }

    public override void SetDefault()
    {
        var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        if (string.IsNullOrEmpty(lang) == false)
            Culture = CulturesExtensions.TryParse(lang, true, out var result) ? result : Cultures.En;
    }
}