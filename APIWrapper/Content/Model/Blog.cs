using APIWrapper.Client;
using Newtonsoft.Json;

namespace APIWrapper.Content.Model {
    public class Blog {

        [JsonProperty("primary")]
        public bool IsPrimaryBlog { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        public string Avatar {
            get { return "http://api.tumblr.com/v2/blog/" + Name + ".tumblr.com/avatar/128"; }
        }

        [JsonProperty("description")]
        private string _description;

        public string Description {
            get { return _description; }
            set { _description = CreateRequest.GetPlainTextFromHtml(value); }
        }

        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("posts")]
        public string PostCount { get; set; }

        [JsonProperty("theme")]
        public Theme BlogTheme { get; set; }

        public int LikedPostCount { get; set; }

        [JsonProperty("followers")]
        public int FollowersCount { get; set; }

        public int FollowingCount { get; set; }

        [JsonProperty("following")]
        public bool IsFollowing { get; set; }

        [JsonProperty("share_likes")]
        public bool LikesVisible { get; set; }

        [JsonProperty("ask")]
        public bool AsksEnabled { get; set; }

        public class AltBlog : Blog {
            new public bool IsFollowing { get; set; }
        }
    }
}
