using Anna.DomainModel;
using Anna.UseCase.Interfaces;

namespace Anna.UseCase
{
    public interface IDialogOperator
    {
        ValueTask<(SortModes mode, SortOrders order)> SelectSortModeAndOrderAsync(
            IShortcutKeyReceiver shortcutKeyReceiver, SortModes initialMode, SortOrders initialOrder);
    }
}