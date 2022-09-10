using Anna.DomainModel.FileSystem;
using Anna.DomainModel.Interface;
using Anna.Foundations;
using Anna.Interactor.Foundations;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anna.ViewModels;

public class DirectoryViewViewModel : ViewModelBase
{
    public ReadOnlyReactiveCollection<Entry> Entries { get; }

    public DirectoryViewViewModel(Directory directory, IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        lock (directory.UpdateLockObj)
        {
            Entries = directory.Entries.ToReadOnlyReactiveCollection().AddTo(Trash);
        }
    }
}

public class DesignDirectoryViewViewModel : DirectoryViewViewModel
{
    public DesignDirectoryViewViewModel() : base(new Directory(), new NopObjectLifetimeChecker())
    {
    }
}