using System.ComponentModel;

namespace Anna.DomainModel.Service.BackgroundProcess;

internal interface IBackgroundProcess : IDisposable, INotifyPropertyChanged
{
    double Progress { get; }
    
    string Message { get; }

    ValueTask ExecuteAsync();
}