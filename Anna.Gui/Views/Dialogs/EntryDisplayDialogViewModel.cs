using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.ShortcutKey;
using Anna.Gui.Views.Dialogs.Base;
using Anna.Gui.Views.Panels;
using Anna.UseCase;
using Reactive.Bindings.Extensions;
using System.IO;

namespace Anna.Gui.Views.Dialogs;

public class EntryDisplayDialogViewModel
    : HasModelDialogViewModel<Entry>, IHasArg<(string CurrentFolderPath, JumpFolderConfigData Config)>
{
    public string Title => Model.NameWithExtension + " - " + Path.GetDirectoryName(Model.Path);

    public TextViewerViewModel TextViewerViewModel { get; }

    public EntryDisplayDialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        TextViewerShortcutKey textViewerShortcutKey,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, resourcesHolder, logger, objectLifetimeChecker)
    {
        TextViewerViewModel = dic.GetInstance<TextViewerViewModel, Entry>(Model).AddTo(Trash);
    }
}