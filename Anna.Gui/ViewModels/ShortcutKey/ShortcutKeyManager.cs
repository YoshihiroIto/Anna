using Anna.Gui.UseCase;
using Anna.Gui.UseCase.Interfaces;
using Avalonia.Input;

namespace Anna.ViewModels.ShortcutKey;

public class ShortcutKeyManager
{
    public ShortcutKeyManager(IDialogOperator dialogOperator)
    {
        _registry.Register(Key.S, KeyModifiers.None, dialogOperator.ShowSortEntries);
    }

    public void OnKeyDown(IShortcutKeyReceiver receiver, KeyEventArgs e)
    {
        _registry.OnKeyDown(receiver, e);
    }

    private readonly ShortcutKeyRegistry _registry = new();
}