using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Views.Windows.Base;
using Anna.Gui.Views.Windows.Dialogs;
using Anna.Service;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ConfirmationDialog=Anna.Gui.Views.Windows.Dialogs.ConfirmationDialog;
using EntryDisplayDialog=Anna.Gui.Views.Windows.Dialogs.EntryDisplayDialog;
using JumpFolderDialog=Anna.Gui.Views.Windows.Dialogs.JumpFolderDialog;
using MessageDialog=Anna.Gui.Views.Windows.Dialogs.MessageDialog;
using SortModeAndOrderDialog=Anna.Gui.Views.Windows.Dialogs.SortModeAndOrderDialog;

namespace Anna.Gui.Views.Windows;

public static class WindowOperator
{
    public static async ValueTask<(DialogResultTypes Result, SortModes SortMode, SortOrders SortOrder)>
        SelectSortModeAndOrderAsync(IServiceProvider dic, Window owner)
    {
        using var viewModel = dic.GetInstance<SortModeAndOrderDialogViewModel>();

        var view = dic.GetInstance<SortModeAndOrderDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return (viewModel.DialogResult, viewModel.ResultSortMode, viewModel.ResultSortOrder);
    }

    public static async ValueTask<(DialogResultTypes Result, string Path)>
        JumpFolderAsync(IServiceProvider dic, Window owner)
    {
        var currentFolderPath = ((owner as FolderWindow)?.DataContext as FolderWindowViewModel)?.Model.Path ?? "";

        using var viewModel = dic.GetInstance<JumpFolderDialogViewModel, (string, JumpFolderConfigData)>((
            currentFolderPath, dic.GetInstance<JumpFolderConfig>().Data));

        var view = dic.GetInstance<JumpFolderDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return (viewModel.DialogResult, viewModel.ResultPath);
    }

    public static async ValueTask<(DialogResultTypes Result, string Path)>
        SelectFolderAsync(IServiceProvider dic, Window owner)
    {
        var currentFolderPath = ((owner as FolderWindow)?.DataContext as FolderWindowViewModel)?.Model.Path ?? "";

        using var viewModel = dic.GetInstance<SelectFolderDialogViewModel, (string, int)>((currentFolderPath, 0));

        var view = dic.GetInstance<SelectFolderDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return (viewModel.DialogResult, viewModel.ResultPath);
    }

    public static async ValueTask<(DialogResultTypes Result, string Name)>
        ChangeEntryNameAsync(IServiceProvider dic, Window owner, string currentName)
    {
        using var viewModel = dic.GetInstance<ChangeEntryNameDialogViewModel, (string, int)>((currentName, 0));

        var view = dic.GetInstance<ChangeEntryNameDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return (viewModel.DialogResult, viewModel.ResultName);
    }

    public static async ValueTask EntryDisplay(IServiceProvider dic, Window owner, Entry target)
    {
        using var viewModel = dic.GetInstance<EntryDisplayDialogViewModel, Entry>(target);

        var view = dic.GetInstance<EntryDisplayDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);
    }

    public static async ValueTask<DialogResultTypes> DisplayInformationAsync(
        IServiceProvider dic,
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
        IServiceProvider dic,
        Window owner,
        string title,
        string text,
        ConfirmationTypes confirmationType)
    {
        using var viewModel = dic.GetInstance<ConfirmationDialogViewModel, (string, string, ConfirmationTypes)>((title,
            text,
            confirmationType));

        var view = dic.GetInstance<ConfirmationDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return viewModel.DialogResult;
    }

    public static async ValueTask<(DialogResultTypes Result, string DestFolder)>
        EntryCopyAsync(IServiceProvider dic, Window owner, Entry[] targets, EntriesStats stats)
    {
        using var viewModel =
            dic.GetInstance<CopyEntryDialogViewModel, (Entry[], EntriesStats, ReadOnlyObservableCollection<string>)>
                ((targets, stats, dic.GetInstance<IFolderHistoryService>().DestinationFolders));

        var view = dic.GetInstance<CopyEntryDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return (viewModel.DialogResult, viewModel.ResultDestFolder);
    }
}