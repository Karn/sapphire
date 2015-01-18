using APIWrapper.AuthenticationManager;
using APIWrapper.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIWrapper.Client {
    internal class EndpointManager {

        //API connection URI
        private static string APIURI = "https://api.tumblr.com/v2";
        //API endpoints
        public static string UserInfo = APIURI + "/user/info";
        public static string Dashboard = APIURI + "/user/dashboard";
        public static string Notifications = APIURI + "/user/notifications";
        public static string LikePost = APIURI + "/user/like";
        public static string UnlikePost = APIURI + "/user/unlike";
        public static string FollowUser = APIURI + "/user/follow";
        public static string UnfollowUser = APIURI + "/user/unfollow";
        public static string Reply = APIURI + "/user/post/reply";
        public static string Blog = APIURI + "/blog/";
        public static string Search = APIURI + "/search/";
        public static string Spotlight = APIURI + "/spotlight/directories?api_key=" + Authentication.ConsumerKey;
        public static string TagDiscovery = APIURI + "/tag_discovery?api_key=" + Authentication.ConsumerKey;
        public static string Following = APIURI + "/user/following";
        public static string Followers = APIURI + "/blog/" + UserStorageUtils.CurrentBlog.Name + ".tumblr.com/followers";
        public static string Post = APIURI + "/blog/" + UserStorageUtils.CurrentBlog.Name + ".tumblr.com/post";
        public static string Reblog(string blogName = "") { return APIURI + "/blog/" + (!string.IsNullOrWhiteSpace(blogName) ? blogName : UserStorageUtils.CurrentBlog.Name) + ".tumblr.com/post/reblog"; }
        public static string Edit = APIURI + "/blog/" + UserStorageUtils.CurrentBlog.Name + ".tumblr.com/post/edit";
        public static string DeletePost = APIURI + "/blog/" + UserStorageUtils.CurrentBlog.Name + ".tumblr.com/post/delete";
        public static string Inbox = APIURI + "/blog/" + UserStorageUtils.CurrentBlog.Name + "/posts/submission";
        public static string Drafts = APIURI + "/blog/" + UserStorageUtils.CurrentBlog.Name + "/posts/draft";
        public static string Queue = APIURI + "/blog/" + UserStorageUtils.CurrentBlog.Name + "/posts/queue";
    }
}
