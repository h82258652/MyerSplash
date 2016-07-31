using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using MyerSplashShared.API;
using MyerSplashCustomControl;
using Windows.Storage.Pickers;
using JP.Utils.Data;

namespace MyerSplash.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        public static string SAVING_POSITION = "SAVING_POSITION";
        public static string DEFAULT_SAVING_POSITION = "\\Pictures\\MyerSplash";

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

        private RelayCommand _chooseSavingPositionCommand;
        public RelayCommand ChooseSavingPositionCommand
        {
            get
            {
                if (_chooseSavingPositionCommand != null) return _chooseSavingPositionCommand;
                return _chooseSavingPositionCommand = new RelayCommand(async () =>
                  {
                      FolderPicker savePicker = new FolderPicker();
                      savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                      savePicker.FileTypeFilter.Add(".jpg");
                      var folder = await savePicker.PickSingleFolderAsync();
                      if (folder != null)
                      {
                          SavingPositionPath = folder.Path;
                          LocalSettingHelper.AddValue(SAVING_POSITION, SavingPositionPath);
                      }
                  });
            }
        }

        private string _savingPositionPath;
        public string SavingPositionPath
        {
            get
            {
                return _savingPositionPath;
            }
            set
            {
                if (_savingPositionPath != value)
                {
                    _savingPositionPath = value;
                    RaisePropertyChanged(() => SavingPositionPath);
                }
            }
        }

        public SettingsViewModel()
        {
            if (LocalSettingHelper.HasValue(SAVING_POSITION))
            {
                var position = LocalSettingHelper.GetValue(SAVING_POSITION);
                SavingPositionPath = position;
            }
            else
            {
                SavingPositionPath = "\\Pictures\\MyerSplash";
            }
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
            
            ToastService.SendToast("All clear.", TimeSpan.FromMilliseconds(1000));
        }
    }
}
