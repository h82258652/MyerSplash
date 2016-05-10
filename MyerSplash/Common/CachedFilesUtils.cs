using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyerSplash.Common
{
    public static class CachedFilesUtils
    {
        public static StorageFolder GetCachedFileFolder()
        {
            return ApplicationData.Current.LocalFolder;
        }
    }
}
