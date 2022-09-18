using Anna.DomainModel;
using Anna.UseCase;
using Anna.UseCase.Interfaces;
using Avalonia.Input;

namespace Anna.Gui.ViewModels.ShortcutKey;

public class ShortcutKeyManager
{
    public ShortcutKeyManager(IDialogOperator dialogOperator)
    {
        _registry.Register(Key.S,
        KeyModifiers.None,
        async x =>
        {
            await dialogOperator.SelectSortModeAndOrderAsync(x, SortModes.Name, SortOrders.Ascending);
        }
        );
    }

    public void OnKeyDown(IShortcutKeyReceiver receiver, KeyEventArgs e)
    {
        _registry.OnKeyDown(receiver, e);
    }

    private readonly ShortcutKeyRegistry _registry = new();
}