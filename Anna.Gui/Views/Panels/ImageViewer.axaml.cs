using Anna.Gui.Foundations;
using Anna.Gui.Messaging;
using Anna.Gui.Interactions.Hotkey;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using WindowBase=Anna.Gui.Views.Windows.Base.WindowBase;

namespace Anna.Gui.Views.Panels;

public sealed partial class ImageViewer : UserControl, IImageViewerHotkeyReceiver
{
    public string TargetFilePath => ViewModel.Model.Path;
    
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
}