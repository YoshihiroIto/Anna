using Anna.Gui.Messaging;
using Anna.Gui.ShortcutKey;
using Anna.Gui.Views.Dialogs.Base;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using System;
using System.Reflection;
using TextMateSharp.Grammars;

namespace Anna.Gui.Views.Dialogs;

// ReSharper disable once PartialTypeWithSinglePart
public partial class EntryDisplayDialog
    : DialogBase<EntryDisplayDialogViewModel>, IEntryDisplayDialogShortcutKeyReceiver
{
    Window IShortcutKeyReceiver.Owner => this;
    InteractionMessenger IShortcutKeyReceiver.Messenger => ViewModel.Messenger;

    public TextEditor TextEditor { get; private set; } = null!;

    public ScrollViewer ScrollViewer
    {
        get
        {
            if (_scrollViewer is not null)
                return _scrollViewer;

            var prop = typeof(TextEditor).GetProperty("ScrollViewer", BindingFlags.Instance | BindingFlags.NonPublic) ??
                       throw new NullReferenceException();
            _scrollViewer = prop.GetValue(TextEditor) as ScrollViewer ?? throw new NullReferenceException();

            return _scrollViewer;
        }
    }

    private ScrollViewer? _scrollViewer;

    public EntryDisplayDialog()
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

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        TextEditor = this.FindControl<TextEditor>("ViewTextEditor") ?? throw new NullReferenceException();

        var registryOptions = new RegistryOptions(ThemeName.TomorrowNightBlue);
        TextEditor.InstallTextMate(registryOptions);

        var lang = registryOptions.GetLanguageByExtension(ViewModel.Model.Extension);
        if (lang is not null)
        {
            var textMateInstallation = TextEditor.InstallTextMate(registryOptions);
            textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(lang.Id));
        }

        TextEditor.AddHandler(KeyDownEvent, TextEditor_OnKeyDown, RoutingStrategies.Tunnel);

        // https://stackoverflow.com/questions/43654090/avalonedit-as-a-text-viewer-no-caret
        TextEditor.TextArea.Caret.CaretBrush = Brushes.Transparent;
        TextEditor.TextArea.DefaultInputHandler.NestedInputHandlers.Remove(
            TextEditor.TextArea.DefaultInputHandler.CaretNavigation);

        try
        {
            TextEditor.Text = await ViewModel.Model.ReadStringAsync();
        }
        catch
        {
            // ignored
        }

        TextEditor.TextArea.Focus();
    }

    private async void TextEditor_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ViewModel.DialogResult = DialogResultTypes.Cancel;
            Close();
            e.Handled = true;
        }
        else
        {
            await ViewModel.ShortcutKey.OnKeyDownAsync(this, e);
        }
    }
}