using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;

namespace MyerSplash.Common
{
    public class IncrementalLoadingCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        // 这里为了简单使用了Tuple<IList<T>, bool>作为返回值，第一项是新项目集合，第二项是否还有更多，也可以自定义实体类
        Func<uint, Task<Tuple<List<T>, bool>>> _dataFetchDelegate = null;

        public IncrementalLoadingCollection(Func<uint, Task<Tuple<List<T>, bool>>> dataFetchDelegate)
        {
            if (dataFetchDelegate == null) throw new ArgumentNullException("dataFetchDelegate");

            this._dataFetchDelegate = dataFetchDelegate;
        }

        public bool HasMoreItems
        {
            get;
            private set;
        }

        public bool IsAppending { get; set; } = true;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
        }

        protected async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            try
            {
                if (IsBusy)
                {
                    return new LoadMoreItemsResult() { Count = 0 };
                }

                IsBusy = true;

                // 我们忽略了CancellationToken，因为我们暂时不需要取消，需要的可以加上
                var result = await this._dataFetchDelegate(count);

                var items = result.Item1;

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (IsAppending)
                        {
                            this.Add(item);
                        }
                        else this.Insert(0, item);
                    }
                }

                // 是否还有更多
                this.HasMoreItems = result.Item2;

                await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // 加载完成事件
                    this.OnLoadMoreCompleted?.Invoke(items == null ? 0 : items.Count);
                });

                return new LoadMoreItemsResult { Count = items == null ? 0 : (uint)items.Count };
            }
            finally
            {
                IsBusy = false;
            }
        }


        public delegate void LoadMoreStarted(uint count);
        public delegate void LoadMoreCompleted(int count);

        public event LoadMoreStarted OnLoadMoreStarted;
        public event LoadMoreCompleted OnLoadMoreCompleted;

        public bool IsBusy = false;
    }
}
