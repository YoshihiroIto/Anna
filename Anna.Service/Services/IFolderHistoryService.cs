using System.Collections.ObjectModel;

namespace Anna.Service.Services;

public interface IFolderHistoryService
{
    ReadOnlyObservableCollection<string> DestinationFolders { get; }
    
    void AddDestinationFolder(string path);
}