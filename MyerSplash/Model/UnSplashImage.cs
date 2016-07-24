using GalaSoft.MvvmLight;
using JP.Utils.Data.Json;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml.Media.Imaging;
using System;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using System.Runtime.Serialization;
using JP.Utils.UI;
using System.Threading;
using Windows.Networking.BackgroundTransfer;
using MyerSplash.Common;
using GalaSoft.MvvmLight.Command;
using MyerSplashCustomControl;
using JP.Utils.Data;
using MyerSplash.ViewModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using System.Collections.Generic;

namespace MyerSplash.Model
{
    public class UnsplashImage : ViewModelBase
    {
        public string ID { get; set; }

        public string RawImageUrl { get; set; }

        public string FullImageUrl { get; set; }

        public string RegularImageUrl { get; set; }

        public string SmallImageUrl { get; set; }

        public string ThumbImageUrl { get; set; }

        private BitmapImage _listImageBitmap;
        [IgnoreDataMember]
        public BitmapImage ListImageBitmap
        {
            get
            {
                return _listImageBitmap;
            }
            set
            {
                if (_listImageBitmap != value)
                {
                    _listImageBitmap = value;
                    RaisePropertyChanged(() => ListImageBitmap);
                }
            }
        }

        public string ListImageCachedFilePath { get; set; }

        private BitmapImage _largeBitmap;
        [IgnoreDataMember]
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

        private SolidColorBrush _majorColor;
        [IgnoreDataMember]
        public SolidColorBrush MajorColor
        {
            get
            {
                return _majorColor;
            }
            set
            {
                if (_majorColor != value)
                {
                    _majorColor = value;
                    RaisePropertyChanged(() => MajorColor);
                }
            }
        }

        private SolidColorBrush _backColor;
        [IgnoreDataMember]
        public SolidColorBrush BackColor
        {
            get
            {
                return _backColor;
            }
            set
            {
                if (_backColor != value)
                {
                    _backColor = value;
                    RaisePropertyChanged(() => BackColor);
                }
            }
        }

        public string ColorValue { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        private UnsplashUser _owner;
        public UnsplashUser Owner
        {
            get
            {
                return _owner;
            }
            set
            {
                if (_owner != value)
                {
                    _owner = value;
                    RaisePropertyChanged(() => Owner);
                }
            }
        }

        private bool _liked;
        public bool Liked
        {
            get
            {
                return _liked;
            }
            set
            {
                if (_liked != value)
                {
                    _liked = value;
                    RaisePropertyChanged(() => Liked);
                }
            }
        }

        private BackgroundDownloader _backgroundDownloader = new BackgroundDownloader();

        public UnsplashImage()
        {

        }

        public async Task SetDataRequestData(DataRequest request)
        {
            DataPackage requestData = request.Data;
            requestData.Properties.Title = "Share photo";
            requestData.Properties.ContentSourceWebLink = new Uri(FullImageUrl);
            requestData.Properties.ContentSourceApplicationLink = new Uri(FullImageUrl);

            var shareText = $"Share {this.Owner.Name}'s amazing photo. Download it directly: {FullImageUrl}  From MyerSplash UWP app.";

            requestData.SetText(shareText);

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(shareText);
            Clipboard.SetContent(dataPackage);

            DataRequestDeferral deferral = request.GetDeferral();

            var file = await StorageFile.GetFileFromPathAsync(ListImageCachedFilePath);
            if (file != null)
            {
                List<IStorageItem> imageItems = new List<IStorageItem>();
                imageItems.Add(file);
                requestData.SetStorageItems(imageItems);

                RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromFile(file);
                requestData.SetBitmap(imageStreamRef);
                requestData.Properties.Thumbnail = imageStreamRef;
            }

            deferral.Complete();
        }

        public async Task RestoreAsync()
        {
            this.MajorColor = new SolidColorBrush(ColorConverter.HexToColor(ColorValue).Value);
            await DownloadImgForListAsync();
        }

        public async Task DownloadImgForListAsync()
        {
            var url = GetListImageUrlFromSettings();

            if (string.IsNullOrEmpty(url)) return;

            var file = await App.CacheUtilInstance.DownloadImageAsync(url, this.ID + ".jpg");
            ListImageCachedFilePath = file.Path;
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                ListImageBitmap = new BitmapImage();
                await ListImageBitmap.SetSourceAsync(stream);
            }
        }

