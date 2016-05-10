using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JP.Utils.Data;
using JP.Utils.Framework;
using MyerSplash.Common;
using MyerSplash.LiveTile;
using MyerSplash.Model;
using MyerSplash.View;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Media;
using JP.Utils.UI;

namespace MyerSplash.ViewModel
{
    public class MainViewModel : ViewModelBase, INavigable
    {
        private ImageDataViewModel _mainDataVM;
        public ImageDataViewModel MainDataVM
        {
            get
            {
                return _mainDataVM;
            }
            set
            {
                if (_mainDataVM != value)
                {
                    _mainDataVM = value;
                }
            }
        }

        private ObservableCollection<UnsplashImage> _likedList;
        public ObservableCollection<UnsplashImage> LikeList
        {
            get
            {
                return _likedList;
            }
            set
            {
                if (_likedList != value)
                {
                    _likedList = value;
                    RaisePropertyChanged(() => LikeList);
                }
            }
        }

        private ObservableCollection<UnsplashImage> _mainList;
        public ObservableCollection<UnsplashImage> MainList
        {
            get
            {
                return _mainList;
            }
            set
            {
                if (_mainList != value)
                {
                    _mainList = value;
                    RaisePropertyChanged(() => MainList);
                }
            }
        }

        public bool IsInView { get; set; }

        public bool IsFirstActived { get; set; } = true;

        private RelayCommand _refreshCommand;
        public RelayCommand RefreshCommand
        {
            get
            {
                if (_refreshCommand != null) return _refreshCommand;
                return _refreshCommand = new RelayCommand(async () =>
                  {
                      await Refresh();
                  });
            }
        }

        private RelayCommand _openDrawerCommand;
        public RelayCommand OpenDrawerCommand
        {
            get
            {
                if (_openDrawerCommand != null) return _openDrawerCommand;
                return _openDrawerCommand = new RelayCommand(() =>
                  {
                      DrawerOpened = !DrawerOpened;
                  });
            }
        }

        private bool _drawerOpened;
        public bool DrawerOpened
        {
            get
            {
                return _drawerOpened;
            }
            set
            {
                if (_drawerOpened != value)
                {
                    _drawerOpened = value;
                    RaisePropertyChanged(() => DrawerOpened);
                }
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            set
            {
                if (_isRefreshing != value)
                {
                    _isRefreshing = value;
                    RaisePropertyChanged(() => IsRefreshing);
                }
            }
        }

        private Visibility _showFooterLoading;
        public Visibility ShowFooterLoading
        {
            get
            {
                return _showFooterLoading;
            }
            set
            {
                if (_showFooterLoading != value)
                {
                    _showFooterLoading = value;
                    RaisePropertyChanged(() => ShowFooterLoading);
                }
            }
        }

        private Visibility _showNoItemHint;
        public Visibility ShowNoItemHint
        {
            get
            {
                return _showNoItemHint;
            }
            set
            {
                if (_showNoItemHint != value)
                {
                    _showNoItemHint = value;
                    RaisePropertyChanged(() => ShowNoItemHint);
                }
            }
        }

        private RelayCommand _goToSettingsCommand;
        public RelayCommand GoToSettingsCommand
        {
            get
            {
                if (_goToSettingsCommand != null) return _goToSettingsCommand;
                return _goToSettingsCommand = new RelayCommand(() =>
                  {
                      NavigationService.NaivgateToPage(typeof(SettingsPage));
                  });
            }
        }

        private RelayCommand _goToAboutCommand;
        public RelayCommand GoToAboutCommand
        {
            get
            {
                if (_goToAboutCommand != null) return _goToAboutCommand;
                return _goToAboutCommand = new RelayCommand(() =>
                  {
                      NavigationService.NaivgateToPage(typeof(AboutPage));
                  });
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    RaisePropertyChanged(() => SelectedIndex);
                    if (value == 0)
                    {
                        MainList = MainDataVM.DataList;
                    }
                    else if (value == 1)
                    {
                        MainList = LikeList;
                    }
                }
            }
        }

        public MainViewModel()
        {
            MainDataVM = new ImageDataViewModel() { MainVM = this };
            ShowFooterLoading = Visibility.Visible;
            ShowNoItemHint = Visibility.Collapsed;
            SelectedIndex = -1;
        }

        private async Task RestoreData()
        {
            var file =await CachedFilesUtils.GetCachedFileFolder().TryGetFileAsync(CachedFileNames.MainListFileName);
            if(file!=null)
            {
                var list=await SerializerHelper.DeserializeFromJsonByFileName<ObservableCollection<UnsplashImage>>(CachedFileNames.MainListFileName, CachedFilesUtils.GetCachedFileFolder());
                if(list!=null)
                {
                    this.MainList = list;
                    for(int i=0;i<MainList.Count;i++)
                    {
                        var item = MainList[i];
                        if (i % 2 == 0) item.BackColor = new SolidColorBrush(ColorConverter.HexToColor("#FF2E2E2E").Value);
                        else item.BackColor = new SolidColorBrush(ColorConverter.HexToColor("#FF383838").Value);
                        var task = item.RestoreAsync();
                    }
                    this.ShowNoItemHint = Visibility.Collapsed;
                }
            }
        }

        private async Task Refresh()
        {
            IsRefreshing = true;
            await MainDataVM.RefreshAsync();
            IsRefreshing = false;

            if(this.MainDataVM.DataList.Count>0)
            {
                await SerializerHelper.SerializerToJson<ObservableCollection<UnsplashImage>>(this.MainDataVM.DataList, CachedFileNames.MainListFileName, CachedFilesUtils.GetCachedFileFolder());
                MainList = MainDataVM.DataList;
            }

            var list = new List<string>();
            foreach (var item in MainDataVM.DataList)
            {
                list.Add(item.ListImageCachedFilePath);
            }
            if (App.AppSettings.EnableTile && list.Count > 0)
            {
                await LiveTileUpdater.UpdateImagesTileAsync(list);
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
            if (IsFirstActived)
            {
                IsFirstActived = false;
                await RestoreData();
                await Refresh();
            }
        }
    }
}
