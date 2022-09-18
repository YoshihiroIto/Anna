using Anna.DomainModel.UseCases;
using Anna.Foundation;
using System.Collections.ObjectModel;

namespace Anna.DomainModel;

public class App : DisposableNotificationObject
{
    public ReadOnlyObservableCollection<Directory> Directories { get; }

    private readonly ObservableCollection<Directory> _Directories = new();
    private readonly IDomainModelUseCase _domainModelUseCase;

    public App(IDomainModelUseCase domainModelUseCase)
    {
        _domainModelUseCase = domainModelUseCase;
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
        _Directories.Add(_domainModelUseCase.CreateDirectory(path));
    }
}