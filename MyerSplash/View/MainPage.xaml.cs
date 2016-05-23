using MyerSplash.Common;
using MyerSplash.Model;
using MyerSplash.ViewModel;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

namespace MyerSplash.View
{
    public sealed partial class MainPage : BindablePage
    {
        private MainViewModel MainVM { get; set; }

        private Compositor _compositor;
        private Visual _loadingVisual;
        private Visual _refreshVisual;
        private Visual _drawerVisual;
        private Visual _drawerMaskVisual;
        private Visual _titleGridVisual;
        private Visual _refreshBtnVisual;

        private double _lastVerticalOffset = 0;
        private bool _isHideTitleGrid = false;

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(MainViewModel), new PropertyMetadata(false, OnLoadingPropertyChanged));

        public static void OnLoadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var page = d as MainPage;
            if (!(bool)e.NewValue)
            {
                page.HideLoading();
            }
            else page.ShowLoading();
        }

        public bool DrawerOpended
        {
            get { return (bool)GetValue(DrawerOpendedProperty); }
            set { SetValue(DrawerOpendedProperty, value); }
        }

        public static readonly DependencyProperty DrawerOpendedProperty =
            DependencyProperty.Register("DrawerOpended", typeof(bool), typeof(MainPage), new PropertyMetadata(false, OnDrawerOpenedPropertyChanged));

        public static void OnDrawerOpenedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var page = d as MainPage;
            page.ToggleDrawerAnimation((bool)e.NewValue);
            page.ToggleDrawerMaskAnimation((bool)e.NewValue);
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = MainVM = new MainViewModel();
            this.SizeChanged += MainPage_SizeChanged;
            this.Loaded += MainPage_Loaded;

            InitComposition();
            InitBinding();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _loadingVisual.Offset = new Vector3(0f, -60f, 0f);
            _drawerMaskVisual.Opacity = 0;
            _drawerVisual.Offset = new Vector3(-300f, 0f, 0f);

            DrawerMaskBorder.Visibility = Visibility.Collapsed;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!DrawerOpended)
            {
                _drawerVisual.Offset = new Vector3(-(float)Window.Current.Bounds.Width, 0f, 0f);
            }
        }

        private void InitBinding()
        {
            var b = new Binding()
            {
                Source = MainVM,
                Path = new PropertyPath("IsRefreshing"),
                Mode = BindingMode.TwoWay,
            };
            this.SetBinding(IsLoadingProperty, b);

            var b2 = new Binding()
            {
                Source = MainVM,
                Path = new PropertyPath("DrawerOpened"),
                Mode = BindingMode.TwoWay,
            };
            this.SetBinding(DrawerOpendedProperty, b2);
        }

        private void InitComposition()
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _loadingVisual = ElementCompositionPreview.GetElementVisual(LoadingGrid);
            _refreshVisual = ElementCompositionPreview.GetElementVisual(LoadingSymbol);
            _drawerVisual = ElementCompositionPreview.GetElementVisual(DrawerControl);
            _drawerMaskVisual = ElementCompositionPreview.GetElementVisual(DrawerMaskBorder);
            _titleGridVisual = ElementCompositionPreview.GetElementVisual(TitleGrid);
            _refreshBtnVisual = ElementCompositionPreview.GetElementVisual(RefreshBtn);
        }

        #region Loading animation
        private void ShowLoading()
        {
            var showAnimation = _compositor.CreateVector3KeyFrameAnimation();
            showAnimation.InsertKeyFrame(1, new Vector3(0f, 50f, 0f));
            showAnimation.Duration = TimeSpan.FromMilliseconds(500);

            var linearEasingFunc = _compositor.CreateLinearEasingFunction();

            var rotateAnimation = _compositor.CreateScalarKeyFrameAnimation();
            rotateAnimation.InsertKeyFrame(1, 3600f, linearEasingFunc);
            rotateAnimation.Duration = TimeSpan.FromMilliseconds(10000);
            rotateAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

            _loadingVisual.IsVisible = true;
            _refreshVisual.CenterPoint = new Vector3((float)LoadingSymbol.ActualWidth / 2, (float)LoadingSymbol.ActualHeight / 2, 0f);
            _refreshVisual.RotationAngleInDegrees = 0;

            _refreshVisual.StopAnimation("RotationAngleInDegrees");
            _refreshVisual.StartAnimation("RotationAngleInDegrees", rotateAnimation);
            _loadingVisual.StartAnimation("Offset", showAnimation);
        }

        private void HideLoading()
        {
            var showAnimation = _compositor.CreateScalarKeyFrameAnimation();
            showAnimation.InsertKeyFrame(1, -60f);
            showAnimation.Duration = TimeSpan.FromMilliseconds(500);

            var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _loadingVisual.StartAnimation("Offset.y", showAnimation);
            batch.Completed += (sender, e) =>
              {
                  _loadingVisual.IsVisible = false;
              };
            batch.End();
        }
        #endregion

        #region Drawer animation
        private void ToggleDrawerAnimation(bool show)
        {
            var offsetAnim = _compositor.CreateScalarKeyFrameAnimation();
            offsetAnim.InsertKeyFrame(1f, show ? 0f : -300);
            offsetAnim.Duration = TimeSpan.FromMilliseconds(300);

            _drawerVisual.StartAnimation("Offset.X", offsetAnim);
        }

        private void ToggleDrawerMaskAnimation(bool show)
        {
            if (show) DrawerMaskBorder.Visibility = Visibility.Visible;

            var fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(1f, show ? 1f : 0f, _compositor.CreateLinearEasingFunction());
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(300);

            var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _drawerMaskVisual.StartAnimation("Opacity", fadeAnimation);
            batch.Completed += (sender, e) =>
              {
                  if (!show) DrawerMaskBorder.Visibility = Visibility.Collapsed;
              };
            batch.End();
        }
        #endregion

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ListControl.ScrollToTop();
        }

        private void DetailControl_OnHideControl()
        {
            ListControl.HideItemDetailAnimation();
        }

        private void ListControl_OnClickItemStarted(UnsplashImage img, FrameworkElement container)
        {
            DetailControl.Visibility = Visibility.Visible;

            DetailControl.UnsplashImage = img;

            var currentPos = container.TransformToVisual(ListControl).TransformPoint(new Point(0, 0));
            var targetPos = DetailControl.GetContentGridPosition();
            var targetRatio = DetailControl.GetContentGridSize().Width / container.ActualWidth;
            var targetOffsetX = targetPos.X - currentPos.X;
            var targetOffsetY = targetPos.Y - currentPos.Y;

            ListControl.MoveItemAnimation(new Vector3((float)targetOffsetX, (float)targetOffsetY, 0f), (float)targetRatio);
            DetailControl.ToggleDetailGridAnimation(true);

            NavigationService.HistoryOperationsBeyondFrame.Push(() =>
            {
                DetailControl.HideDetailControl();
                return true;
            });
        }

        private void DetailControl_Loaded(object sender, RoutedEventArgs e)
        {
            DetailControl.Visibility = Visibility.Collapsed;
        }

        #region Scrolling
        private void ToggleTitleBarAnimation(bool show)
        {
            var offsetAnimation = _compositor.CreateScalarKeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, show ? 0f : -100f);
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);

            _titleGridVisual.StartAnimation("Offset.Y", offsetAnimation);
        }

        private void ToggleRefreshBtnAnimation(bool show)
        {
            var offsetAnimation = _compositor.CreateScalarKeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, show ? 0f : 100f);
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);

            _refreshBtnVisual.StartAnimation("Offset.Y", offsetAnimation);
        }

        private void ListControl_OnScrollViewerViewChanged(ScrollViewer scrollViewer)
        {
            if ((scrollViewer.VerticalOffset - _lastVerticalOffset) > 30 && !_isHideTitleGrid)
            {
                _isHideTitleGrid = true;
                ToggleRefreshBtnAnimation(false);
                ToggleTitleBarAnimation(false);
            }
            else if (scrollViewer.VerticalOffset < _lastVerticalOffset && _isHideTitleGrid)
            {
                _isHideTitleGrid = false;
                ToggleRefreshBtnAnimation(true);
                ToggleTitleBarAnimation(true);
            }
            _lastVerticalOffset = scrollViewer.VerticalOffset;
        }
        #endregion

        #region Drawer manipulation
        private void TouchGrid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X >= 70)
            {
                if(!DrawerOpended)
                {
                    DrawerOpended = true;
                }
                else
                {
                    ToggleDrawerAnimation(true);
                    ToggleDrawerMaskAnimation(true);
                }
            }
            else
            {
                if (DrawerOpended)
                {
                    DrawerOpended = false;
                }
                else
                {
                    ToggleDrawerAnimation(false);
                    ToggleDrawerMaskAnimation(false);
                }
            }
        }

        private void TouchGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (_drawerMaskVisual.Opacity < 1)
            {
                DrawerMaskBorder.Visibility = Visibility.Visible;
                _drawerMaskVisual.Opacity += 0.02f;
            }
            var targetOffsetX = _drawerVisual.Offset.X + e.Delta.Translation.X;
            _drawerVisual.Offset = new Vector3((float)(targetOffsetX > 1 ? 1 : targetOffsetX), 0f, 0f);
        }

        private void DrawerControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (_drawerMaskVisual.Opacity > 0)
            {
                _drawerMaskVisual.Opacity -= 0.01f;
            }
            var targetOffsetX = _drawerVisual.Offset.X - Math.Abs(e.Delta.Translation.X);
            _drawerVisual.Offset = new Vector3((float)(targetOffsetX <= -300f ? -300f : targetOffsetX), 0f, 0f);
        }

        private void DrawerControl_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            DrawerMaskBorder.Visibility = Visibility.Collapsed;

            if (e.Cumulative.Translation.X <= -30)
            {
                DrawerOpended = false;
            }
            else
            {

            }
        }
        #endregion
    }
}
