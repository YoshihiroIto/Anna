using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Strings;
using Anna.UseCase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using System;
using System.Reactive.Linq;

namespace Anna.Gui.Views.Panels;

public class InfoPanelViewModel : HasModelRefViewModelBase<Directory>, ILocalizableViewModel
{
    public Resources R => _resourcesHolder.Instance;

    public ReadOnlyReactivePropertySlim<int> EntriesCount { get; }
    public ReadOnlyReactivePropertySlim<int> SelectedEntriesCount { get; }

    public ReactiveProperty<long> TotalSize { get; }
    public ReactiveProperty<long> SelectedTotalSize { get; }

    public InfoPanelViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, objectLifetimeChecker)
    {
        _resourcesHolder = resourcesHolder;

        var selectedEntries = Model.Entries
            .ToFilteredReadOnlyObservableCollection(x => x.IsSelected)
            .AddTo(Trash);

        var entrySizeChanged = Observable
            .FromEventPattern(
                h => Model.EntrySizeChanged += h,
                h => Model.EntrySizeChanged -= h);

        EntriesCount = Observable
            .Merge(Observable.Return(0).ToUnit())
            .Merge(Model.Entries.CollectionChangedAsObservable().ToUnit())
            .Select(_ => Model.Entries.Count - (Model.IsRoot ? 0 : 1))
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);

        SelectedEntriesCount = selectedEntries.CollectionChangedAsObservable()
            .Select(_ => selectedEntries.Count)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);

        TotalSize = new ReactiveProperty<long>().AddTo(Trash);
        SelectedTotalSize = new ReactiveProperty<long>().AddTo(Trash);

        Observable
            .Merge(Observable.Return(0).ToUnit())
            .Merge(selectedEntries.CollectionChangedAsObservable().ToUnit())
            .Merge(Model.Entries.CollectionChangedAsObservable().ToUnit())
            .Merge(entrySizeChanged.ToUnit())
            .Throttle(TimeSpan.FromMilliseconds(10))
            .Subscribe(_ => UpdateTotalSize())
            .AddTo(Trash);
    }

    private void UpdateTotalSize()
    {
        var totalSize = 0L;
        var selectedTotalSize = 0L;

        lock (Model.EntitiesUpdatingLockObj)
        {
            foreach (var entry in Model.Entries)
            {
                totalSize += entry.Size;

                if (entry.IsSelected)
                    selectedTotalSize += entry.Size;
            }
        }

        TotalSize.Value = totalSize;
        SelectedTotalSize.Value = selectedTotalSize;
    }

    private readonly ResourcesHolder _resourcesHolder;
}