using Anna.DomainModel.FileSystem;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel;

public sealed class DomainModelOperator
{
    private readonly IServiceProvider _dic;

    public DomainModelOperator(IServiceProvider dic)
    {
        _dic = dic;
    }

    public Folder CreateFolder(int id, string path)
    {
        return new FileSystemFolder(_dic, id, path);
    }
}