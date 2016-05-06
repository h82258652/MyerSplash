using JP.Utils.Debug;
using JP.Utils.UI;
using MyerSplash.Model;
using MyerSplashCustomControl;
using MyerSplashShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;

namespace MyerSplash.ViewModel
{
    public class ImageDataViewModel : DataViewModelBase<UnsplashImage>
    {
        public MainViewModel MainVM { get; set; }

        public ImageDataViewModel()
        {
            this.OnLoadIncrementalDataCompleted += async (list, index) =>
            {
                var tasks = new List<Task>();
                for (var i = 0; i < list.Count(); i++)
                {
                    var item = list.ElementAt(i);

                    if (i % 2 == 0) item.BackColor = new SolidColorBrush(ColorConverter.HexToColor("#FF2E2E2E").Value);
                    else item.BackColor = new SolidColorBrush(ColorConverter.HexToColor("#FF383838").Value);

                    tasks.Add(item.DownloadImgForList());
                    item.MajorColor = new SolidColorBrush(ColorConverter.HexToColor(item.ColorValue).Value);
                }
                await Task.WhenAll(tasks);
            };
        }

        protected override void ClickItem(UnsplashImage item)
        {

        }

        protected async override Task<IEnumerable<UnsplashImage>> GetList(int pageIndex)
        {
            try
            {
                var result = await CloudService.GetImages(pageIndex, (int)DEFAULT_PER_PAGE, CTSFactory.MakeCTS(10000).Token);
                if (result.IsSuccessful)
                {
                    var list = UnsplashImage.ParseListFromJson(result.JsonSrc);
                    return list;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            catch (ArgumentException)
            {
                await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    MainVM.ShowFooterLoading = Visibility.Collapsed;
                    MainVM.IsRefreshing = false;
                    if (MainVM.DataVM.DataList.Count == 0)
                        MainVM.ShowNoItemHint =Visibility.Visible;
                    ToastService.SendToast("请求失败");
                });
                return new List<UnsplashImage>();
            }
            catch (TaskCanceledException)
            {
                await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    MainVM.ShowFooterLoading = Visibility.Collapsed;
                    MainVM.IsRefreshing = false;
                    if(MainVM.DataVM.DataList.Count==0)
                        MainVM.ShowNoItemHint = Visibility.Visible;
                    ToastService.SendToast("请求超时");
                });
                return new List<UnsplashImage>();
            }
            catch (Exception e)
            {
                var task = ExceptionHelper.WriteRecordAsync(e, nameof(ImageDataViewModel), nameof(GetList));
                return new List<UnsplashImage>();
            }
        }
    }
}
