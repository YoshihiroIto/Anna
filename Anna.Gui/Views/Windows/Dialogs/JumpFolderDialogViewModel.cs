﻿using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging;
using Anna.Gui.Views.Windows.Base;
using Anna.Service;
using Anna.Strings;
using Avalonia.Input;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Linq;

namespace Anna.Gui.Views.Windows.Dialogs;

public class JumpFolderDialogViewModel
    : HasModelWindowViewModelBase<(string CurrentFolderPath, JumpFolderConfigData Config)>
{
    public string ResultPath { get; private set; } = "";

    public JumpFolderPathViewModel[] Paths { get; }

    public ReactivePropertySlim<JumpFolderPathViewModel> SelectedPath { get; }

    public string CurrentFolderPath => Model.CurrentFolderPath;

    public JumpFolderDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        Paths = Model.Config.Paths
            .Select(x => dic.GetInstance<JumpFolderPathViewModel, JumpFolderConfigData.PathData>(x).AddTo(Trash))
            .ToArray();

        SelectedPath = new ReactivePropertySlim<JumpFolderPathViewModel>(Paths[0]).AddTo(Trash);
    }

    public async void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                ResultPath = SelectedPath.Value.Model.Path;
                DialogResult = DialogResultTypes.Ok;
                await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
                return;

            case Key.Delete:
                if (SelectedPath.Value.Model.Path == "")
                    return;

                var mes = await Messenger.RaiseAsync(
                    new ConfirmationMessage(
                        Resources.AppName,
                        string.Format(Resources.Message_ConfirmDelete, SelectedPath.Value.Model.Path),
                        ConfirmationTypes.YesNo,
                        MessageKeyYesNoConfirmation));

                if (mes.Response == DialogResultTypes.Yes)
                    SelectedPath.Value.Model.Path = "";

                return;
        }

        foreach (var path in Model.Config.Paths)
        {
            if (path.Key != e.Key)
                continue;

            ResultPath = path.Path;
            DialogResult = string.IsNullOrWhiteSpace(path.Path) ? DialogResultTypes.Cancel : DialogResultTypes.Ok;
            await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
            return;
        }
    }
}

public class JumpFolderPathViewModel : HasModelRefViewModelBase<JumpFolderConfigData.PathData>
{
    public string Key { get; }
    public ReactivePropertySlim<string> Path { get; }
    public ReactivePropertySlim<bool> IsEditing { get; }

    public JumpFolderPathViewModel(IServiceProvider dic)
        : base(dic)
    {
        var isNumber = Model.Key is >= Avalonia.Input.Key.D0 and <= Avalonia.Input.Key.D9;

        Key = (isNumber ? (Model.Key - Avalonia.Input.Key.D0).ToString() : Model.Key.ToString()) + " : ";
        Path = Model.ToReactivePropertySlimAsSynchronized(x => x.Path).AddTo(Trash);
        IsEditing = new ReactivePropertySlim<bool>().AddTo(Trash);
    }
}