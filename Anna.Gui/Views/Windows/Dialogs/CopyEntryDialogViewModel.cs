using Anna.Constants;
using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Panels;
using Anna.Gui.Views.Windows.Base;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class CopyEntryDialogViewModel
    : HasModelWindowBaseViewModel<
        (string CurrentFolderPath, Entry[] Targets, EntriesStats Stats, ReadOnlyObservableCollection<string>
        DestFoldersHistory)>
{
    public string ResultDestFolder { get; private set; } = "";

    public string LeaderName => Model.Targets[0].NameWithExtension;

    public ReactivePropertySlim<string> DestFolder { get; }
    public ReadOnlyObservableCollection<string> DestFoldersHistory => Model.DestFoldersHistory;
    public ReactivePropertySlim<int> SelectedDestFolderHistory { get; }

    public DelegateCommand OpenJumpFolderDialogCommand { get; }

    public EntriesStatsPanelViewModel EntriesStatsPanelViewModel { get; }

    public CopyEntryDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        DestFolder = new ReactivePropertySlim<string>("").AddTo(Trash);

        SelectedDestFolderHistory = new ReactivePropertySlim<int>(-1).AddTo(Trash);
        SelectedDestFolderHistory
            .Where(x => x != -1)
            .Subscribe(x => DestFolder.Value = Model.DestFoldersHistory[x])
            .AddTo(Trash);

        OpenJumpFolderDialogCommand = new DelegateCommand(OnJumpFolderDialog);
        _OkCommand = new AsyncReactiveCommand()
            .WithSubscribe(async x => await OnDecisionAsync())
            .AddTo(Trash);

        EntriesStatsPanelViewModel =
            dic.GetInstance<EntriesStatsPanelViewModel, EntriesStats>(Model.Stats).AddTo(Trash);
    }

    private async Task OnDecisionAsync()
    {
        if (DestFolder.Value == "")
        {
            var message =
                await Messenger.RaiseAsync(new SelectFolderMessage(Model.CurrentFolderPath, MessageKeySelectFolder));
            if (message.Response.DialogResult != DialogResultTypes.Ok)
                return;

            DestFolder.Value = message.Response.Path;
        }
        else
        {
            ResultDestFolder = DestFolder.Value;
            DialogResult = DialogResultTypes.Ok;

            await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
        }
    }

    public async void OnJumpFolderDialog()
    {
        var message = await Messenger.RaiseAsync(new JumpFolderMessage(MessageKeyJumpFolder));

        if (message.Response.DialogResult != DialogResultTypes.Ok)
            return;

        DestFolder.Value = message.Response.Path;
    }
}