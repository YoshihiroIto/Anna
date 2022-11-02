using Anna.Service.Interfaces;
using Anna.Service.Log;
using Anna.Service.ObjectLifetimeChecker;
using Anna.Service.Services;
using Anna.Service.Workers;
using Anna.ServiceProvider;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Anna.TestFoundation;

public sealed class TestServiceProvider : ServiceProviderBase
{
    public TestServiceProvider()
    {
        RegisterSingleton<IObjectLifetimeCheckerService, NopObjectLifetimeChecker>();
        RegisterSingleton<ILogService, NopLog>();
        RegisterSingleton<IBackgroundWorker, MockBackgroundWorker>();
        RegisterSingleton<IFolderHistoryService, MockFolderHistoryService>();

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif
    }
}

internal sealed class MockBackgroundWorker : IBackgroundWorker
{
#pragma warning disable 0067
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<ExceptionThrownEventArgs>? ExceptionThrown;
#pragma warning restore 0067

    public bool IsInProcessing => false;
    public double Progress => 0;
    public ValueTask PushOperatorAsync(IBackgroundOperator @operator)
    {
        throw new NotImplementedException();
    }
}

internal sealed class MockFolderHistoryService : IFolderHistoryService
{
    public ReadOnlyObservableCollection<string> DestinationFolders =>
        default!;

    public void AddDestinationFolder(string path)
    {
    }
}
