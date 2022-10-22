using Anna.Constants;
using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Panels;
using Anna.Gui.Views.Windows.Base;
using Anna.Service;
using Reactive.Bindings.Extensions;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class DeleteEntryDialogViewModel
    : HasModelWindowBaseViewModel<(Entry[] Targets, EntriesStats Stats)>
{
    public EntryDeleteModes ResultMode { get; private set; } = EntryDeleteModes.Delete;

    public AsyncDelegateCommand ToTrashCanCommand { get; }

    public EntriesStatsPanelViewModel EntriesStatsPanelViewModel { get; }

    public DeleteEntryDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        ToTrashCanCommand = new AsyncDelegateCommand(async () =>
        {
            DialogResult = DialogResultTypes.Yes;
            ResultMode = EntryDeleteModes.TrashCan;

            await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
        });

        EntriesStatsPanelViewModel =
            dic.GetInstance<EntriesStatsPanelViewModel, EntriesStats>(Model.Stats).AddTo(Trash);
    }
}