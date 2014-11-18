using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Storage;

namespace APIWrapper.Content {
    public class UserStore {

        public static Model.Blog CurrentBlog;
        public static ObservableCollection<Model.Blog> UserBlogs = new ObservableCollection<Model.Blog>();

        private static ApplicationDataContainer Settings = ApplicationData.Current.RoamingSettings;

        //Appication settings
        private static readonly string[] SettingNames = {
            "NotificationsEnabled", "EnableAds", "NotificationIds",
            "OneClickReblog", "TagsInPosts", "CachedSpotlight", "CachedAccountData",
            "Theme"
        };

        public UserStore() {
            foreach (var name in SettingNames) {
                if (Settings.Values[name] == null) {
                    Debug.WriteLine("Creating Setting: " + name);
                    Settings.Values[name] = "";
                }
            }
        }

        public static string SelectedTheme {
            get {
                if (Settings.Values["Theme"] != null)
                    return Settings.Values["Theme"].ToString();
                return "Light";
            }
            set {
                Settings.Values["Theme"] = value;
            }
        }

        public static bool NotificationsEnabled {
            get {
                if (!string.IsNullOrEmpty(Settings.Values["NotificationsEnabled"].ToString()))
                    return Settings.Values["NotificationsEnabled"].ToString().Contains("T") ? true : false;
                return true;
            }
            set {
                Settings.Values["NotificationsEnabled"] = value ? "True" : "False";
            }
        }

        public static Dictionary<string, int> NotificationIDs {
            get {
                if (!string.IsNullOrEmpty(Settings.Values["NotificationIds"].ToString()))
                    return JsonConvert.DeserializeObject<Dictionary<string, int>>(Settings.Values["NotificationIds"].ToString());
                return new Dictionary<string, int>();
            }
            set {
                Settings.Values["NotificationIds"] = JsonConvert.SerializeObject(value);
            }
        }

        public static bool OneClickReblog {
            get {
                if (!string.IsNullOrEmpty(Settings.Values["OneClickReblog"].ToString()))
                    return Settings.Values["OneClickReblog"].ToString().Contains("T") ? true : false;
                return true;
            }
            set {
                Settings.Values["OneClickReblog"] = value ? "True" : "False";
            }
        }
        public static bool TagsInPosts {
            get {
                if (!string.IsNullOrEmpty(Settings.Values["TagsInPosts"].ToString()))
                    return Settings.Values["TagsInPosts"].ToString().Contains("T") ? true : false;
                return false;
            }
            set {
                Settings.Values["TagsInPosts"] = value ? "True" : "False";
            }
        }

        public static string CachedSpotlight {
            get {
                if (!string.IsNullOrEmpty(Settings.Values["CachedSpotlight"].ToString()))
                    return Settings.Values["CachedSpotlight"].ToString();
                return "";
            }
            set {
                Settings.Values["CachedSpotlight"] = value;
            }
        }


    }
}
