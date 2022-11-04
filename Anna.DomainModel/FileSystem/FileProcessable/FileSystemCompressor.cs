using Anna.Service;
using Anna.Service.Services;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel.FileSystem.FileProcessable;

public sealed class FileSystemCompressor
    : IHasArg<(IEnumerable<string> SourceEntryPaths, string DestArchiveFilePath)>
        , IFileProcessable
{
    public static readonly FileSystemCompressor T = default!;

    public event EventHandler? FileProcessed;

    private readonly IServiceProvider _dic;
    private readonly (IEnumerable<string> SourceEntryPaths, string DestArchiveFilePath) _arg;

    public FileSystemCompressor(IServiceProvider dic)
    {
        _dic = dic;

        dic.PopArg(out _arg);
    }

    public void Invoke()
    {
        _dic.GetInstance<ICompressorService>().Compress(
            _arg.SourceEntryPaths,
            _arg.DestArchiveFilePath,
            () => FileProcessed?.Invoke(this, EventArgs.Empty));

        var destFolder = Path.GetDirectoryName(_arg.DestArchiveFilePath) ?? throw new NullReferenceException();

        _dic.GetInstance<IFolderHistoryService>().AddDestinationFolder(destFolder);
    }
}