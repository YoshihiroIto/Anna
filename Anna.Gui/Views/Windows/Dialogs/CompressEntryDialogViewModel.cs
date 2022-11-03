using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Panels;
using Anna.Gui.Views.Windows.Base;
using Anna.Service.Services;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class CompressEntryDialogViewModel : HasModelWindowBaseViewModel<CompressEntryDialogViewModel,
    (string CurrentFolderPath, Entry[] Targets, EntriesStats Stats )>
{
    public string ResultDestArchiveName { get; private set; } = "";
    public string ResultDestFolder { get; private set; } = "";

    public string LeaderName => Model.Targets[0].NameWithExtension;

    public ReactivePropertySlim<string> DestArchiveName { get; }
    public ReactivePropertySlim<string> DestFolder { get; }

    public ReadOnlyObservableCollection<string> DestFoldersHistory =>
        Dic.GetInstance<IFolderHistoryService>().DestinationFolders;

    public ReactivePropertySlim<int> SelectedDestFolderHistory { get; }

    public ICommand OpenJumpFolderDialogCommand { get; }
    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public EntriesStatsPanelViewModel EntriesStatsPanelViewModel { get; }

    public CompressEntryDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        DestArchiveName = new ReactivePropertySlim<string>(Path.ChangeExtension(LeaderName, ".zip")).AddTo(Trash);
        DestFolder = new ReactivePropertySlim<string>("").AddTo(Trash);

        SelectedDestFolderHistory = new ReactivePropertySlim<int>(-1).AddTo(Trash);
        SelectedDestFolderHistory
            .Where(x => x != -1)
            .Subscribe(x => DestFolder.Value = DestFoldersHistory[x])
            .AddTo(Trash);

        OpenJumpFolderDialogCommand = new AsyncReactiveCommand()
            .WithSubscribe(OnJumpFolderDialogSync)
            .AddTo(Trash);

        OkCommand = new AsyncReactiveCommand()
            .WithSubscribe(OnDecisionAsync)
            .AddTo(Trash);

        CancelCommand = CreateButtonCommand(DialogResultTypes.Cancel);

        EntriesStatsPanelViewModel = dic.GetInstance(EntriesStatsPanelViewModel.T, Model.Stats).AddTo(Trash);
    }

    private async Task OnDecisionAsync()
    {
        if (DestFolder.Value == "")
        {
            using var viewModel = await Messenger.RaiseTransitionAsync(
                SelectFolderDialogViewModel.T,
                (
                    Model.CurrentFolderPath,
                    0
                ),
                MessageKey.SelectFolder);

            DestFolder.Value = viewModel.ResultPath;
        }
        else
        {
            ResultDestArchiveName =
                DestArchiveName.Value.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                    ? DestArchiveName.Value
                    : DestArchiveName.Value + ".zip";
            
            ResultDestFolder = DestFolder.Value;
            DialogResult = DialogResultTypes.Ok;

            await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKey.Close));
        }
    }

    public async Task OnJumpFolderDialogSync()
    {
        using var viewModel = await Messenger.RaiseTransitionAsync(
            JumpFolderDialogViewModel.T,
            (
                Model.CurrentFolderPath,
                Dic.GetInstance<JumpFolderConfig>().Data
            ),
            MessageKey.JumpFolder);

        if (viewModel.DialogResult != DialogResultTypes.Ok)
            return;

        DestFolder.Value = viewModel.ResultPath;
    }
}