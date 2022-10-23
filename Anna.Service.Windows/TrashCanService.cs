using Anna.Foundation;
using Anna.Service.Interfaces;
using Anna.Service.Services;
using Jewelry.Text;

namespace Anna.Service.Windows;

public sealed class TrashCanService : ITrashCanService
{
    private bool IsInitialized => _sid != "";
    
    private string _sid = "";
    
    public void SendToTrashCan(IEnumerable<IEntry> sourceEntries)
    {
        if (IsInitialized == false)
            Initialize();
        
        throw new NotImplementedException();
    }
    
    private void Initialize()
    {
        var userInfo = ProcessHelper.ExecuteAndGetStdoutAsync("whoami", "/user").Result;

        using var ss = new StringSplitter(stackalloc StringSplitter.StringSpan[32]);

        var parts = ss.Split(userInfo, ' ', StringSplitOptions.RemoveEmptyEntries);
        var last = parts[^1];

        _sid = last.ToString(userInfo).TrimEnd();
    }
}