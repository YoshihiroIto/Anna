using Anna.DomainModel;
using Anna.Gui.UseCase.Interfaces;

namespace Anna.Gui.UseCase
{
    public interface IDialogOperator
    {
        (SortModes mode, SortOrders order) SelectSortModeAndOrder(
            IShortcutKeyReceiver shortcutKeyReceiver, SortModes initialMode, SortOrders initialOrder);
    }
}