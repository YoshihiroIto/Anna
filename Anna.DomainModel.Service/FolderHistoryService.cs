using Anna.DomainModel.Config;
using Anna.Service.Services;
using System.Collections.ObjectModel;

namespace Anna.DomainModel.Service;

public sealed class FolderHistoryService : IFolderHistoryService
{
    public ReadOnlyObservableCollection<string> DestinationFolders => new(_appConfig.Data.DestinationFolders);
    
    private readonly AppConfig _appConfig;

    public FolderHistoryService(AppConfig appConfig)
    {
        _appConfig = appConfig;
    }
    
    public void AddDestinationFolder(string path)
    {
        _appConfig.Data.DestinationFolders.Remove(path);
        _appConfig.Data.DestinationFolders.Insert(0, path);
    }
}