using System.ComponentModel;

namespace Anna.Service.Interfaces;

public interface IBackgroundOperator : IDisposable, INotifyPropertyChanged
{
    string Name { get; }
    
    double Progress { get; }
    
    ValueTask ExecuteAsync();
}