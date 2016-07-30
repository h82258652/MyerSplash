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
using Windows.UI.Xaml.Media;
using JP.Utils.UI;
using MyerSplashShared.API;
using System.Linq;
using System.Diagnostics;

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

        private ObservableCollection<UnsplashCategory> _categories;
        public ObservableCollection<UnsplashCategory> Categories
        {
            get
            {
                return _categories;
            }
            set
            {
                if (_categories != value)
                {
                    _categories = value;
                    RaisePropertyChanged(() => Categories);
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
                return _retryCommand = new RelayCommand(async () =>
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
                      if (DrawerOpened)
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
                    RaisePropertyChanged(() => SelectedTitle);
                    DrawerOpened = false;
                    if (value == 0)
                    {
                        MainDataVM = new ImageDataViewModel(this, UrlHelper.GetFeaturedImages);
                    }
                    else if (Categories?.Count > 0)
                    {
                        MainDataVM = new ImageDataViewModel(this, Categories[value].RequestUrl);
                    }
                    if (MainDataVM != null)
                    {
                        var task = MainDataVM.RefreshAsync();
                    }
                }
            }
        }

        public string SelectedTitle
        {
            get
            {
                if (Categories?.Count > 0)
                {
                    return Categories[SelectedIndex].Title.ToUpper();
                }
                else return "FEATURED";
            }
        }

        private bool[] _hintStates = new bool[3];

        public MainViewModel()
        {
            MainList = new ObservableCollection<UnsplashImage>();

            ShowFooterLoading = Visibility.Collapsed;
            ShowNoItemHint = Visibility.Collapsed;
            ShowFooterReloadGrid = Visibility.Collapsed;

            App.MainVM = this;

            SelectedIndex = -1;
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
                var list = await SerializerHelper.DeserializeFromJsonByFile<IncrementalLoadingCollection<UnsplashImage>>(CachedFileNames.MainListFileName, CacheUtil.GetCachedFileFolder());
                if (list != null)
                {
                    this.MainDataVM = new ImageDataViewModel(this, UrlHelper.GetFeaturedImages);
                    list.ToList().ForEach(s => MainDataVM.DataList.Add(s));

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
                else MainDataVM = new ImageDataViewModel(this, UrlHelper.GetFeaturedImages);
            }
            else MainDataVM = new ImageDataViewModel(this, UrlHelper.GetFeaturedImages);
        }

        private async Task RefreshAsync()
        {
            MainDataVM.MainVM = this;

            var task1 = GetCategoriesAsync();

            IsRefreshing = true;
            await MainDataVM.RefreshAsync();
            IsRefreshing = false;

            MainList = MainDataVM.DataList;

            await SaveMainListDataAsync();
            await UpdateLiveTileAsync();
        }

        private async Task GetCategoriesAsync()
        {
            if (Categories?.Count > 0) return;

            var result = await CloudService.GetCategories(CTSFactory.MakeCTS(20000).Token);
            if (result.IsRequestSuccessful)
            {
                var list = UnsplashCategory.GenerateListFromJson(result.JsonSrc);
                this.Categories = list;
                this.Categories.Insert(0, new UnsplashCategory()
                {
                    Title = "Featured",
                });

                SelectedIndex = 0;
            }
        }

        private async Task SaveMainListDataAsync()
        {
            if (this.MainDataVM.DataList?.Count > 0)
            {
                await SerializerHelper.SerializerToJson<IncrementalLoadingCollection<UnsplashImage>>(this.MainDataVM.DataList, CachedFileNames.MainListFileName, CacheUtil.GetCachedFileFolder());
                //if (MainList?.ToList().FirstOrDefault()?.ID != MainDataVM?.DataList?.FirstOrDefault()?.ID && SelectedIndex == 0)
                //{
                //    MainList = MainDataVM.DataList;
                //}
            }
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
                Debug.WriteLine("About to update tile.");
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
                await RefreshAsync();
            }
        }
    }
}
