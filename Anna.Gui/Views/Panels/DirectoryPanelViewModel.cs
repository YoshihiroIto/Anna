using Anna.Constants;
using Anna.DomainModel;
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
using System.Linq;
using System.Reactive.Linq;

namespace Anna.Gui.Views.Panels;

public class DirectoryPanelViewModel : ViewModelBase, ILocalizableViewModel
{
    public ReadOnlyReactiveCollection<EntryViewModel> Entries { get; private set; } = null!;
    public ReadOnlyReactivePropertySlim<EntryViewModel?> CursorEntry { get; private set; } = null!;

    public Directory Model { get; private set; } = null!;
    public Resources R => _resourcesHolder.Instance;

    public readonly ShortcutKeyManager ShortcutKeyManager;

    public ReactivePropertySlim<int> CursorIndex { get; }

    public ReactivePropertySlim<IntSize> ItemCellSize { get; }

    public DirectoryPanelViewModel(
        Container dic,
        ResourcesHolder resourcesHolder,
        ShortcutKeyManager shortcutKeyManager,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _dic = dic;
        _resourcesHolder = resourcesHolder;
        ShortcutKeyManager = shortcutKeyManager;
        _logger = logger;

        CursorIndex = new ReactivePropertySlim<int>().AddTo(Trash);
        ItemCellSize = new ReactivePropertySlim<IntSize>().AddTo(Trash);
    }

    public DirectoryPanelViewModel Setup(Directory model)
    {
        Model = model;
        _oldPath = Model.Path;

        Observable
            .FromEventPattern(
                h => _resourcesHolder.CultureChanged += h,
                h => _resourcesHolder.CultureChanged -= h)
            .Subscribe(_ => RaisePropertyChanged(nameof(R)))
            .AddTo(Trash);

        lock (model.UpdateLockObj)
        {
            if (_isBufferingUpdate)
            {
                var bufferedCollectionChanged =
                    model.Entries
                        .ToCollectionChanged()
                        .Buffer(TimeSpan.FromMilliseconds(50))
                        .Where(x => x.Any())
                        .SelectMany(x => x);

                Entries = model.Entries
                    .ToReadOnlyReactiveCollection(
                        bufferedCollectionChanged,
                        x => _dic.GetInstance<EntryViewModel>().Setup(x))
                    .AddTo(Trash);
            }
            else
            {
                Entries = model.Entries
                    .ToReadOnlyReactiveCollection(x => _dic.GetInstance<EntryViewModel>().Setup(x))
                    .AddTo(Trash);
            }

            Entries
                .CollectionChangedAsObservable()
                .Subscribe(_ =>
                    {
                        UpdateCursorIndex(CursorEntry.Value);

                        // If it is not a directory move, do nothing.
                        if (string.CompareOrdinal(_oldPath, Model.Path) != 0)
                        {
                            SetCurrentIndex(_oldPath);
                            _oldPath = Model.Path;
                        }
                    }
                )
                .AddTo(Trash);
        }

        CursorEntry = CursorIndex
            .ObserveOnUIDispatcher()
            .Select(UpdateCursorEntry)
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
        if (CursorEntry.Value == null)
            return;

        if (CursorEntry.Value.IsSelectable)
            CursorEntry.Value.IsSelected.Value = !CursorEntry.Value.IsSelected.Value;

        if (isMoveDown)
            MoveCursor(Directions.Down);
    }

    public void OpenCursorEntry()
    {
        if (CursorEntry.Value == null)
            return;

        if (CursorEntry.Value.IsDirectory)
            JumpToDirectory(CursorEntry.Value.Model.Path);
        else
            _logger.Information("Not implemented: OpenCursorEntry");
    }

    public void JumpToDirectory(string path)
    {
        _oldPath = Model.Path;
        Model.Path = PathStringHelper.Normalize(path);
    }

    private string _oldPath = "";

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

    private void UpdateCursorIndex(EntryViewModel? entry)
    {
        if (entry == null)
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

    private readonly Container _dic;
    private readonly ILoggerUseCase _logger;
    private readonly ResourcesHolder _resourcesHolder;
    private EntryViewModel? _oldEntry;
    
    private readonly bool _isBufferingUpdate = false;
}