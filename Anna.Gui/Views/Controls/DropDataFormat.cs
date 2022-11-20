using Anna.Gui.Views.Panels;
namespace Anna.Gui.Views.Controls;

public class DropDataFormat
{
    public const string FolderPanel = nameof(FolderPanel);
    
    public sealed record FolderPanelData(FolderPanel FolderPanel);
}