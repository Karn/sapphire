using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace API.Content {
    public class Blog {
        public bool primary { get; set; }
        public string Name { get; set; }
        public string title { get; set; }
        private string _description;
        public string description {
            get {
                return _description;
            }
            set {
                _description = RequestHandler.GetPlainTextFromHtml(value);
            }
        }
        public string url { get; set; }
        public string followers { get; set; }
        public bool following { get; set; }
        public int followingCount { get; set; }
        public bool IsFollowing { get; set; }
        public string posts { get; set; }
        public int updated { get; set; }
        public Theme theme { get; set; }
        public string avatar {
            get {
                return "http://api.tumblr.com/v2/blog/" + Name + ".tumblr.com/avatar/128";
            }
        }
        public Visibility ShareLikes { get; set; }
        public int likes { get; set; }
        public bool ask { get; set; }
        public bool followed { get; set; }
        public string ask_page_title { get; set; }
        public bool ask_anon { get; set; }
        public bool is_nsfw { get; set; }

        public class AltBlog {
            public string Name { get; set; }
            public bool following { get; set; }
        }
    }
}
