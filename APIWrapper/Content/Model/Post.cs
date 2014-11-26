using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;

namespace APIWrapper.Content.Model {
    public class Post {
        //Default properties
        public string blog_name { get; set; }
        public Visibility IsEditable {
            get {
                if (UserStore.CurrentBlog != null) {
                    if (blog_name == UserStore.CurrentBlog.Name)
                        return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        public string avatar {
            get {
                return "http://api.tumblr.com/v2/blog/" + blog_name + ".tumblr.com/avatar/96";
            }
        }
        public string id { get; set; }
        public string post_url { get; set; }
        public string type { get; set; }
        public string timestamp { get; set; }
        public string date { get; set; }
        public string format { get; set; }
        public string reblog_key { get; set; }
        public string[] tags { get; set; }
        public string source_url { get; set; }
        public string source_title { get; set; }
        public bool liked { get; set; }
        public string state { get; set; }
        public Blog blog { get; set; }
        public string slug { get; set; }
        public string short_url { get; set; }
        public bool followed { get; set; }
        public List<object> highlighted { get; set; }
        public int note_count { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public bool can_reply { get; set; }
        public string caption { get; set; }
        public string image_permalink { get; set; }
        public ObservableCollection<Photo> photos { get; set; }
        public string link_url { get; set; }
        public string photoset_layout { get; set; }
        public string permalink_url { get; set; }
        public bool html5_capable { get; set; }
        public string thumbnail_url { get; set; }
        public int thumbnail_width { get; set; }
        public int thumbnail_height { get; set; }
        public object player { get; set; }
        public string video_type { get; set; }
        public string video_url { get; set; }
        public string artist { get; set; }
        public string album { get; set; }
        public int? year { get; set; }
        public string track { get; set; }
        public string track_name { get; set; }
        public string album_art { get; set; }
        public string embed { get; set; }
        public int? plays { get; set; }
        public string audio_url { get; set; }
        public bool? is_external { get; set; }
        public string audio_type { get; set; }
        //public List<Dialogue> dialogue { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public string text { get; set; }
        public string source { get; set; }
        public string path_to_low_res_pic { get; set; }
        public int pic_height { get; set; }
        public bool IsPhoto {
            get {
                return type == "photo";
            }
        }
        public string asking_name { get; set; }
        public string asking_avatar {
            get {
                return "http://api.tumblr.com/v2/blog/" + asking_name + ".tumblr.com/avatar/96";
            }
        }
        public object asking_url { get; set; }
        public string question { get; set; }
        public string answer { get; set; }

        public SourceAttribution source_attribution { get; set; }

        private List<Note> _notes = new List<Note>();
        public string special_case { get; set; }

        public List<Note> notes {
            get {
                return _notes;
            }
            set {
                _notes = value;
            }
        }

        public class Note {
            public string timestamp { get; set; }
            public string blog_name { get; set; }
            public string avatar {
                get {
                    return "http://api.tumblr.com/v2/blog/" + blog_name + ".tumblr.com/avatar/96";
                }
            }
            public string blog_url { get; set; }
            public string post_id { get; set; }
            private string _type { get; set; }
            public string type {
                get {
                    return _type;
                }
                set {
                    if (value.Contains("like")) {
                        _type = "likes this.";
                    } else if (value.Contains("reblog")) {
                        _type = "reblogged this.";
                    }
                }
            }
        }
    }
}
