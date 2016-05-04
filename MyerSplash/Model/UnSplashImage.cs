using GalaSoft.MvvmLight;
using JP.API;
using JP.Utils.Data.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml.Media.Imaging;
using System;

namespace MyerSplash.Model
{
    public class UnSplashImage : ViewModelBase
    {
        public string RawImageUrl { get; set; }

        public string FullImageUrl { get; set; }

        public string RegularImageUrl { get; set; }

        public string SmallImageUrl { get; set; }

        public string ThumbImageUrl { get; set; }

        private BitmapImage _smallBitmap;
        public BitmapImage SmallBitmap
        {
            get
            {
                return _smallBitmap;
            }
            set
            {
                if (_smallBitmap != value)
                {
                    _smallBitmap = value;
                    RaisePropertyChanged(() => SmallBitmap);
                }
            }
        }

        private BitmapImage _largeBitmap;
        public BitmapImage LargeBitmap
        {
            get
            {
                return _largeBitmap;
            }
            set
            {
                if (_largeBitmap != value)
                {
                    _largeBitmap = value;
                    RaisePropertyChanged(() => LargeBitmap);
                }
            }
        }


        public UnSplashImage()
        {

        }

        public async Task DownloadSmallImage()
        {
            if (string.IsNullOrEmpty(SmallImageUrl)) return;
            using (var stream = await APIHelper.GetIRandomAccessStreamFromUrlAsync(SmallImageUrl))
            {
                SmallBitmap = new BitmapImage();
                await SmallBitmap.SetSourceAsync(stream);
            }
        }

        public async Task DownloadLargeImage()
        {
            if (string.IsNullOrEmpty(RegularImageUrl)) return;
            using (var stream = await APIHelper.GetIRandomAccessStreamFromUrlAsync(RegularImageUrl))
            {
                LargeBitmap = new BitmapImage();
                await LargeBitmap.SetSourceAsync(stream);
            }
        }

        public static ObservableCollection<UnSplashImage> ParseListFromJson(string json)
        {
            var list = new ObservableCollection<UnSplashImage>();
            var array = JsonArray.Parse(json);
            foreach(var item in array)
            {
                var unsplashImage = ParseObjectFromJson(item.ToString());
                list.Add(unsplashImage);
            }
            return list;
        }

        private static UnSplashImage ParseObjectFromJson(string json)
        {
            var obj = JsonObject.Parse(json);
            var urls = JsonParser.GetJsonObjFromJsonObj(obj, "urls");
            var smallImageUrl = JsonParser.GetStringFromJsonObj(urls, "small");
            var fullImageUrl = JsonParser.GetStringFromJsonObj(urls, "full");
            var regularImageUrl = JsonParser.GetStringFromJsonObj(urls, "regular");
            var thumbImageUrl = JsonParser.GetStringFromJsonObj(urls, "thumb");
            var rawImageUrl = JsonParser.GetStringFromJsonObj(urls, "raw");

            var unsplashImage = new UnSplashImage();
            unsplashImage.SmallImageUrl = smallImageUrl;
            unsplashImage.FullImageUrl = fullImageUrl;
            unsplashImage.RegularImageUrl = regularImageUrl;
            unsplashImage.ThumbImageUrl = thumbImageUrl;
            unsplashImage.RawImageUrl = rawImageUrl;

            return unsplashImage;
        }
    }
}
