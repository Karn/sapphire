﻿using APIWrapper.AuthenticationManager;
using APIWrapper.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIWrapper.Client {
    internal class Endpoints {

        //API connection URI
        private static readonly string APIURI = "https://api.tumblr.com/v2";
        //OAuth URI Endpoints
        public static readonly string RequestTokenURI = "http://www.tumblr.com/oauth/request_token";
        public static readonly string AuthorizationURI = "http://www.tumblr.com/oauth/authorize";
        public static readonly string AccessTokenURI = "http://www.tumblr.com/oauth/access_token";
        //XAuth URI Endpoints
        public static readonly string SecureAccessTokenURI = "https://www.tumblr.com/oauth/access_token";
        //API endpoints
        public static readonly string UserInfo = APIURI + "/user/info";
        public static readonly string Dashboard = APIURI + "/user/dashboard";
        public static readonly string Notifications = APIURI + "/user/notifications";
        public static readonly string LikePost = APIURI + "/user/like";
        public static readonly string UnlikePost = APIURI + "/user/unlike";
        public static readonly string FollowUser = APIURI + "/user/follow";
        public static readonly string UnfollowUser = APIURI + "/user/unfollow";
        public static readonly string Reply = APIURI + "/user/post/reply";
        public static readonly string Blog = APIURI + "/blog/";
        public static readonly string Search = APIURI + "/search/";
        public static readonly string Spotlight = APIURI + "/spotlight/directories?api_key=" + Authentication.ConsumerKey;
        public static readonly string TagDiscovery = APIURI + "/tag_discovery?api_key=" + Authentication.ConsumerKey;
        public static readonly string Following = APIURI + "/user/following";
        public static readonly string Followers = APIURI + "/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/followers";
        public static readonly string Post = APIURI + "/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/post";
        //public static readonly string ReblogPost = APIURI + "/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/post/reblog";
        public static string Reblog(string blogName = "") { return APIURI + "/blog/" + (!string.IsNullOrWhiteSpace(blogName) ? blogName : UserStore.CurrentBlog.Name) + ".tumblr.com/post/reblog"; }
        public static readonly string Edit = APIURI + "/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/post/edit";
        public static readonly string DeletePost = APIURI + "/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/post/delete";
        public static readonly string Inbox = APIURI + "/blog/" + UserStore.CurrentBlog.Name + "/posts/submission";
        public static readonly string Drafts = APIURI + "/blog/" + UserStore.CurrentBlog.Name + "/posts/draft";
        public static readonly string Queue = APIURI + "/blog/" + UserStore.CurrentBlog.Name + "/posts/queue";
    }
}
