using Avalonia.Input;

namespace Anna.Service.Services;

public interface IPlatformValueService
{
    KeyModifiers MetaKey {get;}
    
    string DefaultTerminalApp { get; }
    string DefaultTerminalAppOptions { get; }
}