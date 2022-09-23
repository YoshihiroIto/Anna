using Anna.Constants;
using Anna.Foundation;
using Anna.UseCase;
using System.Globalization;

namespace Anna.DomainModel;

public class Config : NotificationObject
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

    private ConfigData _ConfigData = new();

    public ConfigData ConfigData
    {
        get => _ConfigData;
        set => SetProperty(ref _ConfigData, value);
    }

    #endregion

    public Config(IObjectSerializerUseCase objectSerializer)
    {
        _objectSerializer = objectSerializer;
    }

    public async ValueTask LoadAsync()
    {
        var result = await _objectSerializer.ReadAsync(FilePath,
        () =>
        {
            var configData = new ConfigData();
            configData.SetDefault();
            return configData;
        });

        ConfigData = result.obj;
    }
    public async ValueTask SaveAsync()
    {
        await _objectSerializer.WriteAsync(FilePath, ConfigData);
    }
    
    private readonly IObjectSerializerUseCase _objectSerializer;
}

public class ConfigData : NotificationObject
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