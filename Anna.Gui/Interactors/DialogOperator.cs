using Anna.DomainModel;
using Anna.UseCase;
using Anna.UseCase.Interfaces;
using Anna.Views.Dialogs;
using System.Threading.Tasks;
using SortModeAndOrderDialog=Anna.Gui.Views.Dialogs.SortModeAndOrderDialog;

namespace Anna.Gui.Interactors
{
    public class DialogOperator : IDialogOperator
    {
        public (SortModes mode, SortOrders order) SelectSortModeAndOrder(
            IShortcutKeyReceiver shortcutKeyReceiver, SortModes initialMode, SortOrders initialOrder)
        {
            Debug.WriteLine("SelectSortModeAndOrder");

            return (initialMode, initialOrder);
        }
    }
}