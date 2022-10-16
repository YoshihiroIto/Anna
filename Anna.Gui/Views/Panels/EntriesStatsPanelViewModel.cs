using Anna.DomainModel;
using Anna.Gui.Foundations;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Panels;

public sealed class EntriesStatsPanelViewModel : HasModelRefViewModelBase<EntriesStats>
{
    public ReadOnlyReactivePropertySlim<bool> IsSingleTarget { get; }
    public ReadOnlyReactivePropertySlim<bool> IsInMeasuring { get; }
    public ReadOnlyReactivePropertySlim<int> FileCount { get; }
    public ReadOnlyReactivePropertySlim<int> FolderCount { get; }
    public ReadOnlyReactivePropertySlim<long> AllSize { get; }
    
    public EntriesStatsPanelViewModel(IServiceProvider dic)
        : base(dic)
    {
        IsInMeasuring = Model.ObserveProperty(x => x.IsInMeasuring)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.IsInMeasuring)
            .AddTo(Trash);

        FileCount = Model.ObserveProperty(x => x.FileCount)
            .Sample(TimeSpan.FromMilliseconds(200))
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.FileCount)
            .AddTo(Trash);

        FolderCount = Model.ObserveProperty(x => x.FolderCount)
            .Sample(TimeSpan.FromMilliseconds(200))
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.FolderCount)
            .AddTo(Trash);

        AllSize = Model.ObserveProperty(x => x.AllSize)
            .Sample(TimeSpan.FromMilliseconds(200))
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Model.AllSize)
            .AddTo(Trash);
        
        IsSingleTarget = Observable
            .Merge(FileCount)
            .Merge(FolderCount)
            .Select(x => FileCount.Value + FolderCount.Value == 1)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(FileCount.Value + FolderCount.Value == 1)
            .AddTo(Trash);
    }
}