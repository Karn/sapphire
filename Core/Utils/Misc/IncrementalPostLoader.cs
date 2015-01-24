using APIWrapper.Client;
using APIWrapper.Content.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace Core.Utils.Misc {
    public class IncrementalPostLoader : ObservableCollection<Post>, ISupportIncrementalLoading {

        private static string TAG = "IncrementalPostLoader";
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
#pragma warning disable CS1998
                return AsyncInfo.Run(async c => {
#pragma warning restore CS1998
                    return new LoadMoreItemsResult() {
                        Count = 0
                    };
                });
            } else {
                _IsRunning = true;
                try {
                    return AsyncInfo.Run(async c => {
                        App.DisplayStatus(App.LocaleResources.GetString("LoadingPostsMessage"));

                        var posts = new List<Post>();
                        if (string.IsNullOrEmpty(LastPostID))
                            posts = await CreateRequest.RetrievePosts(URL);
                        else
                            posts = await CreateRequest.RetrievePosts(URL, LastPostID);

                        if (posts.Count != 0) {
                            foreach (var post in posts)
                                this.Add(post);

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
                        } else {
                            App.Alert(App.LocaleResources.GetString("PostLoadFailed"));
                        }

                        if (AppLicenseHandler.IsTrial && posts.Count > 5)
                            this.Add(new Post { type = "advert" });

                        if (posts.Count < 20)
                            HasMoreItems = false;

                        _IsRunning = false;
                        App.HideStatus();
                        return new LoadMoreItemsResult() { Count = (uint)posts.Count };
                    });
                } catch (Exception ex) {
                    Analytics.AnalyticsManager.LogException(ex, TAG, "Load failed due to exception.");
                    App.Alert("Load failed due to exception.");
                    throw new Exception();
                }
            }
        }
    }
}
