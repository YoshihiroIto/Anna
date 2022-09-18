using Anna.DomainModel;
using Anna.UseCase;
using Anna.UseCase.Interfaces;
using Anna.Gui.Views.Dialogs;
using System.Threading.Tasks;

namespace Anna.Gui.Interactors
{
    public class DialogOperator : IDialogOperator
    {
        public async ValueTask<(SortModes mode, SortOrders order)> SelectSortModeAndOrderAsync(
            IShortcutKeyReceiver shortcutKeyReceiver, SortModes initialMode, SortOrders initialOrder)
        {
            var d = new SortModeAndOrderDialog();

            await d.ShowDialog(shortcutKeyReceiver.Owner);

            return (initialMode, initialOrder);
        }
    }
}