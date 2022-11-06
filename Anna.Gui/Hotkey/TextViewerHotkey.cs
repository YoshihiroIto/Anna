using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Hotkey;

public sealed class TextViewerHotkey : HotkeyBase
{
    public TextViewerHotkey(IServiceProvider dic)
        : base(dic)
    {
    }

    protected override IReadOnlyDictionary<Operations, Func<IHotkeyReceiver, ValueTask>> SetupOperators()
    {
        return new Dictionary<Operations, Func<IHotkeyReceiver, ValueTask>>
        {
            { Operations.OpenEntry, s => CloseAsync((ITextViewerHotkeyReceiver)s) },
            { Operations.OpenApp1, s => OpenFileByEditorAsync((ITextViewerHotkeyReceiver)s, 1) },
            { Operations.OpenApp2, s => OpenFileByEditorAsync((ITextViewerHotkeyReceiver)s, 2) },
            { Operations.OpenAssociatedApp, s => OpenAssociatedAppAsync((ITextViewerHotkeyReceiver)s) },
            { Operations.MoveCursorUp, s => ScrollAsync((ITextViewerHotkeyReceiver)s, Directions.Up) },
            { Operations.MoveCursorDown, s => ScrollAsync((ITextViewerHotkeyReceiver)s, Directions.Down) },
            { Operations.MoveCursorLeft, s => ScrollAsync((ITextViewerHotkeyReceiver)s, Directions.Left) },
            { Operations.MoveCursorRight, s => ScrollAsync((ITextViewerHotkeyReceiver)s, Directions.Right) },
        };
    }

    private static async ValueTask CloseAsync(ITextViewerHotkeyReceiver receiver)
    {
        await receiver.Messenger.RaiseAsync(
            new WindowActionMessage(WindowAction.Close, MessageKey.Close));
    }

    private ValueTask OpenFileByEditorAsync(ITextViewerHotkeyReceiver receiver, int index)
    {
        return OpenAppAsync(index, receiver.TargetFilepath, receiver.LineIndex, receiver.Messenger);
    }

    private ValueTask OpenAssociatedAppAsync(ITextViewerHotkeyReceiver receiver)
    {
        return StartAssociatedAppAsync(receiver.TargetFilepath, receiver.Messenger);
    }

    private static ValueTask ScrollAsync(ITextViewerHotkeyReceiver receiver, Directions dir)
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