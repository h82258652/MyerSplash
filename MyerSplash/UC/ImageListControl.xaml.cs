using JP.Utils.UI;
using MyerSplash.Model;
using MyerSplash.ViewModel;
using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace MyerSplash.UC
{
    public sealed partial class ImageListControl : UserControl
    {
        public event Action<UnsplashImage, FrameworkElement> OnClickItemStarted;
        public event Action<ScrollViewer> OnScrollViewerViewChanged;

        private MainViewModel MainVM
        {
            get
            {
                return this.DataContext as MainViewModel;
            }
        }

        private Visual _containerVisual;
        private Compositor _compositor;

        private int _zindex = 1;

        public int TargetOffsetX;
        public int TargetOffsetY;

        public ImageListControl()
        {
            this.InitializeComponent();
            this._compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        }

        private void ImageGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem;
            var container = ImageGridView.ContainerFromItem(item) as FrameworkElement;
            Canvas.SetZIndex(container, ++_zindex);

            _containerVisual = ElementCompositionPreview.GetElementVisual(container);

            var img = item as UnsplashImage;
            OnClickItemStarted?.Invoke(img, container);
        }

        public void MoveItemAnimation(Vector3 targetOffset, float widthRatio)
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
        }

        public void HideItemDetailAnimation()
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

        public void ScrollToTop()
        {
            ImageGridView.GetScrollViewer().ChangeView(null, 0, null);
        }

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
                var delayIndex = itemIndex - itemsPanel.FirstVisibleIndex;

                itemVisual.Opacity = 0f;
                itemVisual.Offset = new Vector3(50, 0, 0);

                // Create KeyFrameAnimations
                var offsetAnimation = _compositor.CreateScalarKeyFrameAnimation();
                offsetAnimation.InsertExpressionKeyFrame(1f, "0");
                offsetAnimation.Duration = TimeSpan.FromMilliseconds(700);
                offsetAnimation.DelayTime = TimeSpan.FromMilliseconds((delayIndex * 100));

                var fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
                fadeAnimation.InsertExpressionKeyFrame(1f, "1");
                fadeAnimation.Duration = TimeSpan.FromMilliseconds(700);
                fadeAnimation.DelayTime = TimeSpan.FromMilliseconds(delayIndex * 100);

                // Start animations
                itemVisual.StartAnimation("Offset.X", offsetAnimation);
                itemVisual.StartAnimation("Opacity", fadeAnimation);
            }
            itemContainer.Loaded -= ItemContainer_Loaded;
        }
        #endregion

        private void ImageGridView_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = ImageGridView.GetScrollViewer();
            scrollViewer.ViewChanging -= ScrollViewer_ViewChanging;
            scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            OnScrollViewerViewChanged?.Invoke(sender as ScrollViewer);
        }
    }
}
