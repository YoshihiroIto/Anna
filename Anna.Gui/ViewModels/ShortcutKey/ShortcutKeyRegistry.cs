using Anna.UseCase.Interfaces;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anna.Gui.ViewModels.ShortcutKey;

public class ShortcutKeyRegistry
{
    public void Register(Key key, KeyModifiers modifierKeys, Func<IShortcutKeyReceiver, ValueTask> action)
    {
        var k = (key, modifierKeys);

        if (_shortcutKeys.ContainsKey(k))
            throw new Exception();

        _shortcutKeys[k] = action;
    }

    public ValueTask OnKeyDown(IShortcutKeyReceiver receiver, KeyEventArgs e)
    {
        var k = (e.Key, e.KeyModifiers);

        if (_shortcutKeys.TryGetValue(k, out var value) == false)
            return ValueTask.CompletedTask;

        e.Handled = true;
        return value(receiver);
    }

    private readonly Dictionary<(Key, KeyModifiers), Func<IShortcutKeyReceiver, ValueTask>> _shortcutKeys = new();
}