using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.UseCase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.IO;
using System.Reactive.Linq;

namespace Anna.Gui.ViewModels;

public class EntryViewModel : HasModelRefViewModelBase<Entry>
{
    public bool IsDirectory => Model.IsDirectory;
    public bool IsSelectable => Model.IsParentDirectory == false;

    public ReadOnlyReactivePropertySlim<string> NameWithExtension { get; }
    public ReadOnlyReactivePropertySlim<string> Name { get; }
    public ReadOnlyReactivePropertySlim<string> Extension { get; }
    public ReadOnlyReactivePropertySlim<FileAttributes> Attributes { get; }

    public ReactivePropertySlim<bool> IsSelected { get; }
    public ReactivePropertySlim<bool> IsOnCursor { get; }

    public EntryViewModel(
        IServiceProviderContainer dic,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, objectLifetimeChecker)
    {
        IsOnCursor = new ReactivePropertySlim<bool>().AddTo(Trash);

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

        IsSelected = Model
            .ToReactivePropertySlimAsSynchronized(x => x.IsSelected)
            .AddTo(Trash);
    }
}