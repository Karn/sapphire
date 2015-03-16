using Core.Client;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Core.Content.Model {
	public class Blog : IEquatable<Blog> {

		[JsonProperty("primary")]
		public bool IsPrimaryBlog { get; set; }

		public string _name = "";

		[JsonProperty("name")]
		public string Name {
			get { return _name; }
			set { _name = value; }
		}

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
		public bool _following { get; set; }

		[JsonProperty("followed")]
		public bool _followed { get; set; }

		public bool IsFollowing {
			get {
				return (_following || _followed);
			}
			set {
				_following = value;
			}
		}

		[JsonProperty("share_likes")]
		public bool LikesVisible { get; set; }

		private bool _AsksEnabled { get; set; }

		[JsonProperty("ask")]
		public bool AsksEnabled {
			get {
				if (Utils.AppLicenseHandler.IsTrial)
					return false;
				return _AsksEnabled;
			}
			set {
				_AsksEnabled = value;
			}
		}

		public bool Equals(Blog other) {
			return this.Name == other.Name;
		}
	}
}
