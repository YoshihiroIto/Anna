using System.Collections.ObjectModel;

namespace Anna.Service;

public interface IFolderHistoryService
{
    ReadOnlyObservableCollection<string> DestinationFolders { get; }
    
    void AddDestinationFolder(string path);
}