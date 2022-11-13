using Anna.Foundation;
using Anna.Service.Services;
using IServiceProvider=Anna.Service.IServiceProvider;

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
    
    private readonly IServiceProvider _dic;
    
    public ConfigBase(IServiceProvider dic)
    {
        _dic = dic;
    }

    public void Load()
    {
        var result = _dic.GetInstance<IObjectSerializerService>().Read(FilePath,
            () =>
            {
                var data = new T();
                data.SetDefault(_dic.GetInstance<IDefaultValueService>());
                return data;
            });

        Data = result.obj;
        
        Loaded();
    }
    
    public void Save()
    {
        _dic.GetInstance<IObjectSerializerService>().Write(FilePath, Data);
    }
    
    public virtual void Loaded(){}
}

public abstract class ConfigData : NotificationObject
{
    public abstract void SetDefault(IDefaultValueService defaultValue);
}