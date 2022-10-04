﻿using Anna.Constants;
using Anna.DomainModel;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.ViewModels;
using Anna.Gui.ViewModels.Messaging;
using Anna.Gui.ViewModels.ShortcutKey;
using Anna.Strings;
using Anna.UseCase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Anna.Gui.Views.Panels;

public class FolderPanelViewModel : HasModelRefViewModelBase<Folder>, ILocalizableViewModel
{
    public const string MessageKeyInformation = nameof(MessageKeyInformation);
    
    public ReadOnlyReactiveCollection<EntryViewModel> Entries { get; }
    public ReadOnlyReactivePropertySlim<EntryViewModel?> CursorEntry { get; }

    public Resources R => _resourcesHolder.Instance;

    public readonly ShortcutKeyManager ShortcutKeyManager;

    public ReactivePropertySlim<int> CursorIndex { get; }

    public ReactivePropertySlim<IntSize> ItemCellSize { get; }
    
    public FolderPanelViewModel(
        IServiceProviderContainer dic,
        IFolderServiceUseCase folderService,
        ResourcesHolder resourcesHolder,
        ShortcutKeyManager shortcutKeyManager,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, objectLifetimeChecker)
    {
        _folderService = folderService;
        _resourcesHolder = resourcesHolder;
        ShortcutKeyManager = shortcutKeyManager;
        _logger = logger;

        CursorIndex = new ReactivePropertySlim<int>().AddTo(Trash);
        ItemCellSize = new ReactivePropertySlim<IntSize>().AddTo(Trash);

        _oldPath = Model.Path;

        Observable
            .FromEventPattern(
                h => _resourcesHolder.CultureChanged += h,
                h => _resourcesHolder.CultureChanged -= h)
            .Subscribe(_ => RaisePropertyChanged(nameof(R)))
            .AddTo(Trash);

        CursorEntry = CursorIndex
            .ObserveOnUIDispatcher()
            .Select(UpdateCursorEntry)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);

        lock (Model.EntitiesUpdatingLockObj)
        {
            if (_isBufferingUpdate)
            {
                var bufferedCollectionChanged =
                    Model.Entries
                        .ToCollectionChanged()
                        .Buffer(TimeSpan.FromMilliseconds(50))
                        .Where(x => x.Any())
                        .SelectMany(x => x);

                Entries = Model.Entries
                    .ToReadOnlyReactiveCollection(
                        bufferedCollectionChanged,
                        dic.GetInstance<EntryViewModel, Entry>)
                    .AddTo(Trash);
            }
            else
            {
                Entries = Model.Entries
                    .ToReadOnlyReactiveCollection(dic.GetInstance<EntryViewModel, Entry>)
                    .AddTo(Trash);
            }

            Entries
                .CollectionChangedAsObservable()
                .Subscribe(_ =>
                    {
                        // If it is not a folder move, do nothing.
                        if (string.CompareOrdinal(_oldPath, Model.Path) != 0)
                        {
                            SetCurrentIndex(_oldPath);
                            _oldPath = Model.Path;
                        }
                    }
                )
                .AddTo(Trash);

            Entries
                .CollectionChangedAsObservable()
                .Throttle(TimeSpan.FromMilliseconds(50))
                .Subscribe(_ => UpdateCursorIndex(CursorEntry.Value))
                .AddTo(Trash);
        }
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

    public void MoveCursor(Directions dir)
    {
        var index = dir switch
        {
            Directions.Up => CursorIndex.Value - 1,
            Directions.Down => CursorIndex.Value + 1,
            Directions.Left => CursorIndex.Value - ItemCellSize.Value.Height,
            Directions.Right => CursorIndex.Value + ItemCellSize.Value.Height,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };

        index = Math.Clamp(index, 0, Entries.Count - 1);

        CursorIndex.Value = index;
    }

    public void ToggleSelectionCursorEntry(bool isMoveDown)
    {
        if (CursorEntry.Value is null)
            return;

        var entry = CursorEntry.Value.Model;
        if (entry.IsSelectable)
            entry.IsSelected = !entry.IsSelected;

        if (isMoveDown)
            MoveCursor(Directions.Down);
    }

    public ValueTask OpenCursorEntryAsync()
    {
        if (CursorEntry.Value is null)
            return ValueTask.CompletedTask;

        if (CursorEntry.Value.IsFolder)
            return JumpToFolderAsync(CursorEntry.Value.Model.Path);
        else
            _logger.Information("Not implemented: OpenCursorEntry");

        return ValueTask.CompletedTask;
    }

    public ValueTask JumpToFolderAsync(string path)
    {
        if (_folderService.IsAccessible(path) == false)
        {
            Messenger.Raise(new InformationMessage(Resources.AppName, string.Format(Resources.Message_AccessDenied, path), MessageKeyInformation));
            return ValueTask.CompletedTask;
        }

        _oldPath = Model.Path;
        Model.Path = PathStringHelper.Normalize(path);

        return ValueTask.CompletedTask;
    }

    private string _oldPath;

    private EntryViewModel? UpdateCursorEntry(int index)
    {
        if (_oldEntry is not null)
            _oldEntry.IsOnCursor.Value = false;

        EntryViewModel? newEntity = null;

        if (index >= 0 && index < Entries.Count)
            newEntity = Entries[index];

        if (newEntity is not null)
            newEntity.IsOnCursor.Value = true;

        _oldEntry = newEntity;

        return newEntity;
    }

    private void UpdateCursorIndex(EntryViewModel? entry)
    {
        if (entry is null)
            return;

        var index = Entries.IndexOf(entry);
        if (index == -1)
        {
            // If no entry is found under the cursor,
            // the entry at the current cursor position is assumed to be under the cursor.

            index = Math.Clamp(CursorIndex.Value, 0, Entries.Count - 1);

            CursorIndex.Value = index;
            CursorIndex.ForceNotify();

            return;
        }

        index = Math.Clamp(index, 0, Entries.Count - 1);
        CursorIndex.Value = index;
    }

    private void SetCurrentIndex(string targetPath)
    {
        for (var i = 0; i != Entries.Count; ++i)
        {
            if (string.CompareOrdinal(Entries[i].Model.Path, targetPath) != 0)
                continue;

            CursorIndex.Value = i;
            return;
        }

        CursorIndex.Value = 0;
    }

    private readonly ILoggerUseCase _logger;
    private readonly IFolderServiceUseCase _folderService;
    private readonly ResourcesHolder _resourcesHolder;
    private EntryViewModel? _oldEntry;

    private readonly bool _isBufferingUpdate = false;
}