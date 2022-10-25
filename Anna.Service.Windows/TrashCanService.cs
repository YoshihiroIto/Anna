using Anna.Foundation;
using Anna.Service.Interfaces;
using Anna.Service.Services;
using Jewelry.Text;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Exception=System.Exception;

namespace Anna.Service.Windows;

public sealed class TrashCanService : ITrashCanService
{
    private readonly IServiceProvider _dic;
    private bool IsInitialized => _sid != "";
    private string _sid = "";
    private readonly Random _random = new();

    private static readonly char[] FilenameChars =
    {
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
        'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
    };

    public TrashCanService(IServiceProvider dic)
    {
        _dic = dic;
    }

    public void SendToTrashCan(IEntry target)
    {
        if (IsInitialized == false)
            Initialize();

        var metaDataBuffer = ArrayPool<byte>.Shared.Rent(target.Path.Length * 2 + 64);
        var metaDataBufferSpan = metaDataBuffer.AsSpan();

        try
        {
            var trashedPath = MakeTrashedPath(target.Path);

            var size = target.IsFolder ? FileHelper.MeasureFolderSize(new DirectoryInfo(target.Path)) : target.Size;
            var metaData = MakeMetaData(metaDataBufferSpan, target.Path, size, DateTime.Now);

            Directory.Move(target.Path, trashedPath.Trashed);
            FileHelper.WriteSpan(trashedPath.MetaData, metaData);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(metaDataBuffer);
        }
    }

    public bool EmptyTrashCan()
    {
        try
        {
            SHEmptyRecycleBin(IntPtr.Zero,
                null,
                SHERB_NOCONFIRMATION |
                SHERB_NOPROGRESSUI |
                SHERB_NOSOUND
            );

            return true;
        }
        catch (Exception e)
        {
            _dic.GetInstance<ILoggerService>().Error("EmptyTrashCan:" + e.Message);

            return false;
        }
    }
    
    public void OpenTrashCan()
    {
        ProcessHelper.RunAssociatedApp("shell:::{645FF040-5081-101B-9F08-00AA002F954E}");
    }

    private string FindTrashCanPath(string targetPath)
    {
        var driveName = Path.GetPathRoot(targetPath) ?? throw new NullReferenceException();

        return Path.Combine($"{driveName}$Recycle.Bin", _sid);
    }

    private (string Trashed, string MetaData) MakeTrashedPath(string targetPath)
    {
        const string filePrefix = "$R";
        const string metadataPrefix = "$I";

        var trashCanPath = FindTrashCanPath(targetPath);
        var extension = Path.GetExtension(targetPath);
        var name = MakeRandomName();

        return (
            Path.Combine(trashCanPath, $"{filePrefix}{name}{extension}"),
            Path.Combine(trashCanPath, $"{metadataPrefix}{name}{extension}")
        );
    }

    public string MakeRandomName()
    {
        const int fileNameLength = 6;

        return string.Create(
            fileNameLength,
            this,
            (buffer, t) =>
            {
                for (var i = 0; i != fileNameLength; ++i)
                    buffer[i] = FilenameChars[t._random.Next(FilenameChars.Length)];
            });
    }

    private void Initialize()
    {
        var userInfo = ProcessHelper.ExecuteAndGetStdoutAsync("whoami", "/user").Result;

        using var ss = new StringSplitter(stackalloc StringSplitter.StringSpan[32]);

        var parts = ss.Split(userInfo, ' ', StringSplitOptions.RemoveEmptyEntries);
        var last = parts[^1];

        _sid = last.ToString(userInfo).TrimEnd();
    }

    private static ReadOnlySpan<byte> MakeMetaData(
        Span<byte> buffer, string filePath, long fileSize, DateTime removingDateTime)
    {
        var index = 0;

        Unsafe.As<byte, long>(ref buffer[index]) = 2;
        index += Unsafe.SizeOf<long>();

        Unsafe.As<byte, long>(ref buffer[index]) = fileSize;
        index += Unsafe.SizeOf<long>();

        Unsafe.As<byte, long>(ref buffer[index]) = removingDateTime.ToFileTime();
        index += Unsafe.SizeOf<long>();

        Unsafe.As<byte, int>(ref buffer[index]) = filePath.Length + 1;
        index += Unsafe.SizeOf<int>();

        MemoryMarshal.Cast<char, byte>(filePath.AsSpan()).CopyTo(buffer[index..]);
        index += filePath.Length * 2;

        Unsafe.As<byte, long>(ref buffer[index]) = 0;
        index += Unsafe.SizeOf<short>();

        return buffer[..index];
    }

    // ReSharper disable all
    private const uint SHERB_NOCONFIRMATION = 0x00000001;
    private const uint SHERB_NOPROGRESSUI = 0x00000002;
    private const uint SHERB_NOSOUND = 0x00000004;

    [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
    private static extern uint SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);
    // ReSharper restore all
}