using Anna.DomainModel;
using Anna.UseCase.Interfaces;

namespace Anna.UseCase
{
    public interface IDialogOperator
    {
        (SortModes mode, SortOrders order) SelectSortModeAndOrder(
            IShortcutKeyReceiver shortcutKeyReceiver, SortModes initialMode, SortOrders initialOrder);
    }
}