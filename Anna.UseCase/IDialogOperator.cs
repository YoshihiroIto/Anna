using Anna.DomainModel;
using Anna.UseCase.Interfaces;
using SimpleInjector;

namespace Anna.UseCase
{
    public interface IDialogOperator
    {
        ValueTask<(bool IsCancel, SortModes SortMode, SortOrders SortOrder)> SelectSortModeAndOrderAsync(
            Container dic,
            IShortcutKeyReceiver shortcutKeyReceiver);
    }
}