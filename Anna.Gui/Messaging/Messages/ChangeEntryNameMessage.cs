using Anna.Gui.Views.Windows.Base;
using Avalonia;

namespace Anna.Gui.Messaging.Messages;

public sealed class ChangeEntryNameMessage : InteractionMessage
{
    public static readonly StyledProperty<string> NameProperty =
        AvaloniaProperty.Register<InformationMessage, string>(nameof(Name));

    public string Name
    {
        get => GetValue(NameProperty);
        init => SetValue(NameProperty, value);
    }
    
    public (DialogResultTypes DialogResult, string Name) Response { get; internal set; }

    public ChangeEntryNameMessage(string name, string messageKey)
        : base(messageKey)
    {
        Name = name;
    }
}