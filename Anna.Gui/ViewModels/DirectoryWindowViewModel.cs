using Anna.DomainModel.Interface;
using Anna.Foundations;
using Reactive.Bindings.Extensions;
using SimpleInjector;

namespace Anna.ViewModels;

public class DirectoryWindowViewModel : ViewModelBase
{
    public DirectoryViewViewModel ViewViewModel { get; }
        
    public DirectoryWindowViewModel(Container dic, IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        ViewViewModel = dic.GetInstance<DirectoryViewViewModel>().AddTo(Trash);
    }
}