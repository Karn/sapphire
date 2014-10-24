using API;
using API.Content;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Core.Utils.Misc {
    public class ItemsToShow : ObservableCollection<Post>, ISupportIncrementalLoading {

        List<Post> PostsToShow = new List<Post>();

        public string url = "";
        public string LastPostId = "";
        public string parameters = "";

        public bool HasMoreItems {
            get {
                return true;
            }
        }

        public Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) {
            CoreDispatcher coreDispatcher = Window.Current.Dispatcher;

            return Task.Run<LoadMoreItemsResult>(async () => {

                foreach (var x in await RequestHandler.RetrievePosts(url, LastPostId)) {
                    this.Add(x);
                }
                LastPostId = this.Last().id;

                //Add posts using this
                //this.Add();
                return new LoadMoreItemsResult() { Count = count };
            }).AsAsyncOperation<LoadMoreItemsResult>();

        }
    }
}
