using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace API.Content {
    public class UserData {

        public static Content.Blog CurrentBlog;
        public static ObservableCollection<Content.Blog> UserBlogs = new ObservableCollection<Content.Blog>();

        private static ApplicationDataContainer UserDataStore;

        private static string[] settingNames = {
                                                   "LOGGED_IN",
                                                   "ONE_CLICK_REBLOG",
                                                   "FAV_BLOGS",
                                                   "NOTIFICATIONS_ENABLED",
                                                   "NOTIFICATION_FREQUENCY",
                                                   "NEW_FOLLOWERS",
                                                   "REBLOGS",
                                                   "LIKES",
                                                   "USER_MENTIONS",
                                                   "ENABLE_ADS"
                                               };

        public static void CreateDataContainer() {
            ApplicationData.Current.LocalSettings.CreateContainer("UserDataStore", ApplicationDataCreateDisposition.Always);
            UserDataStore = ApplicationData.Current.LocalSettings.Containers["UserDataStore"];

            foreach (var setting in settingNames) {
                UserDataStore.Values[setting] = "";
            }
        }

        public static void LoadData() {
            try {
                UserDataStore = ApplicationData.Current.LocalSettings.Containers["UserDataStore"];
            } catch (Exception e) {
                CreateDataContainer();
                Debug.WriteLine("Load failed, creating container.");
            }

            foreach (var setting in settingNames) {
                try {
                    if (string.IsNullOrEmpty(UserDataStore.Values[setting].ToString())) {
                        Debug.WriteLine("Blank Setting: " + setting);
                        UserDataStore.Values[setting] = "";
                    }
                } catch (NullReferenceException NRE) {
                    UserDataStore.Values[setting] = "";
                }
            }

            if ((UserDataStore.Values["FAV_BLOGS"].ToString() != ""))
                FavBlogs = JsonConvert.DeserializeObject<ObservableCollection<Blog>>(UserDataStore.Values["FAV_BLOGS"].ToString());
        }

        public static void ClearDataContainer() {
            ApplicationData.Current.LocalSettings.DeleteContainer("UserDataStore");
            ApplicationData.Current.LocalSettings.CreateContainer("UserDataStore", ApplicationDataCreateDisposition.Always);

            foreach (var setting in settingNames) {
                UserDataStore.Values[setting] = "";
            }
        }

        public static bool IsLoggedIn {
            get {
                return (UserDataStore.Values["LOGGED_IN"].ToString() == "True" ? true : false);
            }
            set {
                UserDataStore.Values["LOGGED_IN"] = value;
            }
        }

        public static bool OneClickReblog {
            get {
                return (UserDataStore.Values["ONE_CLICK_REBLOG"].ToString() == "True" ? true : false);
            }
            set {
                UserDataStore.Values["LOGGED_IN"] = value;
            }
        }
        /// <summary>
        /// List of blogs that were viewed recently
        /// </summary>
        public static ObservableCollection<Blog> FavBlogs = new ObservableCollection<Blog>();

        public static void UpdateFavBlogs() {
            try {
                UserDataStore.Values["FAV_BLOGS"] = JsonConvert.SerializeObject(FavBlogs, Formatting.Indented);
                Debug.WriteLine("Updated Fav List");
            } catch (Exception e) {
                Debug.WriteLine("Failed to update fav list. " + e.Message);
            }
        }


        public static string AreNotificationsEnabled {
            get {
                return UserDataStore.Values["NOTIFICATIONS_ENABLED"].ToString();
            }
            set {
                UserDataStore.Values["NOTIFICATIONS_ENABLED"] = value;
            }
        }

        public static string NotificationsFrequency {
            get {
                return UserDataStore.Values["NOTIFICATION_FREQUENCY"].ToString();
            }
            set {
                UserDataStore.Values["NOTIFICATION_FREQUENCY"] = value;
            }
        }

        public static string NewFollowerNotifications {
            get {
                return UserDataStore.Values["NEW_FOLLOWERS"].ToString();
            }
            set {
                UserDataStore.Values["NEW_FOLLOWERS"] = value;
            }
        }

        public static string ReblogNotifications {
            get {
                return UserDataStore.Values["REBLOGS"].ToString();
            }
            set {
                UserDataStore.Values["REBLOGS"] = value;
            }
        }

        public static string LikeNotifications {
            get {
                return UserDataStore.Values["LIKES"].ToString();
            }
            set {
                UserDataStore.Values["LIKES"] = value;
            }
        }

        public static string UserMentionsNotifications {
            get {
                return UserDataStore.Values["USER_MENTIONS"].ToString();
            }
            set {
                UserDataStore.Values["USER_MENTIONS"] = value;
            }
        }

        public static string EnableAds {
            get {
                return UserDataStore.Values["ENABLE_ADS"].ToString();
            }
            set {
                UserDataStore.Values["ENABLE_ADS"] = value;
            }
        }
    }
}
