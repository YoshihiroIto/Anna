using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging;
using Anna.Gui.Views.Dialogs.Base;
using Anna.UseCase;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anna.Gui.ShortcutKey;

public class EntryDisplayDialogShortcutKey : ShortcutKeyBase
{
    private readonly AppConfig _appConfig;
    public EntryDisplayDialogShortcutKey(
        IFolderServiceUseCase folderService,
        AppConfig appConfig,
        KeyConfig keyConfig,
        ILoggerUseCase logger)
        : base(folderService, keyConfig, logger)
    {
        _appConfig = appConfig;
    }

    protected override IReadOnlyDictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>> SetupOperators()
    {
        return new Dictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>>
        {
            { Operations.OpenEntry, CloseAsync },
            { Operations.OpenEntryByEditor1, s => OpenFileByEditorAsync(s, 1) },
            { Operations.OpenEntryByEditor2, s => OpenFileByEditorAsync(s, 2) },
            { Operations.MoveCursorUp, s => ScrollAsync(s, Directions.Up) },
            { Operations.MoveCursorDown, s => ScrollAsync(s, Directions.Down) },
            { Operations.MoveCursorLeft, s => ScrollAsync(s, Directions.Left) },
            { Operations.MoveCursorRight, s => ScrollAsync(s, Directions.Right) },
        };
    }

    private static async ValueTask CloseAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var r = shortcutKeyReceiver as IEntryDisplayDialogShortcutKeyReceiver ?? throw new InvalidOperationException();

        await r.Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, DialogViewModel.MessageKeyClose));
    }

    private async ValueTask OpenFileByEditorAsync(IShortcutKeyReceiver shortcutKeyReceiver, int index)
    {
        var r = shortcutKeyReceiver as IEntryDisplayDialogShortcutKeyReceiver ?? throw new InvalidOperationException();

        var targetFilepath = r.TargetFilepath;
        var lineIndex = r.LineIndex;

 #pragma warning disable CS4014
        Task.Run(() =>
        {
            var editor = _appConfig.Data.FindEditor(index);
            var arguments = ProcessHelper.MakeEditorArguments(editor.Options, targetFilepath, lineIndex);

            ProcessHelper.Execute(editor.Editor, arguments);
        });
 #pragma warning restore CS4014

        await r.Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, DialogViewModel.MessageKeyClose));
    }

    private static ValueTask ScrollAsync(IShortcutKeyReceiver shortcutKeyReceiver, Directions dir)
    {
        var r = shortcutKeyReceiver as IEntryDisplayDialogShortcutKeyReceiver ?? throw new InvalidOperationException();

        var lineHeight = r.CalcLineHeight();
        var pageHeight = TrimmingScrollY(r.TextEditor.TextArea.Bounds.Height);
        
        r.ScrollViewer.Offset = dir switch
        {
            Directions.Up => new Vector(0, TrimmingScrollY(r.ScrollViewer.Offset.Y - lineHeight)),
            Directions.Down => new Vector(0, TrimmingScrollY(r.ScrollViewer.Offset.Y + lineHeight)),
            Directions.Left => new Vector(0, TrimmingScrollY(r.ScrollViewer.Offset.Y - pageHeight)),
            Directions.Right => new Vector(0, TrimmingScrollY(r.ScrollViewer.Offset.Y + pageHeight)),
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };

        return ValueTask.CompletedTask;

        ////////////////////////////////////////////////////////////////////
        double TrimmingScrollY(double v)
            => Math.Round(v / lineHeight) * lineHeight;
    }
}