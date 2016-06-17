using JP.API;
using JP.Utils.Data;
using JP.Utils.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyerSplashShared.API
{
    public class CacheUtil
    {
        /// <summary>
        /// 第一项是URL 地址，第二是文件的本地 Path
        /// </summary>
        private Dictionary<string, string> CachedFiles { get; set; }

        public CacheUtil()
        {

        }

        public async Task CleanUpAsync()
        {
            var tempFolder = GetCachedFileFolder();
            var items = await tempFolder.GetItemsAsync();
            foreach (var item in items)
            {
                await item.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            CachedFiles.Clear();
            await SaveAsync();
        }

        public async Task<StorageFile> DownloadImageAsync(string url,string desireName="img.jpg")
        {
            if (CachedFiles.ContainsKey(url))
            {
                return await StorageFile.GetFileFromPathAsync(CachedFiles[url]);
            }
            using (var stream = await FileDownloadUtil.GetIRandomAccessStreamFromUrlAsync(url,CTSFactory.MakeCTS().Token))
            {
                var newFile = await GetTempFolder().CreateFileAsync(desireName, CreationCollisionOption.GenerateUniqueName);
                var bytes = new byte[stream.AsStream().Length];
                await stream.AsStream().ReadAsync(bytes, 0, (int)stream.AsStream().Length);
                await FileIO.WriteBytesAsync(newFile, bytes);

                CachedFiles.Add(url, newFile.Path);
                await SaveAsync();
                return newFile;
            }
        }

        public async Task SaveAsync()
        {
            var tempFolder = GetCachedFileFolder();
            await SerializerHelper.SerializerToJson<Dictionary<string, string>>(CachedFiles, "CachedFiles", tempFolder, true);
        }

        public async Task LoadAsync()
        {
            var tempFolder = GetCachedFileFolder();
            this.CachedFiles = await SerializerHelper.DeserializeFromJsonByFile<Dictionary<string, string>>("CachedFiles", tempFolder);
            if (this.CachedFiles == null)
            {
                CachedFiles = new Dictionary<string, string>();
            }
        }

        public async Task<StorageFile> GetCachedFile(string url)
        {
            if (CachedFiles.ContainsKey(url))
            {
                return await StorageFile.GetFileFromPathAsync(CachedFiles[url]);
            }
            return null;
        }

        public static StorageFolder GetCachedFileFolder()
        {
            return ApplicationData.Current.LocalFolder;
        }

        public static StorageFolder GetTempFolder()
        {
            return ApplicationData.Current.TemporaryFolder;
        }
    }
}
