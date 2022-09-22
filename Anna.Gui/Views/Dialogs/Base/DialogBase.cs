using Anna.DomainModel.Interfaces;
using Avalonia.Controls;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Anna.ServiceProvider")]

namespace Anna.Gui.Views.Dialogs.Base
{
    public class DialogBase : Window
    {
        protected internal ILogger Logger { get; set; } = null!;

        public DialogBase Setup(object viewModel)
        {
            Logger.Information($"Start {GetType().Name}");
            
            Closed += (_, _) => Logger.Information($"End {GetType().Name}");

            DataContext = viewModel;

            return this;
        }
    }
}