using Anna.Constants;
using Anna.TestFoundation;
using Xunit;

namespace Anna.DomainModel.Tests;

public class FileSystemDirectoryTests : IDisposable
{
    private readonly TestServiceProviderContainer _dic = new();
    private readonly TempDir _tempDir = new();

    public void Dispose()
    {
        _dic.Dispose();
    }

    [Fact]
    public void Start_and_finish_successfully()
    {
        using var directory = _dic.GetInstance<DomainModelOperator>().CreateDirectory(".");
    }

    [Fact]
    public void Initial()
    {
        using var directory = _dic.GetInstance<DomainModelOperator>().CreateDirectory(_tempDir.RootPath);

        Assert.Equal(_tempDir.RootPath, directory.Path);
        Assert.Equal(SortModes.Name, directory.SortMode);
        Assert.Equal(SortOrders.Ascending, directory.SortOrder);
    }

    [Fact]
    public async Task Root_directory_does_not_contain_parent_directory_in_entry()
    {
        using var directory = _dic.GetInstance<DomainModelOperator>()
            .CreateDirectory(Path.GetPathRoot(_tempDir.RootPath) ?? throw new NullReferenceException());

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        lock (directory.EntitiesUpdatingLockObj)
        {
            Assert.DoesNotContain("..", directory.Entries.Select(x => x.NameWithExtension));
        }
    }

    [Fact]
    public async Task Non_root_directory_contain_parent_directory_in_entries()
    {
        var files0 = new[] { "fileA.dat", "fileB.dat", "fileC.dat" };

        foreach (var file in files0)
            _tempDir.CreateFile(file);

        using var directory = _dic.GetInstance<DomainModelOperator>().CreateDirectory(_tempDir.RootPath);

        var filesWithParentDir = new List<string> { ".." };
        filesWithParentDir.AddRange(files0);

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        lock (directory.EntitiesUpdatingLockObj)
        {
            Assert.Equal(filesWithParentDir, directory.Entries.Select(x => x.NameWithExtension));
        }
    }

    [Fact]
    public async Task Follow_file_addition()
    {
        var files0 = new[] { "fileA.dat", "fileB.dat", "fileC.dat" };

        foreach (var file in files0)
            _tempDir.CreateFile(file);

        using var directory = _dic.GetInstance<DomainModelOperator>().CreateDirectory(_tempDir.RootPath);

        var files1 = new[] { "fileD.dat", "fileE.dat", "fileF.dat" };

        foreach (var file in files1)
            _tempDir.CreateFile(file);

        var filesWithParentDir = new List<string> { ".." };
        filesWithParentDir.AddRange(files0);
        filesWithParentDir.AddRange(files1);

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        lock (directory.EntitiesUpdatingLockObj)
        {
            Assert.Equal(filesWithParentDir, directory.Entries.Select(x => x.NameWithExtension));
        }
    }

    [Fact]
    public async Task Follow_file_deletion()
    {
        var files0 = new[] { "fileA.dat", "fileB.dat", "fileC.dat" };

        foreach (var file in files0)
            _tempDir.CreateFile(file);

        using var directory = _dic.GetInstance<DomainModelOperator>().CreateDirectory(_tempDir.RootPath);

        File.Delete(Path.Combine(_tempDir.RootPath, "fileB.dat"));

        var filesWithParentDir = new List<string> { ".." };
        filesWithParentDir.AddRange(files0);
        filesWithParentDir.Remove("fileB.dat");

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        lock (directory.EntitiesUpdatingLockObj)
        {
            Assert.Equal(filesWithParentDir, directory.Entries.Select(x => x.NameWithExtension));
        }
    }

    [Fact]
    public async Task Follow_file_renaming()
    {
        var files0 = new[] { "fileA.dat", "fileB.dat", "fileC.dat" };

        foreach (var file in files0)
            _tempDir.CreateFile(file);

        using var directory = _dic.GetInstance<DomainModelOperator>().CreateDirectory(_tempDir.RootPath);

        File.Move(Path.Combine(_tempDir.RootPath, "fileC.dat"), Path.Combine(_tempDir.RootPath, "fileZ.dat"));

        var filesWithParentDir = new List<string> { ".." };
        filesWithParentDir.AddRange(files0);
        filesWithParentDir.Remove("fileC.dat");
        filesWithParentDir.Add("fileZ.dat");

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        lock (directory.EntitiesUpdatingLockObj)
        {
            Assert.Equal(filesWithParentDir, directory.Entries.Select(x => x.NameWithExtension));
        }
    }

    [Fact]
    public async Task IsSelected_is_maintained_even_after_file_renaming()
    {
        var files0 = new[] { "fileA.dat", "fileB.dat", "fileC.dat" };

        foreach (var file in files0)
            _tempDir.CreateFile(file);

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        using var directory = _dic.GetInstance<DomainModelOperator>().CreateDirectory(_tempDir.RootPath);

        directory.Entries[3].IsSelected = true;

        File.Move(Path.Combine(_tempDir.RootPath, "fileC.dat"), Path.Combine(_tempDir.RootPath, "fileZ.dat"));

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        Assert.True(directory.Entries[3].IsSelected);
    }

    [Fact]
    public async Task IsSelected_is_maintained_even_after_file_overwrite()
    {
        var files0 = new[] { "fileA.dat", "fileB.dat", "fileC.dat" };

        foreach (var file in files0)
            _tempDir.CreateFile(file);

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        using var directory = _dic.GetInstance<DomainModelOperator>().CreateDirectory(_tempDir.RootPath);

        directory.Entries[3].IsSelected = true;

        await File.WriteAllTextAsync(Path.Combine(_tempDir.RootPath, "fileC.dat"), "overwrite");

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        Assert.True(directory.Entries[3].IsSelected);
    }

    [Fact]
    public async Task Path_is_maintained_even_after_file_renaming()
    {
        var files0 = new[] { "fileA.dat", "fileB.dat", "fileC.dat" };

        foreach (var file in files0)
            _tempDir.CreateFile(file);

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        using var directory = _dic.GetInstance<DomainModelOperator>().CreateDirectory(_tempDir.RootPath);

        File.Move(Path.Combine(_tempDir.RootPath, "fileC.dat"), Path.Combine(_tempDir.RootPath, "fileZ.dat"));

        await Task.Delay(TimeSpan.FromMilliseconds(30));

        Assert.Equal(Path.Combine(_tempDir.RootPath, "fileZ.dat"), directory.Entries[3].Path);
    }
    
    [Fact]
    public void ParentPath()
    {
        using var directory = _dic.GetInstance<DomainModelOperator>().CreateDirectory(_tempDir.RootPath);
        
        Assert.Equal("..", directory.Entries[0].Name);
        Assert.Equal("..", directory.Entries[0].NameWithExtension);
        Assert.Equal("", directory.Entries[0].Extension);
        Assert.Equal(new DirectoryInfo(_tempDir.RootPath).Parent!.FullName, directory.Entries[0].Path);
    }
}