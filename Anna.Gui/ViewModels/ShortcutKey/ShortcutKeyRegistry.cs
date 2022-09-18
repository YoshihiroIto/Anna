using Anna.UseCase.Interfaces;
using Avalonia.Input;
using System;
using System.Collections.Generic;

namespace Anna.Gui.ViewModels.ShortcutKey;

public class ShortcutKeyRegistry
{
    public void Register(Key key, KeyModifiers modifierKeys, Action<IShortcutKeyReceiver> action)
    {
        var k = (key, modifierKeys);

        if (_shortcutKeys.ContainsKey(k))
            throw new Exception();

        _shortcutKeys[k] = action;
    }

    public void OnKeyDown(IShortcutKeyReceiver receiver, KeyEventArgs e)
    {
        var k = (e.Key, e.KeyModifiers);

        if (_shortcutKeys.TryGetValue(k, out var value) == false)
            return;

        value(receiver);
        e.Handled = true;
    }

    private readonly Dictionary<(Key, KeyModifiers), Action<IShortcutKeyReceiver>> _shortcutKeys = new();
}