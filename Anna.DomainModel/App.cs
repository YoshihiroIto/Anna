using Anna.Foundation;
using Anna.UseCase;
using System.Collections.ObjectModel;

namespace Anna.DomainModel;

public class App : DisposableNotificationObject
{
    public ReadOnlyObservableCollection<Folder> Directories { get; }

    private readonly IFolderServiceUseCase _folderService;
    private readonly DomainModelOperator _domainModelOperator;
    private readonly ObservableCollection<Folder> _Directories = new();

    public App(
        IFolderServiceUseCase folderService,
        DomainModelOperator domainModelOperator)
    {
        _folderService = folderService;
        _domainModelOperator = domainModelOperator;

        Directories = new ReadOnlyObservableCollection<Folder>(_Directories);
    }

    public ValueTask ShowFolderAsync(string path)
    {
        if (_folderService.IsAccessible(path) == false)
        {
            // todo: Show warning dialog

            return ValueTask.CompletedTask;
        }

        _Directories.Add(_domainModelOperator.CreateFolder(path));

        return ValueTask.CompletedTask;
    }

    public void CloseFolder(Folder folder)
    {
        folder.Dispose();

        _Directories.Remove(folder);
    }

    public void CloseAllDirectories()
    {
        foreach (var d in Directories.ToArray())
            CloseFolder(d);
    }
}