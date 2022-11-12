using Anna.Constants;
using Anna.DomainModel;
using Anna.Gui.Messaging;
using Anna.Service.Workers;
using Avalonia.Controls;
using AvaloniaEdit;
using System.Collections.Generic;
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
    IBackgroundWorker BackgroundWorker { get; }
    
    IEnumerable<Entry> CollectTargetEntries();
    void MoveCursor(Directions dir);
    void ToggleSelectionCursorEntry(bool isMoveDown);
    void SetListMode(int index);
}

public interface ITextViewerHotkeyReceiver : IHotkeyReceiver
{
    public TextEditor TextEditor { get; }
    public ScrollViewer ScrollViewer { get; }
    public double CalcLineHeight();
    
    public string TargetFilePath { get; }
    public int LineIndex { get; }
}

public interface IImageViewerHotkeyReceiver : IHotkeyReceiver
{
    public string TargetFilePath { get; }
}