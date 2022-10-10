using Anna.DomainModel.FileSystem;
using Anna.UseCase;

namespace Anna.DomainModel;

public class DomainModelOperator
{
    private readonly ILoggerUseCase _logger;
    private readonly IObjectLifetimeCheckerUseCase _objectLifetimeChecker;

    public DomainModelOperator(
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
    {
        _logger = logger;
        _objectLifetimeChecker = objectLifetimeChecker;
    }

    public Folder CreateFolder(string path)
    {
        return new FileSystemFolder(path, _logger, _objectLifetimeChecker);
    }
}