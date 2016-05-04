using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;

namespace MyerSplash.Common
{
    public class StatusBarHelper
    {
        public static StatusBar sb = StatusBar.GetForCurrentView();

        public static void SetUpStatusBar()
        {
            sb.BackgroundOpacity = 0;
            sb.ForegroundColor = Colors.White;
        }

        public static void SetUpBlueStatusBar()
        {
            sb.BackgroundOpacity = 0;
            sb.ForegroundColor = (App.Current.Resources["DefaultColor"] as SolidColorBrush).Color;
        }

        public static void SetUpBlackStatusBar()
        {
            sb.BackgroundOpacity = 0;
            sb.ForegroundColor = (App.Current.Resources["MyerListDark"] as SolidColorBrush).Color;
        }
    }
}
