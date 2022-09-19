using Anna.DomainModel;
using Anna.UseCase;
using Anna.UseCase.Interfaces;
using Avalonia.Input;
using SimpleInjector;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Anna.Gui.ViewModels.ShortcutKey;

public class ShortcutKeyManager
{
    public ShortcutKeyManager(Container dic, IDialogOperator dialogOperator)
    {
        _registry.Register(Key.S,
        KeyModifiers.None,
        async x =>
        {
            var result = await dialogOperator.SelectSortModeAndOrderAsync(dic, x);
            Debug.WriteLine(result);

            if (result.isCancel)
                return;
            
            x.Directory.SetSortModeAndOrder(SortModes.Size, SortOrders.Descending);
        });
    }

    public ValueTask OnKeyDown(IShortcutKeyReceiver receiver, KeyEventArgs e)
    {
        return _registry.OnKeyDown(receiver, e);
    }

    private readonly ShortcutKeyRegistry _registry = new();
}