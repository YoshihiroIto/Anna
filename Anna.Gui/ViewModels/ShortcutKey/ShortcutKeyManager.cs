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
        _ => Debug.WriteLine("ShortcutKeyManager: S"));
    }

    public void OnKeyDown(IShortcutKeyReceiver receiver, KeyEventArgs e)
    {
        _registry.OnKeyDown(receiver, e);
    }

    private readonly ShortcutKeyRegistry _registry = new();
}