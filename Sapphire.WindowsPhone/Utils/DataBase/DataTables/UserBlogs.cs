namespace Sapphire.Utils.DataBase.DataTables {
    public class UserBlogs {
        /// <summary>
        /// Name of the blog associated with the users Tumblr Account.
        /// </summary>
        [SQLite.PrimaryKey]
        public string BlogName { get; set; } //The BlogName property is marked as the Primary Key

        /// <summary>
        /// Title of the blog.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Uri to avatar image.
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// Description of the blog.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Url to this blog.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Number of posts (including reblogs and upload) on this blog.
        /// </summary>
        public string PostCount { get; set; }

        /// <summary>
        /// Number of liked posts for this blog (equivalent to total likes for the account).
        /// </summary>
        public int LikedPostCount { get; set; }

        /// <summary>
        /// Number of users following this blog.
        /// </summary>
        public int FollowersCount { get; set; }

        /// <summary>
        /// Number of users following by the account.
        /// </summary>
        public int FollowingCount { get; set; }

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public UserBlogs() { }

    }
}
