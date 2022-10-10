using Anna.Foundation;
using Anna.UseCase;
using System.Collections.ObjectModel;

namespace Anna.DomainModel;

public class App : DisposableNotificationObject
{
    public ReadOnlyObservableCollection<Folder> Folders { get; }

    private readonly ObservableCollection<Folder> _folders = new();

    public App(IServiceProviderContainer dic)
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