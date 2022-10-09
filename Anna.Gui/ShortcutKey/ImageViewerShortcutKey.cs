using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging;
using Anna.Gui.Views.Dialogs.Base;
using Anna.UseCase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anna.Gui.ShortcutKey;

public class ImageViewerShortcutKey : ShortcutKeyBase
{
    private readonly AppConfig _appConfig;
    public ImageViewerShortcutKey(
        IFolderServiceUseCase folderService,
        AppConfig appConfig,
        KeyConfig keyConfig,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(folderService, keyConfig, logger, objectLifetimeChecker)
    {
        _appConfig = appConfig;
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

    private async ValueTask OpenFileByEditorAsync(IShortcutKeyReceiver shortcutKeyReceiver, int index)
    {
        var r = shortcutKeyReceiver as IImageViewerShortcutKeyReceiver ?? throw new InvalidOperationException();

        var targetFilepath = r.TargetFilepath;

 #pragma warning disable CS4014
        Task.Run(() =>
        {
            var editor = _appConfig.Data.FindEditor(index);
            var arguments = ProcessHelper.MakeEditorArguments(editor.Options, targetFilepath, 0);

            ProcessHelper.Execute(editor.Editor, arguments);
        });
 #pragma warning restore CS4014

        await r.Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, DialogViewModel.MessageKeyClose));
    }

    private ValueTask OpenFileByAppAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var r = shortcutKeyReceiver as IImageViewerShortcutKeyReceiver ?? throw new InvalidOperationException();

        var targetFilepath = r.TargetFilepath;

        Task.Run(() => ProcessHelper.RunAssociatedApp(targetFilepath));

        return ValueTask.CompletedTask;
    }
}