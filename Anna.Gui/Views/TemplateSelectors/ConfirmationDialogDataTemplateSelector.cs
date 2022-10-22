using Anna.Constants;
using Anna.Gui.Views.Windows.Dialogs;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using System;

namespace Anna.Gui.Views.TemplateSelectors;

public sealed class ConfirmationDialogDataTemplateSelector : IControlTemplate
{
    public IControlTemplate? YesNoDataTemplate { get; set; }
    public IControlTemplate? RetryCancelDataTemplate { get; set; }

    public ControlTemplateResult Build(ITemplatedControl param)
    {
        var contentControl = param as ContentControl ?? throw new NullReferenceException();

        if (contentControl.Content is not ConfirmationDialogViewModel vm)
            throw new NotSupportedException();

        return vm.ConfirmationType switch
               {
                   ConfirmationTypes.YesNo => YesNoDataTemplate?.Build(param),
                   ConfirmationTypes.RetryCancel => RetryCancelDataTemplate?.Build(param),
                   _ => throw new NullReferenceException()
               }
               ??
               throw new NullReferenceException();
    }

    public bool Match(object? data)
    {
        return data is ConfirmationDialogViewModel;
    }
}