using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using MyerSplashShared.API;
using MyerSplashCustomControl;

namespace MyerSplash.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private RelayCommand _clearCacheCommand;
        public RelayCommand ClearCacheCommand
        {
            get
            {
                if (_clearCacheCommand != null) return _clearCacheCommand;
                return _clearCacheCommand = new RelayCommand(async () =>
                {
                    await ClearCacheAsync();
                });
            }
        }

        public SettingsViewModel()
        {

        }

        private async Task ClearCacheAsync()
        {
            var files1 = await CacheUtil.GetCachedFileFolder().GetItemsAsync();
            foreach (var file in files1)
            {
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            var files2 = await CacheUtil.GetTempFolder().GetItemsAsync();
            foreach (var file in files2)
            {
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            ToastService.SendToast("Clean :D", TimeSpan.FromMilliseconds(1000));
        }
    }
}
