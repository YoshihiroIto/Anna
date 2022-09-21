using Anna.DomainModel;
using Anna.DomainModel.Interfaces;
using Anna.Gui.Foundations;
using Reactive.Bindings.Extensions;
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

    // todo:impl Messenger
    public event EventHandler? Close;

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

        _dic.GetInstance<App>().Directories.CollectionChangedAsObservable()
            .Subscribe(_ =>
            {
                if (_dic.GetInstance<App>().Directories.IndexOf(_model) == -1)
                    Close?.Invoke(this, EventArgs.Empty);
            }).AddTo(Trash);

        return this;
    }

    public override void Dispose()
    {
        if (_isDispose)
            return;

        _isDispose = true;
        
        _dic.GetInstance<App>().CloseDirectory(_model ?? throw new NullReferenceException());

        ViewViewModel = null;

        base.Dispose();
    }

    private bool _isDispose;
    private readonly Container _dic;
    private Directory? _model;
}