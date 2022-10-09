using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.Gui.ShortcutKey;
using Anna.UseCase;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Reactive.Bindings.Extensions;
using System.IO;
using System.Threading.Tasks;

namespace Anna.Gui.Views.Panels;

public class ImageViewerViewModel : HasModelRefViewModelBase<Entry>
{
    public readonly ImageViewerShortcutKey ShortcutKey;

    #region Image

    private Bitmap? _Image;

    public Bitmap? Image
    {
        get => _Image;
        set => SetProperty(ref _Image, value);
    }

    #endregion

    public ImageViewerViewModel(
        IServiceProviderContainer dic,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, objectLifetimeChecker)
    {
        ShortcutKey = dic.GetInstance<ImageViewerShortcutKey>().AddTo(Trash);

        Task.Run(async () =>
        {
            var image = await Model.ReadAllBinaryAsync();
            using var ms = new MemoryStream(image);

            // ReSharper disable once AccessToDisposedClosure
            await Dispatcher.UIThread.InvokeAsync(() => Image = new Bitmap(ms));
        });
    }
}