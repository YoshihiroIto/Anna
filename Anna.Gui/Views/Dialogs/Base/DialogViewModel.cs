using Anna.Gui.Foundations;
using Anna.UseCase;
using System;

namespace Anna.Gui.Views.Dialogs.Base;

public class DialogViewModel : ViewModelBase
{
    private readonly ILoggerUseCase _logger;
    public DialogResultTypes DialogResult { get; set; } = DialogResultTypes.Cancel;

    protected DialogViewModel(ILoggerUseCase logger, IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _logger = logger;
        
        _logger.Start(GetType().Name);
    }

    public override void Dispose()
    {
        _logger.End(GetType().Name);
        
        base.Dispose();
        
        GC.SuppressFinalize(this);
    }
}

public enum DialogResultTypes
{
    Ok,
    Cancel
}