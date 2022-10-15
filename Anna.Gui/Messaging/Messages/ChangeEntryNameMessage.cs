using Anna.Gui.Views.Windows.Base;

namespace Anna.Gui.Messaging.Messages;

public sealed class ChangeEntryNameMessage : InteractionMessage
{
    public string Name { get; init; }
    
    public (DialogResultTypes DialogResult, string Name) Response { get; internal set; }

    public ChangeEntryNameMessage(string name, string messageKey)
        : base(messageKey)
    {
        Name = name;
    }
}