using Anna.Foundation;
using System.Collections.ObjectModel;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel;

public class App : DisposableNotificationObject
{
    public ReadOnlyObservableCollection<Folder> Folders { get; }

    private readonly ObservableCollection<Folder> _folders = new();

    public App(IServiceProvider dic)
        : base(dic)
    {
        Folders = new ReadOnlyObservableCollection<Folder>(_folders);
    }

    public void AddFolder(string path)
    {
        _folders.Add(Dic.GetInstance<DomainModelOperator>().CreateFolder(path));
    }

    public void RemoveFolder(Folder folder)
    {
        folder.Dispose();

        _folders.Remove(folder);
    }
}