using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using JP.Utils.Data;
using MyerSplash.LiveTile;
using System;
using Windows.Storage;

namespace MyerSplash.Common
{
    public class AppSettings : ViewModelBase
    {
        public ApplicationDataContainer LocalSettings { get; set; }

        public bool EnableTile
        {
            get
            {
                return ReadSettings(nameof(EnableTile), true);
            }
            set
            {
                SaveSettings(nameof(EnableTile), value);
                RaisePropertyChanged(() => EnableTile);
                if(!value)
                {
                    LiveTileUpdater.CleanUpTile();
                }
            }
        }

        public string SaveFolderPath
        {
            get
            {
                return ReadSettings(nameof(SaveFolderPath), "");
            }
            set
            {
                SaveSettings(nameof(SaveFolderPath), value);
                RaisePropertyChanged(() => SaveFolderPath);
            }
        }

        public int LoadQuality
        {
            get
            {
                return ReadSettings(nameof(LoadQuality), 0);
            }
            set
            {
                SaveSettings(nameof(LoadQuality), value);
                RaisePropertyChanged(() => LoadQuality);
            }
        }

        public int SaveQuality
        {
            get
            {
                return ReadSettings(nameof(SaveQuality), 1);
            }
            set
            {
                SaveSettings(nameof(SaveQuality), value);
                RaisePropertyChanged(() => SaveQuality);
            }
        }

        public AppSettings()
        {
            LocalSettings = ApplicationData.Current.LocalSettings;
        }

        private void SaveSettings(string key, object value)
        {
            LocalSettings.Values[key] = value;
        }

        private T ReadSettings<T>(string key, T defaultValue)
        {
            if (LocalSettings.Values.ContainsKey(key))
            {
                return (T)LocalSettings.Values[key];
            }
            if (defaultValue != null)
            {
                return defaultValue;
            }
            return default(T);
        }

        private static readonly Lazy<AppSettings> lazy = new Lazy<AppSettings>(() => new AppSettings());

        public static AppSettings Instance { get { return lazy.Value; } }
    }
}
