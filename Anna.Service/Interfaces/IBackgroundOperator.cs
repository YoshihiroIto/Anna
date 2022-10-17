using System.ComponentModel;

namespace Anna.Service.Interfaces;

public interface IBackgroundOperator : IDisposable, INotifyPropertyChanged
{
    double Progress { get; }
    
    ValueTask ExecuteAsync();
}