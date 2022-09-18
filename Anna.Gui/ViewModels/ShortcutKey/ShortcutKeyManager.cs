using Anna.UseCase;
using Anna.UseCase.Interfaces;
using Avalonia.Input;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Anna.Gui.ViewModels.ShortcutKey;

public class ShortcutKeyManager
{
    public ShortcutKeyManager(IDialogOperator dialogOperator)
    {
        _registry.Register(Key.S,
        KeyModifiers.None,
        async x =>
        {
            var result = await dialogOperator.SelectSortModeAndOrderAsync(x);

            Debug.WriteLine(result);
        });
    }

    public ValueTask OnKeyDown(IShortcutKeyReceiver receiver, KeyEventArgs e)
    {
        return _registry.OnKeyDown(receiver, e);
    }

    private readonly ShortcutKeyRegistry _registry = new();
}