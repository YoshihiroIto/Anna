using Anna.DomainModel;
using Anna.Gui.Messaging;
using Anna.Gui.Views.Panels;
using Avalonia.Controls;
using Entry=Anna.DomainModel.Entry;

namespace Anna.Gui.Interfaces;

public interface IShortcutKeyReceiver
{
    Window Owner { get; }
    Folder Folder { get; }
    Entry CurrentEntry { get; }
    InteractionMessenger Messenger { get; }
    
    FolderPanelViewModel FolderPanelViewModel { get; }

    Entry[] CollectTargetEntries();
}