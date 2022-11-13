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
    private readonly IDefaultValueService _defaultValue;

    public ConfigBase(IObjectSerializerService objectSerializer, IDefaultValueService defaultValue)
    {
        _objectSerializer = objectSerializer;
        _defaultValue = defaultValue;
    }

    public void Load()
    {
        var result = _objectSerializer.Read(FilePath,
            () =>
            {
                var data = new T();
                data.SetDefault(_defaultValue);
                return data;
            });

        Data = result.obj;
        
        Loaded();
    }
    
    public void Save()
    {
        _objectSerializer.Write(FilePath, Data);
    }
    
    public virtual void Loaded(){}
}

public abstract class ConfigData : NotificationObject
{
    public abstract void SetDefault(IDefaultValueService defaultValue);
}