using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Core.Content.Model {
    public class Responses {

        public class Meta {
            public int status { get; set; }
            public string msg { get; set; }
        }

        public class UserResponse {
            public Account user { get; set; }
        }

        public class ActivityResponse {
            public List<Activity.Notification> notifications { get; set; }
        }

		public class NotificationsResponse {
			public List<Activity.Blog> blogs { get; set; }
		}

		public class FollowingResponse {
            public int total_blogs { get; set; }
            public List<Blog> blogs { get; set; }
        }

        public class FollowersResponse {
            public int total_users { get; set; }
            public List<Blog> users { get; set; }
        }

        public class BlogResponse {
            public Blog blog { get; set; }
        }

        public class PostResponse {
            public List<Post> posts { get; set; }
        }

        public class LikedResponse {
            public List<Post> liked_posts { get; set; }
            public int liked_count { get; set; }
        }

        public class SpotlightResponse {
            public string tag { get; set; }
            public string title { get; set; }
            public List<string> images { get; set; }
            public string promo_image {
                get {
                    return images.ElementAt(new Random().Next(0, images.Count));
                }
            }
        }

        public class SearchResponse {
            public List<Blog> blogs { get; set; }
        }

        public class GetInfo {
            public Meta meta { get; set; }
            public UserResponse response { get; set; }
        }

        public class GetActivity {
            public Meta meta { get; set; }
            public ActivityResponse response { get; set; }
        }

		public class GetNotifications {
			public Meta meta { get; set; }
			public NotificationsResponse response { get; set; }
		}

		public class GetBlog {
            public Meta meta { get; set; }
            public BlogResponse response { get; set; }
        }

        public class GetFollowing {
            public Meta meta { get; set; }
            public FollowingResponse response { get; set; }
        }

        public class GetFollowers {
            public Meta meta { get; set; }
            public FollowersResponse response { get; set; }
        }

        public class GetPosts {
            public Meta meta { get; set; }
            public PostResponse response { get; set; }
        }

        public class GetLikes {
            public Meta meta { get; set; }
            public LikedResponse response { get; set; }
        }

        public class GetTagged {
            public Meta meta { get; set; }
            public List<Post> response { get; set; }
        }

        public class GetSpotlight {
            public Meta meta { get; set; }
            public List<SpotlightResponse> response { get; set; }
        }

        public class GetSearch {
            public Meta meta { get; set; }
            public SearchResponse response { get; set; }
        }
    }
}
