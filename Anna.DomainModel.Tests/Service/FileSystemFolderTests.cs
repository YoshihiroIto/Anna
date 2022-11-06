using Anna.Constants;
using Anna.TestFoundation;
using Xunit;

namespace Anna.DomainModel.Tests.Service;

public sealed class FileSystemFolderTests : IDisposable
{
    private readonly TestServiceProvider _dic = new();
    private readonly TempFolder _tempFolder = new();

    public void Dispose()
    {
        _tempFolder.Dispose();
        _dic.Dispose();
    }

    [Fact]
    public void Start_and_finish_successfully()
    {
        using var folder = _dic.GetInstance<DomainModelOperator>().CreateFolder(0, ".");
    }

    [Fact]
    public void Initial()
    {
        using var folder = _dic.GetInstance<DomainModelOperator>().CreateFolder(0, _tempFolder.RootPath);

        Assert.Equal(_tempFolder.RootPath, folder.Path);
        Assert.Equal(SortModes.Name, folder.SortMode);
        Assert.Equal(SortOrders.Ascending, folder.SortOrder);
    }

    [Fact]
    public async Task Root_folder_does_not_contain_parent_folder_in_entry()
    {
        using var folder = _dic.GetInstance<DomainModelOperator>()
            .CreateFolder(0, Path.GetPathRoot(_tempFolder.RootPath) ?? throw new NullReferenceException());

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        lock (folder.EntriesUpdatingLockObj)
        {
            Assert.DoesNotContain("..", folder.Entries.Select(x => x.NameWithExtension));
        }
    }

    [Fact]
    public async Task Non_root_folder_contain_parent_folder_in_entries()
    {
        var files0 = new[] { "fileA.dat", "fileB.dat", "fileC.dat" };

        foreach (var file in files0)
            _tempFolder.CreateFile(file);

        using var folder = _dic.GetInstance<DomainModelOperator>().CreateFolder(0, _tempFolder.RootPath);

        var filesWithParentDir = new List<string> { ".." };
        filesWithParentDir.AddRange(files0);

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        lock (folder.EntriesUpdatingLockObj)
        {
            Assert.Equal(filesWithParentDir, folder.Entries.Select(x => x.NameWithExtension));
        }
    }

    [Fact]
    public async Task Follow_file_addition()
    {
        var files0 = new[] { "fileA.dat", "fileB.dat", "fileC.dat" };

        foreach (var file in files0)
            _tempFolder.CreateFile(file);

        using var folder = _dic.GetInstance<DomainModelOperator>().CreateFolder(0, _tempFolder.RootPath);

        var files1 = new[] { "fileD.dat", "fileE.dat", "fileF.dat" };

        foreach (var file in files1)
            _tempFolder.CreateFile(file);

        var filesWithParentDir = new List<string> { ".." };
        filesWithParentDir.AddRange(files0);
        filesWithParentDir.AddRange(files1);

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        lock (folder.EntriesUpdatingLockObj)
        {
            Assert.Equal(filesWithParentDir, folder.Entries.Select(x => x.NameWithExtension));
        }
    }

    [Fact]
    public async Task Follow_file_deletion()
    {
        var files0 = new[] { "fileA.dat", "fileB.dat", "fileC.dat" };

        foreach (var file in files0)
            _tempFolder.CreateFile(file);

        using var folder = _dic.GetInstance<DomainModelOperator>().CreateFolder(0, _tempFolder.RootPath);

        File.Delete(Path.Combine(_tempFolder.RootPath, "fileB.dat"));

        var filesWithParentDir = new List<string> { ".." };
        filesWithParentDir.AddRange(files0);
        filesWithParentDir.Remove("fileB.dat");

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        lock (folder.EntriesUpdatingLockObj)
        {
            Assert.Equal(filesWithParentDir, folder.Entries.Select(x => x.NameWithExtension));
        }
    }

    [Fact]
    public async Task Follow_file_renaming()
    {
        var files0 = new[] { "fileA.dat", "fileB.dat", "fileC.dat" };

        foreach (var file in files0)
            _tempFolder.CreateFile(file);

        using var folder = _dic.GetInstance<DomainModelOperator>().CreateFolder(0, _tempFolder.RootPath);

        File.Move(Path.Combine(_tempFolder.RootPath, "fileC.dat"), Path.Combine(_tempFolder.RootPath, "fileZ.dat"));

        var filesWithParentDir = new List<string> { ".." };
        filesWithParentDir.AddRange(files0);
        filesWithParentDir.Remove("fileC.dat");
        filesWithParentDir.Add("fileZ.dat");

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        lock (folder.EntriesUpdatingLockObj)
        {
            Assert.Equal(filesWithParentDir, folder.Entries.Select(x => x.NameWithExtension));
        }
    }

    [Fact]
    public async Task IsSelected_is_maintained_even_after_file_renaming()
    {
        var files0 = new[] { "fileA.dat", "fileB.dat", "fileC.dat" };

        foreach (var file in files0)
            _tempFolder.CreateFile(file);

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        using var folder = _dic.GetInstance<DomainModelOperator>().CreateFolder(0, _tempFolder.RootPath);

        folder.Entries[3].IsSelected = true;

        File.Move(Path.Combine(_tempFolder.RootPath, "fileC.dat"), Path.Combine(_tempFolder.RootPath, "fileZ.dat"));

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        Assert.True(folder.Entries[3].IsSelected);
    }

    [Fact]
    public async Task IsSelected_is_maintained_even_after_file_overwrite()
    {
        var files0 = new[] { "fileA.dat", "fileB.dat", "fileC.dat" };

        foreach (var file in files0)
            _tempFolder.CreateFile(file);

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        using var folder = _dic.GetInstance<DomainModelOperator>().CreateFolder(0, _tempFolder.RootPath);

        folder.Entries[3].IsSelected = true;

        await File.WriteAllTextAsync(Path.Combine(_tempFolder.RootPath, "fileC.dat"), "overwrite");

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        Assert.True(folder.Entries[3].IsSelected);
    }

    [Fact]
    public async Task Path_is_maintained_even_after_file_renaming()
    {
        var files0 = new[] { "fileA.dat", "fileB.dat", "fileC.dat" };

        foreach (var file in files0)
            _tempFolder.CreateFile(file);

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        using var folder = _dic.GetInstance<DomainModelOperator>().CreateFolder(0, _tempFolder.RootPath);

        File.Move(Path.Combine(_tempFolder.RootPath, "fileC.dat"), Path.Combine(_tempFolder.RootPath, "fileZ.dat"));

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        Assert.Equal(Path.Combine(_tempFolder.RootPath, "fileZ.dat"), folder.Entries[3].Path);
    }
    
    [Fact]
    public void ParentPath()
    {
        using var folder = _dic.GetInstance<DomainModelOperator>().CreateFolder(0, _tempFolder.RootPath);
        
        Assert.Equal("..", folder.Entries[0].Name);
        Assert.Equal("..", folder.Entries[0].NameWithExtension);
        Assert.Equal("", folder.Entries[0].Extension);
        Assert.Equal(new DirectoryInfo(_tempFolder.RootPath).Parent!.FullName, folder.Entries[0].Path);
    }
}