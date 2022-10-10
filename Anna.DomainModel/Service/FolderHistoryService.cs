using Anna.DomainModel.Config;
using Anna.Service;
using System.Collections.ObjectModel;

namespace Anna.DomainModel.Service;

public class FolderHistoryService : IFolderHistoryService
{
    public ReadOnlyObservableCollection<string> DestinationFolders { get; }
    
    private readonly AppConfig _appConfig;

    public FolderHistoryService(AppConfig appConfig)
    {
        _appConfig = appConfig;
        
        DestinationFolders = new ReadOnlyObservableCollection<string>(_appConfig.Data.DestinationFolders);
    }
    
    public void AddDestinationFolder(string path)
    {
        _appConfig.Data.DestinationFolders.Remove(path);
        _appConfig.Data.DestinationFolders.Insert(0, path);
    }
}