using Anna.DomainModel.FileSystem;
using Anna.DomainModel.Interface;
using Anna.Foundations;
using Anna.Interactor.Foundations;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anna.ViewModels;

public class DirectoryViewViewModel : ViewModelBase
{
    // public ReadOnlyReactiveCollection<FileSystemEntry> Directories { get; }
    // public ReadOnlyReactiveCollection<FileSystemEntry> Files { get; }
    public ReadOnlyReactiveCollection<FileSystemEntry> DirectoriesAndFiles { get; }

    public DirectoryViewViewModel(Directory directory, IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        // Directories = directory.Directories.ToReadOnlyReactiveCollection().AddTo(Trash);
        // Files = directory.Files.ToReadOnlyReactiveCollection().AddTo(Trash);
        DirectoriesAndFiles = directory.DirectoriesAndFiles.ToReadOnlyReactiveCollection().AddTo(Trash);
    }
}

public class DesignDirectoryViewViewModel : DirectoryViewViewModel
{
    public DesignDirectoryViewViewModel() : base(new Directory(), new NopObjectLifetimeChecker())
    {
    }
}