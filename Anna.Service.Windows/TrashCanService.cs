using Anna.Foundation;
using Anna.Service.Interfaces;
using Anna.Service.Services;

namespace Anna.Service.Windows;

public sealed class TrashCanService : ITrashCanService
{
    public void SendToTrashCan(IEntry target)
    {
        ShellApiWrapper.SendToTrashCan(target.Path);
    }

    public bool EmptyTrashCan()
    {
        return ShellApiWrapper.EmptyTrashCan();
    }

    public void OpenTrashCan()
    {
        ProcessHelper.RunAssociatedApp("shell:::{645FF040-5081-101B-9F08-00AA002F954E}");
    }
}