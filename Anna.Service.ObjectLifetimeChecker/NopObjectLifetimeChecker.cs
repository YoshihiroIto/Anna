﻿using Anna.Service.Services;

namespace Anna.Service.ObjectLifetimeChecker;

public sealed class NopObjectLifetimeChecker : IObjectLifetimeCheckerService
{
    public void Start(Action<string> showError) {}
    public void End() {}
    public void Add(IDisposable disposable) {}
    public void Remove(IDisposable disposable) {}
}