using Anna.Views;
using Avalonia.Input;
using System.Diagnostics;

namespace Anna.ViewModels.ShortcutKey;

public class ShortcutKeyManager
{
    public ShortcutKeyManager()
    {
        _registry.Register(
        Key.S,
        KeyModifiers.None,
        () => Debug.WriteLine("ShortcutKeyManager: S"));
    }

    public void OnKeyDown(DirectoryView sender, KeyEventArgs e)
    {
        _registry.OnKeyDown(sender, e);
    }

    private readonly ShortcutKeyRegistry _registry = new();
}