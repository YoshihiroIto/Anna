﻿using Anna.Foundation;
using Anna.Service.Interfaces;
using Anna.Service.Services;
using Jewelry.Text;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Anna.Service.Windows;

public sealed class TrashCanService : ITrashCanService
{
    private bool IsInitialized => _sid != "";
    private string _sid = "";
    private readonly Random _random = new();
    
    private static readonly char[] FilenameChars =
    {
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
        'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
    };

    public void SendToTrashCan(IEnumerable<IEntry> targets)
    {
        if (IsInitialized == false)
            Initialize();

        var removingDateTime = DateTime.Now;

        foreach (var target in targets)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(target.Path.Length * 2 + 64);
            var bufferSpan = buffer.AsSpan();

            try
            {
                var trashedPath = MakeTrashedPath(target.Path);

                // todo: Folder support
                if (target.IsFolder)
                    throw new NotImplementedException();
                
                var bufferSize = MakeMetaData(bufferSpan, target.Path, target.Size, removingDateTime);
                
                File.Move(target.Path, trashedPath.Trashed);
                FileHelper.WriteSpan(trashedPath.MetaData, bufferSpan[..bufferSize]);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
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

    private static int MakeMetaData(Span<byte> buffer, string filePath, long fileSize, DateTime removingDateTime)
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

        return index;
    }
}