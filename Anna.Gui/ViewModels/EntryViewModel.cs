using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.UseCase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.IO;

namespace Anna.Gui.ViewModels;

public class EntryViewModel : ViewModelBase
{
    public string NameWithExtension => Model.IsDirectory
        ? Path.AltDirectorySeparatorChar + Model.NameWithExtension
        : Model.NameWithExtension;

    public string Name => Model.Name;
    public string Extension => Model.Extension;
    public bool IsDirectory => Model.IsDirectory;
    public bool IsSelectable => Model.IsParentDirectory == false;

    public ReadOnlyReactivePropertySlim<FileAttributes> Attributes { get; private set; } = null!;

    public ReactivePropertySlim<bool> IsSelected { get; private set; } = null!;
    public ReactivePropertySlim<bool> IsOnCursor { get; }

    public Entry Model { get; private set; } = null!;

    public EntryViewModel(IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        IsOnCursor = new ReactivePropertySlim<bool>().AddTo(Trash);
    }

    public EntryViewModel Setup(Entry model)
    {
        Model = model;

        Attributes = Model.ObserveProperty(x => x.Attributes)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.Attributes)
            .AddTo(Trash);
        
        IsSelected = model
            .ToReactivePropertySlimAsSynchronized(x => x.IsSelected)
            .AddTo(Trash);

        return this;
    }
}