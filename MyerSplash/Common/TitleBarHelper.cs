using JP.Utils.UI;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace MyerSplash.Common
{
    public static class TitleBarHelper
    {
        public static void SetUpThemeTitleBar()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = ColorConverter.HexToColor("#00bebe");
            titleBar.ForegroundColor = Colors.Black;
            titleBar.InactiveBackgroundColor = ColorConverter.HexToColor("#00bebe");
            titleBar.InactiveForegroundColor = Colors.Black;
            titleBar.ButtonBackgroundColor = ColorConverter.HexToColor("#00bebe");
            titleBar.ButtonForegroundColor = Colors.Black;
            titleBar.ButtonInactiveBackgroundColor = ColorConverter.HexToColor("#00bebe");
            titleBar.ButtonInactiveForegroundColor = Colors.Black;
            titleBar.ButtonHoverBackgroundColor = ColorConverter.HexToColor("#FF10D6D6");
            titleBar.ButtonHoverForegroundColor = Colors.Black;
            titleBar.ButtonPressedBackgroundColor = ColorConverter.HexToColor("#FF06AEAE");
        }
    }
}
