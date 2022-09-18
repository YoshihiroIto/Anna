using Anna.DomainModel;
using Anna.DomainModel.Interfaces;
using Anna.Foundations;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.IO;

namespace Anna.ViewModels;

public class EntryViewModel : ViewModelBase
{
    public string NameWithExtension => _model?.NameWithExtension ?? throw new NullReferenceException();

    public string Name => _model?.Name ?? throw new NullReferenceException();

    public string Extension => _model?.Extension ?? throw new NullReferenceException();

    public bool IsDirectory => _model?.IsDirectory ?? throw new NullReferenceException();

    public ReadOnlyReactivePropertySlim<FileAttributes> Attributes { get; private set; } = null!;

    public ReactivePropertySlim<bool> IsSelected { get; }
    public ReactivePropertySlim<bool> IsOnCursor { get; }

    private Entry? _model;

    public EntryViewModel(IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        IsSelected = new ReactivePropertySlim<bool>().AddTo(Trash);
        IsOnCursor = new ReactivePropertySlim<bool>().AddTo(Trash);
    }

    public EntryViewModel Setup(Entry model)
    {
        _model = model;

        Attributes = _model.ObserveProperty(x => x.Attributes)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(_model.Attributes)
            .AddTo(Trash);

        return this;
    }
}