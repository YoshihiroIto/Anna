using Anna.Gui.Foundations;
using Anna.Gui.Messaging.Messages;
using Anna.Service;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Xaml.Interactivity;
using System;
using System.IO;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;
using WindowBase=Anna.Gui.Views.Windows.Base.WindowBase;

namespace Anna.Gui.Messaging.Behaviors.Actions;

public sealed class TransitionAction : AvaloniaObject, IAction, IAsyncAction
{
    public static readonly StyledProperty<IServiceProvider> DicProperty =
        AvaloniaProperty.Register<TransitionAction, IServiceProvider>(
            nameof(Dic),
            defaultBindingMode: BindingMode.OneTime);

    public static readonly StyledProperty<Type> WindowTypeProperty =
        AvaloniaProperty.Register<TransitionAction, Type>(
            nameof(WindowType),
            defaultBindingMode: BindingMode.OneTime);

    public static readonly StyledProperty<TransitionMode> ModeProperty =
        AvaloniaProperty.Register<TransitionAction, TransitionMode>(
            nameof(Mode),
            defaultBindingMode: BindingMode.OneTime,
            defaultValue: TransitionMode.Unknown);

    public IServiceProvider Dic
    {
        get => GetValue(DicProperty);
        set => SetValue(DicProperty, value);
    }

    public Type WindowType
    {
        get => GetValue(WindowTypeProperty);
        set => SetValue(WindowTypeProperty, value);
    }

    public TransitionMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public object Execute(object? sender, object? parameter)
    {
        throw new NotSupportedException();
    }

    public async ValueTask ExecuteAsync(
        Trigger sender,
        MessageBase message,
        IHasServiceProviderContainer hasServiceProviderContainer)
    {
        if (message is not TransitionMessage transitionMessage)
            return;

        if (sender is not { AssociatedObject: IControl control })
            return;

        if (Mode == TransitionMode.Unknown && transitionMessage.Mode == TransitionMode.Unknown)
            throw new InvalidDataException();

        var mode = transitionMessage.Mode == TransitionMode.Unknown ? Mode : transitionMessage.Mode;
        var owner = ControlHelper.FindOwnerWindow(control);

        switch (mode)
        {
            case TransitionMode.Dialog:
                var targetWindow = Dic.GetInstance<WindowBase>(WindowType);

                targetWindow.DataContext = transitionMessage.ViewModel;

                await targetWindow.ShowDialog(owner);

                break;

            case TransitionMode.Unknown:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}