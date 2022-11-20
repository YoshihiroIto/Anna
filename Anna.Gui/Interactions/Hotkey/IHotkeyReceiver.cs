using Anna.Constants;
using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.Messaging;
using Anna.Service.Interfaces;
using Anna.Service.Workers;
using Avalonia.Controls;
using AvaloniaEdit;
using System;
using System.Collections.Generic;
using Control=Avalonia.Controls.Control;
using Entry=Anna.DomainModel.Entry;
using WindowBase=Anna.Gui.Views.Windows.Base.WindowBase;

namespace Anna.Gui.Interactions.Hotkey;

public interface IHotkeyReceiver
{
}

public interface IFolderPanelHotkeyReceiver : IHotkeyReceiver
{
    Messenger Messenger { get; }

    Folder Folder { get; }
    Entry CurrentEntry { get; }
    IBackgroundWorker BackgroundWorker { get; }

    IEnumerable<IEntry> CollectTargetEntries();
    void MoveCursor(Directions dir);
    void ToggleSelectionCursorEntry(bool isMoveDown);
    void SetListMode(uint index);
}

public interface IViewerHotkeyReceiver : IHotkeyReceiver
{
    Messenger Messenger => (ControlHelper.FindOwnerWindow((Control)this) as WindowBase)?.ViewModel.Messenger ??
                           throw new NullReferenceException();

    string TargetFilePath => (((Control)this).DataContext as IViewerViewModel)?.TargetFilePath ??
                             throw new NullReferenceException();
}

public interface ITextViewerHotkeyReceiver : IViewerHotkeyReceiver
{
    TextEditor TextEditor { get; }
    ScrollViewer ScrollViewer { get; }
    double CalcLineHeight();

    int LineIndex { get; }
}

public interface IImageViewerHotkeyReceiver : IViewerHotkeyReceiver
{
}