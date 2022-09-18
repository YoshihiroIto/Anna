using Anna.DomainModel;
using Anna.Gui.UseCase;
using Anna.Gui.UseCase.Interfaces;
using System.Diagnostics;

namespace Anna.Interactors
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