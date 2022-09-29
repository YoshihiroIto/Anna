using Anna.Constants;
using Anna.Gui.Views.Dialogs;
using Anna.Gui.Views.Dialogs.Base;
using Anna.UseCase;
using Avalonia.Controls;
using System.Threading.Tasks;

namespace Anna.Gui;

public static class DialogOperator
{
    public static async ValueTask<(bool IsCancel, SortModes SortMode, SortOrders SortOrder)>
        SelectSortModeAndOrderAsync(IServiceProviderContainer dic, Window owner)
    {
        using var viewModel = dic.GetInstance<SortModeAndOrderDialogViewModel>();

        var view = dic.GetInstance<SortModeAndOrderDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return (viewModel.DialogResult == DialogResultTypes.Cancel, viewModel.SortMode, viewModel.SortOrder);
    }

    public static async ValueTask DisplayInformationAsync(
        IServiceProviderContainer dic,
        Window owner,
        string title,
        string text)
    {
        using var viewModel = dic.GetInstance<MessageDialogViewModel, (string, string)>((title, text));

        var view = dic.GetInstance<MessageDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);
    }
}