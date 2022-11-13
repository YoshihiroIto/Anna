using Anna.Service.Services;
using Avalonia.Input;

namespace Anna.Service.Linux;

public sealed class DefaultValueService : IDefaultValueService
{
    public KeyModifiers MetaKey => KeyModifiers.Meta;
    
    public string TerminalApp => "";
    public string TerminalAppOptions => "";
}