using Avalonia.Input;
using System.Diagnostics;

namespace Anna.Views.ShortcutKey;

public class ShortcutKeyManager
{
    public void OnKeyDown(DirectoryView sender, KeyEventArgs e)
    {
        Debug.WriteLine($"{sender}, {e.Key}, {e.KeyModifiers}");
    }
}