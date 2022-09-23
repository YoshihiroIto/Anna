using Anna.DomainModel.Constants;
using Anna.UseCase.Interfaces;
using SimpleInjector;

namespace Anna.UseCase;

public interface IDialogUseCase
{
    ValueTask<(bool IsCancel, SortModes SortMode, SortOrders SortOrder)> SelectSortModeAndOrderAsync(
        Container dic,
        IShortcutKeyReceiver shortcutKeyReceiver);
}