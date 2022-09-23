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

    public override void SetDefault()
    {
        var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        if (string.IsNullOrEmpty(lang) == false)
            Culture = Enum.TryParse<Cultures>(lang, true, out var result) ? result : Cultures.En;
    }
}