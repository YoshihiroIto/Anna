using Anna.Gui.Interfaces;
using Avalonia.Input;
using SimpleInjector;
using System.Threading.Tasks;

namespace Anna.Gui.ViewModels.ShortcutKey;

public class ShortcutKeyManager
{
    public ShortcutKeyManager(Container dic)
    {
        _registry.Register(Key.S,
        KeyModifiers.None,
        async x =>
        {
            var result = await DialogOperator.SelectSortModeAndOrderAsync(dic, x);

            if (result.IsCancel)
                return;
            
            x.Directory.SetSortModeAndOrder(result.SortMode, result.SortOrder);
        });
    }

    public ValueTask OnKeyDown(IShortcutKeyReceiver receiver, KeyEventArgs e)
    {
        return _registry.OnKeyDown(receiver, e);
    }

    private readonly ShortcutKeyRegistry _registry = new();
}