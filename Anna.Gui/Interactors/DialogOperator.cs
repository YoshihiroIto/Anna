using Anna.DomainModel;
using Anna.UseCase;
using Anna.UseCase.Interfaces;
using Anna.Gui.Views.Dialogs;
using SimpleInjector;
using System.Threading.Tasks;

namespace Anna.Gui.Interactors;

public class DialogOperator : IDialogOperator
{
    public async ValueTask<(bool IsCancel, SortModes SortMode, SortOrders SortOrder)> SelectSortModeAndOrderAsync(
        Container dic,
        IShortcutKeyReceiver shortcutKeyReceiver)
    {
        using var viewModel = dic.GetInstance<SortModeAndOrderDialogViewModel>();
        var view = new SortModeAndOrderDialog { DataContext = viewModel };

        await view.ShowDialog(shortcutKeyReceiver.Owner);

        return (viewModel.DialogResult == DialogResultTypes.Cancel, viewModel.SortMode, viewModel.SortOrder);
    }
}