using Anna.DomainModel.FileSystem;
using Anna.DomainModel.Interface;
using Anna.Foundations;
using Anna.Interactor.Foundations;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anna.ViewModels;

public class DirectoryViewViewModel : ViewModelBase
{
    public ReadOnlyReactiveCollection<Entry>? Entries { get; private set; }

    public DirectoryViewViewModel(IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
    }

    public DirectoryViewViewModel Setup(Directory model)
    {
        lock (model.UpdateLockObj)
        {
            Entries = model.Entries.ToReadOnlyReactiveCollection().AddTo(Trash);
        }

        return this;
    }
}

public class DesignDirectoryViewViewModel : DirectoryViewViewModel
{
    public DesignDirectoryViewViewModel() : base(new NopObjectLifetimeChecker())
    {
        Setup(new Directory("C:/Windows/System32"));
    }
}