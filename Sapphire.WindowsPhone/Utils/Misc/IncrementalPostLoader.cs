using Core.Client;
using Core.Content.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace Sapphire.Utils.Misc {
	public class IncrementalPostLoader : ObservableCollection<Post>, ISupportIncrementalLoading {

		private static string TAG = "IncrementalPostLoader";
		public string URL { get; set; }

		public bool HasMoreItems { get; set; }
		private bool _IsRunning { get; set; }

		private string FirstPostID;
		private string LastPostID;
		public int offset = 0;

		public IncrementalPostLoader(string URL) {
			this.URL = URL;
			HasMoreItems = true;
		}

		public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) {
			if (_IsRunning || !HasMoreItems) {
				return AsyncInfo.Run(async c => {
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
						else {
							var _key = "offset";
							if (URL.Contains("/user/dashboard") || URL.Contains("/submission") || URL.Contains("/draft") || URL.Contains("/queue")) {
								_key = "max_id";
							} else if (URL.Contains("/tagged")) {
								_key = "before";
							}
							posts = await CreateRequest.RetrievePosts(URL, new Core.Service.Requests.RequestParameters() {
								{_key, LastPostID }
							});
						}

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

						if (posts.Count < 20)
							HasMoreItems = false;

						_IsRunning = false;
						App.HideStatus();
						return new LoadMoreItemsResult() { Count = (uint)posts.Count };
					});
				} catch (Exception ex) {
				}
				return null;
			}
		}
	}
}
