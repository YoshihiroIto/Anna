using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.Gui.Interactions.Hotkey;
using Anna.Gui.Interfaces;
using Anna.Service;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Reactive.Bindings.Extensions;
using System.IO;
using System.Threading.Tasks;

namespace Anna.Gui.Views.Panels;

public sealed class ImageViewerViewModel : HasModelViewModelBase<ImageViewerViewModel, Entry>, IViewerViewModel
{
    public HotkeyBase Hotkey { get; }
    public string TargetFilePath => Model.Path;

    #region Image

    private Bitmap? _Image;

    public Bitmap? Image
    {
        get => _Image;
        set => SetProperty(ref _Image, value);
    }

    #endregion

    public ImageViewerViewModel(IServiceProvider dic)
        : base(dic)
    {
        Hotkey = dic.GetInstance<ImageViewerHotkey>().AddTo(Trash);

        Task.Run(async () =>
        {
            var image = await Model.ReadAllBinaryAsync();
            using var ms = new MemoryStream(image);

            // ReSharper disable once AccessToDisposedClosure
            await Dispatcher.UIThread.InvokeAsync(() => Image = new Bitmap(ms));
        });
    }
}