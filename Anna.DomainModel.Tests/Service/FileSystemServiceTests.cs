﻿using Anna.DomainModel.Service;
using Anna.Service.Interfaces;
using Anna.TestFoundation;
using Xunit;

namespace Anna.DomainModel.Tests.Service;

public sealed class FileSystemServiceTests : IDisposable
{
    private readonly TempFolder _tempFolder = new();

    public void Dispose()
    {
        _tempFolder.Dispose();
    }

    [Theory]
    [InlineData("a")]
    [InlineData("a", "b")]
    [InlineData("a", "b", "c")]
    public void File_copy(params string[] srcNames)
    {
        var fss = new FileSystemService();

        _tempFolder.CreateFolder("x");
        var dstFolderPath = Path.Combine(_tempFolder.RootPath, "x");

        foreach (var srcName in srcNames)
        {
            _tempFolder.CreateFile(srcName);

            var srcPath = Path.Combine(_tempFolder.RootPath, srcName);
            File.SetAttributes(srcPath, FileAttributes.ReadOnly );

            var srcEntry = new TestEntry(srcPath, false);

            fss.Copy(dstFolderPath, new[] { srcEntry });

            var dstPath = Path.Combine(dstFolderPath, srcName);

            Assert.True(File.Exists(dstPath));
            Assert.Equal(FileAttributes.ReadOnly, File.GetAttributes(dstPath));
        }
    }

    [Theory]
    [InlineData("a")]
    [InlineData("a", "b")]
    [InlineData("a", "b", "c")]
    public void Folder_copy(params string[] srcNames)
    {
        var fss = new FileSystemService();

        _tempFolder.CreateFolder("x");
        var dstFolderPath = Path.Combine(_tempFolder.RootPath, "x");

        foreach (var srcName in srcNames)
        {
            _tempFolder.CreateFolder(srcName);

            var srcPath = Path.Combine(_tempFolder.RootPath, srcName);
            File.SetAttributes(srcPath, FileAttributes.ReadOnly);

            var srcEntry = new TestEntry(srcPath, true);

            fss.Copy(dstFolderPath, new[] { srcEntry });

            var dstPath = Path.Combine(dstFolderPath, srcName);

            Assert.True(Directory.Exists(dstPath));
            Assert.Equal(FileAttributes.ReadOnly | FileAttributes.Directory, File.GetAttributes(dstPath));
        }
    }

    [Fact]
    public void SubFolder_copy()
    {
        var fss = new FileSystemService();

        _tempFolder.CreateFolder("x/y/z");
        _tempFolder.CreateFile("a");
        _tempFolder.CreateFile("x/b");
        _tempFolder.CreateFile("x/y/c");
        _tempFolder.CreateFile("x/y/z/d");

        var dstFolderPath = Path.Combine(_tempFolder.RootPath, "dst");

        var srcEntry0 = new TestEntry(Path.Combine(_tempFolder.RootPath, "a"), false);
        var srcEntry1 = new TestEntry(Path.Combine(_tempFolder.RootPath, "x"), true);

        fss.Copy(dstFolderPath, new[] { srcEntry0, srcEntry1 });
        
        Assert.True(Directory.Exists(Path.Combine(dstFolderPath, "x")));
        Assert.True(Directory.Exists(Path.Combine(dstFolderPath, "x/y")));
        Assert.True(Directory.Exists(Path.Combine(dstFolderPath, "x/y/z")));
        
        Assert.True(File.Exists(Path.Combine(dstFolderPath, "a")));
        Assert.True(File.Exists(Path.Combine(dstFolderPath, "x/b")));
        Assert.True(File.Exists(Path.Combine(dstFolderPath, "x/y/c")));
        Assert.True(File.Exists(Path.Combine(dstFolderPath, "x/y/z/d")));
    }
}

internal record TestEntry(string Path, bool IsFolder) : IEntry;