using Anna.Service.Services;
using Avalonia.Input;

namespace Anna.Service.Windows;

public sealed class PlatformValueService : IPlatformValueService
{
    public KeyModifiers MetaKey => KeyModifiers.Control;

    public string DefaultTerminalApp => "cmd";
    public string DefaultTerminalAppOptions => "/K \"cd /d %CurrentFolder%\"";
}