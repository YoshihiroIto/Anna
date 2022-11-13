using Anna.Service.Services;
using Avalonia.Input;

namespace Anna.Service.MacOS;

public sealed class DefaultValueService : IDefaultValueService
{
    public KeyModifiers MetaKey => KeyModifiers.Control;
    
    public string TerminalApp => "";
    public string TerminalAppOptions => "";
}