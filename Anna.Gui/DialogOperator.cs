using Anna.Constants;
using Anna.Gui.Interfaces;
using Anna.Gui.Views.Dialogs;
using Anna.Gui.Views.Dialogs.Base;
using Anna.UseCase;
using System.Threading.Tasks;

namespace Anna.Gui;

internal static class DialogOperator
{
    public static async ValueTask<(bool IsCancel, SortModes SortMode, SortOrders SortOrder)>
        SelectSortModeAndOrderAsync(
            IServiceProviderContainer dic,
            IShortcutKeyReceiver shortcutKeyReceiver)
    {
        using var viewModel = dic.GetInstance<SortModeAndOrderDialogViewModel>();

        var view = dic.GetInstance<SortModeAndOrderDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(shortcutKeyReceiver.Owner);

        return (viewModel.DialogResult == DialogResultTypes.Cancel, viewModel.SortMode, viewModel.SortOrder);
    }

    public static async ValueTask DisplayInformationAsync(
        string title,
        string message,
        IServiceProviderContainer dic,
        IShortcutKeyReceiver shortcutKeyReceiver)
    {
        using var viewModel = dic.GetInstance<MessageDialogViewModel, (string, string)>((title, message));

        var view = dic.GetInstance<MessageDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(shortcutKeyReceiver.Owner);
    }
}