using APIWrapper.Content;
using APIWrapper.Content.Model;
using APIWrapper.Utils;
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

namespace APIWrapper.Client {
    public class CreateRequest {

        private static string TAG = "CreateRequest";

        private static HttpClient WebClient = new HttpClient();

        public static bool ReloadAccountData = false;

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
            string requestResult = await RequestBuilder.GetAPI("https://api.tumblr.com/v2/user/info");

            if (requestResult.Contains("status\":200")) {
                try {
                    var parsedData = JsonConvert.DeserializeObject<Responses.GetInfo>(requestResult);

                    UserStore.UserBlogs.Clear();
                    if (ReloadAccountData) {
                        UserStore.CurrentBlog = null;
                        ReloadAccountData = false;
                    }
                    foreach (var b in parsedData.response.user.blogs) {
                        Debug.WriteLine(b.Name);
                        b.followingCount = parsedData.response.user.following;
                        b.likes = parsedData.response.user.likes;

                        UserStore.UserBlogs.Add(b);
                        if (UserStore.CurrentBlog == null && b.primary)
                            UserStore.CurrentBlog = b;
                        else if (b.Name == UserStore.CurrentBlog.Name)
                            UserStore.CurrentBlog = b;
                    }
                    return true;
                } catch (Exception ex) {
                    DiagnosticsManager.LogException(ex, TAG, "Failed to serailize account information.");
                }
            }
            DiagnosticsManager.LogException(null, TAG, "Unable to retreive account information.");
            return false;
        }

        /// <summary>
        /// Method to change status of post to 'Liked'
        /// </summary>
        /// <param name="id">The post's unique ID</param>
        /// <param name="reblogKey">The key used to handle reblogging/liking this post</param>
        /// <returns>Boolean to indicate if the request was completed</returns>
        public static async Task<bool> LikePost(string id, string reblogKey) {
            var requestResult = await RequestBuilder.GetAPI(APIEndpoints.LikePost, "id=" + id + "&reblog_key=" + reblogKey);
            if (requestResult.Contains("status\":200"))
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
            var requestResult = await RequestBuilder.GetAPI(APIEndpoints.UnlikePost, "id=" + id + "&reblog_key=" + reblogKey);
            if (requestResult.Contains("status\":200"))
                return true;
            return false;
        }

        /// <summary>
        /// Method to reblog a post
        /// </summary>
        /// <param name="id">The post's unique ID</param>
        /// <param name="reblogKey">The key used to handle reblogging/liking this post</param>
        /// <returns>Boolean to indicate if the request was completed</returns>
        public async static Task<bool> ReblogPost(string id, string reblogKey, string caption = "", string tags = "") {
            var requestResult = await RequestBuilder.PostAPI(APIEndpoints.ReblogPost, "id=" + id + "&reblog_key=" + reblogKey + (!string.IsNullOrEmpty(caption) ? "&comment=" + caption : "") + (!string.IsNullOrEmpty(caption) ? "&tags=" + tags : ""));
            if (requestResult.Contains("status\":201"))
                return true;
            return false;
        }
        public async static Task<bool> PostDraft(string id) {
            var requestResult = await RequestBuilder.PostAPI(APIEndpoints.EditPost, "state=published&id=" + id);
            if (requestResult.Contains("status\":200"))
                return true;
            return false;
        }

        /// <summary>
        /// Method to delete a post
        /// </summary>
        /// <param name="id">The post's unique ID</param>
        /// <returns>Boolean to indicate if the request was completed</returns>
        public async static Task<bool> DeletePost(string id) {
            string requestResult = await RequestBuilder.PostAPI(APIEndpoints.DeletePost, "id=" + id);
            if (requestResult.Contains("status\":200"))
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
            string requestResult = (follow ? await RequestBuilder.PostAPI(APIEndpoints.FollowUser, "url=" + blogName + ".tumblr.com") : await RequestBuilder.PostAPI(APIEndpoints.UnfollowUser, "url=" + blogName + ".tumblr.com"));
            if (requestResult.Contains("status\":200"))
                return true;
            return false;
        }

        /// <summary>
        /// Method tor retrieve followers associated with a blog
        /// </summary>
        /// <returns>List of blogs</returns>
        public static async Task<List<Blog>> RetrieveFollowers(int offset) {
            var requestResult = await RequestBuilder.GetAPI(APIEndpoints.Followers, "offset=" + offset);

            if (requestResult.Contains("status\":200")) {
                var parsedData = JsonConvert.DeserializeObject<Responses.GetFollowers>(requestResult);
                return parsedData.response.users;
            }
            return new List<Blog>();
        }

        /// <summary>
        /// Metho to retrieve blogs that are being followed
        /// </summary>
        /// <returns>List of blogs</returns>
        public static async Task<List<Blog>> RetrieveFollowing(int offset) {
            var requestResult = await RequestBuilder.GetAPI(APIEndpoints.Following, "offset=" + offset);

            if (requestResult.Contains("status\":200")) {
                var following = JsonConvert.DeserializeObject<Responses.GetFollowing>(requestResult);
                foreach (var x in following.response.blogs)
                    x.following = true;
                return following.response.blogs;
            }
            return new List<Blog>();
        }


