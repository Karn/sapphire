using APIWrapper.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Storage;

namespace APIWrapper.Content {
    public class UserStore {

        public static string TAG = "UserStore";

        public static Model.Blog CurrentBlog;
        public static ObservableCollection<Model.Blog> UserBlogs = new ObservableCollection<Model.Blog>();

        private static ApplicationDataContainer Settings = ApplicationData.Current.RoamingSettings;

        //Appication settings
        private static readonly string[] SettingNames = {
            "NotificationsEnabled", "EnableAds", "NotificationIds",
            "OneClickReblog", "TagsInPosts", "CachedSpotlight", "CachedAccountData",
            "Theme", "EnableAnalytics", "StatusBarBG"
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
                if (!string.IsNullOrEmpty(Settings.Values["Theme"].ToString()))
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
                try {
                    if (!string.IsNullOrEmpty(Settings.Values["NotificationIds"].ToString()))
                        return JsonConvert.DeserializeObject<Dictionary<string, int>>(Settings.Values["NotificationIds"].ToString());
                    return new Dictionary<string, int>();
                } catch (Exception ex) {
                    DiagnosticsManager.LogException(ex, TAG, "Failed to load notification ids.");
                    return new Dictionary<string, int>();
                }

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

        public static bool EnableAnalytics {
            get {
                if (!string.IsNullOrEmpty(Settings.Values["EnableAnalytics"].ToString()))
                    return Settings.Values["EnableAnalytics"].ToString().Contains("T") ? true : false;
                return true;
            }
            set {
                Settings.Values["EnableAnalytics"] = value ? "True" : "False";
            }
        }

        public static bool EnableStatusBarBG {
            get {
                if (!string.IsNullOrEmpty(Settings.Values["StatusBarBG"].ToString()))
                    return Settings.Values["StatusBarBG"].ToString().Contains("T") ? true : false;
                return true;
            }
            set {
                Settings.Values["StatusBarBG"] = value ? "True" : "False";
            }
        }

    }
}
