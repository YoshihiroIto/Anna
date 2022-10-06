using Anna.Gui.Views.Dialogs.Base;
using Avalonia;

namespace Anna.Gui.ViewModels.Messaging;

public class ConfirmationMessage : InteractionMessage
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ConfirmationMessage, string>(nameof(Title));

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<ConfirmationMessage, string>(nameof(Text));

    public static readonly StyledProperty<ConfirmationTypes> ConfirmationTypeProperty =
        AvaloniaProperty.Register<ConfirmationMessage, ConfirmationTypes>(nameof(ConfirmationTypes));

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

    public ConfirmationTypes ConfirmationType
    {
        get => GetValue(ConfirmationTypeProperty);
        init => SetValue(ConfirmationTypeProperty, value);
    }

    public DialogResultTypes Response { get; internal set; }

    public ConfirmationMessage(
        string title,
        string text,
        ConfirmationTypes confirmationTypes,
        string messageKey)
        : base(messageKey)
    {
        Title = title;
        Text = text;
        ConfirmationType = confirmationTypes;
    }
}