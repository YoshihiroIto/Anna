using Anna.Gui.Views.Controls;
using Anna.Gui.Views.Dialogs.Base;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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

        var firstItem = FindListBox()
            .ItemContainerGenerator
            .Containers
            .FirstOrDefault(x => x.Index == 0);

        if (firstItem is not null)
            FocusManager.Instance?.Focus(firstItem.ContainerControl, NavigationMethod.Directional);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (IsEditing)
            return;

        if (e.Key is Key.Insert or Key.Space)
        {
            EditSelectedItem();
            e.Handled = true;
        }
        else
            ViewModel.OnKeyDown(e);
    }

    private JumpFolderDialogViewModel ViewModel =>
        DataContext as JumpFolderDialogViewModel ?? throw new NullReferenceException();

    private void PathOnPreviewPointerPress(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(null).Properties.IsRightButtonPressed)
            return;

        var editableTextBlock = sender as EditableTextBlock ?? throw new InvalidCastException();

        var listBox = FindListBox();
        listBox.SelectedItem = editableTextBlock.DataContext;

        editableTextBlock.IsEditing = !editableTextBlock.IsEditing;

        if (listBox.SelectedItem is JumpFolderPathViewModel vm)
        {
            if (vm.Path.Value == "")
                vm.Path.Value = ViewModel.CurrentFolderPath;
        }

        e.Handled = true;
    }

    private void OnEditingFinished()
    {
        var listBox = FindListBox();

        var currentItem = listBox
            .ItemContainerGenerator
            .Containers
            .FirstOrDefault(x => x.Item == listBox.SelectedItem);

        if (currentItem is not null)
            FocusManager.Instance?.Focus(currentItem.ContainerControl, NavigationMethod.Directional);
    }

    public void EditSelectedItem()
    {
        if (FindListBox().SelectedItem is JumpFolderPathViewModel vm)
            vm.IsEditing.Value = true;
    }

    private ListBox FindListBox()
    {
        return this.FindControl<ListBox>("ListBox") ?? throw new NullReferenceException();
    }

    private void OnIsEditingChanged(object? sender, IsEditingChangedArgs e)
    {
        var old = IsEditing;

        _editingDepth += e.IsEditing ? +1 : -1;

        if (IsEditing == false && old != IsEditing)
            OnEditingFinished();
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        ViewModel.OnKeyDown(new KeyEventArgs { Key = Key.Enter });
    }
    
    private void EditButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button ?? throw new NullReferenceException();

        FindListBox().SelectedItem = button.DataContext;
        
        EditSelectedItem();
        e.Handled = true;
    }

    private bool IsEditing => _editingDepth > 0;
    private int _editingDepth;
}