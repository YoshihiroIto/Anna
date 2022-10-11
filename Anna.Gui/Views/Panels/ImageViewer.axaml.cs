using Anna.Gui.Foundations;
using Anna.Gui.Messaging;
using Anna.Gui.ShortcutKey;
using Anna.Gui.Views.Windows.Base;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
using System.Threading.Tasks;
using WindowBase=Anna.Gui.Views.Windows.Base.WindowBase;

namespace Anna.Gui.Views.Panels;

public partial class ImageViewer : UserControl, IImageViewerShortcutKeyReceiver
{
    Window IShortcutKeyReceiver.Owner => ControlHelper.FindOwnerWindow(this);
    public string TargetFilepath => ViewModel.Model.Path;

    public InteractionMessenger Messenger =>
        ((ControlHelper.FindOwnerWindow(this) as WindowBase)?.DataContext as ViewModelBase)?.Messenger ??
        throw new NullReferenceException();

    public ImageViewer()
    {
        InitializeComponent();

        PropertyChanged += async (_, e) =>
        {
            if (e.Property == DataContextProperty)
                await SetupAsync();
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private ImageViewerViewModel ViewModel =>
        DataContext as ImageViewerViewModel ?? throw new NotSupportedException();

    private Task SetupAsync()
    {
        return Task.CompletedTask;
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, WindowBaseViewModel.MessageKeyClose));
            e.Handled = true;
        }
        else
        {
            await ViewModel.ShortcutKey.OnKeyDownAsync(this, e);
        }
    }
}