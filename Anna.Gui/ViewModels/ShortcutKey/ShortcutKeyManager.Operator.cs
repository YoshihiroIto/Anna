using Anna.Gui.Interfaces;
using SimpleInjector;
using System.Threading.Tasks;

namespace Anna.Gui.ViewModels.ShortcutKey;

public partial class ShortcutKeyManager
{
    private static async ValueTask SelectSortModeAndOrderAsync(Container dic, IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var result = await DialogOperator.SelectSortModeAndOrderAsync(dic, shortcutKeyReceiver);

        if (result.IsCancel)
            return;

        shortcutKeyReceiver.Directory.SetSortModeAndOrder(result.SortMode, result.SortOrder);
    }
}