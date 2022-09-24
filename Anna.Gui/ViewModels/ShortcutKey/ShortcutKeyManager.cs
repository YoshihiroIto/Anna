using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.Foundation;
using Anna.Gui.Interfaces;
using Anna.UseCase;
using Avalonia.Input;
using Reactive.Bindings.Extensions;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anna.Gui.ViewModels.ShortcutKey;

public partial class ShortcutKeyManager : DisposableNotificationObject
{
    public ShortcutKeyManager(Container dic, KeyConfig keyConfig, ILoggerUseCase logger)
    {
        _dic = dic;
        _logger = logger;

        keyConfig.ObserveProperty(x => x.Data)
            .Subscribe(UpdateShortcutKeys)
            .AddTo(Trash);
    }

    public ValueTask OnKeyDown(IShortcutKeyReceiver receiver, KeyEventArgs e)
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
                    if (Operators.TryGetValue(key.Operation, out var value) == false)
                        return;

                    await value(_dic, x);
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

    private readonly Container _dic;
    private readonly ILoggerUseCase _logger;
    private readonly Dictionary<(Key, KeyModifiers), Func<IShortcutKeyReceiver, ValueTask>> _shortcutKeys = new();

    private static readonly IReadOnlyDictionary<Operations, Func<Container, IShortcutKeyReceiver, ValueTask>>
        Operators = new Dictionary<Operations, Func<Container, IShortcutKeyReceiver, ValueTask>>
        {
            { Operations.SortEntry, SelectSortModeAndOrderAsync }
        };
}