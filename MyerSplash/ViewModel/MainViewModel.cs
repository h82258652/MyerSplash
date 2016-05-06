using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JP.Utils.Framework;
using MyerSplash.Common;
using MyerSplash.LiveTile;
using MyerSplash.View;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace MyerSplash.ViewModel
{
    public class MainViewModel : ViewModelBase, INavigable
    {
        public ImageDataViewModel DataVM { get; set; }

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

        public MainViewModel()
        {
            DataVM = new ImageDataViewModel() { MainVM = this };
            ShowFooterLoading = Visibility.Visible;
            ShowNoItemHint = Visibility.Collapsed;
        }

        private async Task Refresh()
        {
            IsRefreshing = true;
            await DataVM.RefreshAsync();
            IsRefreshing = false;

            var list = new List<string>();
            foreach(var item in DataVM.DataList)
            {
                list.Add(item.ListImageCachedFilePath);
            }
            if(App.AppSettings.EnableTile)
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
            if(IsFirstActived)
            {
                IsFirstActived = false;
                await Refresh();
            }
        }
    }
}
