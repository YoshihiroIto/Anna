using Anna.DomainModel;
using Anna.UseCase.Interfaces;
using SimpleInjector;

namespace Anna.UseCase
{
    public interface IDialogOperator
    {
        ValueTask<(bool isCancel, SortModes mode, SortOrders order)> SelectSortModeAndOrderAsync(
            Container dic,
            IShortcutKeyReceiver shortcutKeyReceiver);
    }
}