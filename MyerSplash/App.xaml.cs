﻿using JP.Utils.Data;
using JP.Utils.Debug;
using JP.Utils.Helper;
using MyerSplash.Common;
using MyerSplash.View;
using MyerSplashShared.API;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MyerSplash
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static CacheUtil CacheUtilInstance { get; set; }

        public static ViewModel.MainViewModel MainVM { get; set; }

        public static AppSettings AppSettings
        {
            get
            {
                return App.Current.Resources["AppSettings"] as AppSettings;
            }
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            CacheUtilInstance = new CacheUtil();
            await CacheUtilInstance.LoadAsync();

            GlobalLocaleHelper.SetupLang(null);

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.Background = App.Current.Resources["MyerSplashDarkColor"] as SolidColorBrush;

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter

                if(!LocalSettingHelper.HasValue("UPDATED_TO_1.1"))
                {
                    var files1 = await CacheUtil.GetCachedFileFolder().GetItemsAsync();
                    foreach (var file in files1)
                    {
                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                    var files2 = await CacheUtil.GetTempFolder().GetItemsAsync();
                    foreach (var file in files2)
                    {
                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                    LocalSettingHelper.AddValue("UPDATED_TO_1.1", true);
                }

                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();

            TitleBarHelper.SetUpThemeTitleBar();
            if(DeviceHelper.IsMobile)
            {
                StatusBarHelper.SetUpStatusBar();
            }

            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
            if (APIInfoHelper.HasHardwareButton)
            {
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            }
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = NavigationService.GoBack();
        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = NavigationService.GoBack();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
