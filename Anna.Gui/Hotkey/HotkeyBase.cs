using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.Foundation;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows;
using Anna.Gui.Views.Windows.Dialogs;
using Anna.Localization;
using Anna.Service.Services;
using Avalonia.Input;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Hotkey;

public abstract class HotkeyBase : DisposableNotificationObject
{
    protected abstract IReadOnlyDictionary<Operations, Func<IHotkeyReceiver, ValueTask>> SetupOperators();

    private readonly Dictionary<(Key, KeyModifiers), Func<IHotkeyReceiver, ValueTask>> _hotkeys = new();
    private readonly IReadOnlyDictionary<Operations, Func<IHotkeyReceiver, ValueTask>> _operators;

    protected HotkeyBase(IServiceProvider dic)
        : base(dic)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        _operators = SetupOperators();

        dic.GetInstance<KeyConfig>().ObserveProperty(x => x.Data)
            .Subscribe(UpdateHotkeys)
            .AddTo(Trash);
    }

    public ValueTask OnKeyDownAsync(IHotkeyReceiver receiver, KeyEventArgs e)
    {
        var k = (e.Key, e.KeyModifiers);

        if (_hotkeys.TryGetValue(k, out var value) == false)
            return ValueTask.CompletedTask;

        e.Handled = true;
        return value(receiver);
    }

    private void UpdateHotkeys(KeyConfigData keyConfig)
    {
        _hotkeys.Clear();

        foreach (var key in keyConfig.Keys)
        {
            Register(
                key.Key,
                key.Modifier,
                async x =>
                {
                    if (_operators.TryGetValue(key.Operation, out var value) == false)
                        return;

                    await value(x);
                });
        }
    }

    private void Register(Key key, KeyModifiers modifierKeys, Func<IHotkeyReceiver, ValueTask> action)
    {
        var k = (key, modifierKeys);

        if (_hotkeys.ContainsKey(k))
        {
            Dic.GetInstance<ILoggerService>().Warning("Already registered");
            return;
        }

        _hotkeys[k] = action;
    }

    protected async ValueTask<bool> CheckIsAccessibleAsync(string path, Messenger messenger)
    {
        if (Dic.GetInstance<IFileSystemIsAccessibleService>().IsAccessible(path))
            return true;

        using var viewModel = await messenger.RaiseTransitionAsync(
            ConfirmationDialogViewModel.T,
            (
                Resources.AppName,
                string.Format(Resources.Message_AccessDenied, path),
                DialogResultTypes.Ok
            ),
            MessageKey.Confirmation);

        return false;
    }

    protected async ValueTask OpenAppAsync(AppConfigData.ExternalApp app,
        string targetFilePath, string targetFileFolder, int targetLineIndex, Messenger messenger)
    {
        var editor = Dic.GetInstance<AppConfig>().Data.FindExternalApp(app);
        var arguments = ProcessHelper.MakeEditorArguments(editor.Options, targetFilePath, targetFileFolder, targetLineIndex);

        try
        {
            ProcessHelper.Execute(editor.ExternalApp, arguments);

            // close preview dialog
            await messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKey.Close));
        }
        catch
        {
            Dic.GetInstance<ILoggerService>()
                .Warning($"{nameof(HotkeyBase)}.{nameof(OpenAppAsync)}: FailedToStartEditor, {app}, {targetFilePath}");

            using var _ = await messenger.RaiseTransitionAsync(
                ConfirmationDialogViewModel.T,
                (
                    Resources.AppName,
                    string.Format(Resources.Message_FailedToStartEditor, editor.ExternalApp),
                    DialogResultTypes.Ok
                ),
                MessageKey.Confirmation);
        }
    }

    protected async ValueTask StartAssociatedAppAsync(string targetFilePath, Messenger messenger)
    {
        try
        {
            ProcessHelper.RunAssociatedApp(targetFilePath);
        }
        catch
        {
            Dic.GetInstance<ILoggerService>()
                .Warning(
                    $"{nameof(HotkeyBase)}.{nameof(StartAssociatedAppAsync)}: FailedToStartEditor, {targetFilePath}");

            using var _ = await messenger.RaiseTransitionAsync(
                ConfirmationDialogViewModel.T,
                (
                    Resources.AppName,
                    Resources.Message_FailedToStartAssociatedApp,
                    DialogResultTypes.Ok
                ),
                MessageKey.Confirmation);
        }
    }
}