        /// <summary>
        /// This needs to be moved so that background task and activity feed can be loaded from a central source
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Activity.Notification>> RetrieveActivity() {

            string response = await RequestBuilder.GetAPI(APIEndpoints.Notifications);
            if (response.Contains("status\":200")) {
                try {
                    Responses.GetActivity activity = JsonConvert.DeserializeObject<Responses.GetActivity>(response);
                    var Notifications = new List<APIWrapper.Content.Model.Activity.Notification>();
                    var NotificationDictionary = UserStore.NotificationIDs;

                    foreach (var b in activity.response.blogs) {
                        if (UserStore.CurrentBlog != null) {
                            if (b.blog_name.ToLower() == UserStore.CurrentBlog.Name.ToLower()) {
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

                    UserStore.NotificationIDs = NotificationDictionary;
                    return Notifications;
                } catch (Exception ex) {
                    DiagnosticsManager.LogException(ex, TAG, "Failed to serailize blog activity.");
                }
            }
            DiagnosticsManager.LogException(null, TAG, "Failed to serailize account information.");
            return new List<Activity.Notification>();
        }

        public static async Task<List<Post>> RetrievePosts(string url, string lastPostID = "", string optionalParams = "") {
            var LoadedPosts = new List<Post>();
            if (AuthenticationManager.Authentication.Utils.NetworkAvailable()) {
                string result = string.Empty;
                Debug.WriteLine(url);
                if (url.Contains("/user/dashboard") || url.Contains("/submission") || url.Contains("/draft") || url.Contains("/queue")) {
                    if (string.IsNullOrEmpty(lastPostID))
                        result = await RequestBuilder.GetAPI(url);
                    else
                        result = await RequestBuilder.GetAPI(url, "max_id=" + lastPostID);
                } else if (url.Contains("/user/likes")) {
                    if (string.IsNullOrEmpty(lastPostID))
                        result = await RequestBuilder.GetAPI(url);
                    else
                        result = await RequestBuilder.GetAPI(url, "offset=" + lastPostID);
                } else if (url.Contains("/tagged")) {
                    var searchTag = url.Split('?')[1];
                    var newUrl = url.Split('?')[0].Replace("?", "");
                    if (string.IsNullOrEmpty(lastPostID))
                        newUrl = newUrl + "?api_key=" + APIWrapper.AuthenticationManager.Authentication.ConsumerKey + "&" + searchTag;
                    else
                        newUrl = newUrl + "?api_key=" + APIWrapper.AuthenticationManager.Authentication.ConsumerKey + "&" + searchTag + "&before=" + lastPostID;
                    Debug.WriteLine(newUrl);
                    var response = await WebClient.GetAsync(new Uri(newUrl));
                    result = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(result);
                } else {
                    if (string.IsNullOrEmpty(lastPostID))
                        result = await RequestBuilder.GetAPI(url, "api_key=" + APIWrapper.AuthenticationManager.Authentication.ConsumerKey);
                    else
                        result = await RequestBuilder.GetAPI(url, "offset=" + lastPostID + "&api_key=" + APIWrapper.AuthenticationManager.Authentication.ConsumerKey);
                }

                if (!string.IsNullOrEmpty(result) && result.Contains("status\":200")) {
                    try {
                        var PostList = new List<Post>();

                        if (url.Contains("/likes")) {
                            var posts = JsonConvert.DeserializeObject<Responses.GetLikes>(result);
                            PostList = posts.response.liked_posts;
                        } else if (url.Contains("/tagged")) {
                            var posts = JsonConvert.DeserializeObject<Responses.GetTagged>(result);
                            PostList = posts.response;
                        } else {
                            var posts = JsonConvert.DeserializeObject<Responses.GetPosts>(result);
                            PostList = posts.response.posts;
                            if (url.Contains("/submission")) {
                                foreach (var post in posts.response.posts) {
                                    if (post.type == "answer")
                                        post.body = post.question;
                                    post.type = "mail";
                                }
                            } else if (url.Contains("/draft") || url.Contains("/queue")) {
                                foreach (var post in posts.response.posts) {
                                    post.special_case = "draft";
                                }
                            }
                        }

                        foreach (var p in PostList) {
                            if (p.type == "photo") {
                                p.path_to_low_res_pic = p.photos.First().alt_sizes.First().url;
                                if (p.path_to_low_res_pic.Contains(".gif")) {
                                    p.type = "gif";
                                }
                                if (p.photos.Count > 1)
                                    p.type = "photoset";
                            } else if (p.type == "video") {
                                if (p.video_type == "youtube")
                                    p.type = "youtube";
                            }

                            //Parse images here
                            if (!string.IsNullOrEmpty(p.title))
                                p.title = CreateRequest.GetPlainTextFromHtml(p.title);
                            if (!string.IsNullOrEmpty(p.body))
                                p.body = CreateRequest.GetPlainTextFromHtml(p.body);
                            if (!string.IsNullOrEmpty(p.answer))
                                p.answer = CreateRequest.GetPlainTextFromHtml(p.answer);

                            LoadedPosts.Add(p);
                        }
                        if (LoadedPosts.Count == 0) {
                            //DebugHandler.Error("[Client.cs]: Failed to add loaded posts.", "");
                        }
                        return LoadedPosts;
                    } catch (Exception ex) {
                        DiagnosticsManager.LogException(ex, TAG, "Failed to serailize posts.");
                    }
                } else {
                    DiagnosticsManager.LogException(null, TAG, "Failed to retrieve posts.");
                }
            }
            LoadedPosts.Add(new Post() { type = "nocontent" });
            return LoadedPosts;
        }

        public static async Task<ObservableCollection<Post>> RetrievePost(string post_id) {
            string UserInfoURI = "https://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/posts?id=" + post_id + "&notes_info=true&api_key=" + APIWrapper.AuthenticationManager.Authentication.ConsumerKey;
            var response = await WebClient.GetAsync(new Uri(UserInfoURI));
            var result = await response.Content.ReadAsStringAsync();

            if (result.Contains("status\":200")) {
                try {
                    var LoadedPosts = new ObservableCollection<Post>();

                    var posts = JsonConvert.DeserializeObject<Responses.GetPosts>(result);

                    foreach (var p in posts.response.posts) {
                        if (p.type == "photo") {
                            p.path_to_low_res_pic = p.photos.First().alt_sizes.First().url;
                            if (p.path_to_low_res_pic.Contains(".gif")) {
                                p.type = "gif";
                            }
                            if (p.photos.Count > 1)
                                p.type = "photoset";
                        } else if (p.type == "video") {
                            if (p.video_type == "youtube")
                                p.type = "youtube";
                        }

                        //Parse images here
                        if (!string.IsNullOrEmpty(p.title))
                            p.title = CreateRequest.GetPlainTextFromHtml(p.title);
                        if (!string.IsNullOrEmpty(p.body))
                            p.body = CreateRequest.GetPlainTextFromHtml(p.body);
                        if (!string.IsNullOrEmpty(p.answer))
                            p.answer = CreateRequest.GetPlainTextFromHtml(p.answer);

                        LoadedPosts.Add(p);
                    }
                    return LoadedPosts;
                } catch (Exception ex) {
                    DiagnosticsManager.LogException(ex, TAG, "Failed to serailize single post.");
                }
            }
            DiagnosticsManager.LogException(null, TAG, "Failed to retrieve single post.");
            return new ObservableCollection<Post>();
        }

        public static async Task<Blog> GetBlog(string name) {
            var result = await RequestBuilder.GetAPI(APIEndpoints.Blog + name + ".tumblr.com/info", "api_key=" + APIWrapper.AuthenticationManager.Authentication.ConsumerKey);
            if (result.Contains("status\":200")) {
                return JsonConvert.DeserializeObject<Responses.GetBlog>(result).response.blog;
            }
            return new Blog();
        }

        public static async Task<List<Blog>> RetrieveSearch(string tag) {
            var result = await RequestBuilder.GetAPI(APIEndpoints.Search + tag, "api_key=" + APIWrapper.AuthenticationManager.Authentication.ConsumerKey);
            if (result.Contains("status\":200")) {
                return JsonConvert.DeserializeObject<Responses.GetSearch>(result).response.blogs;
            }
            return new List<Blog>();
        }

        public static async Task<List<Responses.SpotlightResponse>> RetrieveSpotlight(bool forceRefresh = false) {
            if (string.IsNullOrEmpty(UserStore.CachedSpotlight) || forceRefresh) {
                var response = await WebClient.GetAsync(new Uri(APIEndpoints.Spotlight));
                var result = await response.Content.ReadAsStringAsync();

                Debug.WriteLine(result);

                if (result.Contains("status\":200")) {
                    UserStore.CachedSpotlight = result;
                    return JsonConvert.DeserializeObject<Responses.GetSpotlight>(result).response;
                }
            } else {
                Responses.GetSpotlight spotlight = JsonConvert.DeserializeObject<Responses.GetSpotlight>(UserStore.CachedSpotlight);
                return spotlight.response;
            }

            return new List<Responses.SpotlightResponse>();
        }

        public static async Task<bool> CreatePost(string parameters) {
            if (!string.IsNullOrEmpty(parameters)) {
                string result = await RequestBuilder.PostAPI(APIEndpoints.Post, parameters);
                if (result.Contains("status\":201"))
                    return true;
            }
            return false;
        }

        public static async Task<bool> CreateMediaPost(string parameters, string media) {
            Debug.WriteLine("media");
            //if (!string.IsNullOrEmpty(parameters)) {
            //    string result = await RequestBuilder.PostAPIWithData("http://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/post", parameters, media);
            //    if (result.Contains("status\":201"))
            //        return true;
            //}
            return false;
        }
    }
}
