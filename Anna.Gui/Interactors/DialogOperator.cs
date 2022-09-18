using Anna.Gui.UseCase;
using Anna.Gui.UseCase.Interfaces;
using System.Diagnostics;

namespace Anna.Interactors
{
    public class DialogOperator : IDialogOperator
    {
        public void ShowSortEntries(IShortcutKeyReceiver shortcutKeyReceiver)
        {
            Debug.WriteLine("ShowSortEntries");
        }
    }
}