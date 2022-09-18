using Anna.DomainModel.Interface;
using Anna.Foundation;
using System.Collections.ObjectModel;

namespace Anna.DomainModel;

public class App : DisposableNotificationObject
{
    public ReadOnlyObservableCollection<Directory> Directories { get; }

    private readonly ObservableCollection<Directory> _Directories = new();
    private readonly IDomainModelOperator _domainModelOperator;

    public App(IDomainModelOperator domainModelOperator)
    {
        _domainModelOperator = domainModelOperator;
        Directories = new ReadOnlyObservableCollection<Directory>(_Directories);
    }

    public void Clean()
    {
        foreach (var d in Directories)
            (d as IDisposable)?.Dispose();

        _Directories.Clear();
    }

    public void ShowDirectory(string path)
    {
        _Directories.Add(_domainModelOperator.CreateDirectory(path));
    }
}