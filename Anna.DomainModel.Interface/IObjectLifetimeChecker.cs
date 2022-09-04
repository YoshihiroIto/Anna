﻿using System;

namespace Anna.DomainModel.Interface;

public interface IObjectLifetimeChecker
{
    void Start(Action<string> showError);
    void End();
    void Add(IDisposable disposable);
    void Remove(IDisposable disposable);
}