using Anna.DomainModel.FileSystem;
using Anna.DomainModel.Interfaces;
using Anna.DomainModel.UseCases;
using SimpleInjector;

namespace Anna.DomainModel.Interactor;

public class DomainModelInteractor : IDomainModelUseCase
{
    private readonly Container _dic;

    public DomainModelInteractor(Container dic)
    {
        _dic = dic;
    }

    public Directory CreateDirectory(string path)
    {
        return new FileSystemDirectory(path, _dic.GetInstance<ILogger>(), _dic.GetInstance<IObjectLifetimeChecker>());
    }
}