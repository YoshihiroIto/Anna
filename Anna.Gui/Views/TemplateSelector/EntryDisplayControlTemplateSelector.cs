using Anna.Gui.Views.Panels;
using Anna.Gui.Views.Windows.Dialogs;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using System;

namespace Anna.Gui.Views.TemplateSelector;

public sealed class EntryDisplayControlTemplateSelector : IControlTemplate
{
    public IControlTemplate? TextTemplate { get; set; }
    public IControlTemplate? ImageTemplate { get; set; }

    public ControlTemplateResult Build(ITemplatedControl param)
    {
        var contentControl = param as ContentControl ?? throw new NullReferenceException();

        if (contentControl.Content is not EntryDisplayDialogViewModel vm)
            throw new NotSupportedException();

        return vm.ContentViewModel switch
               {
                   ImageViewerViewModel => ImageTemplate?.Build(param),
                   TextViewerViewModel => TextTemplate?.Build(param),
                   _ => throw new NullReferenceException()
               } ??
               throw new NullReferenceException();
    }
}