﻿using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.DomainModel.FileSystem.FileProcessable;
using Anna.Gui.Views.Windows.Dialogs;
using Anna.Service;
using Anna.Service.Services;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ConfirmationDialog=Anna.Gui.Views.Windows.Dialogs.ConfirmationDialog;
using EntryDisplayDialog=Anna.Gui.Views.Windows.Dialogs.EntryDisplayDialog;
using JumpFolderDialog=Anna.Gui.Views.Windows.Dialogs.JumpFolderDialog;
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
        JumpFolderAsync(IServiceProvider dic, Window owner, string currentFolderPath)
    {
        using var viewModel = dic.GetInstance<JumpFolderDialogViewModel, (string, JumpFolderConfigData)>((
            currentFolderPath, dic.GetInstance<JumpFolderConfig>().Data));

        var view = dic.GetInstance<JumpFolderDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return (viewModel.DialogResult, viewModel.ResultPath);
    }

    public static async ValueTask<(DialogResultTypes Result, string Path)>
        SelectFolderAsync(IServiceProvider dic, Window owner, string currentFolderPath)
    {
        using var viewModel = dic.GetInstance<SelectFolderDialogViewModel, (string, int)>((currentFolderPath, 0));

        var view = dic.GetInstance<SelectFolderDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return (viewModel.DialogResult, viewModel.ResultPath);
    }

    public static async ValueTask<(DialogResultTypes Result, string FilePath)>
        ChangeEntryNameAsync(IServiceProvider dic, Window owner, string currentFolderPath, string currentFilename)
    {
        using var viewModel =
            dic.GetInstance<ChangeEntryNameDialogViewModel, (string, string)>((currentFolderPath, currentFilename));

        var view = dic.GetInstance<ChangeEntryNameDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return (viewModel.DialogResult, viewModel.ResultFilePath);
    }

    public static async ValueTask
        EntryDisplay(IServiceProvider dic, Window owner, Entry target)
    {
        using var viewModel = dic.GetInstance<EntryDisplayDialogViewModel, Entry>(target);

        var view = dic.GetInstance<EntryDisplayDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);
    }

    public static async ValueTask<DialogResultTypes>
        DisplayConfirmationAsync(IServiceProvider dic, Window owner, string title, string text,
            DialogResultTypes confirmations)
    {
        using var viewModel = dic.GetInstance<ConfirmationDialogViewModel, (string, string, DialogResultTypes)>((title,
            text,
            confirmations));

        var view = dic.GetInstance<ConfirmationDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return viewModel.DialogResult;
    }

    public static async ValueTask<(DialogResultTypes Result, string DestFolder)>
        EntryCopyOrMoveAsync(IServiceProvider dic, Window owner, 
            CopyOrMove copyOrMove,
            string currentFolderPath, Entry[] targets, EntriesStats stats)
    {
        using var viewModel =
            dic.GetInstance<CopyOrMoveEntryDialogViewModel,
                    (CopyOrMove, string, Entry[], EntriesStats, ReadOnlyObservableCollection<string>)>
                ((copyOrMove, currentFolderPath, targets, stats, dic.GetInstance<IFolderHistoryService>().DestinationFolders));

        var view = dic.GetInstance<CopyOrMoveEntryDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return (viewModel.DialogResult, viewModel.ResultDestFolder);
    }

    public static async ValueTask<(DialogResultTypes Result, EntryDeleteModes Mode)>
        EntryDeleteAsync(IServiceProvider dic, Window owner, Entry[] targets, EntriesStats stats)
    {
        using var viewModel =
            dic.GetInstance<DeleteEntryDialogViewModel, (Entry[], EntriesStats)>
                ((targets, stats));

        var view = dic.GetInstance<DeleteEntryDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return (viewModel.DialogResult, viewModel.ResultMode);
    }

    public static async
        ValueTask<(DialogResultTypes Result, FileSystemCopier.CopyActionWhenExistsResult CopyActionWhenExistsResult)>
        SelectFileCopyAction(IServiceProvider dic, Window owner, string srcFilepath, string destFilepath,
            bool isSameActionThereafter)
    {
        using var viewModel =
            dic.GetInstance<SelectFileCopyActionDialogViewModel, (string, string, bool)>
                ((srcFilepath, destFilepath, isSameActionThereafter));

        var view = dic.GetInstance<SelectFileCopyActionDialog>();
        view.DataContext = viewModel;

        await view.ShowDialog(owner);

        return (viewModel.DialogResult, viewModel.Result);
    }
}