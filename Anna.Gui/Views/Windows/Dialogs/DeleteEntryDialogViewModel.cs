﻿using Anna.Constants;
using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Panels;
using Anna.Gui.Views.Windows.Base;
using Anna.Service;
using Anna.Service.Interfaces;
using Reactive.Bindings.Extensions;
using System.Windows.Input;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class DeleteEntryDialogViewModel : HasModelWindowBaseViewModel<DeleteEntryDialogViewModel,
    (IEntry[] Targets, EntriesStats Stats)>
{
    public EntryDeleteModes ResultMode { get; private set; } = EntryDeleteModes.Delete;

    public ICommand ToTrashCanCommand { get; }
    public ICommand YesCommand { get; }
    public ICommand NoCommand { get; }

    public EntriesStatsPanelViewModel EntriesStatsPanelViewModel { get; }

    public DeleteEntryDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        YesCommand = CreateButtonCommand(DialogResultTypes.Yes);
        NoCommand = CreateButtonCommand(DialogResultTypes.No);

        ToTrashCanCommand = new AsyncDelegateCommand(async () =>
        {
            DialogResult = DialogResultTypes.Yes;
            ResultMode = EntryDeleteModes.TrashCan;

            await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKey.Close));
        });

        EntriesStatsPanelViewModel = dic.GetInstance(EntriesStatsPanelViewModel.T, Model.Stats).AddTo(Trash);
    }
}