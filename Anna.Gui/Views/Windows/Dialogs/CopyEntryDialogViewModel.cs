﻿using Anna.DomainModel;
using Anna.Gui.Views.Windows.Base;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class CopyEntryDialogViewModel : HasModelWindowViewModelBase<Entry[]>
{
    public string LeaderName => Model[0].NameWithExtension;

    public ReadOnlyReactivePropertySlim<bool> IsSingleTarget { get; }

    public ReadOnlyReactivePropertySlim<int> FileCount { get; }
    public ReadOnlyReactivePropertySlim<int> FolderCount { get; }
    public ReadOnlyReactivePropertySlim<long> AllSize { get; }

    private EntriesStats Stats { get; }

    public CopyEntryDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        Stats = dic.GetInstance<EntriesStats>()
            .Measure(Model);

        FileCount = Stats.ObserveProperty(x => x.FileCount)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Stats.FileCount)
            .AddTo(Trash);

        FolderCount = Stats.ObserveProperty(x => x.FolderCount)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Stats.FolderCount)
            .AddTo(Trash);

        AllSize = Stats.ObserveProperty(x => x.AllSize)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Stats.AllSize)
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