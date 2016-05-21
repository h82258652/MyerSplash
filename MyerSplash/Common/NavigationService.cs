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

        public static Stack<Func<bool>> HistoryOperationsBeyondFrame = new Stack<Func<bool>>();

        public static void NaivgateToPage(Type pagetype, object param = null)
        {
            RootFrame.Navigate(pagetype, param);
        }

        public static bool GoBack()
        {
            try
            {
                var op = HistoryOperationsBeyondFrame.Pop();
                if (!op.Invoke())
                {
                    throw new InvalidOperationException();
                }
                else return true;
            }
            catch (InvalidOperationException)
            {
                if (RootFrame.CanGoBack)
                {
                    RootFrame.GoBack();
                    return true;
                }
                else return false;
            }
        }
    }
}
