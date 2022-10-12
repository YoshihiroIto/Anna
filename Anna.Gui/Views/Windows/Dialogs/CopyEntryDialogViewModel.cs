using Anna.DomainModel;
using Anna.Foundation;
using Anna.Gui.Foundations;
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

    public CopyEntryDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        DestFolder = new ReactivePropertySlim<string>("").AddTo(Trash);
        
        SelectedDestFolderHistory = new ReactivePropertySlim<int>(-1).AddTo(Trash);
        SelectedDestFolderHistory
            .Where(x => x != -1)
            .Subscribe(x => DestFolder.Value = Model.DestFoldersHistory[x])
            .AddTo(Trash);

        _OkCommand = new DelegateCommand(OnDecision);

        /////////////////////////////////////////////////////////////////
        var cts = new CancellationTokenSource();
        Trash.Add(() => cts.Cancel());

        var stats = dic.GetInstance<EntriesStats>()
            .Measure(Model.Targets, cts.Token)
            .AddTo(Trash);

        IsInMeasuring = stats.ObserveProperty(x => x.IsInMeasuring)
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(stats.IsInMeasuring)
            .AddTo(Trash);

        FileCount = stats.ObserveProperty(x => x.FileCount)
            .Sample(TimeSpan.FromMilliseconds(200))
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(stats.FileCount)
            .AddTo(Trash);

        FolderCount = stats.ObserveProperty(x => x.FolderCount)
            .Sample(TimeSpan.FromMilliseconds(200))
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(stats.FolderCount)
            .AddTo(Trash);

        AllSize = stats.ObserveProperty(x => x.AllSize)
            .Sample(TimeSpan.FromMilliseconds(200))
            .ObserveOnUIDispatcher()
            .ToReadOnlyReactivePropertySlim(stats.AllSize)
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