using Anna.DomainModel.Constants;
using Anna.DomainModel.Interfaces;
using Anna.UseCase.Interfaces;
using System.Text.Json;

namespace Anna.Repository;

public class FileSystemObjectReader : IObjectReader
{
    private readonly ILogger _logger;

    public FileSystemObjectReader(ILogger logger)
    {
        _logger = logger;
    }

    public async ValueTask<(T obj, ResultCode code)> ReadAsync<T>(string path, Func<T> defaultGenerator)
    {
        try
        {
            var json = await File.ReadAllTextAsync(path);

            var obj = JsonSerializer.Deserialize<T>(json);

            if (obj is null)
                throw new JsonException();

            return (obj, ResultCode.Ok);
        }
        catch (Exception e)
        {
            _logger.Error($"FileSystemObjectReader.ReadAsync: {e.Message}");
        }

        return (defaultGenerator(), ResultCode.Error);
    }
}