using Anna.Service;
using Anna.Service.Services;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel.FileSystem.FileProcessable;

public sealed class FileSystemDecompressor
    : IHasArg<(IEnumerable<string> ArchiveFilePaths, string DestFolderPath)>
        , IFileProcessable
{
    public static readonly FileSystemDecompressor T = default!;

    public event EventHandler? FileProcessed;

    private readonly IServiceProvider _dic;
    private readonly (IEnumerable<string> ArchiveFilePaths, string DestFolderPath) _arg;

    public FileSystemDecompressor(IServiceProvider dic)
    {
        _dic = dic;

        dic.PopArg(out _arg);
    }

    public void Invoke()
    {
        _dic.GetInstance<ICompressionService>().Decompress(
            _arg.ArchiveFilePaths,
            _arg.DestFolderPath,
            p => FileProcessed?.Invoke(this, new FileProcessedDirectEventArgs(p))
        );

        _dic.GetInstance<IFolderHistoryService>().AddDestinationFolder(_arg.DestFolderPath);
    }
}