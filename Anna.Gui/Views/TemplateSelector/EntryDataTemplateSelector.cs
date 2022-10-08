using Anna.Gui.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;

namespace Anna.Gui.Views.TemplateSelector;

public class EntryDataTemplateSelector : IDataTemplate
{
    public IDataTemplate? FileDataTemplate { get; set; }
    public IDataTemplate? FolderDataTemplate { get; set; }

    public IControl? Build(object? param)
    {
        if (param is not EntryViewModel vm)
            throw new NotSupportedException();

        return vm.IsFolder
            ? FolderDataTemplate?.Build(param)
            : FileDataTemplate?.Build(param);
    }

    public bool Match(object? data)
    {
        return data is EntryViewModel;
    }
}