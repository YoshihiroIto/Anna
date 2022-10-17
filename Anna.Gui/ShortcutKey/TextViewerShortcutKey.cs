using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
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
            { Operations.OpenEntry, CloseAsync },
            { Operations.OpenEntryByEditor1, s => OpenFileByEditorAsync(s, 1) },
            { Operations.OpenEntryByEditor2, s => OpenFileByEditorAsync(s, 2) },
            { Operations.OpenEntryByApp, OpenFileByAppAsync },
            { Operations.MoveCursorUp, s => ScrollAsync(s, Directions.Up) },
            { Operations.MoveCursorDown, s => ScrollAsync(s, Directions.Down) },
            { Operations.MoveCursorLeft, s => ScrollAsync(s, Directions.Left) },
            { Operations.MoveCursorRight, s => ScrollAsync(s, Directions.Right) },
        };
    }

    private static async ValueTask CloseAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (ITextViewerShortcutKeyReceiver)shortcutKeyReceiver;

        await receiver.Messenger.RaiseAsync(
            new WindowActionMessage(WindowAction.Close, WindowBaseViewModel.MessageKeyClose));
    }

    private ValueTask OpenFileByEditorAsync(IShortcutKeyReceiver shortcutKeyReceiver, int index)
    {
        var receiver = (ITextViewerShortcutKeyReceiver)shortcutKeyReceiver;

        return OpenFileByEditorAsync(index, receiver.TargetFilepath, receiver.LineIndex, receiver.Messenger);
    }

    private ValueTask OpenFileByAppAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (ITextViewerShortcutKeyReceiver)shortcutKeyReceiver;

        return StartAssociatedAppAsync(receiver.TargetFilepath, receiver.Messenger);
    }

    private static ValueTask ScrollAsync(IShortcutKeyReceiver shortcutKeyReceiver, Directions dir)
    {
        var receiver = (ITextViewerShortcutKeyReceiver)shortcutKeyReceiver;

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