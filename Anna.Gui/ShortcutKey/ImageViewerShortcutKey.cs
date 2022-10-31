using Anna.DomainModel.Config;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.ShortcutKey;

public sealed class ImageViewerShortcutKey : ShortcutKeyBase
{
    public ImageViewerShortcutKey(IServiceProvider dic)
        : base(dic)
    {
    }

    protected override IReadOnlyDictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>> SetupOperators()
    {
        return new Dictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>>
        {
            { Operations.OpenEntry, s => CloseAsync((IImageViewerShortcutKeyReceiver)s) },
            { Operations.OpenEntryByEditor1, s => OpenFileByEditorAsync((IImageViewerShortcutKeyReceiver)s, 1) },
            { Operations.OpenEntryByEditor2, s => OpenFileByEditorAsync((IImageViewerShortcutKeyReceiver)s, 2) },
            { Operations.OpenEntryByApp, s => OpenFileByAppAsync((IImageViewerShortcutKeyReceiver)s) },
        };
    }

    private static async ValueTask CloseAsync(IImageViewerShortcutKeyReceiver receiver)
    {
        await receiver.Messenger.RaiseAsync(
            new WindowActionMessage(WindowAction.Close, MessageKey.Close));
    }

    private ValueTask OpenFileByEditorAsync(IImageViewerShortcutKeyReceiver receiver, int index)
    {
        return OpenFileByEditorAsync(index, receiver.TargetFilepath, 1, receiver.Messenger);
    }

    private ValueTask OpenFileByAppAsync(IImageViewerShortcutKeyReceiver receiver)
    {
        return StartAssociatedAppAsync(receiver.TargetFilepath, receiver.Messenger);
    }
}