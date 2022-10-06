﻿using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.ViewModels.Messaging;
using Anna.Strings;
using Anna.UseCase;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;

namespace Anna.Gui.Views.Dialogs.Base;

public class DialogViewModel : ViewModelBase, ILocalizableViewModel
{
    public const string MessageKeyClose = nameof(MessageKeyClose);
    public const string MessageKeyInformation = nameof(MessageKeyInformation);
    public const string MessageKeyYesNoConfirmation = nameof(MessageKeyYesNoConfirmation);

    public Resources R => _resourcesHolder.Instance;

    public DelegateCommand OkCommand { get; }

    public DialogResultTypes DialogResult { get; set; } = DialogResultTypes.Cancel;

    private readonly ResourcesHolder _resourcesHolder;
    private readonly ILoggerUseCase _logger;

    protected DialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, objectLifetimeChecker)
    {
        _resourcesHolder = resourcesHolder;
        _logger = logger;

        Observable
            .FromEventPattern(
                h => _resourcesHolder.CultureChanged += h,
                h => _resourcesHolder.CultureChanged -= h)
            .Subscribe(_ => RaisePropertyChanged(nameof(R)))
            .AddTo(Trash);

        OkCommand = new DelegateCommand(() =>
        {
            DialogResult = DialogResultTypes.Ok;
            
 #pragma warning disable CS4014
            Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
 #pragma warning restore CS4014
        });

        _logger.Start(GetType().Name);
    }

    public override void Dispose()
    {
        _logger.End(GetType().Name);

        base.Dispose();

        GC.SuppressFinalize(this);
    }
}

public enum ConfirmationTypes
{
    YesNo
}

public enum DialogResultTypes
{
    Ok,
    Cancel,
    Yes,
    No
}