using Anna.Constants;
using Anna.DomainModel;
using Anna.Gui.Messaging;
using Avalonia.Controls;
using AvaloniaEdit;
using Entry=Anna.DomainModel.Entry;

namespace Anna.Gui.ShortcutKey;

public interface IShortcutKeyReceiver
{
    Window Owner { get; }
    InteractionMessenger Messenger { get; }
}

public interface IFolderPanelShortcutKeyReceiver : IShortcutKeyReceiver
{
    Folder Folder { get; }
    Entry CurrentEntry { get; }
    
    void MoveCursor(Directions dir);
    void ToggleSelectionCursorEntry(bool isMoveDown);
}

public interface IEntryDisplayDialogShortcutKeyReceiver : IShortcutKeyReceiver
{
    public TextEditor TextEditor { get; }
    public ScrollViewer ScrollViewer { get; }
    public double CalcLineHeight();
    
    public string TargetFilepath { get; }
    public int LineIndex { get; }
}
