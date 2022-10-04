using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.Foundation;
using Anna.Gui.Interfaces;
using Anna.UseCase;
using Avalonia.Input;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anna.Gui.ViewModels.ShortcutKey;

public partial class ShortcutKeyManager : DisposableNotificationObject
{
    public ShortcutKeyManager(
        IServiceProviderContainer dic,
        IFolderServiceUseCase folderService,
        KeyConfig keyConfig,
        ILoggerUseCase logger)
    {
        _dic = dic;
        _folderService = folderService;
        _logger = logger;

        SetupOperators();

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
                    _ = _operators ?? throw new NullReferenceException();

                    if (_operators.TryGetValue(key.Operation, out var value) == false)
                    {
                        _logger.Error($"UpdateShortcutKeys: Not found ({key.Key}, {key.Modifier})");
                        return;
                    }

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

    private void SetupOperators()
    {
        _operators = new Dictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>>
        {
            { Operations.SortEntry, SelectSortModeAndOrderAsync },
            { Operations.JumpFolder, JumpFolderAsync },
            { Operations.MoveCursorUp, s => MoveCursorAsync(s, Directions.Up) },
            { Operations.MoveCursorDown, s => MoveCursorAsync(s, Directions.Down) },
            { Operations.MoveCursorLeft, s => MoveCursorAsync(s, Directions.Left) },
            { Operations.MoveCursorRight, s => MoveCursorAsync(s, Directions.Right) },
            { Operations.ToggleSelectionCursorEntry, s => ToggleSelectionCursorEntryAsync(s, true) },
            { Operations.OpenEntry, OpenEntryAsync },
            { Operations.JumpToParentFolder, JumpToParentFolderAsync },
            { Operations.JumpToRootFolder, JumpToRootFolderAsync },
        };
    }

    private readonly IServiceProviderContainer _dic;
    private readonly IFolderServiceUseCase _folderService;
    private readonly ILoggerUseCase _logger;
    private readonly Dictionary<(Key, KeyModifiers), Func<IShortcutKeyReceiver, ValueTask>> _shortcutKeys = new();

    private IReadOnlyDictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>>? _operators;
}