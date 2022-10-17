namespace Anna.Service.Services;

public interface IFileSystemIsAccessibleService
{
    bool IsAccessible(string path);
}