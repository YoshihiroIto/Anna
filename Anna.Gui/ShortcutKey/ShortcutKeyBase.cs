using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Anna.Localization;
using Anna.Service.Services;
using Avalonia.Input;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.ShortcutKey;

public abstract class ShortcutKeyBase : DisposableNotificationObject
{
    protected abstract IReadOnlyDictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>> SetupOperators();

    private readonly Dictionary<(Key, KeyModifiers), Func<IShortcutKeyReceiver, ValueTask>> _shortcutKeys = new();
    private readonly IReadOnlyDictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>> _operators;

    protected ShortcutKeyBase(IServiceProvider dic)
        : base(dic)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        _operators = SetupOperators();

        dic.GetInstance<KeyConfig>().ObserveProperty(x => x.Data)
            .Subscribe(UpdateShortcutKeys)
            .AddTo(Trash);
    }

    public ValueTask OnKeyDownAsync(IShortcutKeyReceiver receiver, KeyEventArgs e)
    {
        var k = (e.Key, e.KeyModifiers);

        if (_shortcutKeys.TryGetValue(k, out var value) == false)
            return ValueTask.CompletedTask;

        e.Handled = true;
        return value(receiver);
    }

    private void UpdateShortcutKeys(KeyConfigData keyConfig)
    {
        _shortcutKeys.Clear();

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

    private void Register(Key key, KeyModifiers modifierKeys, Func<IShortcutKeyReceiver, ValueTask> action)
    {
        var k = (key, modifierKeys);

        if (_shortcutKeys.ContainsKey(k))
        {
            Dic.GetInstance<ILoggerService>().Warning("Already registered");
            return;
        }

        _shortcutKeys[k] = action;
    }

    protected async ValueTask<bool> CheckIsAccessibleAsync(string path, InteractionMessenger messenger)
    {
        if (Dic.GetInstance<IFileSystemIsAccessibleService>().IsAccessible(path))
            return true;

        await messenger.RaiseAsync(
            new ConfirmationMessage(
                Resources.AppName,
                string.Format(Resources.Message_AccessDenied, path),
                DialogResultTypes.Ok,
                WindowBaseViewModel.MessageKeyConfirmation));

        return false;
    }

    protected async ValueTask OpenFileByEditorAsync(
        int index, string targetFilepath, int targetLineIndex, InteractionMessenger messenger)
    {
        var editor = Dic.GetInstance<AppConfig>().Data.FindEditor(index);
        var arguments = ProcessHelper.MakeEditorArguments(editor.Options, targetFilepath, targetLineIndex);

        try
        {
            ProcessHelper.Execute(editor.Editor, arguments);

            await messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close,
                WindowBaseViewModel.MessageKeyClose));
        }
        catch
        {
            Dic.GetInstance<ILoggerService>()
                .Warning($"OpenFileByEditorAsync: FailedToStartEditor, {index}, {targetFilepath}");

            await messenger.RaiseAsync(
                new ConfirmationMessage(
                    Resources.AppName,
                    string.Format(Resources.Message_FailedToStartEditor, editor.Editor),
                    DialogResultTypes.Ok,
                    WindowBaseViewModel.MessageKeyConfirmation));
        }
    }

    protected async ValueTask StartAssociatedAppAsync(string targetFilepath, InteractionMessenger messenger)
    {
        try
        {
            ProcessHelper.RunAssociatedApp(targetFilepath);
        }
        catch
        {
            Dic.GetInstance<ILoggerService>()
                .Warning($"StartAssociatedAppAsync: FailedToStartEditor, {targetFilepath}");

            await messenger.RaiseAsync(
                new ConfirmationMessage(
                    Resources.AppName,
                    Resources.Message_FailedToStartAssociatedApp,
                    DialogResultTypes.Ok,
                    WindowBaseViewModel.MessageKeyConfirmation));
        }
    }
}