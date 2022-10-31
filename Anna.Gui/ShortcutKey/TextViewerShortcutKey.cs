using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.ShortcutKey;

public sealed class TextViewerShortcutKey : ShortcutKeyBase
{
    public TextViewerShortcutKey(IServiceProvider dic)
        : base(dic)
    {
    }

    protected override IReadOnlyDictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>> SetupOperators()
    {
        return new Dictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>>
        {
            { Operations.OpenEntry, s => CloseAsync((ITextViewerShortcutKeyReceiver)s) },
            { Operations.OpenEntryByEditor1, s => OpenFileByEditorAsync((ITextViewerShortcutKeyReceiver)s, 1) },
            { Operations.OpenEntryByEditor2, s => OpenFileByEditorAsync((ITextViewerShortcutKeyReceiver)s, 2) },
            { Operations.OpenEntryByApp, s => OpenFileByAppAsync((ITextViewerShortcutKeyReceiver)s) },
            { Operations.MoveCursorUp, s => ScrollAsync((ITextViewerShortcutKeyReceiver)s, Directions.Up) },
            { Operations.MoveCursorDown, s => ScrollAsync((ITextViewerShortcutKeyReceiver)s, Directions.Down) },
            { Operations.MoveCursorLeft, s => ScrollAsync((ITextViewerShortcutKeyReceiver)s, Directions.Left) },
            { Operations.MoveCursorRight, s => ScrollAsync((ITextViewerShortcutKeyReceiver)s, Directions.Right) },
        };
    }

    private static async ValueTask CloseAsync(ITextViewerShortcutKeyReceiver receiver)
    {
        await receiver.Messenger.RaiseAsync(
            new WindowActionMessage(WindowAction.Close, MessageKey.Close));
    }

    private ValueTask OpenFileByEditorAsync(ITextViewerShortcutKeyReceiver receiver, int index)
    {
        return OpenFileByEditorAsync(index, receiver.TargetFilepath, receiver.LineIndex, receiver.Messenger);
    }

    private ValueTask OpenFileByAppAsync(ITextViewerShortcutKeyReceiver receiver)
    {
        return StartAssociatedAppAsync(receiver.TargetFilepath, receiver.Messenger);
    }

    private static ValueTask ScrollAsync(ITextViewerShortcutKeyReceiver receiver, Directions dir)
    {
        var lineHeight = receiver.CalcLineHeight();
        var pageHeight = TrimmingScrollY(receiver.TextEditor.TextArea.Bounds.Height);

        receiver.ScrollViewer.Offset = dir switch
        {
            Directions.Up => new Vector(0, TrimmingScrollY(receiver.ScrollViewer.Offset.Y - lineHeight)),
            Directions.Down => new Vector(0, TrimmingScrollY(receiver.ScrollViewer.Offset.Y + lineHeight)),
            Directions.Left => new Vector(0, TrimmingScrollY(receiver.ScrollViewer.Offset.Y - pageHeight)),
            Directions.Right => new Vector(0, TrimmingScrollY(receiver.ScrollViewer.Offset.Y + pageHeight)),
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };

        return ValueTask.CompletedTask;

        ////////////////////////////////////////////////////////////////////
        double TrimmingScrollY(double v)
            => Math.Round(v / lineHeight) * lineHeight;
    }
}