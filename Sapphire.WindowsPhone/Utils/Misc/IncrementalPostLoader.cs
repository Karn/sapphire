using Core.AuthenticationManager;
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
		public string URL { get; set; }

		public bool HasMoreItems { get; set; }
		private bool _IsRunning { get; set; }

		private string tag;
		private string LastPostID;
		public int offset = 0;

		public IncrementalPostLoader(string URL = "") {
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
						Core.Services.Utils.RequestParameters _params = new Core.Services.Utils.RequestParameters() {
								{ "api_key", Authentication.ConsumerKey }
							};

						if (URL.Contains("/tagged")) {
							if (tag == null) {
								var x = URL.Split('?');
								URL = x[0];
								x = x[1].Split('=');
								tag = x[1];
							}

							_params.Add("tag", tag);
							if (!string.IsNullOrEmpty(LastPostID))
								_params.Add("before", LastPostID);
						} else if (!string.IsNullOrEmpty(LastPostID)) {
							var _key = "offset";
							if (URL.Contains("/user/dashboard") || URL.Contains("/submission") || URL.Contains("/draft") || URL.Contains("/queue")) {
								_key = "max_id";
							}
							_params.Add(_key, LastPostID);
						}

						posts = await CreateRequest.RetrievePosts(URL, _params);

						if (posts != null && posts.Count != 0) {
							foreach (var post in posts)
								this.Add(post);

							if (posts.Last().type != "nocontent") {
								if (URL.Contains("/user/dashboard")) {
									LastPostID = posts.Last().id;
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

						if (posts == null || posts.Count < 20)
							HasMoreItems = false;

						_IsRunning = false;
						App.HideStatus();
						return new LoadMoreItemsResult() { Count = (posts == null) ? 0 : (uint)posts.Count };
					});
				} catch (Exception ex) {
				}
				return null;
			}
		}
	}
}
