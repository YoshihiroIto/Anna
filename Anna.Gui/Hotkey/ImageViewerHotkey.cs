using Anna.DomainModel.Config;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Hotkey;

public sealed class ImageViewerHotkey : HotkeyBase
{
    public ImageViewerHotkey(IServiceProvider dic)
        : base(dic)
    {
    }

    protected override IReadOnlyDictionary<Operations, Func<IHotkeyReceiver, ValueTask>> SetupOperators()
    {
        return new Dictionary<Operations, Func<IHotkeyReceiver, ValueTask>>
        {
            { Operations.OpenEntry, s => CloseAsync((IImageViewerHotkeyReceiver)s) },
            { Operations.OpenApp1, s => OpenFileByEditorAsync((IImageViewerHotkeyReceiver)s, 1) },
            { Operations.OpenApp2, s => OpenFileByEditorAsync((IImageViewerHotkeyReceiver)s, 2) },
            { Operations.OpenAssociatedApp, s => OpenFileByAppAsync((IImageViewerHotkeyReceiver)s) },
        };
    }

    private static async ValueTask CloseAsync(IImageViewerHotkeyReceiver receiver)
    {
        await receiver.Messenger.RaiseAsync(
            new WindowActionMessage(WindowAction.Close, MessageKey.Close));
    }

    private ValueTask OpenFileByEditorAsync(IImageViewerHotkeyReceiver receiver, int index)
    {
        return OpenAppAsync(index, receiver.TargetFilepath, 1, receiver.Messenger);
    }

    private ValueTask OpenFileByAppAsync(IImageViewerHotkeyReceiver receiver)
    {
        return StartAssociatedAppAsync(receiver.TargetFilepath, receiver.Messenger);
    }
}