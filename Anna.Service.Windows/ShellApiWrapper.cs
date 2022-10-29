// ReSharper disable all

using Jewelry.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Anna.Service.Windows;

// https://stackoverflow.com/questions/3282418/send-a-file-to-the-recycle-bin 

internal static class ShellApiWrapper
{
    public static bool EmptyTrashCan()
    {
        var result = SHEmptyRecycleBin(IntPtr.Zero,
            null,
            SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);

        return result == 0;
    }

    public static bool SendToTrashCan(IEnumerable<string> targetFilePaths)
    {
        var fileOp = new SHFILEOPSTRUCT
        {
            wFunc = FileOperationType.FO_DELETE,
            pFrom = MakePathsString(targetFilePaths),
            fFlags = FileOperationFlags.FOF_ALLOWUNDO |
                     FileOperationFlags.FOF_NOCONFIRMATION |
                     FileOperationFlags.FOF_NOERRORUI |
                     FileOperationFlags.FOF_SILENT
        };

        var result = SHFileOperation(ref fileOp);

        return result == 0;
    }

    public static (long EntryAllSize, long EntryCount) GetTrashCanInfo()
    {
        var info = new SHQUERYRBINFO();
        info.cbSize = Unsafe.SizeOf<SHQUERYRBINFO>();

        _ = SHQueryRecycleBin("", ref info);

        return (info.i64Size, info.i64NumItems);
    }

    private static string MakePathsString(IEnumerable<string> paths)
    {
        using var lsb = new LiteStringBuilder(stackalloc char[64 * 1024]);

        foreach (var path in paths)
        {
            lsb.Append(path);
            lsb.Append('\0');
        }

        lsb.Append('\0');
        
        return lsb.ToString();
    }

    [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
    private static extern uint SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHQueryRecycleBin(string pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);

    private const uint SHERB_NOCONFIRMATION = 0x00000001;
    private const uint SHERB_NOPROGRESSUI = 0x00000002;
    private const uint SHERB_NOSOUND = 0x00000004;

    [Flags]
    private enum FileOperationFlags : ushort
    {
        FOF_SILENT = 0x0004,
        FOF_NOCONFIRMATION = 0x0010,
        FOF_ALLOWUNDO = 0x0040,
        FOF_SIMPLEPROGRESS = 0x0100,
        FOF_NOERRORUI = 0x0400,
        FOF_WANTNUKEWARNING = 0x4000,
    }

    private enum FileOperationType : uint
    {
        FO_MOVE = 0x0001,
        FO_COPY = 0x0002,
        FO_DELETE = 0x0003,
        FO_RENAME = 0x0004,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;

        [MarshalAs(UnmanagedType.U4)]
        public FileOperationType wFunc;

        public string pFrom;
        public string pTo;
        public FileOperationFlags fFlags;

        [MarshalAs(UnmanagedType.Bool)]
        public bool fAnyOperationsAborted;

        public IntPtr hNameMappings;
        public string lpszProgressTitle;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct SHQUERYRBINFO
    {
        public int cbSize;
        public long i64Size;
        public long i64NumItems;
    }
}