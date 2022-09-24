using Anna.Foundation;
using System.Collections.ObjectModel;

namespace Anna.DomainModel;

public class App : DisposableNotificationObject
{
    public ReadOnlyObservableCollection<Directory> Directories { get; }

    private readonly DomainModelOperator _domainModelOperator;
    private readonly ObservableCollection<Directory> _Directories = new();

    public App(DomainModelOperator domainModelOperator)
    {
        _domainModelOperator = domainModelOperator;
        Directories = new ReadOnlyObservableCollection<Directory>(_Directories);
    }

    public void ShowDirectory(string path)
    {
        _Directories.Add(_domainModelOperator.CreateDirectory(path));
    }

    public void CloseDirectory(Directory directory)
    {
        (directory as IDisposable)?.Dispose();

        _Directories.Remove(directory);
    }

    public void CloseAllDirectories()
    {
        foreach (var d in Directories.ToArray())
            CloseDirectory(d);
    }
}