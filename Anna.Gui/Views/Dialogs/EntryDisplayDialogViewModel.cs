using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.ShortcutKey;
using Anna.Gui.Views.Dialogs.Base;
using Anna.UseCase;
using System.IO;

namespace Anna.Gui.Views.Dialogs;

public class EntryDisplayDialogViewModel
    : HasModelDialogViewModel<Entry>, IHasArg<(string CurrentFolderPath, JumpFolderConfigData Config)>
{
    public string Title => Model.NameWithExtension + " - " + Path.GetDirectoryName(Model.Path);

    public readonly EntryDisplayDialogShortcutKey ShortcutKey;

    public EntryDisplayDialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        EntryDisplayDialogShortcutKey entryDisplayDialogShortcutKey,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, resourcesHolder, logger, objectLifetimeChecker)
    {
        ShortcutKey = entryDisplayDialogShortcutKey;
    }
}