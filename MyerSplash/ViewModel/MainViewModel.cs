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
using MyerSplashShared.API;
using System.Linq;

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
                    RaisePropertyChanged(() => MainDataVM);
                }
            }
        }

        private ObservableCollection<UnsplashImage> _likedList;
        public ObservableCollection<UnsplashImage> LikedList
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
                    RaisePropertyChanged(() => LikedList);
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
                      await RefreshAsync();
                  });
            }
        }

        private RelayCommand _retryCommand;
        public RelayCommand RetryCommand
        {
            get
            {
                if (_retryCommand != null) return _retryCommand;
                return _retryCommand = new RelayCommand(async() =>
                  {
                      ShowFooterLoading = Visibility.Visible;
                      ShowFooterReloadGrid = Visibility.Collapsed;
                      await MainDataVM.RetryAsync();
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
                      if(DrawerOpened)
                      {
                          NavigationService.HistoryOperationsBeyondFrame.Push(() =>
                          {
                              if (DrawerOpened)
                              {
                                  DrawerOpened = false;
                                  return true;
                              }
                              else return false;
                          });
                      }
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

        private Visibility _showFooterReloadGrid;
        public Visibility ShowFooterReloadGrid
        {
            get
            {
                return _showFooterReloadGrid;
            }
            set
            {
                if (_showFooterReloadGrid != value)
                {
                    _showFooterReloadGrid = value;
                    RaisePropertyChanged(() => ShowFooterReloadGrid);
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
                      DrawerOpened = false;
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
                      DrawerOpened = false;
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
                    DrawerOpened = false;
                    if (value == 0)
                    {
                        RestoreHintState();
                        MainList = MainDataVM.DataList;
                    }
                    else if (value == 1)
                    {
                        SaveHintState();
                        MainList = LikedList;
                    }
                }
            }
        }

        private bool[] _hintStates = new bool[3];

        public MainViewModel()
        {
            MainList = new ObservableCollection<UnsplashImage>();
            LikedList = new ObservableCollection<UnsplashImage>();

            ShowFooterLoading = Visibility.Visible;
            ShowNoItemHint = Visibility.Collapsed;
            ShowFooterReloadGrid = Visibility.Collapsed;
            SelectedIndex = 0;

            App.MainVM = this;
        }

        private void SaveHintState()
        {
            _hintStates[0] = ShowFooterLoading == Visibility.Visible ? true : false;
            _hintStates[1] = ShowNoItemHint == Visibility.Visible ? true : false;
            _hintStates[2] = ShowFooterReloadGrid == Visibility.Visible ? true : false;

            ShowFooterLoading = ShowNoItemHint = ShowFooterReloadGrid = Visibility.Collapsed;
        }

        private void RestoreHintState()
        {
            ShowFooterLoading = _hintStates[0] ? Visibility.Visible : Visibility.Collapsed;
            ShowNoItemHint = _hintStates[1] ? Visibility.Visible : Visibility.Collapsed;
            ShowFooterReloadGrid = _hintStates[2] ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task RestoreMainListDataAsync()
        {
            var file = await CacheUtil.GetCachedFileFolder().TryGetFileAsync(CachedFileNames.MainListFileName);
            if (file != null)
            {
                var list = await SerializerHelper.DeserializeFromJsonByFileName<IncrementalLoadingCollection<UnsplashImage>>(CachedFileNames.MainListFileName, CacheUtil.GetCachedFileFolder());
                if (list != null)
                {
                    this.MainDataVM = new ImageDataViewModel(this);
                    list.ToList().ForEach(s => MainDataVM.DataList.Add(s));

                    this.MainList = MainDataVM.DataList;
                    for (int i = 0; i < MainDataVM.DataList.Count; i++)
                    {
                        var item = MainDataVM.DataList[i];
                        if (i % 2 == 0) item.BackColor = new SolidColorBrush(ColorConverter.HexToColor("#FF2E2E2E").Value);
                        else item.BackColor = new SolidColorBrush(ColorConverter.HexToColor("#FF383838").Value);
                        var task = item.RestoreAsync();
                    }
                    this.ShowNoItemHint = Visibility.Collapsed;
                    await UpdateLiveTileAsync();
                }
                else MainDataVM = new ImageDataViewModel(this);
            }
            else MainDataVM = new ImageDataViewModel(this);
        }

        private async Task RestoreLikedListDataAsync()
        {
            var file = await CacheUtil.GetCachedFileFolder().TryGetFileAsync(CachedFileNames.LikedListFileName);
            if (file != null)
            {
                var list = await SerializerHelper.DeserializeFromJsonByFileName<ObservableCollection<UnsplashImage>>(CachedFileNames.LikedListFileName, CacheUtil.GetCachedFileFolder());
                if (list != null)
                {
                    this.LikedList = list;
                    for (int i = 0; i < LikedList.Count; i++)
                    {
                        var item = LikedList[i];
                        if (i % 2 == 0) item.BackColor = new SolidColorBrush(ColorConverter.HexToColor("#FF2E2E2E").Value);
                        else item.BackColor = new SolidColorBrush(ColorConverter.HexToColor("#FF383838").Value);
                        var task = item.RestoreAsync();
                    }
                }
            }
        }

        private async Task RefreshAsync()
        {
            MainDataVM.MainVM = this;

            IsRefreshing = true;
            await MainDataVM.RefreshAsync();
            IsRefreshing = false;

            await SaveMainListDataAsync();
            await UpdateLiveTileAsync();
        }

        private async Task SaveMainListDataAsync()
        {
            if (this.MainDataVM.DataList?.Count > 0)
            {
                await SerializerHelper.SerializerToJson<IncrementalLoadingCollection<UnsplashImage>>(this.MainDataVM.DataList, CachedFileNames.MainListFileName, CacheUtil.GetCachedFileFolder());
                if (MainList?.ToList().FirstOrDefault()?.ID != MainDataVM?.DataList?.FirstOrDefault()?.ID && SelectedIndex==0)
                    MainList = MainDataVM.DataList;
            }
        }

        private async Task SaveLikedListDataAsync()
        {
            if (this.LikedList?.Count > 0)
            {
                await SerializerHelper.SerializerToJson<ObservableCollection<UnsplashImage>>(this.LikedList, CachedFileNames.LikedListFileName, CacheUtil.GetCachedFileFolder());
            }
        }

        public async Task AddToLlikedListAndSaveAsync(UnsplashImage img)
        {
            LikedList.Add(img);
            await SaveLikedListDataAsync();
        }

        public async Task RemoveFromLlikedListAndSaveAsync(UnsplashImage img)
        {
            LikedList.Remove(img);
            await SaveLikedListDataAsync();
        }

        private async Task UpdateLiveTileAsync()
        {
            var list = new List<string>();

            if (MainList == null) return;

            foreach (var item in MainList)
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
                await RestoreMainListDataAsync();
                await RestoreLikedListDataAsync();
                await RefreshAsync();
            }
        }
    }
}
