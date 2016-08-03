using GalaSoft.MvvmLight;
using JP.Utils.Data.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace MyerSplash.Model
{
    public class UnsplashCategory : ViewModelBase
    {
        private int _id;
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    RaisePropertyChanged(() => Id);
                }
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChanged(() => Title);
                }
            }
        }

        private int _photoCount;
        public int PhotoCount
        {
            get
            {
                return _photoCount;
            }
            set
            {
                if (_photoCount != value)
                {
                    _photoCount = value;
                    RaisePropertyChanged(() => PhotoCount);
                }
            }
        }

        public string RequestUrl { get; set; }

        public UnsplashCategory()
        {

        }

        public static ObservableCollection<UnsplashCategory> GenerateListFromJson(string json)
        {
            var list = new ObservableCollection<UnsplashCategory>();
            JsonArray array;
            if(JsonArray.TryParse(json, out array))
            {
                foreach(var obj in array)
                {
                    var jsonObj = JsonObject.Parse(obj.ToString());
                    var id = JsonParser.GetNumberFromJsonObj(jsonObj, "id");
                    var title = JsonParser.GetStringFromJsonObj(jsonObj, "title");
                    var linksObj = JsonParser.GetJsonObjFromJsonObj(jsonObj, "links");
                    var url = JsonParser.GetStringFromJsonObj(linksObj, "photos");

                    var cate = new UnsplashCategory();
                    cate.Id = (int)id;
                    cate.Title = title;
                    cate.RequestUrl = url;

                    list.Add(cate);
                }
            }
            return list;
        }
    }
}