        public string GetListImageUrlFromSettings()
        {
            var quality = App.AppSettings.LoadQuality;
            switch (quality)
            {
                case 0: return RegularImageUrl;
                case 1: return SmallImageUrl;
                case 2: return ThumbImageUrl;
                default: return "";
            }
        }

        public string GetSaveImageUrlFromSettings()
        {
            var quality = App.AppSettings.SaveQuality;
            switch (quality)
            {
                case 0: return RawImageUrl;
                case 1: return FullImageUrl;
                case 2: return RegularImageUrl;
                default: return "";
            }
        }

        public async Task DownloadFullImage(CancellationToken token)
        {
            var url = GetSaveImageUrlFromSettings();

            if (string.IsNullOrEmpty(url)) return;

            ToastService.SendToast("Downloading in background...", 2000);

            StorageFolder folder = null;
            if (LocalSettingHelper.HasValue(SettingsViewModel.SAVING_POSITION))
            {
                var path = LocalSettingHelper.GetValue(SettingsViewModel.SAVING_POSITION);
                if (path == SettingsViewModel.DEFAULT_SAVING_POSITION)
                {
                    folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("MyerSplash", CreationCollisionOption.OpenIfExists);
                }
                else
                {
                    folder = await StorageFolder.GetFolderFromPathAsync(path);
                }
            }
            if (folder == null)
            {
                folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("MyerSplash", CreationCollisionOption.OpenIfExists);
            }
            var newFile = await folder.CreateFileAsync($"{ID}.jpg", CreationCollisionOption.GenerateUniqueName);

            //backgroundDownloader.FailureToastNotification = ToastHelper.CreateToastNotification("Failed to download :-(", "You may cancel it. Otherwise please check your network.");
            _backgroundDownloader.SuccessToastNotification = ToastHelper.CreateToastNotification("Saved:D",
                $"You can find it in {folder.Path}.");

            var downloadOperation = _backgroundDownloader.CreateDownload(new Uri(url), newFile);

            var progress = new Progress<DownloadOperation>();
            try
            {
                await downloadOperation.StartAsync().AsTask(token, progress);
            }
            catch (TaskCanceledException)
            {
                await downloadOperation.ResultFile.DeleteAsync();
                downloadOperation = null;
                throw;
            }
        }

        public static ObservableCollection<UnsplashImage> ParseListFromJson(string json)
        {
            var list = new ObservableCollection<UnsplashImage>();
            var array = JsonArray.Parse(json);
            foreach (var item in array)
            {
                var unsplashImage = ParseObjectFromJson(item.ToString());
                list.Add(unsplashImage);
            }
            return list;
        }

        private static UnsplashImage ParseObjectFromJson(string json)
        {
            var obj = JsonObject.Parse(json);
            var urls = JsonParser.GetJsonObjFromJsonObj(obj, "urls");
            var smallImageUrl = JsonParser.GetStringFromJsonObj(urls, "small");
            var fullImageUrl = JsonParser.GetStringFromJsonObj(urls, "full");
            var regularImageUrl = JsonParser.GetStringFromJsonObj(urls, "regular");
            var thumbImageUrl = JsonParser.GetStringFromJsonObj(urls, "thumb");
            var rawImageUrl = JsonParser.GetStringFromJsonObj(urls, "raw");
            var color = JsonParser.GetStringFromJsonObj(obj, "color");
            var width = JsonParser.GetNumberFromJsonObj(obj, "width");
            var height = JsonParser.GetNumberFromJsonObj(obj, "height");
            var userObj = JsonParser.GetJsonObjFromJsonObj(obj, "user");
            var userName = JsonParser.GetStringFromJsonObj(userObj, "name");
            var id = JsonParser.GetStringFromJsonObj(obj, "id");

            var img = new UnsplashImage();
            img.SmallImageUrl = smallImageUrl;
            img.FullImageUrl = fullImageUrl;
            img.RegularImageUrl = regularImageUrl;
            img.ThumbImageUrl = thumbImageUrl;
            img.RawImageUrl = rawImageUrl;
            img.ColorValue = color;
            img.Width = width;
            img.Height = height;
            img.Owner = new UnsplashUser() { Name = userName };
            img.ID = id;

            return img;
        }
    }
}
