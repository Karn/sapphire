using APIWrapper.AuthenticationManager;
using APIWrapper.Content;
using APIWrapper.Content.Model;
using APIWrapper.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APIWrapper.Client {
    public class CreateRequest {

        private static string TAG = "Request";

        private static HttpClient WebClient = new HttpClient();

        public static bool ReloadAccountData = false;

        public static string GetPlainTextFromHtml(string htmlString) {
            if (htmlString != null) {
                string htmlTagPattern = "<.*?>";
                var regexCss = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                htmlString = regexCss.Replace(htmlString, string.Empty);
                htmlString = Regex.Replace(htmlString, htmlTagPattern, string.Empty);
                htmlString = Regex.Replace(htmlString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                htmlString = htmlString.Replace("&nbsp;", string.Empty);
                return WebUtility.HtmlDecode(htmlString);
            }
            return htmlString;
        }

        public static string UrlEncode(string value) {
            string reservedCharacters = "-._~";
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

        public static async Task<bool> RetrieveAccountInformation(string account = "") {
            var requestResult = await RequestHandler.GET("https://api.tumblr.com/v2/user/info");

            if (requestResult.StatusCode == HttpStatusCode.OK) {
                try {
                    var parsedData = JsonConvert.DeserializeObject<Responses.GetInfo>(await requestResult.Content.ReadAsStringAsync());

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
                        if (string.IsNullOrWhiteSpace(account)) {
                            if (UserStore.CurrentBlog == null && b.primary)
                                UserStore.CurrentBlog = b;
                            else if (b.Name == UserStore.CurrentBlog.Name)
                                UserStore.CurrentBlog = b;
                        } else {
                            if (b.Name == account)
                                UserStore.CurrentBlog = b;
                        }
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
            return (await RequestHandler.GET(Endpoints.LikePost,
                string.Format("id={0}&reblog_key={1}", id, reblogKey)
                )).StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Method to change status of post to 'Liked'
        /// </summary>
        /// <param name="id">The post's unique ID</param>
        /// <param name="reblogKey">The key used to handle reblogging/liking this post</param>
        /// <returns>Boolean to indicate if the request was completed</returns>
        public static async Task<bool> UnlikePost(string id, string reblogKey) {
            return (await RequestHandler.GET(Endpoints.UnlikePost,
                string.Format("id={0}&reblog_key={1}", id, reblogKey)
                )).StatusCode == HttpStatusCode.OK;
        }

        public static async Task<HttpResponseMessage> CreatePost(string parameters) {
            return await RequestHandler.POST(Endpoints.Post, parameters);
        }

        /// <summary>
        /// Method to reblog a post
        /// </summary>
        /// <param name="id">The post's unique ID</param>
        /// <param name="reblogKey">The key used to handle reblogging/liking this post</param>
        /// <returns>Boolean to indicate if the request was completed</returns>
        public async static Task<bool> ReblogPost(string id, string reblogKey, string caption = "", string tags = "", string additionalParameters = "") {
            return (await RequestHandler.POST(Endpoints.ReblogPost,
                (string.Format("id={0}&reblog_key={1}", id, reblogKey) +
                (!string.IsNullOrEmpty(caption) ? "&comment=" + caption : "") +
                (!string.IsNullOrEmpty(tags) ? "&tags=" + tags : "") +
                (!string.IsNullOrEmpty(additionalParameters) ? additionalParameters : "")
                ))).StatusCode == HttpStatusCode.Created;

        }

        public async static Task<bool> PostDraft(string id) {
            return (await RequestHandler.POST(Endpoints.EditPost,
                string.Format("state=published&id={0}", id)
                )).StatusCode == HttpStatusCode.Created;
        }

        /// <summary>
        /// Method to delete a post
        /// </summary>
        /// <param name="id">The post's unique ID</param>
        /// <returns>Boolean to indicate if the request was completed</returns>
        public async static Task<bool> DeletePost(string id) {
            return (await RequestHandler.POST(Endpoints.DeletePost,
                string.Format("state=published&id={0}", id)
                )).StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Method to follow or unfollow a blog
        /// </summary>
        /// <param name="follow">Value to indicate whether the blog is to be followed (true) or not</param>
        /// <param name="blogName">The name of the blog that is affectd by the API call</param>
        /// <returns></returns>
        public static async Task<bool> FollowUnfollow(bool follow, string blogName) {
            return (follow ? await RequestHandler.POST(Endpoints.FollowUser,
                string.Format("url={0}.tumblr.com", blogName)) : await RequestHandler.POST(Endpoints.UnfollowUser,
                string.Format("url={0}.tumblr.com", blogName))
                ).StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Method tor retrieve followers associated with a blog
        /// </summary>
        /// <returns>List of blogs</returns>
        public static async Task<List<Blog>> RetrieveFollowers(int offset) {
            var requestResult = await RequestHandler.GET(Endpoints.Followers, string.Format("offset={0}", offset));
            return (requestResult.StatusCode == HttpStatusCode.OK) ?
                JsonConvert.DeserializeObject<Responses.GetFollowers>(await requestResult.Content.ReadAsStringAsync())
                .response.users : new List<Blog>();
        }

        /// <summary>
        /// Method to retrieve blogs that are being followed
        /// </summary>
        /// <returns>List of blogs</returns>
        public static async Task<List<Blog>> RetrieveFollowing(int offset) {
            var requestResult = await RequestHandler.GET(Endpoints.Following, string.Format("offset={0}", offset));
            return (requestResult.StatusCode == HttpStatusCode.OK) ?
                JsonConvert.DeserializeObject<Responses.GetFollowing>(await requestResult.Content.ReadAsStringAsync())
                .response.blogs : new List<Blog>();
        }


        /// <summary>
        /// This needs to be moved so that background task and activity feed can be loaded from a central source
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Activity.Notification>> RetrieveActivity() {

            var result = await RequestHandler.GET(Endpoints.Notifications);
            if (result.StatusCode == HttpStatusCode.OK) {
                try {
                    Responses.GetActivity activity = JsonConvert.DeserializeObject<Responses.GetActivity>(await result.Content.ReadAsStringAsync());
                    var Notifications = new List<Activity.Notification>();
                    var NotificationDictionary = UserStore.NotificationIDs;

                    foreach (var b in activity.response.blogs) {
                        if (UserStore.CurrentBlog != null) {
                            if (b.blog_name.ToLower() == UserStore.CurrentBlog.Name.ToLower()) {
                                if (!NotificationDictionary.ContainsKey(b.blog_name)) {
                                    NotificationDictionary.Add(b.blog_name, 0);
                                }
                                foreach (var n in b.notifications) {
                                    n.date = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(n.timestamp).ToString("yyyy'-'MM'-'dd");
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

        public static async Task<List<Post>> RetrievePosts(string url, string lastPostID = "") {
            if (Authentication.Utils.NetworkAvailable()) {
                var result = new HttpResponseMessage();
                if (url.Contains("/user/dashboard") || url.Contains("/submission") || url.Contains("/draft") || url.Contains("/queue")) {
                    result = string.IsNullOrEmpty(lastPostID) ?
                        result = await RequestHandler.GET(url) : await RequestHandler.GET(url, "max_id=" + lastPostID);
                } else if (url.Contains("/user/likes")) {
                    result = string.IsNullOrEmpty(lastPostID) ?
                        await RequestHandler.GET(url) : await RequestHandler.GET(url, "offset=" + lastPostID);
                } else if (url.Contains("/tagged")) {
                    var searchTag = url.Split('?')[1];
                    var newUrl = url.Split('?')[0].Replace("?", "");
                    newUrl = string.IsNullOrEmpty(lastPostID) ?
                        newUrl + "?api_key=" + Authentication.ConsumerKey + "&" + searchTag :
                        newUrl + "?api_key=" + Authentication.ConsumerKey + "&" + searchTag + "&before=" + lastPostID;

                    result = await WebClient.GetAsync(new Uri(newUrl));
                } else {
                    if (string.IsNullOrEmpty(lastPostID))
                        result = await RequestHandler.GET(url, "api_key=" + Authentication.ConsumerKey);
                    else
                        result = await RequestHandler.GET(url, "offset=" + lastPostID + "&api_key=" + Authentication.ConsumerKey);
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

                        }
                        return PostList;
                    } catch (Exception ex) {
                        DiagnosticsManager.LogException(ex, TAG, "Failed to serailize posts.");
                    }
                } else {
                    DiagnosticsManager.LogException(null, TAG, "Failed to retrieve posts.");
                }
            }
            return new List<Post>() { new Post() { type = "nocontent" } };
        }

        public static async Task<ObservableCollection<Post>> RetrievePost(string post_id) {
            string UserInfoURI = string.Format(
                "https://api.tumblr.com/v2/blog/{0}.tumblr.com/posts?id={1}&notes_info=true&api_key={2}",
                UserStore.CurrentBlog.Name, post_id, Authentication.ConsumerKey);
            var response = await WebClient.GetAsync(new Uri(UserInfoURI));
            var result = await response.Content.ReadAsStringAsync();

            if (result.Contains("status\":200")) {
                try {
                    var LoadedPosts = new ObservableCollection<Post>();

                    var posts = JsonConvert.DeserializeObject<Responses.GetPosts>(result);

                    foreach (var p in posts.response.posts) {
                        if (p.type == "photo") {
                            if (p.path_to_low_res_pic.url.Contains(".gif")) {
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
            var requestResult = await RequestHandler.GET(string.Format(Endpoints.Blog + "{0}.tumblr.com/info", name), "api_key=" + Authentication.ConsumerKey);
            return (requestResult.StatusCode == HttpStatusCode.OK) ?
                JsonConvert.DeserializeObject<Responses.GetBlog>(await requestResult.Content.ReadAsStringAsync())
                .response.blog : new Blog();
        }

        public static async Task<List<Blog>> BlogSearch(string tag) {
            var requestResult = await RequestHandler.GET(Endpoints.Search + tag, "api_key=" + Authentication.ConsumerKey);
            return (requestResult.StatusCode == HttpStatusCode.OK) ?
                JsonConvert.DeserializeObject<Responses.GetSearch>(await requestResult.Content.ReadAsStringAsync())
                .response.blogs : new List<Blog>();
        }
        public static async Task<List<Blog>> RetrieveSearch(string blogName, string tag) {
            var requestResult = await RequestHandler.GET(string.Format("https://api.tumblr.com/v2/blog/{0}.tumblr.com/tagged/{1}", blogName, tag), "api_key=" + Authentication.ConsumerKey);
            Debug.WriteLine(requestResult);
            return (requestResult.StatusCode == HttpStatusCode.OK) ?
                JsonConvert.DeserializeObject<Responses.GetSearch>(await requestResult.Content.ReadAsStringAsync())
                .response.blogs : new List<Blog>();
        }

        public static async Task<List<Responses.SpotlightResponse>> RetrieveSpotlight(bool forceRefresh = false) {
            if (string.IsNullOrEmpty(UserStore.CachedSpotlight) || forceRefresh) {
                var response = await WebClient.GetAsync(new Uri(Endpoints.Spotlight));
                var result = await response.Content.ReadAsStringAsync();

                if (result.Contains("status\":200")) {
                    UserStore.CachedSpotlight = result;
                    return JsonConvert.DeserializeObject<Responses.GetSpotlight>(result).response;
                }
            } else {
                return JsonConvert.DeserializeObject<Responses.GetSpotlight>(UserStore.CachedSpotlight).response;
            }

            return new List<Responses.SpotlightResponse>();
        }

        public static async Task<List<Responses.SpotlightResponse>> TagDiscovery(bool forceRefresh = false) {
            var response = await WebClient.GetAsync(new Uri(Endpoints.TagDiscovery));
            var result = await response.Content.ReadAsStringAsync();

            Debug.WriteLine(result);

            return new List<Responses.SpotlightResponse>();
        }

        public static async Task<string> GenerateMP4FromGIF(string gif) {
            var response = await WebClient.GetAsync(new Uri("http://upload.gfycat.com/transcode?fetchUrl=" + gif));
            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<GfyCat>(result).mp4Url;
        }
    }
}
