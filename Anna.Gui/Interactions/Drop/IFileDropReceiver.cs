using Anna.DomainModel;
using Anna.Gui.Messaging;
using Anna.Service.Workers;

namespace Anna.Gui.Interactions.Drop;

public interface IFileDropReceiver
{
    Messenger Messenger { get; }
    
    Folder Folder { get; }
    IBackgroundWorker BackgroundWorker { get; }
}