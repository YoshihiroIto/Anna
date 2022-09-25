﻿using Anna.DomainModel;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.ViewModels;
using Anna.Gui.ViewModels.ShortcutKey;
using Anna.Strings;
using Anna.UseCase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleInjector;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace Anna.Gui.Views;

public class DirectoryViewViewModel : ViewModelBase, ILocalizableViewModel
{
    public ReadOnlyReactiveCollection<EntryViewModel> Entries { get; private set; } = null!;
    public ReadOnlyReactivePropertySlim<EntryViewModel?> CursorEntry { get; private set; } = null!;

    public Directory Model { get; private set; } = null!;

    public Resources R => _resourcesHolder.Instance;

    public readonly ShortcutKeyManager ShortcutKeyManager;

    public ReactivePropertySlim<int> CursorIndex { get; }

    public ReactivePropertySlim<IntSize> ItemCellSize { get; }

    public DirectoryViewViewModel(
        Container dic,
        ResourcesHolder resourcesHolder,
        ShortcutKeyManager shortcutKeyManager,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _dic = dic;
        _resourcesHolder = resourcesHolder;
        ShortcutKeyManager = shortcutKeyManager;

        CursorIndex = new ReactivePropertySlim<int>().AddTo(Trash);
        CursorIndex.Subscribe(x =>
        {
            Debug.WriteLine("CursorIndexIndex:" + x);
        }).AddTo(Trash);

        ItemCellSize = new ReactivePropertySlim<IntSize>().AddTo(Trash);
        ItemCellSize.Subscribe(x =>
        {
            Debug.WriteLine("ItemCellSize:" + x);
        }).AddTo(Trash);
    }

    public DirectoryViewViewModel Setup(Directory model)
    {
        Model = model;

        Observable
            .FromEventPattern(
                h => _resourcesHolder.CultureChanged += h,
                h => _resourcesHolder.CultureChanged -= h)
            .Subscribe(_ => RaisePropertyChanged(nameof(R)))
            .AddTo(Trash);

        var bufferedCollectionChanged =
            model.Entries
                .ToCollectionChanged()
                .Buffer(TimeSpan.FromMilliseconds(50))
                .Where(x => x.Any())
                .SelectMany(x => x);

        lock (model.UpdateLockObj)
        {
            Entries = model.Entries.ToReadOnlyReactiveCollection(
                    bufferedCollectionChanged,
                    x => _dic.GetInstance<EntryViewModel>().Setup(x))
                .AddTo(Trash);
        }

        CursorEntry = CursorIndex.Select(UpdateCursorEntry)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);

        return this;
    }

    public Entry[] CollectTargetEntries()
    {
        var selectedEntries = Entries
            .Where(x => x.IsSelected.Value)
            .Select(x => x.Model)
            .ToArray();

        if (selectedEntries.Length > 0)
            return selectedEntries;

        return CursorEntry.Value is not null
            ? new[] { CursorEntry.Value.Model }
            : Array.Empty<Entry>();
    }

    private EntryViewModel? UpdateCursorEntry(int index)
    {
        if (_oldEntry != null)
            _oldEntry.IsOnCursor.Value = false;

        EntryViewModel? newEntity = null;

        if (index >= 0 && index < Entries.Count)
            newEntity = Entries[index];

        if (newEntity != null)
            newEntity.IsOnCursor.Value = true;

        _oldEntry = newEntity;

        return newEntity;
    }

    private readonly Container _dic;
    private readonly ResourcesHolder _resourcesHolder;
    private EntryViewModel? _oldEntry;
}