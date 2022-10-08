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
        KeyConfig keyConfig,
        ILoggerUseCase logger)
        : base(folderService, keyConfig, logger)
    {
    }

    protected override IReadOnlyDictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>> SetupOperators()
    {
        return new Dictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>>
        {
            { Operations.OpenEntry, CloseAsync },
        };
    }
    
    private static async ValueTask CloseAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var r = shortcutKeyReceiver as IImageViewerShortcutKeyReceiver ?? throw new InvalidOperationException();

        await r.Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, DialogViewModel.MessageKeyClose));
    }
}