namespace Anna.Service;

public interface IFileSystemService
{
    bool IsAccessible(string path);
}