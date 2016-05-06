using JP.Utils.UI;
using MyerSplash.Model;
using MyerSplashCustomControl;
using System;
using System.Numerics;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace MyerSplash.UC
{
    public sealed partial class PhotoDetailControl : UserControl
    {
        public event Action OnHideControl;

        private Compositor _compositor;
        private Visual _detailGridVisual;
        private Visual _borderGridVisual;
        private Visual _downloadBtnVisual;
        private Visual _likeBtnVisual;
        private Visual _infoGridVisual;
        private Visual _downloadingBtnVisual;
        private Visual _okVisual;

        public UnsplashImage UnsplashImage
        {
            get { return (UnsplashImage)GetValue(UnsplashImageProperty); }
            set { SetValue(UnsplashImageProperty, value); }
        }

        public static readonly DependencyProperty UnsplashImageProperty =
            DependencyProperty.Register("UnsplashImage", typeof(UnsplashImage), typeof(PhotoDetailControl), new PropertyMetadata(null, OnImageChanged));

        private static void OnImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PhotoDetailControl;
            var currentImage = e.NewValue as UnsplashImage;
            control.LargeImage.Source = currentImage.ListImageBitmap;
            control.InfoGrid.Background = currentImage.MajorColor;
            control.NameTB.Text = currentImage.Owner.Name;
            if (ColorConverter.IsLight(currentImage.MajorColor.Color))
            {
                control.NameTB.Foreground = new SolidColorBrush(Colors.Black);
                control.ByTB.Foreground = new SolidColorBrush(Colors.Black);
                control.CopyUrlBorder.Background = new SolidColorBrush(Colors.Black);
                control.CopyUrlTB.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                control.NameTB.Foreground = new SolidColorBrush(Colors.White);
                control.ByTB.Foreground = new SolidColorBrush(Colors.White);
                control.CopyUrlBorder.Background = new SolidColorBrush(Colors.White);
                control.CopyUrlTB.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        public PhotoDetailControl()
        {
            InitializeComponent();
            InitComposition();
        }

        private void InitComposition()
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _detailGridVisual = ElementCompositionPreview.GetElementVisual(DetailGrid);
            _borderGridVisual = ElementCompositionPreview.GetElementVisual(MaskBorder);
            _downloadBtnVisual = ElementCompositionPreview.GetElementVisual(DownloadBtn);
            _infoGridVisual = ElementCompositionPreview.GetElementVisual(InfoGrid);
            _downloadingBtnVisual = ElementCompositionPreview.GetElementVisual(LoadingHintBtn);
            _okVisual = ElementCompositionPreview.GetElementVisual(OKBtn);
            _likeBtnVisual = ElementCompositionPreview.GetElementVisual(LikeBtn);

            _infoGridVisual.Offset = new Vector3(0f, 100f, 0);
            _downloadBtnVisual.Offset = new Vector3(100f, 0f, 0f);
            _likeBtnVisual.Offset = new Vector3(200f, 0f, 0f);
            _detailGridVisual.Opacity = 0;
            _okVisual.Offset = new Vector3(100f, 0f, 0f);
            _downloadingBtnVisual.Offset = new Vector3(100f, 0f, 0f);
        }

        private void MaskBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            OnHideControl?.Invoke();
            ToggleDetailGridAnimation(false);
        }

        public void ToggleDetailGridAnimation(bool show)
        {
            DetailGrid.Visibility = Visibility.Visible;

            var fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(1f, show ? 1f : 0f);
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(500);
            fadeAnimation.DelayTime = TimeSpan.FromMilliseconds(show ? 300 : 0);

            var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _detailGridVisual.StartAnimation("Opacity", fadeAnimation);

            ToggleDownloadBtnAnimation(true);
            ToggleLikeBtnAnimation(true);

            StartInfoGridAnimation();
            batch.Completed += (sender, e) =>
            {
                if (!show)
                {
                    _downloadingBtnVisual.Offset = new Vector3(100f, 0f, 0f);
                    _okVisual.Offset = new Vector3(100f, 0f, 0f);
                    _downloadBtnVisual.Offset = new Vector3(100f, 0f, 0f);
                    _likeBtnVisual.Offset = new Vector3(200f, 0f, 0f);
                    _infoGridVisual.Offset = new Vector3(0f, 100f, 0f);
                    DetailGrid.Visibility = Visibility.Collapsed;
                }
            };
            batch.End();
        }

        private void ToggleDownloadBtnAnimation(bool show)
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, new Vector3(show ? 0f : 100f, 0f, 0f));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);
            offsetAnimation.DelayTime = TimeSpan.FromMilliseconds(show ? 300 : 0);

            _downloadBtnVisual.StartAnimation("Offset", offsetAnimation);
        }

        private void ToggleLikeBtnAnimation(bool show)
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, new Vector3(show ? 0f : 200f, 0f, 0f));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);
            offsetAnimation.DelayTime = TimeSpan.FromMilliseconds(show ? 300 : 0);

            _likeBtnVisual.StartAnimation("Offset", offsetAnimation);
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
            this.DetailContentGrid.Height = this.DetailContentGrid.ActualWidth / 1.5 + 100;
            this.DetailContentGrid.Clip = new RectangleGeometry()
            { Rect = new Rect(0, 0, DetailContentGrid.ActualWidth, DetailContentGrid.Height) };
        }

        #region Download animation
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

        private async void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleDownloadBtnAnimation(false);
            ToggleDownloadingBtnAnimation(true);
            await this.UnsplashImage.DownloadFullImage();
            ToggleDownloadingBtnAnimation(false);
            ToggleOkBtnAnimation(true);
            ToastService.SendToast("Saved :D", TimeSpan.FromMilliseconds(1000));
        }
        #endregion

        public Grid ContentGrid
        {
            get
            {
                return this.DetailContentGrid;
            }
        }

        private void LikeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CopyUlrBtn_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(UnsplashImage.GetSaveImageUrlFromSettings());
            Clipboard.SetContent(dataPackage);
            ToastService.SendToast("Copied :D");
        }
    }
}
