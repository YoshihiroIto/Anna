using Anna.Gui.Views.Dialogs.Base;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
using System.Linq;

namespace Anna.Gui.Views.Dialogs;

public partial class JumpFolderDialog : DialogBase
{
    public JumpFolderDialog()
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

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        var firstItem = this.FindControl<ListBox>("ListBox")?
            .ItemContainerGenerator
            .Containers
            .FirstOrDefault(x => x.Index == 0);

        if (firstItem is not null)
            FocusManager.Instance?.Focus(firstItem.ContainerControl, NavigationMethod.Directional);
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        ViewModel.OnKeyDown(e);
    }

    private JumpFolderDialogViewModel ViewModel =>
        DataContext as JumpFolderDialogViewModel ?? throw new NullReferenceException();
}