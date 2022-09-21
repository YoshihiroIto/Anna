﻿using Anna.DomainModel;
using Anna.DomainModel.Interfaces;
using Anna.Gui.Foundations;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.IO;

namespace Anna.Gui.ViewModels;

public class EntryViewModel : ViewModelBase
{
    public string NameWithExtension => Model.NameWithExtension;
    public string Name => Model.Name;
    public string Extension => Model.Extension;
    public bool IsDirectory => Model.IsDirectory;

    public ReadOnlyReactivePropertySlim<FileAttributes> Attributes { get; private set; } = null!;

    public ReactivePropertySlim<bool> IsSelected { get; }
    public ReactivePropertySlim<bool> IsOnCursor { get; }

    public Entry Model { get; private set; } = null!;

    public EntryViewModel(IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        IsSelected = new ReactivePropertySlim<bool>().AddTo(Trash);
        IsOnCursor = new ReactivePropertySlim<bool>().AddTo(Trash);
    }

    public EntryViewModel Setup(Entry model)
    {
        Model = model;

        Attributes = Model.ObserveProperty(x => x.Attributes)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.Attributes)
            .AddTo(Trash);

        IsSelected.Value = Model.Name.Contains("a");
        IsOnCursor.Value = Model.Name.Contains("b");

        return this;
    }
}