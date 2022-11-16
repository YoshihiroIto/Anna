using Anna.Foundation;
using Anna.Service;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anna.Gui.Interactions.Drop;

public class FolderPanelDrop: DisposableNotificationObject
{
    public FolderPanelDrop(IServiceProvider dic) : base(dic)
    {
    }

    public ValueTask OnFileDropAsync(IFileDropReceiver receiver, IEnumerable<string> filePaths)
    {
        return ValueTask.CompletedTask;
    }
}