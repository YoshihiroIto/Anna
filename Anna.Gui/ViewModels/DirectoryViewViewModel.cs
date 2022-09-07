using Anna.DomainModel.Interface;
using Anna.Foundations;
using Anna.Interactor.Foundations;

namespace Anna.ViewModels
{
    public class DirectoryViewViewModel : ViewModelBase
    {
        public DirectoryViewViewModel(IObjectLifetimeChecker objectLifetimeChecker)
            : base(objectLifetimeChecker)
        {
        }
    }

    public class DesignDirectoryViewViewModel : DirectoryViewViewModel
    {
        public DesignDirectoryViewViewModel() : base(new NopObjectLifetimeChecker())
        {
        }
    }
}