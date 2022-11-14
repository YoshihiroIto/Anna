using Anna.Service.Services;
using Avalonia.Input;

namespace Anna.Service.MacOS;

public sealed class PlatformValueService : IPlatformValueService
{
    public KeyModifiers MetaKey => KeyModifiers.Control;
    
    public string DefaultTerminalApp => "";
    public string DefaultTerminalAppOptions => "";
}