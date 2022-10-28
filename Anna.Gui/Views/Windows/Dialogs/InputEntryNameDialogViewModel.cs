using Anna.Constants;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Anna.Service;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class InputEntryNameDialogViewModel
    : HasModelWindowBaseViewModel<(string CurrentFolderPath, string CurrentFileName, string Title, bool IsEnableCurrentInfo, bool IsEnableSkip)>
{
    public string ResultFilePath { get; private set; } = "";

    public string CurrentFolder => Model.CurrentFolderPath;
    public string CurrentName => Model.CurrentFileName;
    public string Title => Model.Title;
    public bool IsEnableCurrentInfo => Model.IsEnableCurrentInfo;
    public bool IsEnableSkip => Model.IsEnableSkip;

    public ReactivePropertySlim<string> FileName { get; }

    public ReadOnlyReactivePropertySlim<bool> IsInvalidName { get; }
    public ReadOnlyReactivePropertySlim<bool> IsEmptyName { get; }
    public ReadOnlyReactivePropertySlim<bool> ExistsEntity { get; }
    
    public ICommand OkCommand { get; }
    public ICommand SkipCommand { get; }
    public ICommand CancelCommand { get; }

    public IEnumerable<ReadOnlyReactivePropertySlim<bool>> AllConditions
    {
        get
        {
            yield return IsInvalidName;
            yield return IsEmptyName;
            yield return ExistsEntity;
        }
    }
    
    private static readonly HashSet<char> InvalidFileNameChars = new(Path.GetInvalidFileNameChars());

    public InputEntryNameDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        FileName = new ReactivePropertySlim<string>(Model.CurrentFileName)
            .AddTo(Trash);

        IsInvalidName = FileName
            .Select(x => x.Any(c => InvalidFileNameChars.Contains(c)))
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);

        IsEmptyName = FileName
            .Select(x => x.Length == 0)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);

        ExistsEntity = FileName
            .Select(x => UpdateExistsEntry())
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);

        OkCommand = AllConditions.CombineLatestValuesAreAllFalse()
            .ToReactiveCommand()
            .WithSubscribe(OnDecisionWithoutCheckAsync)
            .AddTo(Trash);

        SkipCommand = CreateButtonCommand(DialogResultTypes.Skip);
        CancelCommand = CreateButtonCommand(DialogResultTypes.Cancel);
    }

    // ReSharper disable once UnusedMember.Global
    public void OnDecision()
    {
        if (AllConditions.Any(x => x.Value))
            return;

        OnDecisionWithoutCheckAsync();
    }

    public async void OnDecisionWithoutCheckAsync()
    {
        DialogResult = DialogResultTypes.Ok;
        ResultFilePath = Path.Combine(CurrentFolder, FileName.Value);

        await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
    }

    private bool UpdateExistsEntry()
    {
        return Directory.EnumerateFileSystemEntries(CurrentFolder)
            .Any(x => x == Path.Combine(CurrentFolder, FileName.Value));
    }
}