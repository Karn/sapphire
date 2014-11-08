using API;
using API.Content;
using API.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Data;

namespace Core.Utils.Misc {
    public class IncrementalPostLoader : ObservableCollection<Post>, ISupportIncrementalLoading {

        public string URL { get; set; }

        public bool HasMoreItems { get; set; }
        private bool _IsRunning { get; set; }

        private string FirstPostID;
        private string LastPostID;
        public int offset;

        public IncrementalPostLoader(string URL, int offset) {
            this.URL = URL;
            this.offset = offset;
            HasMoreItems = true;
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) {
            if (_IsRunning || !HasMoreItems) {
                Debug.WriteLine("Still running.");
                return AsyncInfo.Run(async c => {
                    return new LoadMoreItemsResult() {
                        Count = 0
                    };
                });
            } else {
                _IsRunning = true;
                try {
                    return AsyncInfo.Run(async c => {
                        var statusBar = StatusBar.GetForCurrentView();
                        statusBar.ProgressIndicator.Text = "Loading posts...";
                        await statusBar.ProgressIndicator.ShowAsync();

                        var posts = new List<Post>();
                        if (string.IsNullOrEmpty(LastPostID))
                            posts = await RequestHandler.RetrievePosts(URL);
                        else
                            posts = await RequestHandler.RetrievePosts(URL, LastPostID);

                        if (posts.Count != 0) {
                            foreach (var post in posts) {
                                this.Add(post);
                            }

                            if (posts.Last().type != "nocontent") {
                                if (URL.Contains("/user/dashboard")) {
                                    LastPostID = posts.Last().id;
                                    FirstPostID = this.First().id;
                                } else if (URL.Contains("/tagged")) {
                                    LastPostID = posts.Last().timestamp;
                                } else {
                                    offset += 20;
                                    LastPostID = offset.ToString();
                                }
                            }
                            DebugHandler.Info(LastPostID);
                        } else {
                            MainPage.ErrorFlyout.DisplayMessage("Unable to find posts.");
                        }

                        if (Utils.IAPHander.ShowAds && posts.Count > 5)
                            this.Add(new Post { type = "advert" });

                        if (posts.Count < 20)
                            HasMoreItems = false;

                        _IsRunning = false;
                        await statusBar.ProgressIndicator.HideAsync();
                        return new LoadMoreItemsResult() {
                            Count = (uint)posts.Count
                        };
                    });
                } catch (Exception e) {
                    DebugHandler.Error("Error awaiting posts. ", e.StackTrace);
                    MainPage.ErrorFlyout.DisplayMessage("Load failed due to exception.");
                    throw new Exception();
                }
            }
        }
    }
}
