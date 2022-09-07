using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Anna.Views
{
    public partial class DirectoryWindow : Window
    {
        public DirectoryWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}