using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Windows.Storage;

namespace API.Data {
    public static class Config {

        public static Dictionary<string, string> AccountTokens;
        public static Dictionary<string, string> AccountSecretTokens;

        public static void ReadLocalAccountStore() {
            try {
                AccountTokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(LocalSettings.Values["AccountTokens"].ToString());
                AccountSecretTokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(LocalSettings.Values["AccountSecretTokens"].ToString());
                
            } catch (Exception e) {
                AccountTokens = new Dictionary<string, string>();
                AccountSecretTokens = new Dictionary<string, string>();
            }
        }

        public static void SaveLocalAccountStore() {
            LocalSettings.Values["AccountTokens"] = JsonConvert.SerializeObject(AccountTokens);
            LocalSettings.Values["AccountSecretTokens"] = JsonConvert.SerializeObject(AccountSecretTokens);
        }

        public static string SelectedAccount {
            get {
                return LocalSettings.Values["SelectedAccount"].ToString();
            }
            set {
                LocalSettings.Values["SelectedAccount"] = value;
            }
        }

        public static string SelectedTheme {
            get {
                if (LocalSettings.Values["THEME"] != null)
                    return LocalSettings.Values["THEME"].ToString();
                return "Light";
            }
            set {
                LocalSettings.Values["THEME"] = value;
            }
        }

        /// <summary>
        /// Application data for this application
        /// </summary>
        public static ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        #region OTHER KEYS
        //LocalSettings.Values["API_KEY"] = "4HL5w2Uht9FwOQZbsmUFFB8lrXUW2D4krfG24BvcF4Sijj3Bgd";
        //LocalSettings.Values["API_SECRET_KEY"] = "kIXPU2gyaZ9EYWpuNX5FbCc9jQmhVuD40Ca7trnSr7PkDdYxLE";
        //Tumblrs API KEYS
        //LocalSettings.Values["API_KEY"] = "BUHsuO5U9DF42uJtc8QTZlOmnUaJmBJGuU1efURxeklbdiLn9L";
        //LocalSettings.Values["API_SECRET_KEY"] = "olOu3aRBCdqCuMFm8fmzNjMAWmICADSIuXWTnVSFng1ZcLU1cV";
        //LocalSettings.Values["API_KEY"] = "4MjLMcdDEDvsyAeM50KBVOLgJ1wN02Rs6AbOjy2Np9X0LA5npB";
        //LocalSettings.Values["API_SECRET_KEY"] = "1lkQGtH9N1dXeUXuDity3PZ78Cs9zqwqWXlwLNVzf5iDu3j2lT";
        #endregion

        /// <summary>
        /// Tumblr's API key, also known as the oAuth consumer key.
        /// </summary>
        public static string APIKey {
            get {
                return "BUHsuO5U9DF42uJtc8QTZlOmnUaJmBJGuU1efURxeklbdiLn9L";
                //return "ylcWC3m72FF679xnYrsxFQ3VwFk4Nb0QjYyAFtVFTQQwSTFQSF";
            }
        }

        /// <summary>
        /// Tumblr's comsumer key, also known as the oAuth consumer key.
        /// </summary>
        public static string ConsumerSecretKey {
            get {
                return "olOu3aRBCdqCuMFm8fmzNjMAWmICADSIuXWTnVSFng1ZcLU1cV";
                //return "UH1sqE7uNdrt5It8YeONcZsG6psAyo6yAX3andyx9YRGSO98XJ";
            }
        }

        /// <summary>
        /// Tumblr's comsumer key, also known as the oAuth consumer key.
        /// </summary>
        public static string ConsumerKey {
            get {
                return APIKey;
            }
        }

        /// <summary>
        /// AuthRequestToken recieved from authentication
        /// </summary>
        public static string AuthRequestToken {
            get {
                return LocalSettings.Values["AUTH_REQ_TOKEN"].ToString();
            }
            set {
                LocalSettings.Values["AUTH_REQ_TOKEN"] = value;
            }
        }

        /// <summary>
        /// Secret AuthRequestToken key
        /// </summary>
        public static string AuthRequestTokenSecret {
            get {
                return LocalSettings.Values["AUTH_REQ_SECRET_TOKEN"].ToString();
            }
            set {
                LocalSettings.Values["AUTH_REQ_SECRET_TOKEN"] = value;
            }
        }

        /// <summary>
        /// AuthRequestToken recieved from authentication
        /// </summary>
        public static string OAuthToken {
            get {
                if (LocalSettings.Values["OAUTH_TOKEN"] != null)
                    return LocalSettings.Values["OAUTH_TOKEN"].ToString();
                return "";
            }
            set {
                LocalSettings.Values["OAUTH_TOKEN"] = value;
            }
        }

        /// <summary>
        /// Secret AuthRequestToken key
        /// </summary>
        public static string OAuthTokenSecret {
            get {
                if (LocalSettings.Values["OAUTH_SECRET_TOKEN"] != null)
                    return LocalSettings.Values["OAUTH_SECRET_TOKEN"].ToString();
                return "";
            }
            set {
                LocalSettings.Values["OAUTH_SECRET_TOKEN"] = value;
            }
        }

        /// <summary>
        /// Secret AuthRequestToken key
        /// </summary>
        public static string OAuthVerifier {
            get {
                if (LocalSettings.Values["OAUTH_VERIFIER"] != null)
                    return LocalSettings.Values["OAUTH_VERIFIER"].ToString();
                return "";
            }
            set {
                LocalSettings.Values["OAUTH_VERIFIER"] = value;
            }
        }

        public static string EnableRecentBlogs {
            get {
                return LocalSettings.Values["ENABLE_RECENT_BLOGS"].ToString();
            }
            set {
                LocalSettings.Values["ENABLE_RECENT_BLOGS"] = false;
            }
        }

        public static int LastNotification {
            get {
                //return string.IsNullOrEmpty(LocalSettings.Values["LAST_NOTIFICATION"].ToString()) ? 0 : int.Parse(LocalSettings.Values["LAST_NOTIFICATION"].ToString());
                return 0;
            }
            set {
                LocalSettings.Values["LAST_NOTIFICATION"] = value.ToString();
            }
        }
    }
}
