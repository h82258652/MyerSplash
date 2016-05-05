using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyerSplash.Common
{
    public static class NavigationService
    {
        private static Frame RootFrame
        {
            get
            {
                return Window.Current.Content as Frame;
            }
        }

        public static void NaivgateToPage(Type pagetype,object param=null)
        {
            RootFrame.Navigate(pagetype, param);
        }
    }
}
