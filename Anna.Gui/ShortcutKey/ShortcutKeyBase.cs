using Anna.DomainModel.Config;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging;
using Anna.Gui.Views.Dialogs.Base;
using Anna.Strings;
using Anna.UseCase;
using Avalonia.Input;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anna.Gui.ShortcutKey;

public abstract class ShortcutKeyBase : DisposableNotificationObject
{
    protected abstract IReadOnlyDictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>> SetupOperators();

    private readonly IFolderServiceUseCase _folderService;
    private readonly AppConfig _appConfig;
    private readonly ILoggerUseCase _logger;
    private readonly Dictionary<(Key, KeyModifiers), Func<IShortcutKeyReceiver, ValueTask>> _shortcutKeys = new();
    private readonly IReadOnlyDictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>> _operators;
    
    protected ShortcutKeyBase(
        IFolderServiceUseCase folderService,
        AppConfig appConfig,
        KeyConfig keyConfig,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _folderService = folderService;
        _appConfig = appConfig;
        _logger = logger;

        // ReSharper disable once VirtualMemberCallInConstructor
        _operators = SetupOperators();

        keyConfig.ObserveProperty(x => x.Data)
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
            _logger.Warning("Already registered");
            return;
        }

        _shortcutKeys[k] = action;
    }

    protected async ValueTask<bool> CheckIsAccessibleAsync(string path, InteractionMessenger messenger)
    {
        if (_folderService.IsAccessible(path))
            return true;

        await messenger.RaiseAsync(
            new InformationMessage(
                Resources.AppName,
                string.Format(Resources.Message_AccessDenied, path),
                DialogViewModel.MessageKeyInformation));

        return false;
    }

    protected async ValueTask OpenFileByEditorAsync(int index, string targetFilepath, InteractionMessenger messenger)
    {
        var editor = _appConfig.Data.FindEditor(index);
        var arguments = ProcessHelper.MakeEditorArguments(editor.Options, targetFilepath, 0);

        try
        {
            ProcessHelper.Execute(editor.Editor, arguments);

            await messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, DialogViewModel.MessageKeyClose));
        }
        catch
        {
            _logger.Warning($"OpenFileByEditorAsync: FailedToStartEditor, {index}, {targetFilepath}");
            
            await messenger.RaiseAsync(
                new InformationMessage(
                    Resources.AppName,
                    string.Format(Resources.Message_FailedToStartEditor, editor.Editor),
                    DialogViewModel.MessageKeyInformation));
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
            _logger.Warning($"StartAssociatedAppAsync: FailedToStartEditor, {targetFilepath}");
            
            await messenger.RaiseAsync(
                new InformationMessage(
                    Resources.AppName,
                    Resources.Message_FailedToStartAssociatedApp,
                    DialogViewModel.MessageKeyInformation));
        }
    }
}