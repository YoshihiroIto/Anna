namespace Anna.Service.Services;

public interface ITrashCanService
{
    void SendToTrashCan(IEnumerable<string> targetFilePaths);

    bool EmptyTrashCan();

    (long EntryAllSize, long EntryCount) GetTrashCanInfo();

    void OpenTrashCan();
}