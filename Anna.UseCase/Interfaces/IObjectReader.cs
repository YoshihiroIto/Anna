using Anna.DomainModel.Constants;

namespace Anna.UseCase.Interfaces;

public interface IObjectReader
{
    ValueTask<(T obj, ResultCode code)> ReadAsync<T>(string path, Func<T> defaultGenerator);
}