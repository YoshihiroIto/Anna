using Anna.Service.Services;
using Avalonia.Input;

namespace Anna.Service.Windows;

public sealed class DefaultValueService : IDefaultValueService
{
    public KeyModifiers MetaKey => KeyModifiers.Control;

    public string TerminalApp => "cmd";
    public string TerminalAppOptions => "/K \"cd /d %CurrentFolder%\"";
}