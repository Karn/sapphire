﻿using APIWrapper.AuthenticationManager;
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
using System.Text;
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
                htmlString = Regex.Replace(htmlString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
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

                        UserStorageUtils.UserBlogs.Clear();

                        foreach (var b in parsedData.response.user.AccountBlogs) {
                            b.FollowingCount = parsedData.response.user.BlogsFollowingCount;
                            b.LikedPostCount = parsedData.response.user.LikedPostCount;

                            if (b.Name == account) {
                                UserStorageUtils.CurrentBlog = b;
                            } else if (UserStorageUtils.CurrentBlog == null && b.IsPrimaryBlog)
                                UserStorageUtils.CurrentBlog = b;
                            else if (b.Name == UserStorageUtils.CurrentBlog.Name)
                                UserStorageUtils.CurrentBlog = b;

                            UserStorageUtils.UserBlogs.Add(b);
                        }

                        if (UserStorageUtils.UserBlogs.Count > 0)
                            return true;
                    }
                    Debug.WriteLine("Rerunning call due to previous faliure. " + i);
                }
            } catch (Exception ex) {

            }
            return false;
        }

        public static async Task<List<Activity.Notification>> RetrieveActivity() {
            if (UserStorageUtils.CurrentBlog != null) {
                try {
                    for (int i = 0; i < 5; i++) {
                        var result = await RequestService.GET("https://api.tumblr.com/v2/user/notifications");

                        if (result.StatusCode == HttpStatusCode.OK) {
                            var activity = JsonConvert.DeserializeObject<Responses.GetActivity>(await result.Content.ReadAsStringAsync());

                            var Notifications = new List<Activity.Notification>();
                            var NotificationDictionary = UserStorageUtils.NotificationIDs;

                            foreach (var b in activity.response.blogs) {
                                if (b.Name == UserStorageUtils.CurrentBlog.Name) {
                                    Debug.WriteLine("Retreiving activity for " + b.Name);
                                    if (!NotificationDictionary.ContainsKey(b.Name)) {
                                        NotificationDictionary.Add(b.Name, 0);
                                    }
                                    foreach (var n in b.notifications) {
                                        n.date = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(n.timestamp).ToString("yyyy'-'MM'-'dd");
                                        Notifications.Add(n);
                                    }
                                    NotificationDictionary[b.Name] = Notifications.First().timestamp;

                                    UserStorageUtils.NotificationIDs = NotificationDictionary;
                                    return Notifications;
                                }
                            }
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

        public static async Task<HttpResponseMessage> CreateReply(string parameters) {
            return await RequestService.POST(EndpointManager.Edit, parameters);
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
                )).StatusCode == HttpStatusCode.Created;
        }

        public async static Task<bool> DeletePost(string id) {
            return (await RequestService.POST(EndpointManager.DeletePost,
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

        public static async Task<List<Post>> RetrievePosts(string url, string lastPostID = "") {
            var result = new HttpResponseMessage();
            if (url.Contains("/user/dashboard") || url.Contains("/submission") || url.Contains("/draft") || url.Contains("/queue")) {
                result = string.IsNullOrEmpty(lastPostID) ?
                    result = await RequestService.GET(url) : await RequestService.GET(url, "max_id=" + lastPostID);
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
                            if (p.photos.Count > 1)
                                p.type = "photoset";
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

            return new List<Post>() { new Post() { type = "nocontent" } };
        }

        public static async Task<List<Post>> RetrievePost(string post_id) {
            var url = string.Format(
                "https://api.tumblr.com/v2/blog/{0}.tumblr.com/posts?id={1}&notes_info=true&api_key={2}",
                UserStorageUtils.CurrentBlog.Name, post_id, Authentication.ConsumerKey);

            var response = await Client.GetAsync(new Uri(url));
            var result = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK) {
                try {
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
                    }
                    return posts.response.posts;
                } catch (Exception ex) {
                }
            }
            return null;
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
            if (string.IsNullOrEmpty(UserStorageUtils.CachedSpotlight) || forceRefresh && UserStorageUtils.CurrentBlog != null) {
                var response = await Client.GetAsync(new Uri(EndpointManager.Spotlight));
                var result = await response.Content.ReadAsStringAsync();

                if (result.Contains("status\":200")) {
                    UserStorageUtils.CachedSpotlight = result;
                    return JsonConvert.DeserializeObject<Responses.GetSpotlight>(result).response;
                }
            } else
                return JsonConvert.DeserializeObject<Responses.GetSpotlight>(UserStorageUtils.CachedSpotlight).response;
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
