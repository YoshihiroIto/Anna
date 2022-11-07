﻿using Anna.Gui.Foundations;
using Anna.Service;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Globalization;
using System.IO;
using System.Reactive.Linq;
using Entry=Anna.DomainModel.Entry;

namespace Anna.Gui.ViewModels;

public sealed class EntryViewModel : HasModelViewModelBase<EntryViewModel, Entry>
{
    public bool IsFolder => Model.IsFolder;

    public ReactivePropertySlim<bool> IsOnCursor => _IsOnCursor ??= SetupIsOnCursor();
    public ReadOnlyReactivePropertySlim<string> NameWithExtension => _NameWithExtension ??= SetupNameWithExtension();
    public ReadOnlyReactivePropertySlim<string> Name => _Name ??= SetupName();
    public ReadOnlyReactivePropertySlim<string> Extension => _Extension ??= SetupExtension();
    public ReadOnlyReactivePropertySlim<string> Size => _Size ??= SetupSize();
    public ReadOnlyReactivePropertySlim<string> Timestamp => _Timestamp ??= SetupTimestamp();
    public ReadOnlyReactivePropertySlim<FileAttributes> Attributes => _Attributes ??= SetupAttributes();
    public ReadOnlyReactivePropertySlim<bool> IsSelected => _IsSelected ??= SetupIsSelected();

    private ReactivePropertySlim<bool>? _IsOnCursor;
    private ReadOnlyReactivePropertySlim<string>? _NameWithExtension;
    private ReadOnlyReactivePropertySlim<string>? _Name;
    private ReadOnlyReactivePropertySlim<string>? _Extension;
    private ReadOnlyReactivePropertySlim<string>? _Size;
    private ReadOnlyReactivePropertySlim<string>? _Timestamp;
    private ReadOnlyReactivePropertySlim<FileAttributes>? _Attributes;

    private ReadOnlyReactivePropertySlim<bool>? _IsSelected;

    public EntryViewModel(IServiceProvider dic)
        : base(dic)
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
            .Select(x => Model.IsFolder ? Path.AltDirectorySeparatorChar + x : x)
            .ToReadOnlyReactivePropertySlim(
                Model.IsFolder
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

    private ReadOnlyReactivePropertySlim<string> SetupSize()
    {
        return Model.ObserveProperty(x => x.Size)
            .ObserveOnUIDispatcher()
            .Select(x => "12345")
            .ToReadOnlyReactivePropertySlim("12345")
            .AddTo(Trash);
    }

    private ReadOnlyReactivePropertySlim<string> SetupTimestamp()
    {
        return Model.ObserveProperty(x => x.Timestamp)
            .ObserveOnUIDispatcher()
            .Select(x => x.ToString(CultureInfo.CurrentCulture))
            .ToReadOnlyReactivePropertySlim(Model.Timestamp.ToString(CultureInfo.CurrentCulture))
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