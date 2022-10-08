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
public partial class EntryDisplayDialog : DialogBase<EntryDisplayDialogViewModel>
{
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

        _textEditor = this.FindControl<TextEditor>("TextEditor") ?? throw new NullReferenceException();

        var registryOptions = new RegistryOptions(ThemeName.TomorrowNightBlue);
        _textEditor.InstallTextMate(registryOptions);

        var lang = registryOptions.GetLanguageByExtension(ViewModel.Model.Extension);
        if (lang is not null)
        {
            var textMateInstallation = _textEditor.InstallTextMate(registryOptions);
            textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(lang.Id));
        }

        _textEditor.AddHandler(KeyDownEvent, TextEditor_OnKeyDown, RoutingStrategies.Tunnel);

        // https://stackoverflow.com/questions/43654090/avalonedit-as-a-text-viewer-no-caret
        _textEditor.TextArea.Caret.CaretBrush = Brushes.Transparent;
        _textEditor.TextArea.DefaultInputHandler.NestedInputHandlers.Remove(
            _textEditor.TextArea.DefaultInputHandler.CaretNavigation);

        try
        {
            _textEditor.Text = await ViewModel.Model.ReadStringAsync();
        }
        catch
        {
            // ignored
        }

        _textEditor.TextArea.Focus();
    }

    private TextEditor? _textEditor;
    private ScrollViewer? _scrollViewer;

    private void TextEditor_OnKeyDown(object? sender, KeyEventArgs e)
    {
        _ = _textEditor ?? throw new NullReferenceException();
        
        if (_scrollViewer is null)
        {
            var prop = typeof(TextEditor).GetProperty("ScrollViewer", BindingFlags.Instance | BindingFlags.NonPublic) ??
                       throw new NullReferenceException();
            _scrollViewer = prop.GetValue(_textEditor) as ScrollViewer ?? throw new NullReferenceException();
        }

        var lineHeight =
            _textEditor.TextArea.TextView.GetVisualTopByDocumentLine(2) -
            _textEditor.TextArea.TextView.GetVisualTopByDocumentLine(1);

        var pageHeight = TrimmingScrollY(_textEditor.TextArea.Bounds.Height, lineHeight);

        switch (e.Key)
        {
            case Key.Escape:
                ViewModel.DialogResult = DialogResultTypes.Cancel;
                Close();
                e.Handled = true;
                break;

            case Key.Up:
                _scrollViewer.Offset = new Vector(0, TrimmingScrollY(_scrollViewer.Offset.Y - lineHeight, lineHeight));
                e.Handled = true;
                break;

            case Key.Down:
                _scrollViewer.Offset = new Vector(0, TrimmingScrollY(_scrollViewer.Offset.Y + lineHeight, lineHeight));
                e.Handled = true;
                break;

            case Key.Left:
                _scrollViewer.Offset = new Vector(0, TrimmingScrollY(_scrollViewer.Offset.Y - pageHeight, lineHeight));
                e.Handled = true;
                break;

            case Key.Right:
                _scrollViewer.Offset = new Vector(0, TrimmingScrollY(_scrollViewer.Offset.Y + pageHeight, lineHeight));
                e.Handled = true;
                break;
        }
    }

    private static double TrimmingScrollY(double v, double lineHeight)
    {
        return Math.Round(v / lineHeight) * lineHeight;
    }
}