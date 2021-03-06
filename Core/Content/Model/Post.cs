﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;

namespace Core.Content.Model {
	public class Post {
		//Default properties
		[JsonProperty("blog_name")]
		public string Name { get; set; }

		public string Avatar {
			get { return "http://api.tumblr.com/v2/blog/" + Name + ".tumblr.com/avatar/96"; }
		}

		public Visibility IsEditable {
			get {
				if (UserPreferences.CurrentBlog != null) {
					if (Name == UserPreferences.CurrentBlog.Name)
						return Visibility.Visible;
				}
				return Visibility.Collapsed;
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
		public string tags_as_str {
			get {
				try {
					var converted = "";
					foreach (var tag in tags)
						converted += string.Format("#{0} ", tag);
					return converted.TrimEnd(' ');
				} catch { }
				return "";
			}
		}
		public string source_url { get; set; }
		public string source_title { get; set; }
		public bool liked { get; set; }
		public string state { get; set; }
		public Blog blog { get; set; }
		public string slug { get; set; }
		public string short_url { get; set; }
		public bool followed { get; set; }
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
		public string url { get; set; }
		public string description { get; set; }
		public string text { get; set; }

		private string _source { get; set; }
		public string source {
			get {
				return (!string.IsNullOrWhiteSpace(_source) ? "- " + _source : "");
			}
			set {
				_source = value;
			}
		}
		public Photo.AltSize path_to_low_res_pic {
			get {
				try {
					return photos.First().alt_sizes.First();
				} catch {
					return new Photo.AltSize();
				}
			}
		}
		public string asking_name { get; set; }
		public string asking_avatar {
			get {
				if (asking_name == "Anonymous")
					return "https://secure.assets.tumblr.com/images/anonymous_avatar_96.gif";
				return "http://api.tumblr.com/v2/blog/" + asking_name + ".tumblr.com/avatar/96";
			}
		}
		public object asking_url { get; set; }
		public string question { get; set; }
		public string answer { get; set; }

		private List<Note> _notes = new List<Note>();

		public List<Note> notes {
			get {
				return _notes;
			}
			set {
				_notes = value;
			}
		}

		public string special_case { get; internal set; }

		private string _RebloggedFrom = "";

		[JsonProperty("reblogged_from_name")]
		public string RebloggedFrom {
			get {
				if (!string.IsNullOrWhiteSpace(_RebloggedFrom))
					return "from " + _RebloggedFrom;
				return "";
			}
			set {
				_RebloggedFrom = value;
			}
		}

		public class Note {
			public string timestamp { get; set; }

			[JsonProperty("blog_name")]
			public string Name { get; set; }
			public string Avatar {
				get {
					return "http://api.tumblr.com/v2/blog/" + Name + ".tumblr.com/avatar/96";
				}
			}
			public string answer_text { get; set; }
			private string _type { get; set; }
			public string type {
				get {
					return _type;
				}
				set {
					if (value.Contains("posted")) {
						_type = "posted this.";
					} else if (value.Contains("like")) {
						_type = "likes this.";
					} else if (value.Contains("reblog")) {
						_type = "reblogged this.";
					} else if (value.Contains("answer")) {
						_type = "answered: " + answer_text;
					}
				}
			}
		}
	}
}
