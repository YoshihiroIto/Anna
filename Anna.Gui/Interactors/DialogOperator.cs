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
            IShortcutKeyReceiver shortcutKeyReceiver)
        {
            var d = new SortModeAndOrderDialog();

            await d.ShowDialog(shortcutKeyReceiver.Owner);

            return (SortModes.Name, SortOrders.Ascending);
        }
    }
}