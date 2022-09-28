using Anna.Foundation;
using Anna.UseCase;
using System.Collections.ObjectModel;

namespace Anna.DomainModel;

public class App : DisposableNotificationObject
{
    public ReadOnlyObservableCollection<Directory> Directories { get; }

    private readonly IDirectoryServiceUseCase _directoryService;
    private readonly DomainModelOperator _domainModelOperator;
    private readonly ObservableCollection<Directory> _Directories = new();

    public App(
        IDirectoryServiceUseCase directoryService,
        DomainModelOperator domainModelOperator)
    {
        _directoryService = directoryService;
        _domainModelOperator = domainModelOperator;

        Directories = new ReadOnlyObservableCollection<Directory>(_Directories);
    }

    public ValueTask ShowDirectoryAsync(string path)
    {
        if (_directoryService.IsAccessible(path) == false)
        {
            // todo: Show warning dialog

            return ValueTask.CompletedTask;
        }

        _Directories.Add(_domainModelOperator.CreateDirectory(path));

        return ValueTask.CompletedTask;
    }

    public void CloseDirectory(Directory directory)
    {
        directory.Dispose();

        _Directories.Remove(directory);
    }

    public void CloseAllDirectories()
    {
        foreach (var d in Directories.ToArray())
            CloseDirectory(d);
    }
}