using Anna.DomainModel.FileSystem;
using Anna.UseCase;
using SimpleInjector;

namespace Anna.DomainModel;

public class DomainModelOperator
{
    private readonly Container _dic;

    public DomainModelOperator(Container dic)
    {
        _dic = dic;
    }

    public Directory CreateDirectory(string path)
    {
        return new FileSystemDirectory(path,
            _dic.GetInstance<ILoggerUseCase>(),
            _dic.GetInstance<IObjectLifetimeCheckerUseCase>());
    }
}