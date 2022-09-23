using Anna.DomainModel.Constants;
using Anna.UseCase;
using Anna.UseCase.Interfaces;
using Anna.Gui.Views.Dialogs;
using Anna.Gui.Views.Dialogs.Base;
using SimpleInjector;
using System.Threading.Tasks;

namespace Anna.Gui.Interactors;

public class DialogInteractor : IDialogUseCase
{
    public async ValueTask<(bool IsCancel, SortModes SortMode, SortOrders SortOrder)> SelectSortModeAndOrderAsync(
        Container dic,
        IShortcutKeyReceiver shortcutKeyReceiver)
    {
        using var viewModel = dic.GetInstance<SortModeAndOrderDialogViewModel>();

        var view = dic.GetInstance<SortModeAndOrderDialog>().Setup(viewModel);

        await view.ShowDialog(shortcutKeyReceiver.Owner);

        return (viewModel.DialogResult == DialogResultTypes.Cancel, viewModel.SortMode, viewModel.SortOrder);
    }
}