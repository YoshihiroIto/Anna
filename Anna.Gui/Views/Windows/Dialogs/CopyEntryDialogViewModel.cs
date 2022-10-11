﻿using Anna.DomainModel;
using Anna.Foundation;
using Anna.Gui.Messaging;
using Anna.Gui.Views.Windows.Base;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class CopyEntryDialogViewModel
    : HasModelWindowBaseViewModel<(Entry[] Targets, ReadOnlyObservableCollection<string> DestFoldersHistory)>
{
    public string ResultDestFolder { get; private set; } = "";

    public string LeaderName => Model.Targets[0].NameWithExtension;

    public ReadOnlyReactivePropertySlim<bool> IsSingleTarget { get; }
    public ReadOnlyReactivePropertySlim<bool> IsInMeasuring { get; }
    public ReadOnlyReactivePropertySlim<int> FileCount { get; }
    public ReadOnlyReactivePropertySlim<int> FolderCount { get; }
    public ReadOnlyReactivePropertySlim<long> AllSize { get; }

    public ReactivePropertySlim<string> DestFolder { get; }
    public ReadOnlyObservableCollection<string> DestFoldersHistory => Model.DestFoldersHistory;
    public ReactivePropertySlim<int> SelectedDestFolderHistory { get; }

    private EntriesStats Stats { get; }


    private readonly CancellationTokenSource _cts = new();

    public CopyEntryDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        DestFolder = new ReactivePropertySlim<string>().AddTo(Trash);
        SelectedDestFolderHistory = new ReactivePropertySlim<int>(-1).AddTo(Trash);

        SelectedDestFolderHistory
            .Subscribe(x =>
            {
                if (x != -1)
                    DestFolder.Value = Model.DestFoldersHistory[x];
            }).AddTo(Trash);

        ///////////////////////////////////////////////////////////////// 
        Trash.Add(() => _cts.Cancel());

        ///////////////////////////////////////////////////////////////// 
        Stats = dic.GetInstance<EntriesStats>()
            .Measure(Model.Targets, _cts.Token)
            .AddTo(Trash);

        IsInMeasuring = Stats.ObserveProperty(x => x.IsInMeasuring)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Stats.IsInMeasuring)
            .AddTo(Trash);

        FileCount = Stats.ObserveProperty(x => x.FileCount)
            .Sample(TimeSpan.FromMilliseconds(200))
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Stats.FileCount)
            .AddTo(Trash);

        FolderCount = Stats.ObserveProperty(x => x.FolderCount)
            .Sample(TimeSpan.FromMilliseconds(200))
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(Stats.FolderCount)
            .AddTo(Trash);

        AllSize = Stats.ObserveProperty(x => x.AllSize)
            .Sample(TimeSpan.FromMilliseconds(200))
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

    public async void OnDecision()
    {
        ResultDestFolder = DestFolder.Value;
        DialogResult = DialogResultTypes.Ok;
        
        await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
    }
}