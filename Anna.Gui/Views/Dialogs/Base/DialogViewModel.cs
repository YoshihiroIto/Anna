﻿using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Strings;
using Anna.UseCase;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;

namespace Anna.Gui.Views.Dialogs.Base;

public class DialogViewModel : ViewModelBase, ILocalizableViewModel
{
    public Resources R => _resourcesHolder.Instance;
    public DialogResultTypes DialogResult { get; set; } = DialogResultTypes.Cancel;

    private readonly ResourcesHolder _resourcesHolder;
    private readonly ILoggerUseCase _logger;

    protected DialogViewModel(
        ResourcesHolder resourcesHolder,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _resourcesHolder = resourcesHolder;
        _logger = logger;

        Observable
            .FromEventPattern(
            h => _resourcesHolder.CultureChanged += h,
            h => _resourcesHolder.CultureChanged -= h)
            .Subscribe(_ => RaisePropertyChanged(nameof(R)))
            .AddTo(Trash);
        
        _logger.Start(GetType().Name);
    }

    public override void Dispose()
    {
        _logger.End(GetType().Name);

        base.Dispose();

        GC.SuppressFinalize(this);
    }
}

public enum DialogResultTypes
{
    Ok,
    Cancel
}