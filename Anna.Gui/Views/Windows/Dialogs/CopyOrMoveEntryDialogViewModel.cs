using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Panels;
using Anna.Gui.Views.Windows.Base;
using Anna.Localization;
using Anna.Service.Services;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class CopyOrMoveEntryDialogViewModel : HasModelWindowBaseViewModel<CopyOrMoveEntryDialogViewModel,
    (CopyOrMove CopyOrMOve, string CurrentFolderPath, Entry[] Targets, EntriesStats Stats )>
{
    public string ResultDestFolder { get; private set; } = "";

    public string LeaderName => Model.Targets[0].NameWithExtension;

    public ReactivePropertySlim<string> DestFolder { get; }

    public ReadOnlyObservableCollection<string> DestFoldersHistory =>
        Dic.GetInstance<IFolderHistoryService>().DestinationFolders;

    public ReactivePropertySlim<int> SelectedDestFolderHistory { get; }

    public ICommand OpenJumpFolderDialogCommand { get; }
    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public EntriesStatsPanelViewModel EntriesStatsPanelViewModel { get; }

    public ReactivePropertySlim<string> Title { get; }

    private string CulturedTitle =>
        Model.CopyOrMOve == CopyOrMove.Copy
            ? Resources.DialogTitle_CopyEntry
            : Resources.DialogTitle_MoveEntry;

    public CopyOrMoveEntryDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
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

        Title = new ReactivePropertySlim<string>(CulturedTitle).AddTo(Trash);

        Observable
            .FromEventPattern(
                h => Dic.GetInstance<ResourcesHolder>().CultureChanged += h,
                h => Dic.GetInstance<ResourcesHolder>().CultureChanged -= h)
            .Subscribe(_ => Title.Value = CulturedTitle)
            .AddTo(Trash);
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