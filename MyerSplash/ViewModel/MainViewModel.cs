using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JP.Utils.Debug;
using JP.Utils.Framework;
using MyerSplash.Model;
using MyerSplashCustomControl;
using MyerSplashShared.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyerSplash.ViewModel
{
    public class MainViewModel : ViewModelBase, INavigable
    {
        private ObservableCollection<UnSplashImage> _images;
        public ObservableCollection<UnSplashImage> Images
        {
            get
            {
                return _images;
            }
            set
            {
                if (_images != value)
                {
                    _images = value;
                    RaisePropertyChanged(() => Images);
                }
            }
        }

        public bool IsInView { get; set; }

        public bool IsFirstActived { get; set; }

        private RelayCommand _refreshCommand;
        public RelayCommand RefreshCommand
        {
            get
            {
                if (_refreshCommand != null) return _refreshCommand;
                return _refreshCommand = new RelayCommand(async() =>
                  {
                      await Refresh();
                  });
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    RaisePropertyChanged(() => IsLoading);
                }
            }
        }


        public MainViewModel()
        {
            Images = new ObservableCollection<UnSplashImage>();
        }

        private async Task Refresh()
        {
            try
            {
                IsLoading = true;
                var result = await CloudService.GetImages(1, 30, CTSFactory.MakeCTS(10000).Token);
                if (result.IsSuccessful)
                {
                    var list = UnSplashImage.ParseListFromJson(result.JsonSrc);
                    this.Images = list;
                    foreach (var item in Images)
                    {
                        var task = item.DownloadSmallImage();
                    }
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            catch (ArgumentException)
            {
                ToastService.SendToast("请求失败");
            }
            catch (TaskCanceledException e)
            {
                var task = ExceptionHelper.WriteRecordAsync(e, nameof(Refresh), nameof(MainViewModel));
                ToastService.SendToast("请求超时");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void Activate(object param)
        {

        }

        public void Deactivate(object param)
        {

        }

        public async void OnLoaded()
        {
            await Refresh();
        }
    }
}
