using Anna.Foundation;
using System.Collections.ObjectModel;

namespace Anna.DomainModel;

public abstract class Directory : NotificationObject
{
    #region Path

    private string _Path = "";

    public string Path
    {
        get => _Path;
        private set
        {
            if (SetProperty(ref _Path, value) == false)
                return;

            Task.Run(() =>
            {
                lock (UpdateLockObj)
                {
                    Update();
                }
            });
        }
    }

    #endregion

    #region Entries

    private ObservableCollection<Entry> _entries = new();

    public ObservableCollection<Entry> Entries
    {
        get => _entries;
        private set => SetProperty(ref _entries, value);
    }

    #endregion

    public readonly object UpdateLockObj = new();

    protected Directory(string path)
    {
        Path = path;
    }

    protected abstract void Update();
}