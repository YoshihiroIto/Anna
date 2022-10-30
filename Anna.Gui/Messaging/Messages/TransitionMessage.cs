using Anna.Gui.Views.Windows.Base;

namespace Anna.Gui.Messaging.Messages;

public enum TransitionMode
{
    Unknown,
    Dialog,
}

public sealed class TransitionMessage : MessageBase
{
    public readonly WindowBaseViewModel ViewModel;
    public readonly TransitionMode Mode;

    public TransitionMessage(WindowBaseViewModel viewModel, TransitionMode mode, string messageKey)
        : base(messageKey)
    {
        ViewModel = viewModel;
        Mode = mode;
    }
    
    public TransitionMessage(WindowBaseViewModel viewModel, string messageKey)
        : this(viewModel, TransitionMode.Unknown, messageKey)
    {
    }
}