﻿using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows;
using System;
using System.Collections.Generic;
using System.IO;
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
            { Operations.OpenExternal1, s => OpenAppAsync((IImageViewerHotkeyReceiver)s, ExternalApp.App1) },
            { Operations.OpenExternal2, s => OpenAppAsync((IImageViewerHotkeyReceiver)s, ExternalApp.App2) },
            { Operations.OpenAssociatedApp, s => OpenFileByAppAsync((IImageViewerHotkeyReceiver)s) },
            { Operations.OpenTerminal, s => OpenAppAsync((IImageViewerHotkeyReceiver)s, ExternalApp.Terminal) },
        };
    }

    private static async ValueTask CloseAsync(IImageViewerHotkeyReceiver receiver)
    {
        await receiver.Messenger.RaiseAsync(
            new WindowActionMessage(WindowAction.Close, MessageKey.Close));
    }

    private ValueTask OpenAppAsync(IImageViewerHotkeyReceiver receiver, ExternalApp app)
    {
        var targetFolderPath = Path.GetDirectoryName(receiver.TargetFilePath) ?? "";
        
        return OpenAppAsync(app, receiver.TargetFilePath, targetFolderPath, 1, receiver.Messenger);
    }

    private ValueTask OpenFileByAppAsync(IImageViewerHotkeyReceiver receiver)
    {
        return StartAssociatedAppAsync(receiver.TargetFilePath, receiver.Messenger);
    }
}