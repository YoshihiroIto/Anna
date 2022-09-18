using Anna.Views;
using Avalonia.Input;
using System;
using System.Collections.Generic;

namespace Anna.ViewModels.ShortcutKey;

public class ShortcutKeyRegistry
{
    public void Register(Key key, KeyModifiers modifierKeys, Action action)
    {
        var k = (key, modifierKeys);

        if (_shortcutKeys.ContainsKey(k))
            throw new Exception();

        _shortcutKeys[k] = action;
    }

    public void OnKeyDown(DirectoryView sender, KeyEventArgs e)
    {
        var k = (e.Key, e.KeyModifiers);

        if (_shortcutKeys.TryGetValue(k, out var value) == false)
            return;

        value();
        e.Handled = true;
    }

    private readonly Dictionary<(Key, KeyModifiers), Action> _shortcutKeys = new();
}