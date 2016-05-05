using JP.Utils.UI;
using MyerSplash.Common;
using MyerSplash.Model;
using MyerSplashCustomControl;
using MyerSplashShared.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace MyerSplash.ViewModel
{
    public class ImageDataViewModel : DataViewModelBase<UnSplashImage>
    {
        public MainViewModel MainVM { get; set; }

        public ImageDataViewModel()
        {
            this.OnLoadIncrementalDataCompleted += (list,index)=>
            {
                foreach (var item in list)
                {
                    var task = item.DownloadSmallImage();
                    item.MajorColor = new SolidColorBrush(ColorConverter.HexToColor(item.ColorValue).Value);
                }
            };
        }

        protected override void ClickItem(UnSplashImage item)
        {
            
        }

        protected async override Task<IEnumerable<UnSplashImage>> GetList(int pageIndex)
        {
            try
            {
                var result = await CloudService.GetImages(pageIndex, 10, CTSFactory.MakeCTS(10000).Token);
                if (result.IsSuccessful)
                {
                    var list = UnSplashImage.ParseListFromJson(result.JsonSrc);
                    return list;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            catch (ArgumentException)
            {
                ToastService.SendToast("请求失败");
                return new List<UnSplashImage>();
            }
            catch (TaskCanceledException)
            {
                ToastService.SendToast("请求超时");
                return new List<UnSplashImage>();
            }
            catch (Exception e)
            {
                return new List<UnSplashImage>();
            }
        }
    }
}
