using API.Authentication;
using API.Data;
using API.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace API {
    public class RequestBuilder {
        /// <summary>
        /// New instance of authentication utilities
        /// </summary>
        //public static AuthenticationUtilities OAuthUtil = new AuthenticationUtilities();

        /// <summary> 
        /// https://dev.twitter.com/docs/auth/creating-signature 
        /// </summary> 
        /// <param name="queryParameters">e.g., count=5&trim_user=true</param> 
        /// <returns></returns> 
        private static string getParamsBaseString(string queryParamsString, string nonce, string timeStamp) {
            // these parameters are required in every request api 
            var baseStringParams = new SortedDictionary<string, string>{ 
                {"oauth_consumer_key", Config.ConsumerKey}, 
                {"oauth_nonce", nonce}, 
                {"oauth_signature_method", "HMAC-SHA1"}, 
                {"oauth_timestamp", timeStamp}, 
                {"oauth_token", Config.OAuthToken}, 
                {"oauth_verifier", Config.OAuthVerifier}, 
                {"oauth_version", "1.0"},                 
            };

            // put each parameter into dictionary 
            if (queryParamsString != "") {
                var queryParams = queryParamsString
                                    .Split('&')
                                    .ToDictionary(p => p.Substring(0, p.IndexOf('=')), p => p.Substring(p.IndexOf('=') + 1));

                foreach (var kv in queryParams) {
                    Debug.WriteLine(kv.Key + " " + kv.Value);
                    baseStringParams.Add(kv.Key, kv.Value);
                }
            }

            // The OAuth spec says to sort lexigraphically, which is the default alphabetical sort for many libraries. 
            var ret = baseStringParams
                .Select(kv => kv.Key + "=" + kv.Value)
                .Aggregate((i, j) => i + "&" + j);

            return ret;
        }

        public static async Task<string> GetAPI(string url, string parameters = null) {
            string nonce = AuthenticationManager.Utils.GetNonce();
            string timeStamp = AuthenticationManager.Utils.GetTimeStamp();

            try {
                HttpClient httpClient = new HttpClient();
                httpClient.MaxResponseContentBufferSize = int.MaxValue;
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                HttpRequestMessage requestMsg = new HttpRequestMessage();
                requestMsg.Method = new HttpMethod("GET");

                var qParams = string.IsNullOrEmpty(parameters) ? "" : parameters;
                var urlWithParams = url + "?" + qParams;

                // HttpClient uses full url 
                requestMsg.RequestUri = new Uri(urlWithParams);

                string paramsBaseString = getParamsBaseString(qParams, nonce, timeStamp);

                string sigBaseString = "GET&";
                sigBaseString += AuthenticationManager.Utils.UrlEncode(url) + "&" + AuthenticationManager.Utils.UrlEncode(paramsBaseString);

                string signature = AuthenticationManager.Utils.UrlEncode(AuthenticationManager.Utils.GetSignature(sigBaseString, Config.ConsumerSecretKey, Config.OAuthTokenSecret)).Replace("+", "%20").Replace("/", "%2F");

                DebugHandler.ErrorLog.Add("Encoded signature: " + signature);

                string data = "oauth_consumer_key=\"" + Config.ConsumerKey +
                              "\", oauth_nonce=\"" + nonce +
                              "\", oauth_signature=\"" + signature +
                              "\", oauth_signature_method=\"HMAC-SHA1\", oauth_timestamp=\"" + timeStamp +
                              "\", oauth_token=\"" + Config.OAuthToken +
                              "\", oauth_verifier=\"" + Config.OAuthVerifier +
                              "\", oauth_version=\"1.0\"";
                requestMsg.Headers.Authorization = new AuthenticationHeaderValue("OAuth", data);
                requestMsg.Headers.IfModifiedSince = DateTime.UtcNow;

                var response = await httpClient.SendAsync(requestMsg);
                var text = await response.Content.ReadAsStringAsync();

                Debug.WriteLine("[API REQUEST] (" + urlWithParams + "): " + text);
                return text.ToString();
            } catch (Exception Err) {
                DebugHandler.ErrorLog.Add("Unable to make POST request: " + Err.StackTrace);
            }
            return "";
        }

        public static async Task<string> PostAPI(string url, string parameters) {

            string nonce = AuthenticationManager.Utils.GetNonce();
            string timeStamp = AuthenticationManager.Utils.GetTimeStamp();

            try {
                HttpClient httpClient = new HttpClient();
                httpClient.MaxResponseContentBufferSize = int.MaxValue;
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                HttpRequestMessage requestMsg = new HttpRequestMessage();
                requestMsg.Method = new HttpMethod("POST");

                var qParams = parameters;
                var urlWithParams = url + "?" + qParams;
                // HttpClient uses full url 
                requestMsg.RequestUri = new Uri(url);

                string paramsBaseString = getParamsBaseString(qParams, nonce, timeStamp);

                string sigBaseString = "POST&";
                // signature base string uses base url 
                //sigBaseString += Uri.EscapeDataString(url) + "&" + Uri.EscapeDataString(paramsBaseString);
                sigBaseString += AuthenticationManager.Utils.UrlEncode(url) + "&" + AuthenticationManager.Utils.UrlEncode(paramsBaseString);

                //string signature = OAuthUtil.GetSignature(sigBaseString, Config.ConsumerSecretKey, Config.OAuthTokenSecret);
                string signature = AuthenticationManager.Utils.UrlEncode(AuthenticationManager.Utils.GetSignature(sigBaseString,
                    Config.ConsumerSecretKey,
                    Config.OAuthTokenSecret)).Replace("+", "%20").Replace("/", "%2F");

                DebugHandler.ErrorLog.Add("Encoded signature: " + signature);

                string data = "oauth_consumer_key=\"" + Config.ConsumerKey +
                              "\", oauth_nonce=\"" + nonce +
                              "\", oauth_signature=\"" + signature +
                              "\", oauth_signature_method=\"HMAC-SHA1\", oauth_timestamp=\"" + timeStamp +
                              "\", oauth_token=\"" + Config.OAuthToken +
                              "\", oauth_verifier=\"" + Config.OAuthVerifier +
                              "\", oauth_version=\"1.0\"";
                requestMsg.Headers.Authorization = new AuthenticationHeaderValue("OAuth", data);
                requestMsg.Content = new StringContent(parameters);
                requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var response = await httpClient.SendAsync(requestMsg);
                var text = await response.Content.ReadAsStringAsync();

                Debug.WriteLine("[API POST] (" + url + "): " + text);
                return text.ToString();
            } catch (Exception Err) {
                DebugHandler.ErrorLog.Add("Unable to make POST request: " + Err.StackTrace);
            }
            return null;
        }
    }
}