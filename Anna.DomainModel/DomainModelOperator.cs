using Anna.DomainModel.FileSystem;
using Anna.Service;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel;

public sealed class DomainModelOperator
{
    private readonly IServiceProvider _dic;

    public DomainModelOperator(IServiceProvider dic)
    {
        _dic = dic;
    }

    public Folder CreateFolder(string path)
    {
        return new FileSystemFolder(path,
            _dic.GetInstance<IBackgroundService>(),
            _dic.GetInstance<ILoggerService>(),
            _dic.GetInstance<IObjectLifetimeCheckerService>());
    }
}