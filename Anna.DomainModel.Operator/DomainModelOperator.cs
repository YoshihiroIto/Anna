using Anna.DomainModel.FileSystem;
using Anna.DomainModel.Interface;

namespace Anna.DomainModel.Operator
{
    public class DomainModelOperator : IDomainModelOperator
    {
        public Directory CreateDirectory(string path)
        {
            return new FileSystemDirectory(path);
        }
    }
}