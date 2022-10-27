﻿using Anna.Log;
using Anna.ObjectLifetimeChecker;
using Anna.Service.Interfaces;
using Anna.Service.Services;
using Anna.Service.Workers;
using Anna.ServiceProvider;
using System.ComponentModel;

namespace Anna.TestFoundation;

public sealed class TestServiceProvider : ServiceProviderBase
{
    public TestServiceProvider()
    {
        RegisterSingleton<IObjectLifetimeCheckerService, NopObjectLifetimeChecker>();
        RegisterSingleton<ILoggerService, NopLogger>();
        RegisterSingleton<IBackgroundWorker, MockBackgroundWorker>();

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
#pragma warning restore 0067

    public event EventHandler<ExceptionThrownEventArgs>? ExceptionThrown;
    public bool IsInProcessing => false;
    public double Progress => 0;
    public ValueTask PushOperatorAsync(IBackgroundOperator @operator)
    {
        throw new NotImplementedException();
    }
}