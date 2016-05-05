using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyerSplash.UC
{
    public sealed partial class HamburgerButton : UserControl
    {
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(HamburgerButton), new PropertyMetadata(null));

        public HamburgerButton()
        {
            this.InitializeComponent();
            this.HamburgerBtn.Click += HamClick;
        }

        private void HamClick(object sender, RoutedEventArgs e)
        {
            Command?.Execute(null);
        }
    }
}
