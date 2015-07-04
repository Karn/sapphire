using Newtonsoft.Json;
using System.Collections.Generic;

namespace Core.Content.Model {

    /// <summary>
    /// Handles 'User' JSON deserialzation and DataBase object manipulations.
    /// </summary>
    public class Account {

        /// <summary>
        /// Number of posts liked by the account.
        /// </summary>
        [JsonProperty("likes")]
        public int LikesCount { get; set; }

        /// <summary>
        /// Number of other blogs that are being followed by the account.
        /// </summary>
        [JsonProperty("following")]
        public int FollowingCount { get; set; }

        /// <summary>
        /// Sets/Returns 'true' if notifications are enabled for the account.
        /// </summary>
        [JsonProperty("push_notifications")]
        public bool PushEnabled { get; set; }

        /// <summary>
        /// List of blogs contained within the account.
        /// </summary>
        [JsonProperty("blogs")]
        [SQLite.Ignore]
        public List<Blog> AccountBlogs { get; set; }

        /// <summary>
        /// Pirivate member used to store email as unencoded object
        /// </summary>
        [SQLite.Ignore]
        private string _accountEmail { get; set; }

        /// <summary>
        /// Email/Tumblr Username associated with the account.
        /// </summary>
        [SQLite.PrimaryKey]
        public string AccountEmail { get; set; }

        /// <summary>
        /// Token retrieved during authentication process.
        /// </summary>
        public string AuthenticatedToken { get; set; }

        /// <summary>
        /// Token secret retrieved during authentication process.
        /// </summary>
        public string AuthenticationTokenSecret { get; set; }
    }
}
