using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Strings;
using Anna.UseCase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using System.Linq;
using System.Reactive.Linq;

namespace Anna.Gui.Views.Panels;

public class InfoPanelViewModel : HasModelRefViewModelBase<Directory>, ILocalizableViewModel
{
    public Resources R => _resourcesHolder.Instance;

    public ReadOnlyReactivePropertySlim<int> SelectedEntriesCount { get; }
    public ReadOnlyReactivePropertySlim<int> EntriesCount { get; }

    public ReadOnlyReactivePropertySlim<long> SelectedTotalSize { get; }
    public ReadOnlyReactivePropertySlim<long> TotalSize { get; }

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

        SelectedEntriesCount = selectedEntries.CollectionChangedAsObservable()
            .Select(_ => selectedEntries.Count)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);

        EntriesCount = Observable
            .Merge(Observable.Return(0).ToUnit())
            .Merge(Model.Entries.CollectionChangedAsObservable().ToUnit())
            .Select(_ => Model.Entries.Count)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);

        SelectedTotalSize = Observable
            .Merge(selectedEntries.CollectionChangedAsObservable().ToUnit())
            .Merge(selectedEntries.ObserveElementProperty(x => x.Size).ToUnit())
            .Select(_ => selectedEntries
                .Select(x => x.Size)
                .Aggregate(0L, (sum, size) => sum + size)
            ).ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);

        TotalSize = Observable
            .Merge(Model.Entries.CollectionChangedAsObservable().ToUnit())
            .Merge(Model.Entries.ObserveElementProperty(x => x.Size).ToUnit())
            .Select(_ => Model.Entries
                .Select(x => x.Size)
                .Aggregate(0L, (sum, size) => sum + size)
            ).ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);
    }

    private readonly ResourcesHolder _resourcesHolder;
}