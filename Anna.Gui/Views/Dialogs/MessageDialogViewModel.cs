﻿using Anna.DomainModel.Config;
using Anna.Gui.Views.Dialogs.Base;
using Anna.UseCase;

namespace Anna.Gui.Views.Dialogs;

public class MessageDialogViewModel : HasModelDialogViewModel<(string Title, string Text)>
{
    public string Title => Model.Title;
    public string Text => Model.Text;

    public MessageDialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        AppConfig appConfig,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, resourcesHolder, appConfig, logger, objectLifetimeChecker)
    {
    }
}