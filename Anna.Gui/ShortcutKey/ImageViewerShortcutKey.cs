using Anna.DomainModel.Config;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
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
            { Operations.OpenEntry, CloseAsync },
            { Operations.OpenEntryByEditor1, s => OpenFileByEditorAsync(s, 1) },
            { Operations.OpenEntryByEditor2, s => OpenFileByEditorAsync(s, 2) },
            { Operations.OpenEntryByApp, OpenFileByAppAsync },
        };
    }

    private static async ValueTask CloseAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IImageViewerShortcutKeyReceiver)shortcutKeyReceiver;

        await receiver.Messenger.RaiseAsync(
            new WindowActionMessage(WindowAction.Close, WindowBaseViewModel.MessageKeyClose));
    }

    private ValueTask OpenFileByEditorAsync(IShortcutKeyReceiver shortcutKeyReceiver, int index)
    {
        var receiver = (IImageViewerShortcutKeyReceiver)shortcutKeyReceiver;

        return OpenFileByEditorAsync(index, receiver.TargetFilepath, 1, receiver.Messenger);
    }

    private ValueTask OpenFileByAppAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IImageViewerShortcutKeyReceiver)shortcutKeyReceiver;

        return StartAssociatedAppAsync(receiver.TargetFilepath, receiver.Messenger);
    }
}