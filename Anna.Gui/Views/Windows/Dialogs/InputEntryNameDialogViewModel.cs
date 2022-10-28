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
    : HasModelWindowBaseViewModel<(string CurrentFolderPath, string CurrentFilename, string Title, bool IsEnableCurrentInfo, bool IsEnableSkip)>
{
    public string ResultFilePath { get; private set; } = "";

    public string CurrentFolder => Model.CurrentFolderPath;
    public string CurrentName => Model.CurrentFilename;
    public string Title => Model.Title;
    public bool IsEnableCurrentInfo => Model.IsEnableCurrentInfo;
    public bool IsEnableSkip => Model.IsEnableSkip;

    public ReactivePropertySlim<string> Filename { get; }

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

    public InputEntryNameDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        Filename = new ReactivePropertySlim<string>(Model.CurrentFilename)
            .AddTo(Trash);

        IsInvalidName = Filename
            .Select(x => x.Any(c => InvalidFilenameChars.Contains(c)))
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);

        IsEmptyName = Filename
            .Select(x => x.Length == 0)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);

        ExistsEntity = Filename
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
        ResultFilePath = Path.Combine(CurrentFolder, Filename.Value);

        await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
    }

    private bool UpdateExistsEntry()
    {
        return Directory.EnumerateFileSystemEntries(CurrentFolder)
            .Any(x => x == Path.Combine(CurrentFolder, Filename.Value));
    }

    private static readonly HashSet<char> InvalidFilenameChars = new(Path.GetInvalidFileNameChars());
}