using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.UseCase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.IO;
using System.Reactive.Linq;

namespace Anna.Gui.ViewModels;

public class EntryViewModel : ViewModelBase
{
    public bool IsDirectory => Model.IsDirectory;
    public bool IsSelectable => Model.IsParentDirectory == false;

    public ReadOnlyReactivePropertySlim<string> NameWithExtension { get; private set; } = null!;
    public ReadOnlyReactivePropertySlim<string> Name { get; private set; } = null!;
    public ReadOnlyReactivePropertySlim<string> Extension { get; private set; } = null!;
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

        NameWithExtension = Model.ObserveProperty(x => x.NameWithExtension)
            .ObserveOnUIDispatcher()
            .Select(x => Model.IsDirectory ? Path.AltDirectorySeparatorChar + x : x)
            .ToReadOnlyReactivePropertySlim(
                Model.IsDirectory
                    ? Path.AltDirectorySeparatorChar + Model.NameWithExtension
                    : Model.NameWithExtension)
            .AddTo(Trash);

        Name = Model.ObserveProperty(x => x.Name)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.Name)
            .AddTo(Trash);

        Extension = Model.ObserveProperty(x => x.Extension)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.Extension)
            .AddTo(Trash);

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