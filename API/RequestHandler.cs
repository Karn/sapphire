using API.Content;
using API.Data;
using API.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace API {
    public class RequestHandler {

        const string TAG = "Client";
        private static HttpClient WebClient = new HttpClient();

        public static bool ReloadAccountData = false;

        public static bool CanRequestData() {
            if (NetworkInterface.GetIsNetworkAvailable()) {
                if (!string.IsNullOrEmpty(Config.OAuthToken))
                    return true;
            }
            return false;
        }

        public static string GetPlainTextFromHtml(string htmlString) {
            string htmlTagPattern = "<.*?>";
            var regexCss = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            htmlString = regexCss.Replace(htmlString, string.Empty);
            htmlString = Regex.Replace(htmlString, htmlTagPattern, string.Empty);
            htmlString = Regex.Replace(htmlString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
            htmlString = htmlString.Replace("&nbsp;", string.Empty);
            return System.Net.WebUtility.HtmlDecode(htmlString); ;
        }

        public static string UrlEncode(string value) {
            string reservedCharacters = "!*'();";
            if (String.IsNullOrEmpty(value))
                return String.Empty;

            var sb = new StringBuilder();

            foreach (char @char in value) {
                if (reservedCharacters.IndexOf(@char) == -1)
                    sb.Append(@char);
                else
                    sb.AppendFormat("%{0:X2}", (int)@char);
            }
            return sb.ToString();
        }

        public static async Task<bool> RetrieveAccountInformation() {
            if (RequestHandler.CanRequestData()) {
                //DebugHandler.Log("Retrieving account information", TAG);

                string requestResult = await RequestBuilder.GetAPI("https://api.tumblr.com/v2/user/info");

                if (requestResult.Contains("200")) {
                    try {
                        var parsedData = JsonConvert.DeserializeObject<Responses.GetInfo>(requestResult);

                        UserData.UserBlogs.Clear();
                        if (ReloadAccountData) {
                            UserData.CurrentBlog = null;
                            ReloadAccountData = false;
                        }
                        foreach (var b in parsedData.response.user.blogs) {
                            Debug.WriteLine(b.Name);
                            b.following = parsedData.response.user.following.ToString();
                            b.likes = parsedData.response.user.likes;

                            UserData.UserBlogs.Add(b);
                            if (UserData.CurrentBlog == null && b.primary)
                                UserData.CurrentBlog = b;
                            else if (b.Name == UserData.CurrentBlog.Name)
                                UserData.CurrentBlog = b;
                        }
                        return true;
                    } catch (Exception e) {
                        DebugHandler.Error("Failed to deserialse userdata.", e.StackTrace);
                    }
                }
                DebugHandler.Error("Userdata response failed.", requestResult);
            }
            return false;
        }

        /// <summary>
        /// Method to change status of post to 'Liked'
        /// </summary>
        /// <param name="id">The post's unique ID</param>
        /// <param name="reblogKey">The key used to handle reblogging/liking this post</param>
        /// <returns>Boolean to indicate if the request was completed</returns>
        public static async Task<bool> LikePost(string id, string reblogKey) {
            string requestResult = await RequestBuilder.GetAPI("https://api.tumblr.com/v2/user/like", "id=" + id + "&reblog_key=" + reblogKey);
            if (requestResult.Contains("200"))
                return true;
            return false;
        }

        /// <summary>
        /// Method to change status of post to 'Liked'
        /// </summary>
        /// <param name="id">The post's unique ID</param>
        /// <param name="reblogKey">The key used to handle reblogging/liking this post</param>
        /// <returns>Boolean to indicate if the request was completed</returns>
        public static async Task<bool> UnlikePost(string id, string reblogKey) {
            string requestResult = await RequestBuilder.GetAPI("https://api.tumblr.com/v2/user/unlike", "id=" + id + "&reblog_key=" + reblogKey);
            if (requestResult.Contains("200"))
                return true;
            return false;
        }

        /// <summary>
        /// Method to reblog a post
        /// </summary>
        /// <param name="id">The post's unique ID</param>
        /// <param name="reblogKey">The key used to handle reblogging/liking this post</param>
        /// <returns>Boolean to indicate if the request was completed</returns>
        public async static Task<bool> ReblogPost(string id, string reblogKey) {
            string requestResult = await RequestBuilder.PostAPI("http://api.tumblr.com/v2/blog/" + UserData.CurrentBlog.Name + ".tumblr.com/post/reblog", "id=" + id + "&reblog_key=" + reblogKey);
            if (requestResult.Contains("201"))
                return true;
            return false;
        }

        /// <summary>
        /// Method to delete a post
        /// </summary>
        /// <param name="id">The post's unique ID</param>
        /// <param name="reblogKey">The key used to handle reblogging/liking this post</param>
        /// <returns>Boolean to indicate if the request was completed</returns>
        public async static Task<bool> DeletePost(string id) {
            string requestResult = await RequestBuilder.PostAPI("http://api.tumblr.com/v2/blog/" + UserData.CurrentBlog.Name + ".tumblr.com/post/delete", "id=" + id);
            if (requestResult.Contains("200"))
                return true;
            return false;
        }

        /// <summary>
        /// Method to follow or unfollow a blog
        /// </summary>
        /// <param name="follow">Value to indicate whether the blog is to be followed (true) or not</param>
        /// <param name="blogName">The name of the blog that is affectd by the API call</param>
        /// <returns></returns>
        public static async Task<bool> FollowUnfollow(bool follow, string blogName) {
            string requestResult = (follow ? await RequestBuilder.PostAPI("https://api.tumblr.com/v2/user/follow", "url=" + blogName + ".tumblr.com") : await RequestBuilder.PostAPI("https://api.tumblr.com/v2/user/unfollow", "url=" + blogName + ".tumblr.com"));
            if (requestResult.Contains("200"))
                return true;
            return false;
        }

        /// <summary>
        /// Method tor retrieve followers associated with a blog
        /// </summary>
        /// <returns>List of blogs</returns>
        public static async Task<List<Blog>> RetrieveFollowers(int offset) {
            //DebugHandler.Log("Retrieving followers", TAG);
            var requestResult = await RequestBuilder.GetAPI("http://api.tumblr.com/v2/blog/" + UserData.CurrentBlog.Name + ".tumblr.com/followers", "offset=" + offset);

            if (requestResult.Contains("200")) {
                //DebugHandler.Log("Followers retrieved", TAG);
                var parsedData = JsonConvert.DeserializeObject<Responses.GetFollowers>(requestResult);
                try {
                    List<Blog> converted = new List<Blog>();
                    foreach (Blog.AltBlog x in parsedData.response.users) {
                        Blog b = new Blog();
                        b.Name = x.Name;
                        b.IsFollowing = x.following;
                        converted.Add(b);
                    }
                    return converted;
                } catch (Exception e) {
                    DebugHandler.Error("Failed to deserialize followers list.", e.StackTrace);
                }
            }
            //DebugHandler.ErrorLog.Add("[Client.cs]: Authorization failed: " + Uri.EscapeDataString(requestResult));
            return new List<Blog>();
        }

        /// <summary>
        /// Metho to retrieve blogs that are being followed
        /// </summary>
        /// <returns>List of blogs</returns>
        public static async Task<List<Blog>> RetrieveFollowing(int offset) {
            //DebugHandler.Log("Retrieving Following", TAG);
            string requestResult = await RequestBuilder.GetAPI("https://api.tumblr.com/v2/user/following", "offset=" + offset);

            if (requestResult.Contains("200")) {
                DebugHandler.Info("Response OK.", "Client");
                Responses.GetFollowing following = JsonConvert.DeserializeObject<Responses.GetFollowing>(requestResult);
                try {
                    List<Blog> loadedBlogs = new List<Blog>();

                    foreach (Blog b in following.response.blogs) {
                        b.IsFollowing = true;
                        loadedBlogs.Add(b);
                    }

                    return loadedBlogs;
                } catch (Exception e) {
                    DebugHandler.Error("[Client.cs]: Failed to deserialize following list.", e.StackTrace);
                }
            }
            DebugHandler.Error("[Client.cs]: Authorization failed: ", requestResult);
            return new List<Blog>();
        }


        /// <summary>
        /// This needs to be moved so that background task and activity feed can be loaded from a central source
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Content.Activity.Notification>> RetrieveActivity() {

            string Response = await RequestBuilder.GetAPI("https://api.tumblr.com/v2/user/notifications");
            if (Response.Contains("200")) {
                try {
                    Responses.GetActivity activity = JsonConvert.DeserializeObject<Responses.GetActivity>(Response);
                    var Notifications = new List<Content.Activity.Notification>();
                    var NotificationDictionary = UserData.RetrieveNotificationIds;

                    foreach (var b in activity.response.blogs) {
                        if (UserData.CurrentBlog != null) {
                            if (b.blog_name.ToLower() == UserData.CurrentBlog.Name.ToLower()) {
                                if (!NotificationDictionary.ContainsKey(b.blog_name)) {
                                    NotificationDictionary.Add(b.blog_name, 0);
                                }
                                foreach (var n in b.notifications) {
                                    n.date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(n.timestamp).ToString("yyyy'-'MM'-'dd");
                                    Notifications.Add(n);
                                }
                                NotificationDictionary[b.blog_name] = Notifications.First().timestamp;
                            }
                        } else {
                            Debug.WriteLine("No current blog set");
                        }
                    }

                    UserData.RetrieveNotificationIds = NotificationDictionary;
                    return Notifications;
                } catch (Exception e) {
                    DebugHandler.Error("Unable to serialize activity data.", e.StackTrace);
                }
            }
            DebugHandler.Error("[Client.cs]: Authorization failed: ", Response);
            return new List<Content.Activity.Notification>();
        }

        public static async Task<List<Content.Post>> RetrievePosts(string url, string lastPostID = "", string optionalParams = "") {
            var LoadedPosts = new List<Content.Post>();
            if (CanRequestData()) {
                //DebugHandler.Log("Retreiving posts", TAG);
                string result = string.Empty;
                Debug.WriteLine(url);
                //Segment API calls
                if (url.Contains("/user") || url.Contains("/submission")) {
                    if (string.IsNullOrEmpty(lastPostID))
                        result = await RequestBuilder.GetAPI(url);
                    else
                        result = await RequestBuilder.GetAPI(url, "before_id=" + lastPostID);
                } else if (url.Contains("/tagged")) {
                    if (string.IsNullOrEmpty(lastPostID))
                        result = url + "&api_key=" + Config.ConsumerKey;
                    else
                        result = url + "&before_id=" + lastPostID + "&api_key=" + Config.ConsumerKey;

                    var response = await WebClient.GetAsync(new Uri(result));
                    result = await response.Content.ReadAsStringAsync();
                } else {
                    string URI = string.Empty;
                    if (string.IsNullOrEmpty(lastPostID))
                        URI = url + "?api_key=" + Config.ConsumerKey;
                    else
                        URI = url + "?before_id=" + lastPostID + "&api_key=" + Config.ConsumerKey;

                    var response = await WebClient.GetAsync(new Uri(URI));
                    result = await response.Content.ReadAsStringAsync();
                }

                if (!string.IsNullOrEmpty(result) && result.Contains("200")) {
                    DebugHandler.Info("[Client.cs]: Response OK.");

                    try {
                        var PostList = new List<API.Content.Post>();

                        if (url.Contains("/likes")) {
                            var posts = JsonConvert.DeserializeObject<Content.Responses.GetLikes>(result);
                            PostList = posts.response.liked_posts;
                        } else if (url.Contains("/tagged")) {
                            var posts = JsonConvert.DeserializeObject<Content.Responses.GetTagged>(result);
                            PostList = posts.response;
                        } else {
                            var posts = JsonConvert.DeserializeObject<Content.Responses.GetPosts>(result);
                            PostList = posts.response.posts;
                        }

                        foreach (var p in PostList) {
                            if (p.type == "photo") {
                                p.path_to_low_res_pic = p.photos.First().alt_sizes.First().url;
                                if (p.path_to_low_res_pic.Contains(".gif")) {
                                    p.type = "gif";
                                    var x = p.photos.First<Content.Photo>().alt_sizes.First<Content.Photo.AltSize>();
                                    p.pic_height = (x.height / x.width) * 380;
                                }
                                if (p.photos.Count > 1)
                                    p.type = "photoset";
                            }

                            //Parse images here
                            if (!string.IsNullOrEmpty(p.title))
                                p.title = RequestHandler.GetPlainTextFromHtml(p.title);
                            if (!string.IsNullOrEmpty(p.body))
                                p.body = RequestHandler.GetPlainTextFromHtml(p.body);
                            if (!string.IsNullOrEmpty(p.answer))
                                p.answer = RequestHandler.GetPlainTextFromHtml(p.answer);

                            LoadedPosts.Add(p);
                        }
                        if (LoadedPosts.Count == 0) {
                            DebugHandler.Error("[Client.cs]: Failed to add loaded posts.", "");
                        }
                        return LoadedPosts;
                    } catch (Exception e) {
                        DebugHandler.Error("[Client.cs]: Unable to serialize post data.", e.StackTrace);
                    }
                } else {
                    DebugHandler.Error("[Client.cs]: Authorization failed. ", result);
                }
            }
            LoadedPosts.Add(new Post() { type = "nocontent" });
            return LoadedPosts;
        }

        public static async Task<ObservableCollection<Content.Post>> RetrievePost(string post_id) {
            string UserInfoURI = "http://api.tumblr.com/v2/blog/" + UserData.CurrentBlog.Name + ".tumblr.com/posts?id=" + post_id + "&notes_info=true&api_key=" + Config.ConsumerKey;
            var response = await WebClient.GetAsync(new Uri(UserInfoURI));
            var result = await response.Content.ReadAsStringAsync();

            if (result.Contains("200")) {
                try {
                    var LoadedPosts = new ObservableCollection<Content.Post>();

                    var posts = JsonConvert.DeserializeObject<Content.Responses.GetPosts>(result);

                    Debug.WriteLine(posts.response.posts.ElementAt(0).note_count);

                    foreach (var p in posts.response.posts) {
                        if (p.type == "photo") {
                            p.path_to_low_res_pic = p.photos.First<Content.Photo>().alt_sizes.First<Content.Photo.AltSize>().url;
                            if (p.path_to_low_res_pic.Contains(".gif")) {
                                var x = p.photos.First<Content.Photo>().alt_sizes.First<Content.Photo.AltSize>();
                                p.type = "gif";
                                p.pic_height = (x.height / x.width) * 380;
                            }
                            if (p.photos.Count > 1) {
                                p.type = "photoset";
                            }
                        }
                        if (p.body != null)
                            p.body = RequestHandler.GetPlainTextFromHtml(p.body);
                        if (p.answer != null)
                            p.answer = RequestHandler.GetPlainTextFromHtml(p.answer);
                        LoadedPosts.Add(p);
                    }
                    return LoadedPosts;
                } catch (Exception e) {
                    DebugHandler.Error("[Client.cs]: Unable to serialize post data.", e.StackTrace);
                }
            }
            DebugHandler.Error("[Client.cs]: Authorization failed. ", result);
            return new ObservableCollection<Content.Post>();
        }

        public static async Task<Content.Blog> GetBlog(string name) {
            DebugHandler.Info("[Client.cs]: Retrieving blog...");
            string UserInfoURI = "http://api.tumblr.com/v2/blog/" + name + ".tumblr.com/info?api_key=" + Config.APIKey;

            var response = await WebClient.GetAsync(new Uri(UserInfoURI));
            var result = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(result);
            if (result.Contains("200")) {
                Content.Responses.GetBlog blogInfo = JsonConvert.DeserializeObject<Content.Responses.GetBlog>(result);

                return blogInfo.response.blog;
            }
            var x = result.Split(',');
            DebugHandler.Error("[Client.cs]: Authorization failed. ", result);
            return new Content.Blog();
        }

        public static async Task<List<Content.Blog>> RetrieveSearch(string tag) {
            string UserInfoURI = "http://api.tumblr.com/v2/search/" + tag + "?api_key=" + Config.APIKey;

            var response = await WebClient.GetAsync(new Uri(UserInfoURI));
            var result = await response.Content.ReadAsStringAsync();

            if (result.Contains("200")) {
                Content.Responses.GetSearch SearchResult = JsonConvert.DeserializeObject<Content.Responses.GetSearch>(result);

                return SearchResult.response.blogs;
            }

            return new List<Content.Blog>();
        }

        public static async Task<List<Responses.SpotlightResponse>> RetrieveSpotlight() {
            string requestString = "http://api.tumblr.com/v2/spotlight/directories?api_key=" + Config.APIKey;

            var response = await WebClient.GetAsync(new Uri(requestString));
            var result = await response.Content.ReadAsStringAsync();

            if (result.Contains("200")) {
                Content.Responses.GetSpotlight spotlight = JsonConvert.DeserializeObject<Content.Responses.GetSpotlight>(result);
                return spotlight.response;
            }

            return new List<Responses.SpotlightResponse>();
        }

        public static async Task<bool> CreatePost(string parameters) {
            if (!string.IsNullOrEmpty(parameters)) {
                string result = await RequestBuilder.PostAPI("http://api.tumblr.com/v2/blog/" + UserData.CurrentBlog.Name + ".tumblr.com/post", parameters);
                if (result.Contains("201"))
                    return true;
            }
            return false;
        }
    }
}
