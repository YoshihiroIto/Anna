using Anna.Foundation;
using System.Collections.ObjectModel;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel;

public sealed class App : DisposableNotificationObject
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
        _folders.Add(Dic.GetInstance<DomainModelOperator>().CreateFolder(FindUnusedFinderId(), path));
    }

    public void RemoveFolder(Folder folder)
    {
        if (_folders.Remove(folder))
            folder.Dispose();
    }

    private int FindUnusedFinderId()
    {
        if (_folders.Count == 0)
            return 0;

        var usedIds = _folders.Select(x => x.Id).ToHashSet();

        for (var i = 0;; ++i)
        {
            if (usedIds.Contains(i) == false)
                return i;
        }
    }
}