using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Anna.Views
{
    public partial class DirectoryView : UserControl
    {
        public DirectoryView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}