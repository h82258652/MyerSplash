using MyerSplash.Common;
using MyerSplash.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MyerSplash.View
{
    public sealed partial class SettingsPage : BindablePage
    {
        private SettingsViewModel SettingsVM { get; set; }

        public SettingsPage()
        {
            this.InitializeComponent();
            this.DataContext = SettingsVM = new SettingsViewModel();
        }
    }
}
