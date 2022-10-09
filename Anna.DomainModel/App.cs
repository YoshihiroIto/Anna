using Anna.Foundation;
using Anna.UseCase;
using System.Collections.ObjectModel;

namespace Anna.DomainModel;

public class App : DisposableNotificationObject
{
    public ReadOnlyObservableCollection<Folder> Folders { get; }

    private readonly IFolderServiceUseCase _folderService;
    private readonly DomainModelOperator _domainModelOperator;
    private readonly ObservableCollection<Folder> _folders = new();

    public App(
        IFolderServiceUseCase folderService,
        DomainModelOperator domainModelOperator,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _folderService = folderService;
        _domainModelOperator = domainModelOperator;

        Folders = new ReadOnlyObservableCollection<Folder>(_folders);
    }

    public ValueTask ShowFolderAsync(string path)
    {
        if (_folderService.IsAccessible(path) == false)
        {
            // todo: Show warning dialog

            return ValueTask.CompletedTask;
        }

        _folders.Add(_domainModelOperator.CreateFolder(path));

        return ValueTask.CompletedTask;
    }

    public void CloseFolder(Folder folder)
    {
        folder.Dispose();

        _folders.Remove(folder);
    }

    public void CloseAllFolders()
    {
        foreach (var d in Folders.ToArray())
            CloseFolder(d);
    }
}