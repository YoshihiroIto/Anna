using Anna.Gui.Foundations;
using Anna.UseCase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.IO;
using System.Reactive.Linq;
using Entry=Anna.DomainModel.Entry;

namespace Anna.Gui.ViewModels;

public class EntryViewModel : HasModelRefViewModelBase<Entry>
{
    public bool IsDirectory => Model.IsDirectory;

    public ReactivePropertySlim<bool> IsOnCursor => _IsOnCursor ??= SetupIsOnCursor();
    public ReadOnlyReactivePropertySlim<string> NameWithExtension => _NameWithExtension ??= SetupNameWithExtension();
    public ReadOnlyReactivePropertySlim<string> Name => _Name ??= SetupName();
    public ReadOnlyReactivePropertySlim<string> Extension => _Extension ??= SetupExtension();
    public ReadOnlyReactivePropertySlim<FileAttributes> Attributes => _Attributes ??= SetupAttributes();
    public ReadOnlyReactivePropertySlim<bool> IsSelected => _IsSelected ??= SetupIsSelected();

    private ReactivePropertySlim<bool>? _IsOnCursor;
    private ReadOnlyReactivePropertySlim<string>? _NameWithExtension;
    private ReadOnlyReactivePropertySlim<string>? _Name;
    private ReadOnlyReactivePropertySlim<string>? _Extension;
    private ReadOnlyReactivePropertySlim<FileAttributes>? _Attributes;
    private ReadOnlyReactivePropertySlim<bool>? _IsSelected;

    public EntryViewModel(
        IServiceProviderContainer dic,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, objectLifetimeChecker)
    {
    }

    private ReactivePropertySlim<bool> SetupIsOnCursor()
    {
        return new ReactivePropertySlim<bool>().AddTo(Trash);
    }

    private ReadOnlyReactivePropertySlim<string> SetupNameWithExtension()
    {
        return Model.ObserveProperty(x => x.NameWithExtension)
            .ObserveOnUIDispatcher()
            .Select(x => Model.IsDirectory ? Path.AltDirectorySeparatorChar + x : x)
            .ToReadOnlyReactivePropertySlim(
                Model.IsDirectory
                    ? Path.AltDirectorySeparatorChar + Model.NameWithExtension
                    : Model.NameWithExtension)
            .AddTo(Trash);
    }

    private ReadOnlyReactivePropertySlim<string> SetupName()
    {
        return Model.ObserveProperty(x => x.Name)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.Name)
            .AddTo(Trash);
    }

    private ReadOnlyReactivePropertySlim<string> SetupExtension()
    {
        return Model.ObserveProperty(x => x.Extension)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.Extension)
            .AddTo(Trash);
    }

    private ReadOnlyReactivePropertySlim<FileAttributes> SetupAttributes()
    {
        return Model.ObserveProperty(x => x.Attributes)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.Attributes)
            .AddTo(Trash);
    }

    private ReadOnlyReactivePropertySlim<bool> SetupIsSelected()
    {
        return Model
            .ObserveProperty(x => x.IsSelected)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);
    }
}