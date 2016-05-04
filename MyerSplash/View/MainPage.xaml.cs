using JP.Utils.UI;
using MyerSplash.Common;
using MyerSplash.Model;
using MyerSplash.ViewModel;
using MyerSplashCustomControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MyerSplash.View
{
    public sealed partial class MainPage : BindablePage
    {
        private MainViewModel MainVM { get; set; }

        private Compositor _compositor;
        private Visual _loadingVisual;
        private Visual _refreshVisual;
        private Visual _detailGridVisual;
        private Visual _borderGridVisual;
        private Visual _containerVisual;
        private Visual _downloadBtnVisual;
        private Visual _infoGridVisual;
        private Visual _downloadingBtnVisual;
        private Visual _okVisual;

        private int _zindex = 1;

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(MainPage), new PropertyMetadata(false, OnLoadingPropertyChanged));


        public static void OnLoadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var page = d as MainPage;
            if (!(bool)e.NewValue)
            {
                page.HideLoading();
            }
            else page.ShowLoading();
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = MainVM = new MainViewModel();

            InitComposition();
            InitBinding();
        }

        private void InitBinding()
        {
            var b = new Binding()
            {
                Source = MainVM,
                Path = new PropertyPath("IsLoading"),
                Mode = BindingMode.TwoWay,
            };
            this.SetBinding(IsLoadingProperty, b);
        }

        private void InitComposition()
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _loadingVisual = ElementCompositionPreview.GetElementVisual(LoadingGrid);
            _refreshVisual = ElementCompositionPreview.GetElementVisual(RefreshSymbol);
            _loadingVisual.Offset = new Vector3(0f, -50f, 0f);
            _detailGridVisual = ElementCompositionPreview.GetElementVisual(DetailGrid);
            _borderGridVisual = ElementCompositionPreview.GetElementVisual(MaskBorder);
            _downloadBtnVisual = ElementCompositionPreview.GetElementVisual(DownloadBtn);
            _infoGridVisual = ElementCompositionPreview.GetElementVisual(InfoGrid);
            _downloadingBtnVisual = ElementCompositionPreview.GetElementVisual(LoadingHintBtn);
            _okVisual = ElementCompositionPreview.GetElementVisual(OKBtn);

            _infoGridVisual.Offset = new Vector3(0f, 100f, 0);
            _downloadBtnVisual.Offset = new Vector3(100f, 0f, 0f);
            _detailGridVisual.Opacity = 0;
            _okVisual.Offset = new Vector3(100f, 0f, 0f);
            _downloadingBtnVisual.Offset = new Vector3(100f, 0f, 0f);

            DetailGrid.Visibility = Visibility.Collapsed;
        }

        #region Loading Animation
        public void ShowLoading()
        {
            var showAnimation = _compositor.CreateVector3KeyFrameAnimation();
            showAnimation.InsertKeyFrame(1, new Vector3(0f, 50f, 0f));
            showAnimation.Duration = TimeSpan.FromMilliseconds(500);

            var linearEasingFunc = _compositor.CreateLinearEasingFunction();

            var rotateAnimation = _compositor.CreateScalarKeyFrameAnimation();
            rotateAnimation.InsertKeyFrame(1, 3600f, linearEasingFunc);
            rotateAnimation.Duration = TimeSpan.FromMilliseconds(10000);
            rotateAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

            _refreshVisual.CenterPoint = new Vector3((float)RefreshSymbol.ActualWidth / 2, (float)RefreshSymbol.ActualHeight / 2, 0f);
            _refreshVisual.RotationAngleInDegrees = 0;

            _refreshVisual.StopAnimation("RotationAngleInDegrees");
            _refreshVisual.StartAnimation("RotationAngleInDegrees", rotateAnimation);
            _loadingVisual.StartAnimation("Offset", showAnimation);
        }

        public void HideLoading()
        {
            var showAnimation = _compositor.CreateScalarKeyFrameAnimation();
            showAnimation.InsertKeyFrame(1, -50f);
            showAnimation.Duration = TimeSpan.FromMilliseconds(500);

            _loadingVisual.StartAnimation("Offset.y", showAnimation);
        }
        #endregion

        #region List Animation
        private void AdaptiveGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            int index = args.ItemIndex;

            if (!args.InRecycleQueue)
            {
                args.ItemContainer.Loaded -= ItemContainer_Loaded;
                args.ItemContainer.Loaded += ItemContainer_Loaded;
            }
        }

        private void ItemContainer_Loaded(object sender, RoutedEventArgs e)
        {
            var itemsPanel = (ItemsWrapGrid)ImageGridView.ItemsPanelRoot;
            var itemContainer = (GridViewItem)sender;
            var itemIndex = ImageGridView.IndexFromContainer(itemContainer);

            // Don't animate if we're not in the visible viewport
            if (itemIndex >= itemsPanel.FirstVisibleIndex && itemIndex <= itemsPanel.LastVisibleIndex)
            {
                var itemVisual = ElementCompositionPreview.GetElementVisual(itemContainer);

                itemVisual.Opacity = 0f;
                itemVisual.Offset = new Vector3(50, 0, 0);

                // Create KeyFrameAnimations
                var offsetAnimation = _compositor.CreateScalarKeyFrameAnimation();
                offsetAnimation.InsertExpressionKeyFrame(1f, "0");
                offsetAnimation.Duration = TimeSpan.FromMilliseconds(700);
                offsetAnimation.DelayTime = TimeSpan.FromMilliseconds((itemIndex * 100));

                var fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
                fadeAnimation.InsertExpressionKeyFrame(1f, "1");
                fadeAnimation.Duration = TimeSpan.FromMilliseconds(700);
                fadeAnimation.DelayTime = TimeSpan.FromMilliseconds(itemIndex * 100);

                // Start animations
                itemVisual.StartAnimation("Offset.X", offsetAnimation);
                itemVisual.StartAnimation("Opacity", fadeAnimation);
            }
            itemContainer.Loaded -= ItemContainer_Loaded;
        }
        #endregion

        private async void ImageGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            DetailGrid.Visibility = Visibility.Visible;

            var item = e.ClickedItem;
            var container = ImageGridView.ContainerFromItem(item) as FrameworkElement;
            Canvas.SetZIndex(container, ++_zindex);

            _containerVisual = ElementCompositionPreview.GetElementVisual(container);

            var img = item as UnSplashImage;
            await img.DownloadLargeImage();
            this.LargeImage.Source = img.LargeBitmap;
            this.InfoGrid.Background = img.MajorColor;
            this.NameTB.Text = img.Owner.Name;
            if (ColorConverter.IsLight(img.MajorColor.Color))
            {
                this.NameTB.Foreground = new SolidColorBrush(Colors.Black);
                this.ByTB.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                this.NameTB.Foreground = new SolidColorBrush(Colors.White);
                this.ByTB.Foreground = new SolidColorBrush(Colors.White);
            }

            var currentPos = container.TransformToVisual(DetailGrid).TransformPoint(new Point(0, 0));
            var targetPos = DetailContentGrid.TransformToVisual(DetailGrid).TransformPoint(new Point(0, 0));
            var targetRatio = DetailContentGrid.ActualWidth / container.ActualWidth;
            var targetOffsetX = targetPos.X - currentPos.X;
            var targetOffsetY = targetPos.Y - currentPos.Y;

            var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            MoveItemAnimation(new Vector3((float)targetOffsetX, (float)targetOffsetY, 0f), (float)targetRatio);
            ToggleDetailGridAnimation(true);
            batch.Completed += (senderx, ex) =>
            {

            };
            batch.End();
        }

        private void MaskBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            HideItemDetailAnimation();
            ToggleDetailGridAnimation(false);
        }

        private void ToggleDetailGridAnimation(bool show)
        {
            DetailGrid.Visibility = Visibility.Visible;

            var fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(1f, show ? 1f : 0f);
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(500);
            fadeAnimation.DelayTime = TimeSpan.FromMilliseconds(show ? 300 : 0);

            var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _detailGridVisual.StartAnimation("Opacity", fadeAnimation);
            ToggleDownloadBtnAnimation(true);
            StartInfoGridAnimation();
            batch.Completed += (sender, e) =>
            {
                if (!show)
                {
                    _downloadingBtnVisual.Offset = new Vector3(100f, 0f, 0f);
                    _okVisual.Offset = new Vector3(100f, 0f, 0f);
                    _downloadBtnVisual.Offset = new Vector3(100f, 0f, 0f);
                    _infoGridVisual.Offset = new Vector3(0f, 100f, 0f);
                    DetailGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _containerVisual.Opacity = 1;
                }
            };
            batch.End();
        }

        private void MoveItemAnimation(Vector3 targetOffset, float widthRatio)
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, targetOffset);
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);

            var scaleAnimation = _compositor.CreateScalarKeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(1f, widthRatio);
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(500);

            var fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(1f, 0.5f);
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(500);

            _containerVisual.StartAnimation("Offset", offsetAnimation);
            _containerVisual.StartAnimation("Scale.x", scaleAnimation);
            _containerVisual.StartAnimation("Scale.y", scaleAnimation);
            _containerVisual.StartAnimation("Opacity", fadeAnimation);
        }

        private void HideItemDetailAnimation()
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, new Vector3(0, 0, 0));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);

            var scaleAnimation = _compositor.CreateScalarKeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(1f, 1f);
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(500);

            _containerVisual.StartAnimation("Offset", offsetAnimation);
            _containerVisual.StartAnimation("Scale.x", scaleAnimation);
            _containerVisual.StartAnimation("Scale.y", scaleAnimation);
        }

        private void ToggleDownloadBtnAnimation(bool show)
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, new Vector3(show ? 0f : 100f, 0f, 0f));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);
            offsetAnimation.DelayTime = TimeSpan.FromMilliseconds(show?300:0);

            _downloadBtnVisual.StartAnimation("Offset", offsetAnimation);
        }

        private void ToggleDownloadingBtnAnimation(bool show)
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, new Vector3(show ? 0f : 100f, 0f, 0f));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);
            offsetAnimation.DelayTime = TimeSpan.FromMilliseconds(200);

            _downloadingBtnVisual.StartAnimation("Offset", offsetAnimation);
        }

        private void ToggleOkBtnAnimation(bool show)
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, new Vector3(show ? 0f : 100f, 0f, 0f));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);
            offsetAnimation.DelayTime = TimeSpan.FromMilliseconds(200);

            _okVisual.StartAnimation("Offset", offsetAnimation);
        }

        private void StartInfoGridAnimation()
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, 0f));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);
            offsetAnimation.DelayTime = TimeSpan.FromMilliseconds(500);

            _infoGridVisual.StartAnimation("Offset", offsetAnimation);
        }

        private void DetailGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.DetailContentGrid.Height = e.NewSize.Height * 0.7;
            this.DetailContentGrid.Clip = new RectangleGeometry()
            { Rect = new Rect(0, 0, DetailContentGrid.ActualWidth, DetailContentGrid.Height) };
        }

        private async void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleDownloadBtnAnimation(false);
            await Task.Delay(100);
            ToggleDownloadingBtnAnimation(true);
            await Task.Delay(1000);
            ToggleDownloadingBtnAnimation(false);
            await Task.Delay(300);
            ToggleOkBtnAnimation(true);
            await Task.Delay(200);
            ToastService.SendToast("Saved :D", TimeSpan.FromMilliseconds(1000));
        }
    }
}
