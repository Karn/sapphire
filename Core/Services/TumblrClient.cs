using Core.Utils;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Core.Services {

    /// <summary>
    /// Class that handles interfacing with the Tumblr API.
    /// Some Endpoints may not be accessible without the necessary keys.
    /// </summary>
    public class TumblrClient {

        /// <summary>
        /// Consumer/API key provided via the Tumblr developer dashboard.
        /// </summary>
        private string ConsumerKey { get; set; }

        /// <summary>
        /// Consumer secret provided by the Tumble developer dashboard.
        /// </summary>
        private string ConsumerSecret { get; set; }

        /// <summary>
        /// Primary constructor
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        public TumblrClient(string consumerKey, string consumerSecret) {
            Debug.WriteLine("Creating client instance with keys.");
            this.ConsumerKey = consumerKey;
            this.ConsumerSecret = consumerSecret;
        }

        /// <summary>
        /// Request user authentication return bool to indicate if request was successfull.
        /// </summary>
        /// <returns></returns>
        public Task<bool> RequestAuthentication(string username, string password) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get account data via API request and update within local database.
        /// </summary>
        /// <returns></returns>
        public Task<object> GetAccount() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request for the details of a single blog.
        /// </summary>
        /// <param name="blogName"></param>
        /// <returns></returns>
        public Task<object> GetBlog(string blogName) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request the activity feed for a given user and update values within local database.
        /// </summary>
        /// <param name="blogName"></param>
        /// <param name="beforeId"></param>
        /// <returns></returns>
        public Task<object> GetActivity(string blogName, string beforeId = null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request the likes of a given blog (must be authenticated and allowed).
        /// </summary>
        /// <param name="blogName"></param>
        /// <param name="beforeId"></param>
        /// <returns></returns>
        public Task<object> GetLikes(string blogName, string beforeId = null) {
            throw new NotImplementedException();
            //TODO: Call Getposts with given params
        }

        /// <summary>
        /// Request the submissions to a given blog (must be authenticated).
        /// </summary>
        /// <param name="blogName"></param>
        /// <param name="beforeId"></param>
        /// <returns></returns>
        public Task<object> GetInbox(string blogName, string beforeId = null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Requests the posts saved as drafts by the blog (must be authenticated).
        /// </summary>
        /// <param name="blogName"></param>
        /// <param name="beforeId"></param>
        /// <returns></returns>
        public Task<object> GetDrafts(string blogName, string beforeId = null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Requests the posts queud by a blog (must be authenticated).
        /// </summary>
        /// <param name="blogName"></param>
        /// <param name="beforeId"></param>
        /// <returns></returns>
        public Task<object> GetQueue(string blogName, string beforeId = null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request the results for a search.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="beforeId"></param>
        /// <returns></returns>
        public Task<object> GetSearchResult(string search, string beforeId = null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Requests posts for a particular blog.
        /// </summary>
        /// <param name="blogName"></param>
        /// <param name="beforeId"></param>
        /// <returns></returns>
        public Task<object> GetPosts(string blogName, string beforeId = null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request for a single post.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<object> GetPost(string id) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Requests followers associated with a blog.
        /// </summary>
        /// <param name="blogName"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Task<object> GetFollowers(string blogName, string offset = null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Requests blogs that are following another blog (must be authenticated).
        /// </summary>
        /// <param name="blogName"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Task<object> GetFollowing(string blogName, string offset = null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request for a post to be added to a users likes.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reblogKey"></param>
        /// <returns></returns>
        public Task<object> LikePost(string id, string reblogKey) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request for a post to be removed from a users likes.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reblogKey"></param>
        /// <returns></returns>
        public Task<object> UnlikePost(string id, string reblogKey) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request for a blog to be added to a users following list.
        /// </summary>
        /// <param name="blogName"></param>
        /// <returns></returns>
        public Task<object> FollowBlog(string blogName) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request for a blog to be removed to a users following list.
        /// </summary>
        /// <param name="blogName"></param>
        /// <returns></returns>
        public Task<object> UnfollowBlog(string blogName) {
            throw new NotImplementedException();
        }
    }
}
