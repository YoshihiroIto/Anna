using Anna.Foundation;
using Anna.UseCase;
using System.Collections.ObjectModel;

namespace Anna.DomainModel;

public class App : DisposableNotificationObject
{
    public ReadOnlyObservableCollection<Folder> Folders { get; }

    private readonly DomainModelOperator _domainModelOperator;
    private readonly ObservableCollection<Folder> _folders = new();

    public App(
        IFolderServiceUseCase folderService,
        DomainModelOperator domainModelOperator,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _domainModelOperator = domainModelOperator;

        Folders = new ReadOnlyObservableCollection<Folder>(_folders);
    }

    public void AddFolder(string path)
    {
        _folders.Add(_domainModelOperator.CreateFolder(path));
    }

    public void RemoveFolder(Folder folder)
    {
        folder.Dispose();

        _folders.Remove(folder);
    }
}