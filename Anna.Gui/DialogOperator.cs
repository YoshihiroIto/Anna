using Anna.Constants;
using Anna.Gui.Interfaces;
using Anna.Gui.Views.Dialogs;
using Anna.Gui.Views.Dialogs.Base;
using SimpleInjector;
using System.Threading.Tasks;

namespace Anna.Gui;

internal static class DialogOperator
{
    public static async ValueTask<(bool IsCancel, SortModes SortMode, SortOrders SortOrder)>
        SelectSortModeAndOrderAsync(
            Container dic,
            IShortcutKeyReceiver shortcutKeyReceiver)
    {
        using var viewModel = dic.GetInstance<SortModeAndOrderDialogViewModel>();

        var view = dic.GetInstance<SortModeAndOrderDialog>().Setup(viewModel);

        await view.ShowDialog(shortcutKeyReceiver.Owner);

        return (viewModel.DialogResult == DialogResultTypes.Cancel, viewModel.SortMode, viewModel.SortOrder);
    }
}