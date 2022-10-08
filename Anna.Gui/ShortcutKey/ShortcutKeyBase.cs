using Anna.DomainModel.Config;
using Anna.Foundation;
using Anna.Gui.Interfaces;
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
    protected ShortcutKeyBase(
        IFolderServiceUseCase folderService,
        KeyConfig keyConfig,
        ILoggerUseCase logger)
    {
        _folderService = folderService;
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

    protected abstract IReadOnlyDictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>> SetupOperators();

    private readonly IFolderServiceUseCase _folderService;
    private readonly ILoggerUseCase _logger;
    private readonly Dictionary<(Key, KeyModifiers), Func<IShortcutKeyReceiver, ValueTask>> _shortcutKeys = new();
    private readonly IReadOnlyDictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>> _operators;
}