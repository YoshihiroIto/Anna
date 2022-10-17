using Anna.Foundation;
using Anna.Service.Services;

namespace Anna.DomainModel.Config;

public class ConfigBase<T> : NotificationObject
    where T : ConfigData, new()
{
    #region FilePath

    private string _FilePath = "";

    public string FilePath
    {
        get => _FilePath;
        set => SetProperty(ref _FilePath, value);
    }

    #endregion

    #region Data

    private T _data = new();

    public T Data
    {
        get => _data;
        set => SetProperty(ref _data, value);
    }

    #endregion
    
    private readonly IObjectSerializerService _objectSerializer;

    public ConfigBase(IObjectSerializerService objectSerializer)
    {
        _objectSerializer = objectSerializer;
    }

    public void Load()
    {
        var result = _objectSerializer.Read(FilePath,
            () =>
            {
                var data = new T();
                data.SetDefault();
                return data;
            });

        Data = result.obj;
    }
    public void Save()
    {
        _objectSerializer.Write(FilePath, Data);
    }
}

public abstract class ConfigData : NotificationObject
{
    public abstract void SetDefault();
}