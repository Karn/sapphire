using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIWrapper.Content.Model {
    public class Activity {
        public class Meta {
            public int status { get; set; }
            public string msg { get; set; }
        }

        public class Notification {
            public int timestamp { get; set; }
            public string date { get; set; }
            public string type { get; set; }
            public string from_tumblelog_name { get; set; }
            public string from_tumblelog_avatar {
                get {
                    return "http://api.tumblr.com/v2/blog/" + from_tumblelog_name + ".tumblr.com/avatar/96";
                }
            }
            public int before { get; set; }
            public int from_tumblelog_id { get; set; }
            public string target_post_id { get; set; }
            public string target_post_summary { get; set; }
            public string target_post_type { get; set; }
            public string target_tumblelog_name { get; set; }
            public bool private_channel { get; set; }
            public string media_url { get; set; }
            public string media_url_large { get; set; }
            public string post_type { get; set; }
            public bool followed { get; set; }
            public string added_text { get; set; }
            public object post_id { get; set; }
        }

        public class Blog {
            public string blog_name { get; set; }
            public string blog_id { get; set; }
            public List<Notification> notifications { get; set; }
        }
    }
}
