﻿using Anna.Gui.Views.Windows.Base;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Anna.Gui.Views.Windows.Dialogs;

public partial class CompressEntryDialog : WindowBase<CompressEntryDialogViewModel>
{
    public CompressEntryDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        DoMoveFocus(e);
    }
}