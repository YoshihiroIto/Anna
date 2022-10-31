using Anna.Gui.Foundations;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Hotkey;
using Anna.Gui.Views.Windows;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
using WindowBase=Anna.Gui.Views.Windows.Base.WindowBase;

namespace Anna.Gui.Views.Panels;

public sealed partial class ImageViewer : UserControl, IImageViewerHotkeyReceiver
{
    Window IHotkeyReceiver.Owner => ControlHelper.FindOwnerWindow(this);
    public string TargetFilepath => ViewModel.Model.Path;
    
    private ImageViewerViewModel ViewModel => _viewModel ?? throw new NullReferenceException();
    private ImageViewerViewModel? _viewModel;

    public Messenger Messenger =>
        (ControlHelper.FindOwnerWindow(this) as WindowBase)?.ViewModel.Messenger ??
        throw new NullReferenceException();

    public ImageViewer()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
        PropertyChanged += (_, e) =>
        {
            if (e.Property == DataContextProperty)
                _viewModel = DataContext as ImageViewerViewModel ?? throw new NotSupportedException();
        };
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKey.Close));
            e.Handled = true;
        }
        else
        {
            await ViewModel.Hotkey.OnKeyDownAsync(this, e);
        }
    }
}