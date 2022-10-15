using Anna.Log;
using Anna.ObjectLifetimeChecker;
using Anna.Service;
using Anna.Service.Interfaces;
using Anna.ServiceProvider;
using System.ComponentModel;

namespace Anna.TestFoundation;

public sealed class TestServiceProvider : ServiceProviderBase
{
    public TestServiceProvider()
    {
        RegisterSingleton<IObjectLifetimeCheckerService, NopObjectLifetimeChecker>();
        RegisterSingleton<ILoggerService, NopLogger>();
        RegisterSingleton<IBackgroundService, MockBackgroundService>();

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif
    }
}

internal class MockBackgroundService : IBackgroundService
{
#pragma warning disable 0067
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 0067

    public bool IsInProcessing => false;
    public double Progress => 0;
    public string Message => "";
    public ValueTask CopyFileSystemEntryAsync(IFileSystemOperator fileSystemOperator, string destPath, IEnumerable<IEntry> sourceEntries,
        IEntriesStats stats)
    {
        throw new NotImplementedException();
    }
}