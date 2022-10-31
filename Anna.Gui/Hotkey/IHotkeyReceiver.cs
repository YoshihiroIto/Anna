using Anna.Constants;
using Anna.DomainModel;
using Anna.Gui.Messaging;
using Anna.Service.Workers;
using Avalonia.Controls;
using AvaloniaEdit;
using Entry=Anna.DomainModel.Entry;

namespace Anna.Gui.Hotkey;

public interface IHotkeyReceiver
{
    Messenger Messenger { get; }
}

public interface IFolderPanelHotkeyReceiver : IHotkeyReceiver
{
    Folder Folder { get; }
    Entry CurrentEntry { get; }
    Entry[] TargetEntries { get; }
    IBackgroundWorker BackgroundWorker { get; }
    
    void MoveCursor(Directions dir);
    void ToggleSelectionCursorEntry(bool isMoveDown);
}

public interface ITextViewerHotkeyReceiver : IHotkeyReceiver
{
    public TextEditor TextEditor { get; }
    public ScrollViewer ScrollViewer { get; }
    public double CalcLineHeight();
    
    public string TargetFilepath { get; }
    public int LineIndex { get; }
}

public interface IImageViewerHotkeyReceiver : IHotkeyReceiver
{
    public string TargetFilepath { get; }
}
