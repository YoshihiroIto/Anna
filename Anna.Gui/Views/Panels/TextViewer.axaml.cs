using Anna.Gui.Foundations;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.ShortcutKey;
using Anna.Gui.Views.Windows.Base;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using System;
using System.Reflection;
using System.Threading.Tasks;
using TextMateSharp.Grammars;
using WindowBase=Anna.Gui.Views.Windows.Base.WindowBase;

namespace Anna.Gui.Views.Panels;

public partial class TextViewer : UserControl, ITextViewerShortcutKeyReceiver
{
    Window IShortcutKeyReceiver.Owner => ControlHelper.FindOwnerWindow(this);

    public InteractionMessenger Messenger =>
        ((ControlHelper.FindOwnerWindow(this) as WindowBase)?.DataContext as ViewModelBase)?.Messenger ??
        throw new NullReferenceException();

    public string TargetFilepath => ViewModel.Model.Path;
    public int LineIndex => (int)(ScrollViewer.Offset.Y / CalcLineHeight()) + 1;

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

    public double CalcLineHeight()
    {
        try
        {
            if (TextEditor.LineCount <= 1)
                return 1;

            return
                TextEditor.TextArea.TextView.GetVisualTopByDocumentLine(2) -
                TextEditor.TextArea.TextView.GetVisualTopByDocumentLine(1);
        }
        catch
        {
            return 1;
        }
    }

    private ScrollViewer? _scrollViewer;

    private static readonly RegistryOptions TextMateRegistryOptions = new(ThemeName.DarkPlus);

    public TextViewer()
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

    private TextViewerViewModel ViewModel =>
        DataContext as TextViewerViewModel ?? throw new NotSupportedException();

    private async Task SetupAsync()
    {
        TextEditor = this.FindControl<TextEditor>("ViewTextEditor") ?? throw new NullReferenceException();

        SetupTextMate();

        // https://stackoverflow.com/questions/43654090/avalonedit-as-a-text-viewer-no-caret
        TextEditor.TextArea.Caret.CaretBrush = Brushes.Transparent;
        TextEditor.TextArea.DefaultInputHandler.NestedInputHandlers.Remove(
            TextEditor.TextArea.DefaultInputHandler.CaretNavigation);

        TextEditor.Text = await ViewModel.ReadText();

        TextEditor.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        TextEditor.TextArea.Focus();
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

    private void SetupTextMate()
    {
        var lang = TextMateRegistryOptions.GetLanguageByExtension(ViewModel.Model.Extension);
        if (lang is null)
            return;

        var installation = TextEditor.InstallTextMate(TextMateRegistryOptions);
        installation.SetGrammar(TextMateRegistryOptions.GetScopeByLanguageId(lang.Id));

        Unloaded += (_, _) => installation.Dispose();
    }
}