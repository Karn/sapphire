using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Storage;
using System.Linq;

namespace APIWrapper.Content {
    public class UserStore {

        public static string TAG = "UserStore";

        public static Model.Blog CurrentBlog;
        public static ObservableCollection<Model.Blog> UserBlogs = new ObservableCollection<Model.Blog>();

        private static ObservableCollection<Model.Blog> FavBlogs;

        private static ApplicationDataContainer Settings = ApplicationData.Current.LocalSettings;


        public UserStore() {
            if (Settings.Values["Theme"] != null) {
                Settings.Values.Clear();
                Debug.WriteLine("Cleared Settings.");
            }

        }

        public static void AddFav(string name) {
            try {
                if (!FavBlogs.Any(b => b.Name == name)) {
                    FavBlogs.Insert(0, new Model.Blog { Name = name });
                    if (FavBlogs.Count > 7) {
                        FavBlogs.Remove(FavBlogs.Last());
                    }
                    FavBlogList = FavBlogs;
                }
            } catch (Exception ex) {
                Utils.DiagnosticsManager.LogException(ex, TAG, "Failed to add blog to favorites.");
            }
        }
        public static void RemoveFav(string name) {
            try {
                if (FavBlogs.Any(b => b.Name == name)) {
                    FavBlogs.Remove(FavBlogs.Where(x => x.Name == name).First());
                    FavBlogList = FavBlogs;
                }
            } catch (Exception ex) {
                Utils.DiagnosticsManager.LogException(ex, TAG, "Failed to remove blog from favorites.");
            }
        }

        public static string SelectedTheme {
            get {
                if (Settings.Values["_Theme"] != null && !string.IsNullOrWhiteSpace(Settings.Values["_Theme"].ToString())) {
                    return Settings.Values["_Theme"].ToString();
                }
                return "Light";
            }
            set {
                Settings.Values["_Theme"] = value;
            }
        }

        public static bool NotificationsEnabled {
            get {
                if (Settings.Values["_NotificationsEnabled"] != null && !string.IsNullOrWhiteSpace(Settings.Values["_NotificationsEnabled"].ToString())) {
                    return Settings.Values["_NotificationsEnabled"].ToString().Contains("T") ? true : false;
                }
                return true;
            }
            set {
                Settings.Values["_NotificationsEnabled"] = value ? "True" : "False";
            }
        }

        public static Dictionary<string, int> NotificationIDs {
            get {
                try {
                    if (Settings.Values["_NotificationIds"] != null && !string.IsNullOrWhiteSpace(Settings.Values["_NotificationIds"].ToString())) {
                        return JsonConvert.DeserializeObject<Dictionary<string, int>>(Settings.Values["_NotificationIds"].ToString());
                    }
                } catch (Exception ex) {
                }
                return new Dictionary<string, int>();
            }
            set {
                Settings.Values["_NotificationIds"] = JsonConvert.SerializeObject(value);
            }
        }

        public static bool OneClickReblog {
            get {
                if (Settings.Values["_OneClickReblog"] != null && !string.IsNullOrWhiteSpace(Settings.Values["_OneClickReblog"].ToString())) {
                    return Settings.Values["_OneClickReblog"].ToString().Contains("T") ? true : false;
                }
                return true;
            }
            set {
                Settings.Values["_OneClickReblog"] = value ? "True" : "False";
            }
        }
        public static bool TagsInPosts {
            get {
                if (Settings.Values["_TagsInPosts"] != null && !string.IsNullOrWhiteSpace(Settings.Values["_TagsInPosts"].ToString())) {
                    return Settings.Values["_TagsInPosts"].ToString().Contains("T") ? true : false;
                }
                return false;
            }
            set {
                Settings.Values["_TagsInPosts"] = value ? "True" : "False";
            }
        }

        public static string CachedSpotlight {
            get {
                if (Settings.Values["_CachedSpotlight"] != null && !string.IsNullOrWhiteSpace(Settings.Values["_CachedSpotlight"].ToString())) {
                    return Settings.Values["_CachedSpotlight"].ToString();
                }
                return "";
            }
            set {
                Settings.Values["_CachedSpotlight"] = value;
            }
        }

        public static bool EnableAnalytics {
            get {
                if (Settings.Values["_EnableAnalytics"] != null && !string.IsNullOrWhiteSpace(Settings.Values["_EnableAnalytics"].ToString())) {
                    return Settings.Values["_EnableAnalytics"].ToString().Contains("T") ? true : false;
                }
                return true;
            }
            set {
                Settings.Values["_EnableAnalytics"] = value ? "True" : "False";
            }
        }

        public static bool EnableStatusBarBG {
            get {
                if (Settings.Values["_StatusBarBG"] != null && !string.IsNullOrWhiteSpace(Settings.Values["_StatusBarBG"].ToString())) {
                    return Settings.Values["_StatusBarBG"].ToString().Contains("T") ? true : false;
                }
                return true;
            }
            set {
                Settings.Values["_StatusBarBG"] = value ? "True" : "False";
            }
        }

        public static ObservableCollection<Model.Blog> FavBlogList {
            get {
                if (FavBlogs == null) {
                    if (Settings.Values["_FavBlogList"] != null && !string.IsNullOrWhiteSpace(Settings.Values["_FavBlogList"].ToString()))
                        FavBlogs = JsonConvert.DeserializeObject<ObservableCollection<Model.Blog>>(Settings.Values["_FavBlogList"].ToString());
                    else
                        FavBlogs = new ObservableCollection<Model.Blog>();
                }
                return FavBlogs;
            }
            set {
                Settings.Values["_FavBlogList"] = JsonConvert.SerializeObject(FavBlogs);
            }
        }

    }
}

