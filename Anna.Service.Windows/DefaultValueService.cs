using Anna.Service.Services;

namespace Anna.Service.Windows;

public sealed class DefaultValueService : IDefaultValueService
{
    public string TerminalApp => "cmd";
    public string TerminalAppOptions => "/K \"cd /d %CurrentFolder%\"";
}