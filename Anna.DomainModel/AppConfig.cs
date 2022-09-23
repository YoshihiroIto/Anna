using Anna.Constants;
using Anna.Foundation;
using Anna.UseCase;
using System.Globalization;

namespace Anna.DomainModel;

public class AppConfig : NotificationObject
{
    #region FilePath

    private string _FilePath = "";

    public string FilePath
    {
        get => _FilePath;
        set => SetProperty(ref _FilePath, value);
    }

    #endregion

    #region ConfigData

    private AppConfigData _appConfigData = new();

    public AppConfigData AppConfigData
    {
        get => _appConfigData;
        set => SetProperty(ref _appConfigData, value);
    }

    #endregion

    public AppConfig(IObjectSerializerUseCase objectSerializer)
    {
        _objectSerializer = objectSerializer;
    }

    public void Load()
    {
        var result =_objectSerializer.Read(FilePath,
        () =>
        {
            var data = new AppConfigData();
            data.SetDefault();
            return data;
        });

        AppConfigData = result.obj;
    }
    public void Save()
    {
        _objectSerializer.Write(FilePath, AppConfigData);
    }

    private readonly IObjectSerializerUseCase _objectSerializer;
}

public class AppConfigData : NotificationObject
{
    #region Culture

    private Cultures _Culture;

    public Cultures Culture
    {
        get => _Culture;
        set => SetProperty(ref _Culture, value);
    }

    #endregion

    public void SetDefault()
    {
        var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        if (string.IsNullOrEmpty(lang) == false)
            Culture = Enum.TryParse<Cultures>(lang, true, out var result) ? result : Cultures.En;
    }
}