namespace Anna.Service.Services;

public interface IClipboardService
{
    ValueTask SetFilesAsync(IEnumerable<string> filePaths);
    ValueTask<IEnumerable<string>> GetFilesAsync();

    ValueTask SetTextAsync(string text);
}