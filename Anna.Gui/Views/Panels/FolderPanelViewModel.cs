using Anna.Constants;
using Anna.DomainModel;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Hotkey;
using Anna.Gui.Interfaces;
using Anna.Gui.Messaging;
using Anna.Gui.ViewModels;
using Anna.Gui.Views.Windows;
using Anna.Gui.Views.Windows.Dialogs;
using Anna.Localization;
using Anna.Service.Services;
using Anna.Service.Workers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Panels;

public sealed class FolderPanelViewModel : HasModelViewModelBase<FolderPanelViewModel, Folder>, ILocalizableViewModel
{
    public ReadOnlyReactiveCollection<EntryViewModel> Entries { get; }
    public ReadOnlyReactivePropertySlim<EntryViewModel?> CursorEntry { get; }

    public Resources R => Dic.GetInstance<ResourcesHolder>().Instance;

    public readonly FolderPanelHotkey Hotkey;

    public ReactivePropertySlim<int> CursorIndex { get; }
    public ReactivePropertySlim<IntSize> ItemCellSize { get; }
    public IObservable<string> CurrentFolderPath { get; }

    public int NameCount => 16;
    public int ExtensionCount => 5;
    public int SizeCount => 10;

    private EntryViewModel? _oldEntry;
    private int _OnEntryExplicitlyCreatedRunning;

    private readonly bool _isBufferingUpdate = false;

    public FolderPanelViewModel(IServiceProvider dic)
        : base(dic)
    {
        Hotkey = dic.GetInstance<FolderPanelHotkey>().AddTo(Trash);

        CursorIndex = new ReactivePropertySlim<int>().AddTo(Trash);
        ItemCellSize = new ReactivePropertySlim<IntSize>().AddTo(Trash);
        CurrentFolderPath = Model.ObserveProperty(x => x.Path);

        Model.BackgroundWorkerExceptionThrown += OnBackgroundWorkerExceptionThrown;
        Trash.Add(() => Model.BackgroundWorkerExceptionThrown -= OnBackgroundWorkerExceptionThrown);

        Model.EntryExplicitlyCreated += OnEntryExplicitlyCreated;
        Trash.Add(() => Model.EntryExplicitlyCreated -= OnEntryExplicitlyCreated);

        var oldPath = Model.Path;

        Observable
            .FromEventPattern(
                h => Dic.GetInstance<ResourcesHolder>().CultureChanged += h,
                h => Dic.GetInstance<ResourcesHolder>().CultureChanged -= h)
            .Subscribe(_ => RaisePropertyChanged(nameof(R)))
            .AddTo(Trash);

        CursorEntry = CursorIndex
            .ObserveOnUIDispatcher()
            .Select(UpdateCursorEntry)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);

        lock (Model.EntriesUpdatingLockObj)
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
                        entry => dic.GetInstance<EntryViewModel, (Entry, int)>((entry, 0)))
                    .AddTo(Trash);
            }
            else
            {
                Entries = Model.Entries
                    .ToReadOnlyReactiveCollection(
                        entry => dic.GetInstance<EntryViewModel, (Entry, int)>((entry, 0)))
                    .AddTo(Trash);
            }

            Entries
                .CollectionChangedAsObservable()
                .Subscribe(_ =>
                    {
                        // If it is not a folder move, do nothing.
                        if (oldPath != Model.Path)
                        {
                            SetCurrentIndex(oldPath);
                            oldPath = Model.Path;
                        }
                    }
                )
                .AddTo(Trash);

            Entries
                .CollectionChangedAsObservable()
                .Throttle(TimeSpan.FromMilliseconds(50))
                .ObserveOnUIDispatcher()
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

        if (selectedEntries.Any())
            return selectedEntries.Select(x => x.Entry).ToArray();

        if (CursorEntry.Value is null)
            return Array.Empty<Entry>();

        if (CursorEntry.Value.Model.Entry.IsSelectable == false)
            return Array.Empty<Entry>();

        return new[] { CursorEntry.Value.Model.Entry };
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

        var entry = CursorEntry.Value.Model.Entry;
        if (entry.IsSelectable)
            entry.IsSelected = !entry.IsSelected;

        if (isMoveDown)
            MoveCursor(Directions.Down);
    }

    private EntryViewModel? UpdateCursorEntry(int index)
    {
        if (_oldEntry is not null)
            _oldEntry.IsOnCursor.Value = false;

        EntryViewModel? newEntry = null;

        if (index >= 0 && index < Entries.Count)
            newEntry = Entries[index];

        if (newEntry is not null)
            newEntry.IsOnCursor.Value = true;

        _oldEntry = newEntry;

        return newEntry;
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

    private void SetCurrentIndex(string path)
    {
        for (var i = 0; i != Entries.Count; ++i)
        {
            if (Entries[i].Model.Entry.Path != path)
                continue;

            CursorIndex.Value = i;
            return;
        }

        CursorIndex.Value = 0;
    }

    private async Task ForceSetCurrentIndex(string path)
    {
        const int timeOutOut = 50;

        int index = -1;

        for (var i = 0; i != timeOutOut; ++i)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            index = Model.IndexOfEntriesByPath(path);

            if (index != -1)
                break;

            await Task.Delay(1);
        }

        if (index == -1)
            return;

        CursorIndex.Value = index;
        CursorIndex.ForceNotify();
    }

    private async void OnBackgroundWorkerExceptionThrown(object? sender, ExceptionThrownEventArgs e)
    {
        using var _ = await Messenger.RaiseTransitionAsync(
            ConfirmationDialogViewModel.T,
            (
                Resources.AppName,
                e.Exception.Message,
                DialogResultTypes.Ok
            ),
            MessageKey.Confirmation);

        Dic.GetInstance<ILoggerService>().Warning(e.Exception.Message);
    }

    private async void OnEntryExplicitlyCreated(object? sender, EntryExplicitlyCreatedEventArgs e)
    {
        if (Interlocked.CompareExchange(ref _OnEntryExplicitlyCreatedRunning, 1, 0) != 0)
            return;

        try
        {
            await ForceSetCurrentIndex(e.Path);
        }
        finally
        {
            Interlocked.Exchange(ref _OnEntryExplicitlyCreatedRunning, 0);
        }
    }
}