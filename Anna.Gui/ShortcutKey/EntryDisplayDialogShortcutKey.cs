using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.UseCase;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anna.Gui.ShortcutKey;

public class EntryDisplayDialogShortcutKey : ShortcutKeyBase
{
    public EntryDisplayDialogShortcutKey(
        IFolderServiceUseCase folderService,
        KeyConfig keyConfig,
        ILoggerUseCase logger)
        : base(folderService, keyConfig, logger)
    {
    }

    protected override IReadOnlyDictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>> SetupOperators()
    {
        return new Dictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>>
        {
            { Operations.MoveCursorUp, s => ScrollAsync(s, Directions.Up) },
            { Operations.MoveCursorDown, s => ScrollAsync(s, Directions.Down) },
            { Operations.MoveCursorLeft, s => ScrollAsync(s, Directions.Left) },
            { Operations.MoveCursorRight, s => ScrollAsync(s, Directions.Right) },
        };
    }

    private static ValueTask ScrollAsync(IShortcutKeyReceiver shortcutKeyReceiver, Directions dir)
    {
        var r = shortcutKeyReceiver as IEntryDisplayDialogShortcutKeyReceiver ?? throw new InvalidOperationException();

        var lineHeight =
            r.TextEditor.TextArea.TextView.GetVisualTopByDocumentLine(2) -
            r.TextEditor.TextArea.TextView.GetVisualTopByDocumentLine(1);

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