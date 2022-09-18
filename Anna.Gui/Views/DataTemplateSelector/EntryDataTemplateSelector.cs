﻿using Anna.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;

namespace Anna.Views;

public class EntryDataTemplateSelector : IDataTemplate
{
    public IDataTemplate? FileDataTemplate { get; set; }
    public IDataTemplate? DirectoryDataTemplate { get; set; }

    public IControl? Build(object? param)
    {
        if (param is not EntryViewModel vm)
            throw new NotSupportedException();

        return vm.IsDirectory
            ? DirectoryDataTemplate?.Build(param) 
            : FileDataTemplate?.Build(param);
    }
        
    public bool Match(object? data)
    {
        return data is EntryViewModel;
    }
}