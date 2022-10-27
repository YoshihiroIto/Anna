using Anna.Service.Interfaces;

namespace Anna.Service.Services;

public interface ITrashCanService
{
    void SendToTrashCan(IEntry target)
    {
        throw new NotImplementedException();
    }
    
    bool EmptyTrashCan()
    {
        throw new NotImplementedException();
    }

    (long EntryAllSize, long EntryCount) GetTrashCanInfo()
    {
        throw new NotImplementedException();
    }
    
    void OpenTrashCan()
    {
        throw new NotImplementedException();
    }
}