using Anna.Service.Services;
using Avalonia.Input;

namespace Anna.Service.Linux;

public sealed class PlatformValueService : IPlatformValueService
{
    public KeyModifiers MetaKey => KeyModifiers.Meta;
    
    public string DefaultTerminalApp => "";
    public string DefaultTerminalAppOptions => "";
}