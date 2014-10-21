using API.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace API.Authentication {
    public class AuthenticationManager {
        #region URIs
        /// <summary>
        /// The URL used to obtain an unauthorized Response Token.
        /// </summary>
        public static string RequestTokenUrl {
            get {
                return "http://www.tumblr.com/oauth/request_token";
            }
        }
        /// <summary>
        /// The URL used to obtain User authorization for Consumer access.
        /// </summary>
        public static string AuthUrl {
            get {
                return "http://www.tumblr.com/oauth/authorize";
            }
        }
        /// <summary>
        /// The URL used to exchange the User-authorized Response Token for an Access Token
        /// </summary>
        public static string AccessTokenUrl {
            get {
                return "http://www.tumblr.com/oauth/access_token";
            }
        }
        /// <summary>
        /// The URL used to exchange the User-authorized Response Token for an Access Token
        /// </summary>
        public static string XAauthAccessTokenUrl {
            get {
                return "https://www.tumblr.com/oauth/access_token";
            }
        }
        #endregion

        #region Keys
        public static string APIKey { get; private set; }

        /// <summary>
        /// Tumblr's comsumer key, also known as the oAuth consumer key.
        /// </summary>
        public static string ConsumerSecretKey { get; private set; }

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
        public static string AuthRequestToken { get; private set; }

        /// <summary>
        /// Secret AuthRequestToken key
        /// </summary>
        public static string AuthRequestTokenSecret { get; private set; }

        /// <summary>
        /// AuthRequestToken recieved from authentication
        /// </summary>
        public static string OAuthToken { get; private set; }

        /// <summary>
        /// Secret AuthRequestToken key
        /// </summary>
        public static string OAuthTokenSecret { get; private set; }

        /// <summary>
        /// Secret AuthRequestToken key
        /// </summary>
        public static string OAuthVerifier { get; private set; }
        #endregion

        public static Utilities Utils = new Utilities();
        public static HttpClient client = new HttpClient();

        public AuthenticationManager() {

        }

        public void InitializeKeys(string consumerKey, string consumerSecretKey) {
            if (!string.IsNullOrEmpty(consumerKey) && !string.IsNullOrEmpty(consumerSecretKey)) {
                APIKey = consumerKey;
                ConsumerSecretKey = consumerSecretKey;
            }
        }

        #region Authenticate
        public async Task<string> RequestToken() {

            string nonce = Utils.GetNonce();
            string timeStamp = Utils.GetTimeStamp();

            string baseParameters = new Dictionary<string, string> {
                {"oauth_consumer_key", ConsumerKey},
                {"oauth_nonce", nonce},
                {"oauth_signature_method", "HMAC-SHA1"},
                {"oauth_timestamp", timeStamp},
                {"oauth_version", "1.0"}}.Select(kv => kv.Key + "=" + kv.Value).Aggregate((i, j) => i + "&" + j);

            string sigBaseString = "POST&" + Uri.EscapeDataString(RequestTokenUrl) + "&" + Uri.EscapeDataString(baseParameters);

            string signature = Utils.GetSignature(sigBaseString, ConsumerSecretKey);
            var response = await Utils.PostData(RequestTokenUrl, baseParameters + "&oauth_signature=" + Uri.EscapeDataString(signature));

            if (!string.IsNullOrEmpty(response)) {
                foreach (var x in Utils.ParseResponseString(response)) {
                    switch (x.Key) {
                        case "oauth_token":
                            AuthRequestToken = x.Value;
                            break;
                        case "oauth_token_secret":
                            AuthRequestTokenSecret = x.Value;
                            break;
                    }
                }
                return Uri.UnescapeDataString(AuthUrl + "?oauth_token=" + AuthRequestToken);
            }
            return string.Empty;
        }

        public async Task<bool> AccessToken() {
            string nonce = Utils.GetNonce();
            string timeStamp = Utils.GetTimeStamp();

            string sigBaseStringParams = new SortedDictionary<string, string> { 
                {"oauth_consumer_key", ConsumerKey},
                {"oauth_nonce", nonce},
                {"oauth_signature_method=", "HMAC-SHA1"},
                {"oauth_timestamp=", timeStamp},
                {"oauth_token", AuthRequestToken},
                {"oauth_verifier", OAuthVerifier},
                {"oauth_version", "1.0"}}.Select(kv => kv.Key + "=" + kv.Value).Aggregate((i, j) => i + "&" + j);

            string sigBaseString = "POST&" + Uri.EscapeDataString(AccessTokenUrl) + "&" + Uri.EscapeDataString(sigBaseStringParams);

            string signature = Utils.GetSignature(sigBaseString, Config.ConsumerSecretKey, Config.AuthRequestTokenSecret);

            var response = await Utils.PostData(AccessTokenUrl, sigBaseStringParams + "&oauth_signature=" + Uri.EscapeDataString(signature));

            if (!string.IsNullOrEmpty(response)) {
                foreach (var x in Utils.ParseResponseString(response)) {
                    switch (x.Key) {
                        case "oauth_token":
                            OAuthToken = x.Value;
                            break;
                        case "oauth_token_secret":
                            OAuthTokenSecret = x.Value;
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(OAuthToken) || !string.IsNullOrEmpty(OAuthTokenSecret)) {
                    return true;
                }
            }
            return false;
        }

        #endregion


        public class Utilities {
            private const string UnreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

            /// <summary>
            /// Alternate Url Encode implementation since the default .NET one outputs the percent encoding in lower case.
            /// While this is not a problem with the percent encoding spec, it is used in upper case throughout OAuth
            /// </summary>
            /// <param name="value">The value to Url encode</param>
            /// <returns>Returns a Url encoded string</returns>
            public string UrlEncode(string value) {
                StringBuilder result = new StringBuilder();

                foreach (char symbol in value) {
                    if (UnreservedChars.IndexOf(symbol) != -1) {
                        result.Append(symbol);
                    } else {
                        result.Append('%' + String.Format("{0:X2}", (int)symbol));
                    }
                }

                return result.ToString();
            }

            public Dictionary<string, string> ParseResponseString(string uri) {
                return uri.Split('&').ToDictionary(p => p.Substring(0, p.IndexOf('=')),
                    p => p.Substring(p.IndexOf('=') + 1));
            }

            public string GetNonce() {
                StringBuilder builder = new StringBuilder();
                Random random = new Random();
                char ch;
                for (int i = 0; i < 32; i++) {
                    ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                    builder.Append(ch);
                }

                return builder.ToString().ToLower();
            }

            public string GetTimeStamp() {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return Convert.ToInt64(ts.TotalSeconds).ToString();
            }

            public string GetSignature(string sigBaseString, string consumerSecretKey, string oAuthTokenSecret = null) {
                IBuffer KeyMaterial = CryptographicBuffer.ConvertStringToBinary(consumerSecretKey + "&" + oAuthTokenSecret, BinaryStringEncoding.Utf8);
                MacAlgorithmProvider HmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
                CryptographicKey MacKey = HmacSha1Provider.CreateKey(KeyMaterial);
                IBuffer DataToBeSigned = CryptographicBuffer.ConvertStringToBinary(sigBaseString, BinaryStringEncoding.Utf8);
                IBuffer SignatureBuffer = CryptographicEngine.Sign(MacKey, DataToBeSigned);
                return CryptographicBuffer.EncodeToBase64String(SignatureBuffer);
            }

            public async Task<string> PostData(string url, string postData) {
                try {
                    HttpClient httpClient = new HttpClient();
                    httpClient.MaxResponseContentBufferSize = int.MaxValue;
                    httpClient.DefaultRequestHeaders.ExpectContinue = false;
                    HttpRequestMessage requestMsg = new HttpRequestMessage();

                    requestMsg.Content = new StringContent(postData);
                    requestMsg.Headers.Authorization = new AuthenticationHeaderValue("OAuth", postData);
                    requestMsg.Method = new HttpMethod("POST");
                    requestMsg.RequestUri = new Uri(url, UriKind.Absolute);
                    requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    var response = await httpClient.SendAsync(requestMsg);
                    return await response.Content.ReadAsStringAsync();

                } catch (Exception Err) {
                    return string.Empty;
                }
            }

            public async Task<string> XAuthPostData(string url, string postData) {
                try {
                    HttpClient httpClient = new HttpClient();
                    httpClient.MaxResponseContentBufferSize = int.MaxValue;
                    httpClient.DefaultRequestHeaders.ExpectContinue = false;
                    HttpRequestMessage requestMsg = new HttpRequestMessage();

                    requestMsg.Content = new StringContent(postData);
                    requestMsg.Method = new HttpMethod("POST");
                    requestMsg.RequestUri = new Uri(url, UriKind.Absolute);
                    requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    var response = await httpClient.SendAsync(requestMsg);
                    return await response.Content.ReadAsStringAsync();

                } catch (Exception Err) {
                    return string.Empty;
                }
            }
        }

        public class Authorize {
            public async Task<string> RequestToken() {

                string nonce = Utils.GetNonce();
                string timeStamp = Utils.GetTimeStamp();

                string baseParameters = new Dictionary<string, string> {
                {"oauth_consumer_key", Config.ConsumerKey},
                {"oauth_nonce", nonce},
                {"oauth_signature_method", "HMAC-SHA1"},
                {"oauth_timestamp", timeStamp},
                {"oauth_version", "1.0"}}.Select(kv => kv.Key + "=" + kv.Value).Aggregate((i, j) => i + "&" + j);

                string sigBaseString = "POST&" + Uri.EscapeDataString(AuthenticationManager.RequestTokenUrl) + "&" + Uri.EscapeDataString(baseParameters);

                string signature = Utils.GetSignature(sigBaseString, Config.ConsumerSecretKey);

                try {
                    var response = await Utils.PostData(AuthenticationManager.RequestTokenUrl, baseParameters + "&oauth_signature=" + Uri.EscapeDataString(signature));
                    Debug.WriteLine("OAuth Token Request Response: " + response);

                    if (!string.IsNullOrEmpty(response)) {
                        foreach (var x in Utils.ParseResponseString(response)) {
                            switch (x.Key) {
                                case "oauth_token":
                                    Config.AuthRequestToken = x.Value;
                                    Debug.WriteLine("OAuth Request Token: " + Config.AuthRequestToken);
                                    break;
                                case "oauth_token_secret":
                                    Config.AuthRequestTokenSecret = x.Value;
                                    Debug.WriteLine("OAuth Request Token Secret: " + Config.AuthRequestTokenSecret);
                                    break;
                            }
                        }
                        return Uri.UnescapeDataString(AuthenticationManager.AuthUrl + "?oauth_token=" + Config.AuthRequestToken);
                    }
                } catch (Exception e) {
                    Debug.WriteLine("Request Token Exception: " + e.Message);
                }
                return "";
            }

            public static async Task<bool> AccessToken() {
                string nonce = Utils.GetNonce();
                string timeStamp = Utils.GetTimeStamp();

                string sigBaseStringParams = new SortedDictionary<string, string> { 
                {"oauth_consumer_key", Config.ConsumerKey},
                {"oauth_nonce", nonce},
                {"oauth_signature_method=", "HMAC-SHA1"},
                {"oauth_timestamp=", timeStamp},
                {"oauth_token", Config.AuthRequestToken},
                {"oauth_verifier", Config.OAuthVerifier},
                {"oauth_version", "1.0"}}.Select(kv => kv.Key + "=" + kv.Value).Aggregate((i, j) => i + "&" + j);

                string sigBaseString = "POST&" + Uri.EscapeDataString(AuthenticationManager.AccessTokenUrl) + "&" + Uri.EscapeDataString(sigBaseStringParams);

                string signature = Utils.GetSignature(sigBaseString, Config.ConsumerSecretKey, Config.AuthRequestTokenSecret);

                var response = await Utils.PostData(AuthenticationManager.AccessTokenUrl, sigBaseStringParams + "&oauth_signature=" + Uri.EscapeDataString(signature));

                Debug.WriteLine("Access Token Response: " + response);

                if (!string.IsNullOrEmpty(response)) {
                    foreach (var x in Utils.ParseResponseString(response)) {
                        switch (x.Key) {
                            case "oauth_token":
                                Config.OAuthToken = x.Value;
                                Debug.WriteLine("OAuth Access Token: " + Config.OAuthToken);
                                break;
                            case "oauth_token_secret":
                                Config.OAuthTokenSecret = x.Value;
                                Debug.WriteLine("OAuth Access Token Secret: " + Config.OAuthTokenSecret);
                                break;
                        }
                    }
                    if (!string.IsNullOrEmpty(Config.OAuthToken) || !string.IsNullOrEmpty(Config.OAuthTokenSecret)) {
                        return true;
                    }
                }
                return false;
            }

            public static async Task<string> XAuthAccessToken(string username, string password) {
                string nonce = Utils.GetNonce();
                string timeStamp = Utils.GetTimeStamp();

                string sigBaseStringParams = new SortedDictionary<string, string> {
                {"oauth_consumer_key", Config.ConsumerKey},
                {"oauth_nonce", nonce},
                {"oauth_signature_method", "HMAC-SHA1"},
                {"oauth_timestamp", timeStamp},
                {"oauth_version", "1.0"},
                {"x_auth_mode", "client_auth"},
                {"x_auth_password", RequestHandler.UrlEncode(Uri.EscapeDataString(password))},
                {"x_auth_username", Uri.EscapeDataString(username)}}.Select(kv => kv.Key + "=" + kv.Value).Aggregate((i, j) => i + "&" + j);

                string sigBaseString = "POST&" + Uri.EscapeDataString(AuthenticationManager.XAauthAccessTokenUrl) + "&" + Uri.EscapeDataString(sigBaseStringParams);

                string signature = Utils.GetSignature(sigBaseString, Config.ConsumerSecretKey);

                var response = await Utils.XAuthPostData(AuthenticationManager.XAauthAccessTokenUrl, sigBaseStringParams + "&oauth_signature=" + Uri.EscapeDataString(signature));

                Debug.WriteLine("Access Token Response: " + response);

                if (!string.IsNullOrEmpty(response)) {
                    if (response.Contains("oauth_token")) {
                        foreach (var x in Utils.ParseResponseString(response)) {
                            if (x.Key == "oauth_token") {
                                if (Config.AccountTokens.Count == 0) {
                                    Config.AccountTokens.Add(username, x.Value);
                                    Config.OAuthToken = x.Value;
                                } else {
                                    Config.AccountTokens.Add(username, x.Value);
                                }
                                Config.SelectedAccount = username;
                                Debug.WriteLine("OAuth Access Token: " + Config.OAuthToken);
                            } else if (x.Key == "oauth_token_secret") {
                                if (Config.AccountSecretTokens.Count == 0) {
                                    Config.AccountSecretTokens.Add(username, x.Value);
                                    Config.OAuthTokenSecret = x.Value;
                                } else {
                                    Config.AccountSecretTokens.Add(username, x.Value);
                                }
                                Config.SelectedAccount = username;
                                Debug.WriteLine("OAuth Access Token Secret: " + Config.OAuthTokenSecret);
                            }
                        }
                        if (Config.AccountTokens.ContainsKey(username) || Config.AccountSecretTokens.ContainsKey(username)) {
                            //Data.AppSettings.UpdateSettings();
                            Config.SaveLocalAccountStore();
                            return "Okay";
                        }
                    } else {
                        if (response == "Invalid xAuth credentials.")
                            return "Invalid email or password.";
                        return "Something went wrong.";
                    }
                }
                return "Failed";
            }
        }
    }
}
