using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.ViewModels.Messaging;
using Anna.Gui.Views.Dialogs.Base;
using Anna.UseCase;
using Avalonia.Input;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Linq;

namespace Anna.Gui.Views.Dialogs;

public class JumpFolderDialogViewModel
    : HasModelDialogViewModel<(string CurrentFolderPath, JumpFolderConfigData Config)>
{
    public string ResultPath { get; set; } = "";

    public JumpFolderPathViewModel[] Paths { get; }

    public ReactivePropertySlim<JumpFolderPathViewModel> SelectedPath { get; }

    public JumpFolderDialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, resourcesHolder, logger, objectLifetimeChecker)
    {
        Paths = Model.Config.Paths
            .Select(x => dic.GetInstance<JumpFolderPathViewModel, JumpFolderConfigData.PathData>(x).AddTo(Trash))
            .ToArray();

        SelectedPath = new ReactivePropertySlim<JumpFolderPathViewModel>(Paths[0]).AddTo(Trash);
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ResultPath = SelectedPath.Value.Model.Path;
            DialogResult = DialogResultTypes.Ok;
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
            return;
        }

        foreach (var path in Model.Config.Paths)
        {
            if (path.Key != e.Key)
                continue;

            ResultPath = path.Path;
            DialogResult = string.IsNullOrWhiteSpace(path.Path) ? DialogResultTypes.Cancel : DialogResultTypes.Ok;
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
            return;
        }
    }
}

public class JumpFolderPathViewModel : HasModelRefViewModelBase<JumpFolderConfigData.PathData>
{
    public string Key { get; }
    public ReactivePropertySlim<string> Path { get; }

    public JumpFolderPathViewModel(
        IServiceProviderContainer dic,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, objectLifetimeChecker)
    {
        var isNumber = Model.Key is >= Avalonia.Input.Key.D0 and <= Avalonia.Input.Key.D9;

        Key = (isNumber ? (Model.Key - Avalonia.Input.Key.D0).ToString() : Model.Key.ToString()) + " : ";
        Path = Model.ToReactivePropertySlimAsSynchronized(x => x.Path).AddTo(Trash);
    }
}