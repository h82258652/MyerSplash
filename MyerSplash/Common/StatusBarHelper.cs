using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;

namespace MyerSplash.Common
{
    public class StatusBarHelper
    {
        private static StatusBar sb = StatusBar.GetForCurrentView();

        public static void SetUpStatusBar()
        {
            sb.BackgroundOpacity = 0;
            sb.ForegroundColor = Colors.Black;
        }
    }
}
