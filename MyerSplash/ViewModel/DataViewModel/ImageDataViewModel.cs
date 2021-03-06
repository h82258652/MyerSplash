﻿using JP.Utils.Debug;
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
using System.Runtime.Serialization;

namespace MyerSplash.ViewModel
{
    public class ImageDataViewModel : DataViewModelBase<UnsplashImage>
    {
        [IgnoreDataMember]
        public MainViewModel MainVM { get; set; }

        public string RequestUrl { get; set; }

        public ImageDataViewModel(MainViewModel mainVM, string url)
        {
            this.MainVM = mainVM;
            this.RequestUrl = url;
        }

        public ImageDataViewModel(string url)
        {
            this.RequestUrl = url;
        }

        protected override void ClickItem(UnsplashImage item)
        {

        }

        protected async override Task<IEnumerable<UnsplashImage>> GetList(int pageIndex)
        {
            try
            {
                if (pageIndex >= 2)
                {
                    MainVM.ShowFooterLoading = Visibility.Visible;
                }

                var result = await CloudService.GetImages(pageIndex, (int)DEFAULT_PER_PAGE, CTSFactory.MakeCTS(10000).Token, RequestUrl);
                if (result.IsRequestSuccessful)
                {
                    var list = UnsplashImage.ParseListFromJson(result.JsonSrc);
                    return list;
                }
                else
                {
                    throw new APIException();
                }
            }
            catch (APIException)
            {
                await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    MainVM.ShowFooterLoading = Visibility.Collapsed;
                    MainVM.ShowFooterReloadGrid = Visibility.Visible;
                    MainVM.IsRefreshing = false;

                    if (MainVM.MainList?.Count == 0)
                        MainVM.ShowNoItemHint = Visibility.Visible;
                    else MainVM.ShowNoItemHint = Visibility.Collapsed;

                    ToastService.SendToast("Request failed.");
                });
                return new List<UnsplashImage>();
            }
            catch (TaskCanceledException)
            {
                await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    MainVM.ShowFooterLoading = Visibility.Collapsed;
                    MainVM.ShowFooterReloadGrid = Visibility.Visible;
                    MainVM.IsRefreshing = false;

                    if (MainVM.MainList.Count == 0)
                        MainVM.ShowNoItemHint = Visibility.Visible;
                    else MainVM.ShowNoItemHint = Visibility.Collapsed;

                    ToastService.SendToast("Request timeout.");
                });
                return new List<UnsplashImage>();
            }
            catch (Exception e)
            {
                var task = ExceptionHelper.WriteRecordAsync(e, nameof(ImageDataViewModel), nameof(GetList));
                return new List<UnsplashImage>();
            }
        }

        protected async override void LoadMoreItemCompleted(IEnumerable<UnsplashImage> list, int index)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < list.Count(); i++)
            {
                var item = list.ElementAt(i);

                if (i % 2 == 0) item.BackColor = new SolidColorBrush(ColorConverter.HexToColor("#FF2E2E2E").Value);
                else item.BackColor = new SolidColorBrush(ColorConverter.HexToColor("#FF383838").Value);

                tasks.Add(item.DownloadImgForListAsync());
                item.MajorColor = new SolidColorBrush(ColorConverter.HexToColor(item.ColorValue).Value);
            }
            await Task.WhenAll(tasks);
        }
    }
}
