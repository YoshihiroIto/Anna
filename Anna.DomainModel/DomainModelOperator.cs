using Anna.DomainModel.FileSystem;
using Anna.Service;

namespace Anna.DomainModel;

public class DomainModelOperator
{
    private readonly ILoggerService _logger;
    private readonly IObjectLifetimeCheckerService _objectLifetimeChecker;

    public DomainModelOperator(
        ILoggerService logger,
        IObjectLifetimeCheckerService objectLifetimeChecker)
    {
        _logger = logger;
        _objectLifetimeChecker = objectLifetimeChecker;
    }

    public Folder CreateFolder(string path)
    {
        return new FileSystemFolder(path, _logger, _objectLifetimeChecker);
    }
}