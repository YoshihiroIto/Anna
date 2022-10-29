using Anna.Service.Services;

namespace Anna.Service.Linux;

public sealed class TrashCanService : ITrashCanService
{
    public void SendToTrashCan(IEnumerable<string> targetFilePaths)
    {
        throw new NotImplementedException();
    }
    public bool EmptyTrashCan()
    {
        throw new NotImplementedException();
    }
    public (long EntryAllSize, long EntryCount) GetTrashCanInfo()
    {
        throw new NotImplementedException();
    }
    public void OpenTrashCan()
    {
        throw new NotImplementedException();
    }
}