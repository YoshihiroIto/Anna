using Anna.DomainModel.FileSystem;
using Anna.UseCase;

namespace Anna.DomainModel;

public class DomainModelOperator
{
    private readonly IServiceProviderContainer _dic;

    public DomainModelOperator(IServiceProviderContainer dic)
    {
        _dic = dic;
    }

    public Folder CreateFolder(string path)
    {
        return new FileSystemFolder(path,
            _dic.GetInstance<ILoggerUseCase>(),
            _dic.GetInstance<IObjectLifetimeCheckerUseCase>());
    }
}