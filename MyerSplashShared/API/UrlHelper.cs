using JP.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyerSplashShared.API
{
    public static class UrlHelper
    {
        public static string HOST => "api.unsplash.com";
        public static string AppKey => "403d9934ce4bb8dbef44765692144e8c6fac6d2698950cb40b07397d6c6635fe";

        public static string GetImages => $"http://{HOST}/photos/?";

        public static string SearchImages => $"http://{HOST}/photos/search?";

        public static string MakeFullUrlForGetReq(string baseUrl, List<KeyValuePair<string, string>> paramList)
        {
            StringBuilder sb = new StringBuilder(baseUrl);
            foreach (var item in paramList)
            {
                sb.Append(item.Key + "=" + item.Value + "&");
            }
            return sb.ToString();
        }

        public static string MakeFullUrlForPostReq(string baseUrl, bool withAuth)
        {
            StringBuilder sb = new StringBuilder(baseUrl);
            if (withAuth)
            {
                sb.Append("&uid=" + LocalSettingHelper.GetValue("uid"));
                sb.Append("&access_token=" + LocalSettingHelper.GetValue("access_token"));
            }
            return sb.ToString();
        }
    }
}
