using Anna.Gui.Interactions.Hotkey;

namespace Anna.Gui.Interfaces;

public interface IViewerViewModel
{
    public HotkeyBase Hotkey { get; }

    public string TargetFilePath { get; }
}