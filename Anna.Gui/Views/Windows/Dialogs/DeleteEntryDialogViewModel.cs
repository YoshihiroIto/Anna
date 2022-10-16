using Anna.Constants;
using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Anna.Service;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class DeleteEntryDialogViewModel
    : HasModelWindowBaseViewModel<(Entry[] Targets, EntriesStats Stats)>
{
    public EntryDeleteModes ResultMode { get; private set; } = EntryDeleteModes.Delete;
    
    public DelegateCommand ToTrashCanCommand { get; }

    public DeleteEntryDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        ToTrashCanCommand = new DelegateCommand(() =>
        {
            DialogResult = DialogResultTypes.Yes;
            ResultMode = EntryDeleteModes.TrashCan;

            _ = Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
        });
    }
}