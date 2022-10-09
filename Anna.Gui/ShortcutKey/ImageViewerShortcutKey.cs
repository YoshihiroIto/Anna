using Anna.DomainModel.Config;
using Anna.Gui.Messaging;
using Anna.Gui.Views.Dialogs.Base;
using Anna.UseCase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anna.Gui.ShortcutKey;

public class ImageViewerShortcutKey : ShortcutKeyBase
{
    public ImageViewerShortcutKey(
        IFolderServiceUseCase folderService,
        AppConfig appConfig,
        KeyConfig keyConfig,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(folderService, appConfig, keyConfig, logger, objectLifetimeChecker)
    {
    }

    protected override IReadOnlyDictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>> SetupOperators()
    {
        return new Dictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>>
        {
            { Operations.OpenEntry, CloseAsync },
            { Operations.OpenEntryByEditor1, s => OpenFileByEditorAsync(s, 1) },
            { Operations.OpenEntryByEditor2, s => OpenFileByEditorAsync(s, 2) },
            { Operations.OpenEntryByApp, OpenFileByAppAsync },
        };
    }

    private static async ValueTask CloseAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var r = shortcutKeyReceiver as IImageViewerShortcutKeyReceiver ?? throw new InvalidOperationException();

        await r.Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, DialogViewModel.MessageKeyClose));
    }

    private ValueTask OpenFileByEditorAsync(IShortcutKeyReceiver shortcutKeyReceiver, int index)
    {
        var r = shortcutKeyReceiver as IImageViewerShortcutKeyReceiver ?? throw new InvalidOperationException();

        return OpenFileByEditorAsync(index, r.TargetFilepath, r.Messenger);
    }

    private ValueTask OpenFileByAppAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var r = shortcutKeyReceiver as IImageViewerShortcutKeyReceiver ?? throw new InvalidOperationException();

        return StartAssociatedAppAsync(r.TargetFilepath, r.Messenger);
    }
}