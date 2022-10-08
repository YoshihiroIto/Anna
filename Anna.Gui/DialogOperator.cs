using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Views.Dialogs;
using Anna.Gui.Views.Dialogs.Base;
using Anna.Gui.Views.Windows;
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

        return (viewModel.DialogResult == DialogResultTypes.Cancel,
            viewModel.ResultSortMode, viewModel.ResultSortOrder);
    }

    public static async ValueTask<(bool IsCancel, string Path)>
        JumpFolderAsync(IServiceProviderContainer dic, Window owner)
    {
        var currentFolderPath = ((owner as FolderWindow)?.DataContext as FolderWindowViewModel)?.Model.Path ?? "";

        using var viewModel =
            dic.GetInstance<JumpFolderDialogViewModel, (string CurrentFolderPath, JumpFolderConfigData Config)>((
                currentFolderPath, dic.GetInstance<JumpFolderConfig>().Data));

        var view = dic.GetInstance<JumpFolderDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return (viewModel.DialogResult == DialogResultTypes.Cancel, viewModel.ResultPath);
    }

    public static async ValueTask EntryDisplay(IServiceProviderContainer dic, Window owner, Entry target)
    {
        using var viewModel = dic.GetInstance<EntryDisplayDialogViewModel, Entry>(target);

        var view = dic.GetInstance<EntryDisplayDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);
    }

    public static async ValueTask<DialogResultTypes> DisplayInformationAsync(
        IServiceProviderContainer dic,
        Window owner,
        string title,
        string text)
    {
        using var viewModel = dic.GetInstance<MessageDialogViewModel, (string Title, string Text)>((title, text));

        var view = dic.GetInstance<MessageDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return viewModel.DialogResult;
    }

    public static async ValueTask<DialogResultTypes> DisplayConfirmationAsync(
        IServiceProviderContainer dic,
        Window owner,
        string title,
        string text,
        ConfirmationTypes confirmationType)
    {
        using var viewModel =
            dic.GetInstance<ConfirmationDialogViewModel, (string Title, string Text, ConfirmationTypes confirmationType
                )>((title, text, confirmationType));

        var view = dic.GetInstance<ConfirmationDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return viewModel.DialogResult;
    }
}