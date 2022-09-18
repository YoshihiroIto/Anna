using Anna.DomainModel.FileSystem;
using Anna.DomainModel.Interface;
using SimpleInjector;

namespace Anna.DomainModel.Operator;

public class DomainModelOperator : IDomainModelOperator
{
    private readonly Container _dic;

    public DomainModelOperator(Container dic)
    {
        _dic = dic;
    }

    public Directory CreateDirectory(string path)
    {
        return new FileSystemDirectory(path,
        _dic.GetInstance<ILogger>(),
        _dic.GetInstance<IObjectLifetimeChecker>());
    }
}