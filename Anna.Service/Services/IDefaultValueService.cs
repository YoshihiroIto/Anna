using Avalonia.Input;

namespace Anna.Service.Services;

public interface IDefaultValueService
{
    KeyModifiers MetaKey {get;}
    
    string TerminalApp { get; }
    string TerminalAppOptions { get; }
}