using Anna.DomainModel.Constants;

namespace Anna.UseCase.Interfaces;

public interface IObjectWriter
{
    ValueTask<ResultCode> WriteAsync<T>(T obj, string path);
}