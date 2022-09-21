using Anna.DomainModel;
using Anna.DomainModel.Interfaces;
using Anna.Gui.Foundations;
using SimpleInjector;
using System;

namespace Anna.Gui.ViewModels;

public class DirectoryWindowViewModel : ViewModelBase
{
    #region ViewViewModel

    private DirectoryViewViewModel? _ViewViewModel;

    public DirectoryViewViewModel? ViewViewModel
    {
        get => _ViewViewModel;
        private set
        {
            var old = _ViewViewModel;
            if (SetProperty(ref _ViewViewModel, value))
                old?.Dispose();
        }
    }

    #endregion

    public DirectoryWindowViewModel(Container dic, IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _dic = dic;
    }

    public DirectoryWindowViewModel Setup(Directory model)
    {
        _model = model;
        
        ViewViewModel = _dic.GetInstance<DirectoryViewViewModel>()
            .Setup(model);

        return this;
    }

    public override void Dispose()
    {
        _dic.GetInstance<App>().CloseDirectory(_model ?? throw new NullReferenceException());

        ViewViewModel = null;

        base.Dispose();
    }
    

    private readonly Container _dic;
    private Directory? _model;
    
}