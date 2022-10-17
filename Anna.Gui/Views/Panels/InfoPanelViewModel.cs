using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Localization;
using Avalonia.Media;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using System;
using System.Reactive.Linq;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Panels;

public sealed class InfoPanelViewModel : HasModelRefViewModelBase<Folder>, ILocalizableViewModel
{
    public Resources R => Dic.GetInstance<ResourcesHolder>().Instance;

    public ReadOnlyReactivePropertySlim<int> EntriesCount { get; }
    public ReadOnlyReactivePropertySlim<int> SelectedEntriesCount { get; }

    public ReactiveProperty<long> TotalSize { get; }
    public ReactiveProperty<long> SelectedTotalSize { get; }
    public ReadOnlyReactivePropertySlim<bool> IsInProcessing { get; }
    public ReadOnlyReactivePropertySlim<double> Progress { get; }

    public ReadOnlyReactivePropertySlim<FontFamily> ViewerFontFamily { get; }
    public ReadOnlyReactivePropertySlim<double> ViewerFontSize { get; }

    public InfoPanelViewModel(IServiceProvider dic)
        : base(dic)
    {
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

        IsInProcessing = Model.BackgroundWorker
            .ObserveProperty(x => x.IsInProcessing)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);
        
        Progress = Model.BackgroundWorker
            .ObserveProperty(x => x.Progress)
            .Sample(TimeSpan.FromMilliseconds(100)) 
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);
        
        ViewerFontFamily = Dic.GetInstance<AppConfig>().Data
            .ObserveProperty(x => x.ViewerFontFamily)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash)!;

        ViewerFontSize = Dic.GetInstance<AppConfig>().Data
            .ObserveProperty(x => x.ViewerFontSize)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);

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

        lock (Model.EntriesUpdatingLockObj)
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
}