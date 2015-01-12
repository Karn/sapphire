using Newtonsoft.Json;
using System.Collections.Generic;

namespace APIWrapper.Content.Model {
    /// <summary>
    /// Class to map an store deserialized account data
    /// </summary>
    public class User {
        [JsonProperty("likes")]
        public int LikedPostCount { get; set; }

        [JsonProperty("following")]
        public int BlogsFollowingCount { get; set; }

        [JsonProperty("push_notifications")]
        public bool PushNotifiationsEnabled { get; set; }

        [JsonProperty("blogs")]
        public List<Blog> AccountBlogs { get; set; }
    }
}
