using Anna.DomainModel;
using Anna.DomainModel.Interface;
using Anna.Foundations;
using SimpleInjector;

namespace Anna.ViewModels;

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

    private readonly Container _dic;

    public DirectoryWindowViewModel(Container dic, IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _dic = dic;
    }

    public DirectoryWindowViewModel Setup(Directory model)
    {
        ViewViewModel = _dic.GetInstance<DirectoryViewViewModel>()
            .Setup(model);

        return this;
    }

    public override void Dispose()
    {
        ViewViewModel = null;

        base.Dispose();
    }
}