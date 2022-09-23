using Anna.DomainModel.Constants;
using Anna.DomainModel.Interfaces;
using Anna.UseCase.Interfaces;
using System.Text.Json;

namespace Anna.Repository;

public class FileSystemObjectWriter : IObjectWriter
{
    private readonly ILogger _logger;

    public FileSystemObjectWriter(ILogger logger)
    {
        _logger = logger;
    }

    public async ValueTask<ResultCode> WriteAsync<T>(T obj, string path)
    {
        try
        {
            var json = JsonSerializer.Serialize(obj);

            await File.WriteAllTextAsync(path, json);

            return ResultCode.Ok;
        }
        catch (Exception e)
        {
            _logger.Error($"FileSystemObjectWriter.WriteAsync: {e.Message}");
        }

        return ResultCode.Error;
    }
}