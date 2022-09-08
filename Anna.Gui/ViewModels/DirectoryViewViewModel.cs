using Anna.DomainModel.FileSystem;
using Anna.DomainModel.Interface;
using Anna.Foundations;
using Anna.Interactor.Foundations;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anna.ViewModels;

public class DirectoryViewViewModel : ViewModelBase
{
    public ReadOnlyReactiveCollection<FileSystemEntry> FileSystemEntries { get; }

    public DirectoryViewViewModel(Directory directory, IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        lock (directory.UpdateLockObj)
        {
            FileSystemEntries = directory.FileSystemEntries.ToReadOnlyReactiveCollection().AddTo(Trash);
        }
    }
}

public class DesignDirectoryViewViewModel : DirectoryViewViewModel
{
    public DesignDirectoryViewViewModel() : base(new Directory(), new NopObjectLifetimeChecker())
    {
    }
}