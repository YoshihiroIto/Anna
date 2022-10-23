using Anna.Service.Interfaces;
using Anna.Service.Services;

namespace Anna.Service.Linux;

public sealed class TrashCanService : ITrashCanService
{
    public void SendToTrashCan(IEnumerable<IEntry> sourceEntries)
    {
        throw new NotImplementedException();
    }
}