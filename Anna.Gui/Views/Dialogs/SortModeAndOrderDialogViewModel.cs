﻿using Anna.Constants;
using Anna.Gui.Views.Dialogs.Base;
using Anna.Strings;
using Anna.UseCase;

namespace Anna.Gui.Views.Dialogs;

public class SortModeAndOrderDialogViewModel : DialogViewModel
{
    public SortModes SortMode { get; set; }
    public SortOrders SortOrder { get; set; }

    public SortModeAndOrderDialogViewModel(
        ResourcesHolder resourcesHolder,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(resourcesHolder, logger, objectLifetimeChecker)
    {
    }
}