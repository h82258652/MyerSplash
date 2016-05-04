using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyerSplash.UC
{
    public sealed partial class HamburgerButton : UserControl
    {
        public event RoutedEventHandler ButtonClick;

        public HamburgerButton()
        {
            this.InitializeComponent();
            this.HamburgerBtn.Click += ButtonClick;
        }

        public void PlayHamInStory()
        {
            //HamInStory.Begin();
        }

        public void PlayHamOutStory()
        {
            //HamOutStory.Begin();
        }

        private void HamClick(object sender,RoutedEventArgs e)
        {
            //PlayHamInStory();
            ButtonClick(sender, e);
        }
    }
}
