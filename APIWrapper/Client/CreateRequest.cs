using APIWrapper.AuthenticationManager;
using APIWrapper.Content;
using APIWrapper.Content.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APIWrapper.Client {
	public class CreateRequest {
		private static HttpClient Client = new HttpClient();

		public static string GetPlainTextFromHtml(string htmlString) {
			if (htmlString != null) {
				string htmlTagPattern = "<.*?>";
				var regexCss = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
				htmlString = regexCss.Replace(htmlString, string.Empty);
				htmlString = Regex.Replace(htmlString, htmlTagPattern, string.Empty);
				htmlString = Regex.Replace(htmlString, @"^\s+$[\r]*", "", RegexOptions.Multiline);
				htmlString = htmlString.Replace("&nbsp;", " ");
				return WebUtility.HtmlDecode(htmlString);
			}
			return htmlString;
		}

		public static async Task<bool> RetrieveAccountInformation(string account) {
			try {
				for (int i = 0; i < 5; i++) {
					var requestResult = await RequestService.GET("https://api.tumblr.com/v2/user/info");

					if (requestResult.StatusCode == HttpStatusCode.OK) {

						var parsedData = JsonConvert.DeserializeObject<Responses.GetInfo>(await requestResult.Content.ReadAsStringAsync());

						UserUtils.UserBlogs.Clear();

						foreach (var b in parsedData.response.user.AccountBlogs) {
							b.FollowingCount = parsedData.response.user.BlogsFollowingCount;
							b.LikedPostCount = parsedData.response.user.LikedPostCount;

							if (b.Name == account) {
								UserUtils.CurrentBlog = b;
							} else if (UserUtils.CurrentBlog == null && b.IsPrimaryBlog)
								UserUtils.CurrentBlog = b;
							else if (b.Name == UserUtils.CurrentBlog.Name)
								UserUtils.CurrentBlog = b;

							UserUtils.UserBlogs.Add(b);
						}

						if (UserUtils.UserBlogs.Count > 0)
							return true;
					}
					Debug.WriteLine("Rerunning call due to previous faliure. " + (i + 1));
				}
			} catch (Exception ex) {

			}
			return false;
		}

		public static async Task<List<Activity.Notification>> RetrieveActivity() {
			if (UserUtils.CurrentBlog != null) {
				try {
					for (int i = 0; i < 5; i++) {
						var result = await RequestService.GET("https://api.tumblr.com/v2/blog/" + UserUtils.CurrentBlog.Name + ".tumblr.com/notifications", "rfg=true");
						Debug.WriteLine(await result.Content.ReadAsStringAsync());
						if (result.StatusCode == HttpStatusCode.OK) {
							var activity = JsonConvert.DeserializeObject<Responses.GetActivity>(await result.Content.ReadAsStringAsync());

							var Notifications = new List<Activity.Notification>();
							var NotificationDictionary = UserUtils.NotificationIDs;

							foreach (var n in activity.response.notifications) {
								n.date = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(n.timestamp).ToString("yyyy'-'MM'-'dd");
								Notifications.Add(n);
							}
							NotificationDictionary[UserUtils.CurrentBlog.Name] = Notifications.First().timestamp;

							UserUtils.NotificationIDs = NotificationDictionary;
							return Notifications;
						}
						Debug.WriteLine("Rerunning call due to previous faliure. " + i);
					}
				} catch (Exception ex) { }
			}
			return new List<Activity.Notification>();
		}

		public static async Task<bool> LikePost(string id, string reblogKey) {
			return (await RequestService.GET(EndpointManager.LikePost,
				string.Format("id={0}&reblog_key={1}", id, reblogKey)
				)).StatusCode == HttpStatusCode.OK;
		}

		public static async Task<bool> UnlikePost(string id, string reblogKey) {
			return (await RequestService.GET(EndpointManager.UnlikePost,
				string.Format("id={0}&reblog_key={1}", id, reblogKey)
				)).StatusCode == HttpStatusCode.OK;
		}

		public static async Task<HttpResponseMessage> CreatePost(string parameters) {
			return await RequestService.POST(EndpointManager.Post, parameters);
		}

		public static async Task<bool> CreateReply(string id, string answer, bool isPrivate) {
			var req = await RequestService.POST("https://api.tumblr.com/v2/blog/" + UserUtils.CurrentBlog.Name + ".tumblr.com/question/reply",
				string.Format("type=answer&id={0}&post_id={0}&answer={2}&is_private={3}", id, answer, isPrivate ? "true" : "false")
				);
			Debug.WriteLine(await req.Content.ReadAsStringAsync());
			return (req).StatusCode == HttpStatusCode.OK;
		}

		public async static Task<bool> ReblogPost(string id, string reblogKey, string caption = "",
			string tags = "", string additionalParameters = "", string blogName = "") {
			return (await RequestService.POST(EndpointManager.Reblog(blogName),
				(string.Format("id={0}&reblog_key={1}", id, reblogKey) +
				(!string.IsNullOrEmpty(caption) ? "&comment=" + caption : "") +
				(!string.IsNullOrEmpty(tags) ? "&tags=" + tags : "") +
				(!string.IsNullOrEmpty(additionalParameters) ? additionalParameters : "")
				))).StatusCode == HttpStatusCode.Created;

		}

		public async static Task<bool> PostDraft(string id) {
			return (await RequestService.POST(EndpointManager.Edit,
				string.Format("state=published&id={0}", id)
				)).StatusCode == HttpStatusCode.OK;
		}

		public async static Task<bool> DeletePost(string id) {
			return (await RequestService.POST(EndpointManager.DeletePost,
				string.Format("state=published&id={0}", id)
				)).StatusCode == HttpStatusCode.OK;
		}

		public async static Task<bool> PostQuestion(string blogName, string question) {
			var req = await RequestService.POST("https://api.tumblr.com/v2/blog/" + blogName + ".tumblr.com/ask",
				string.Format("sender={0}&question={1}", UserUtils.CurrentBlog.Name, question)
				);
			Debug.WriteLine(await req.Content.ReadAsStringAsync());
			return (req).StatusCode == HttpStatusCode.OK;
		}

		/// <summary>
		/// Method to follow or unfollow a blog
		/// </summary>
		/// <param name="follow">Value to indicate whether the blog is to be followed (true) or not</param>
		/// <param name="blogName">The name of the blog that is affectd by the API call</param>
		/// <returns></returns>
		public static async Task<bool> FollowUnfollow(bool follow, string blogName) {
			return (follow ? await RequestService.POST(EndpointManager.FollowUser,
				string.Format("url={0}.tumblr.com", blogName)) : await RequestService.POST(EndpointManager.UnfollowUser,
				string.Format("url={0}.tumblr.com", blogName))
				).StatusCode == HttpStatusCode.OK;
		}

		/// <summary>
		/// Method tor retrieve followers associated with a blog
		/// </summary>
		/// <returns>List of blogs</returns>
		public static async Task<List<Blog>> RetrieveFollowers(int offset) {
			var requestResult = await RequestService.GET(EndpointManager.Followers, string.Format("offset={0}", offset));
			Debug.WriteLine(await requestResult.Content.ReadAsStringAsync());
			return (requestResult.StatusCode == HttpStatusCode.OK) ?
				JsonConvert.DeserializeObject<Responses.GetFollowers>(await requestResult.Content.ReadAsStringAsync())
				.response.users : new List<Blog>();
		}

		/// <summary>
		/// Method to retrieve blogs that are being followed
		/// </summary>
		/// <returns>List of blogs</returns>
		public static async Task<List<Blog>> RetrieveFollowing(int offset) {
			var requestResult = await RequestService.GET(EndpointManager.Following, string.Format("offset={0}", offset));
			return (requestResult.StatusCode == HttpStatusCode.OK) ?
				JsonConvert.DeserializeObject<Responses.GetFollowing>(await requestResult.Content.ReadAsStringAsync())
				.response.blogs : new List<Blog>();
		}

		public static async Task<List<Post>> RetrievePosts(string url, string lastPostID = "", string post_id = "") {
			var result = new HttpResponseMessage();
			if (!string.IsNullOrWhiteSpace(post_id)) {
				var request = string.Format(
				"https://api.tumblr.com/v2/blog/{0}.tumblr.com/posts?id={1}&notes_info=true&reblog_info=true&api_key={2}",
				UserUtils.CurrentBlog.Name, post_id, Authentication.ConsumerKey);
				result = await Client.GetAsync(new Uri(request));
				Debug.WriteLine(await result.Content.ReadAsStringAsync());
			} else if (url.Contains("/user/dashboard") || url.Contains("/submission") || url.Contains("/draft") || url.Contains("/queue")) {
				result = string.IsNullOrEmpty(lastPostID) ?
					await RequestService.GET(url) : await RequestService.GET(url, "max_id=" + lastPostID);
			} else if (url.Contains("/user/likes")) {
				result = string.IsNullOrEmpty(lastPostID) ?
					await RequestService.GET(url) : await RequestService.GET(url, "offset=" + lastPostID);
			} else if (url.Contains("/tagged")) {
				var searchTag = url.Split('?')[1];
				var newUrl = url.Split('?')[0].Replace("?", "");
				newUrl = newUrl + "?api_key=" + Authentication.ConsumerKey + "&" + searchTag +
					(!string.IsNullOrEmpty(lastPostID) ? "&before=" + lastPostID : "");

				result = await Client.GetAsync(new Uri(newUrl));
			} else {
				result = string.IsNullOrEmpty(lastPostID) ?
					await RequestService.GET(url, "api_key=" + Authentication.ConsumerKey) :
					await RequestService.GET(url, "offset=" + lastPostID + "&api_key=" + Authentication.ConsumerKey);
			}

			if (result.StatusCode == HttpStatusCode.OK) {
				try {
					var PostList = new List<Post>();
					var resultAsString = await result.Content.ReadAsStringAsync();
					if (url.Contains("/likes")) {
						PostList = JsonConvert.DeserializeObject<Responses.GetLikes>(resultAsString).response.liked_posts;
					} else if (url.Contains("/tagged")) {
						PostList = JsonConvert.DeserializeObject<Responses.GetTagged>(resultAsString).response;
					} else {
						var posts = JsonConvert.DeserializeObject<Responses.GetPosts>(resultAsString);
						PostList = posts.response.posts;
						if (url.Contains("/submission")) {
							foreach (var post in posts.response.posts) {
								if (post.type == "answer")
									post.body = post.question;
								post.type = "mail";
							}
						} else if (url.Contains("/draft") || url.Contains("/queue")) {
							foreach (var post in posts.response.posts)
								post.special_case = "draft";
						}
					}

					foreach (var p in PostList) {
						if (p.type == "photo") {
							if (p.path_to_low_res_pic.url.Contains(".gif")) {
								p.type = "gif";
							}
							if (p.photos.Count > 1) {
								p.type = "photoset";
								p.photoset_layout = p.photoset_layout.Replace('1', '6').Replace('2', '4').Replace("3", "222").Replace("4", "33");
								for (var i = 0; i < p.photoset_layout.Length; i++) {
									p.photos.ElementAt(i).ColSpan = int.Parse(p.photoset_layout.ElementAt(i).ToString());	//Set the column span for this object
								}
							}
						} else if (p.type == "video") {
							if (p.video_type == "youtube")
								p.type = "youtube";
						}
					}
					return PostList;
				} catch (Exception ex) {
				}
			} else {
			}
			return new List<Post>();
		}

		public static async Task<Blog> GetBlog(string name) {
			var requestResult = await RequestService.GET(string.Format(EndpointManager.Blog + "{0}.tumblr.com/info", name),
				"api_key=" + Authentication.ConsumerKey);
			Debug.WriteLine(await requestResult.Content.ReadAsStringAsync());
			return (requestResult.StatusCode == HttpStatusCode.OK) ?
				JsonConvert.DeserializeObject<Responses.GetBlog>(await requestResult.Content.ReadAsStringAsync())
				.response.blog : new Blog();
		}

		public static async Task<List<Blog>> BlogSearch(string tag) {
			var requestResult = await RequestService.GET(EndpointManager.Search + tag, "api_key=" + Authentication.ConsumerKey);
			return (requestResult.StatusCode == HttpStatusCode.OK) ?
				JsonConvert.DeserializeObject<Responses.GetSearch>(await requestResult.Content.ReadAsStringAsync())
				.response.blogs : new List<Blog>();
		}

		public static async Task<List<Responses.SpotlightResponse>> RetrieveSpotlight(bool forceRefresh = false) {
			if (string.IsNullOrEmpty(UserUtils.CachedSpotlight) || forceRefresh && UserUtils.CurrentBlog != null) {
				var response = await Client.GetAsync(new Uri(EndpointManager.Spotlight));
				var result = await response.Content.ReadAsStringAsync();

				if (result.Contains("status\":200")) {
					UserUtils.CachedSpotlight = result;
					return JsonConvert.DeserializeObject<Responses.GetSpotlight>(result).response;
				}
			} else
				return JsonConvert.DeserializeObject<Responses.GetSpotlight>(UserUtils.CachedSpotlight).response;
			return new List<Responses.SpotlightResponse>();
		}

		public static async Task<List<Responses.SpotlightResponse>> TagDiscovery(bool forceRefresh = false) {
			var response = await Client.GetAsync(new Uri(EndpointManager.TagDiscovery));
			var result = await response.Content.ReadAsStringAsync();

			return new List<Responses.SpotlightResponse>();
		}

		public static async Task<string> GetConvertedGIFUri(string gif) {
			var response = await Client.GetAsync(new Uri("http://upload.gfycat.com/transcode?fetchUrl=" + gif));
			var result = await response.Content.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<GfyCat>(result).mp4Url;
		}
	}
}
