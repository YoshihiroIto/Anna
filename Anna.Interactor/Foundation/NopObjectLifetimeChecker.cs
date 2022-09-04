﻿#if !DEBUG
using Anna.DomainModel.Interface;
using System;

namespace Anna.Interactor.Foundation;

public sealed class NopObjectLifetimeChecker : IObjectLifetimeChecker
{
    public void Start(Action<string> showError) { }
    public void End() { }
    public void Add(IDisposable disposable) { }
    public void Remove(IDisposable disposable) { }
}
#endif