using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Anna.Localization;
using Anna.Service;
using Avalonia.Input;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class JumpFolderDialogViewModel : HasModelWindowBaseViewModel<JumpFolderDialogViewModel,
    (string CurrentFolderPath, JumpFolderConfigData Config)>
{
    public string ResultPath { get; private set; } = "";

    public JumpFolderPathViewModel[] Paths { get; }

    public ReactivePropertySlim<JumpFolderPathViewModel> SelectedPath { get; }

    public string CurrentFolderPath => Model.CurrentFolderPath;

    public JumpFolderDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        Paths = Model.Config.Paths
            .Select(x => dic.GetInstance(JumpFolderPathViewModel.T, x).AddTo(Trash))
            .ToArray();

        SelectedPath = new ReactivePropertySlim<JumpFolderPathViewModel>(Paths[0]).AddTo(Trash);
    }

    public async void OnKeyDown(Key key)
    {
        switch (key)
        {
            case Key.Enter:
                await CloseAsync(SelectedPath.Value.Model.Path);
                return;

            case Key.Delete:
                {
                    if (SelectedPath.Value.Model.Path == "")
                        return;

                    using var viewModel = await Messenger.RaiseTransitionAsync(
                        ConfirmationDialogViewModel.T,
                        (Resources.AppName,
                            string.Format(Resources.Message_ConfirmDelete, SelectedPath.Value.Model.Path),
                            DialogResultTypes.Yes | DialogResultTypes.No),
                        MessageKey.Confirmation);

                    if (viewModel.DialogResult == DialogResultTypes.Yes)
                        SelectedPath.Value.Model.Path = "";

                    return;
                }
        }

        foreach (var path in Model.Config.Paths)
        {
            if (path.Key != key)
                continue;

            await CloseAsync(path.Path);
            return;
        }
    }

    private ValueTask<WindowActionMessage> CloseAsync(string path)
    {
        ResultPath = path;
        DialogResult = string.IsNullOrWhiteSpace(ResultPath) ? DialogResultTypes.Cancel : DialogResultTypes.Ok;

        return Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKey.Close));
    }
}

public sealed class JumpFolderPathViewModel
    : HasModelViewModelBase<JumpFolderPathViewModel, JumpFolderConfigData.PathData>
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