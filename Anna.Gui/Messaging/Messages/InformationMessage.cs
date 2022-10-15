using Anna.Gui.Views.Windows.Base;
using Avalonia;

namespace Anna.Gui.Messaging.Messages;

public sealed class InformationMessage : InteractionMessage
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<InformationMessage, string>(nameof(Title));

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<InformationMessage, string>(nameof(Text));

    public string Title
    {
        get => GetValue(TitleProperty);
        init => SetValue(TitleProperty, value);
    }

    public string Text
    {
        get => GetValue(TextProperty);
        init => SetValue(TextProperty, value);
    }
    
    public DialogResultTypes Response { get; internal set; }

    public InformationMessage(
        string title,
        string text,
        string messageKey)
        : base(messageKey)
    {
        Title = title;
        Text = text;
    }
}