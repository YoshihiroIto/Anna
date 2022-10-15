namespace Anna.Service;

public interface IFileSystemIsAccessibleService
{
    bool IsAccessible(string path);
}