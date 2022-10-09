using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Views.Dialogs.Base;
using Anna.Gui.Views.Panels;
using Anna.UseCase;
using Reactive.Bindings.Extensions;
using System;
using System.IO;

namespace Anna.Gui.Views.Dialogs;

public class EntryDisplayDialogViewModel
    : HasModelDialogViewModel<Entry>, IHasArg<(string CurrentFolderPath, JumpFolderConfigData Config)>
{
    public string Title => Model.NameWithExtension + " - " + Path.GetDirectoryName(Model.Path);

    public ViewModelBase? ContentViewModel { get; }

    public EntryDisplayDialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, resourcesHolder, logger, objectLifetimeChecker)
    {
        switch (Model.Format)
        {
            case FileEntryFormat.Image:
                ContentViewModel = dic.GetInstance<ImageViewerViewModel, Entry>(Model).AddTo(Trash);
                break;
            
            case FileEntryFormat.Text:
                ContentViewModel = dic.GetInstance<TextViewerViewModel, Entry>(Model).AddTo(Trash);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}