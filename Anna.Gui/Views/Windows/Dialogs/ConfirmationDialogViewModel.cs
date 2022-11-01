using Anna.Constants;
using Anna.Foundation;
using Anna.Gui.Views.Windows.Base;
using Anna.Localization;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DialogResultTypes=Anna.Constants.DialogResultTypes;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class ConfirmationDialogViewModel : HasModelWindowBaseViewModel<ConfirmationDialogViewModel,
    (string Title, string Text, DialogResultTypes confirmations)>
{
    public string Title => Model.Title;
    public string Text => Model.Text;

    public int ConfirmationCount => BitOperations.PopCount((uint)Model.confirmations);
    public Confirmation[] Confirmations { get; }

    public ConfirmationDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        Confirmations = ArrayPool<Confirmation>.Shared.Rent(ConfirmationCount);
        Trash.Add(() => ArrayPool<Confirmation>.Shared.Return(Confirmations));

        var confirmations = (int)Model.confirmations;
        for (var i = 0;; ++i)
        {
            var c = RightMostBit(confirmations);
            if (c == 0)
                break;

            confirmations &= ~c;

            var command = CreateButtonCommand((DialogResultTypes)c);
            var caption = Resources.ResourceManager.GetString("Dialog_" + ((DialogResultTypes)c).ToStringFast()) ??
                          throw new NullReferenceException();

            Confirmations[i] = new Confirmation(command, caption);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int RightMostBit(int x)
    {
        return x & -x;
    }

    public record struct Confirmation(ICommand Command, string Caption);
}

public sealed class ConfirmationDialogDataTemplateSelector : IControlTemplate
{
    public IControlTemplate? C1Template { get; set; }
    public IControlTemplate? C2Template { get; set; }
    public IControlTemplate? C3Template { get; set; }
    public IControlTemplate? C4Template { get; set; }

    public ControlTemplateResult Build(ITemplatedControl param)
    {
        var contentControl = param as ContentControl ?? throw new NullReferenceException();

        if (contentControl.Content is not ConfirmationDialogViewModel vm)
            throw new NotSupportedException();

        return vm.ConfirmationCount switch
               {
                   1 => C1Template?.Build(param),
                   2 => C2Template?.Build(param),
                   3 => C3Template?.Build(param),
                   4 => C4Template?.Build(param),
                   _ => throw new NullReferenceException()
               } ??
               throw new NullReferenceException();
    }
}