using Anna.Service.Interfaces;

namespace Anna.Service.Services;

public interface ITrashCanService
{
    void SendToTrashCan(IEntry target);
}