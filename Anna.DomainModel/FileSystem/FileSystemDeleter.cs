using Anna.Constants;
using Anna.Service.Interfaces;
using Anna.Service.Services;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel.FileSystem;

public abstract class FileSystemDeleter
{
    public event EventHandler? FileDeleted;

    protected CancellationTokenSource? CancellationTokenSource { get; private set; }
    private readonly IServiceProvider _dic;

    protected FileSystemDeleter(IServiceProvider dic)
    {
        _dic = dic;
    }

    public void Invoke(IEnumerable<IEntry> sourceEntries, EntryDeleteModes mode)
    {
        CancellationTokenSource = new CancellationTokenSource();

        throw new NotImplementedException();

        try
        {
        }
        catch (OperationCanceledException)
        {
            _dic.GetInstance<ILoggerService>().Information("FileSystemDeleter.Invoke() -- Canceled");
        }
        finally
        {
            var d = CancellationTokenSource;
            CancellationTokenSource = null;
            d.Dispose();
        }
    }
}

public class DefaultFileSystemDeleter : FileSystemDeleter
{
    public DefaultFileSystemDeleter(IServiceProvider dic)
        : base(dic)
    {
    }
}