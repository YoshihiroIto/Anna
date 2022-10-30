using Anna.Constants;
using Anna.DomainModel;
using Anna.Gui.Messaging;
using Anna.Gui.Views.Windows;
using Anna.Service.Workers;
using Avalonia.Controls;
using AvaloniaEdit;
using Entry=Anna.DomainModel.Entry;

namespace Anna.Gui.ShortcutKey;

public interface IShortcutKeyReceiver
{
    Window Owner { get; }
    Messenger Messenger { get; }
}

public interface IFolderPanelShortcutKeyReceiver : IShortcutKeyReceiver
{
    new FolderWindow Owner { get; }
    Folder Folder { get; }
    Entry CurrentEntry { get; }
    Entry[] TargetEntries { get; }
    IBackgroundWorker BackgroundWorker { get; }
    
    void MoveCursor(Directions dir);
    void ToggleSelectionCursorEntry(bool isMoveDown);
}

public interface ITextViewerShortcutKeyReceiver : IShortcutKeyReceiver
{
    public TextEditor TextEditor { get; }
    public ScrollViewer ScrollViewer { get; }
    public double CalcLineHeight();
    
    public string TargetFilepath { get; }
    public int LineIndex { get; }
}

public interface IImageViewerShortcutKeyReceiver : IShortcutKeyReceiver
{
    public string TargetFilepath { get; }
}
