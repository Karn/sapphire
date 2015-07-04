using Core.Content.Model;
using Core.Content.Model.DatabaseHelpers;
using Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.Storage;

namespace Core.AuthenticationManager {

    /// <summary>
    /// This 
    /// </summary>
    public class Authentication {

        //OAuth URI Endpoints
        public static readonly string RequestTokenURI = "http://www.tumblr.com/oauth/request_token";
        public static readonly string AuthorizationURI = "http://www.tumblr.com/oauth/authorize";
        public static readonly string AccessTokenURI = "http://www.tumblr.com/oauth/access_token";
        //XAuth URI Endpoints
        public static readonly string SecureAccessTokenURI = "https://www.tumblr.com/oauth/access_token";
        //Keys
        public static readonly string ConsumerKey = "BUHsuO5U9DF42uJtc8QTZlOmnUaJmBJGuU1efURxeklbdiLn9L";
        public static readonly string ConsumerSecretKey = "olOu3aRBCdqCuMFm8fmzNjMAWmICADSIuXWTnVSFng1ZcLU1cV";

        public static string Token {
            get {
                if (LocalSettings.Values["Token"] != null)
                    return LocalSettings.Values["Token"].ToString();
                return "";
            }
            set {
                LocalSettings.Values["Token"] = value;
            }
        }
        public static string TokenSecret {
            get {
                if (LocalSettings.Values["TokenSecret"] != null)
                    return LocalSettings.Values["TokenSecret"].ToString();
                return "";
            }
            set {
                LocalSettings.Values["TokenSecret"] = value;
            }
        }
        public static string TokenVerifier {
            get {
                if (LocalSettings.Values["TokenVerifier"] != null)
                    return LocalSettings.Values["TokenVerifier"].ToString();
                return "";
            }
            set {
                LocalSettings.Values["TokenVerifier"] = value;
            }
        }

        //Current account
        public static string SelectedAccount {
            get {
                if (LocalSettings.Values["SelectedAccount"] != null)
                    return LocalSettings.Values["SelectedAccount"].ToString();
                return "";
            }
            set {
                LocalSettings.Values["SelectedAccount"] = value;
            }
        }

        //Authentication Utilities
        public static AuthenticationUtilities Utils = new AuthenticationUtilities();

        //Local Application Storage Container
        public static ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        //Request Token
        public static async Task<string> RequestAccessToken(string userName, string password) {
            if (NetworkInterface.GetIsNetworkAvailable()) {
                var nonce = Utils.GetNonce();
                var timeStamp = Utils.GetTimeStamp();

                var signatureParameters = new SortedDictionary<string, string> {
                { "oauth_consumer_key", ConsumerKey},
                {"oauth_nonce", nonce},
                {"oauth_signature_method", "HMAC-SHA1"},
                {"oauth_timestamp", timeStamp},
                {"oauth_version", "1.0"},
                {"x_auth_mode", "client_auth"},
                {"x_auth_password", Authentication.Utils.UrlEncode(password)},
                {"x_auth_username", Authentication.Utils.UrlEncode(userName)}}
                .Select(kv => kv.Key + "=" + kv.Value).Aggregate((i, j) => i + "&" + j);

                var signatureString = "POST&" + Uri.EscapeDataString(SecureAccessTokenURI) + "&" +
                    Uri.EscapeDataString(signatureParameters);
                var signature = Utils.GenerateSignature(signatureString, "auth");

                var response = await Utils.PostAuthenticationData(SecureAccessTokenURI,
                    signatureParameters + "&oauth_signature=" + Uri.EscapeDataString(signature));

                Debug.WriteLine("Access Token Response: " + response);

                //Parse response data
                if (!string.IsNullOrEmpty(response) && response.Contains("oauth_token") && response.Contains("oauth_token_secret")) {
                    var tokens = response.Split('&');
                    var accessToken = tokens[0].Split('=');
                    var accessTokenSecret = tokens[1].Split('=');

                    //Add token to token dictionary
                    Account user = new Account();
                    user.AccountEmail = userName.Substring(0, userName.IndexOf('@'));
                    user.AuthenticatedToken = accessToken[1];
                    user.AuthenticationTokenSecret = accessTokenSecret[1];

                    if (DatabaseController.GetInstance().GetAccount(user.AccountEmail) != null)
                        DatabaseController.GetInstance().UpdateAccount(user);
                    else
                        DatabaseController.GetInstance().AddAccount(user);

                    if (DatabaseController.GetInstance().GetAccounts().Count == 1) {
                        Token = accessToken[1];
                        TokenSecret = accessTokenSecret[1];
                        SelectedAccount = user.AccountEmail;
                    }
                    
                    return "OK";
                } else if (response == "Invalid xAuth credentials.") {
                    return "We were unable to sign you in. Please check your username/password.";
                }
            } else {
                return "No Network";
            }
            return "Error";
        }
    }
}
