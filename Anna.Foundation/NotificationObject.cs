using Anna.Service;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Foundation;

public class NotificationObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // use Hashtable to get free lockless reading
    private static readonly Hashtable PropChanged = new();
    private static readonly object PropChangedLockObj = new();

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        storage = value;

        if (PropertyChanged is not null)
            RaisePropertyChanged(propertyName);

        return true;
    }

    protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
        if (PropertyChanged is null)
            return;
    
        // ReSharper disable once InconsistentlySynchronizedField
        var pc = (PropertyChangedEventArgs?)PropChanged[propertyName];

        if (pc is null)
        {
            // double-checked;
            lock (PropChangedLockObj)
            {
                pc = (PropertyChangedEventArgs?)PropChanged[propertyName];

                if (pc is null)
                {
                    pc = new PropertyChangedEventArgs(propertyName);
                    PropChanged[propertyName] = pc;
                }
            }
        }

        PropertyChanged.Invoke(this, pc);
    }
}
