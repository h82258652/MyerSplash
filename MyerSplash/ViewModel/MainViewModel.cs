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
using Windows.UI.Xaml;

namespace MyerSplash.ViewModel
{
    public class MainViewModel : ViewModelBase, INavigable
    {
        public ImageDataViewModel DataVM { get; set; }

        public bool IsInView { get; set; }

        public bool IsFirstActived { get; set; }

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


        public MainViewModel()
        {
            DataVM = new ImageDataViewModel() { MainVM=this};
            ShowFooterLoading = Visibility.Visible;
        }

        private async Task Refresh()
        {
            IsRefreshing = true;
            await DataVM.RefreshAsync();
            IsRefreshing = false;
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
