using APIWrapper.AuthenticationManager;
using APIWrapper.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace APIWrapper.Client {
    public class RequestBuilder {

        public static string TAG = "RequestBuilder";

        private static string GenerateParameterString(string additionalParameters, string nonce, string timeStamp) {
            // these parameters are required in every request api 
            var defaultParameters = new SortedDictionary<string, string>{
                {"oauth_consumer_key", Authentication.ConsumerKey},
                {"oauth_nonce", nonce},
                {"oauth_signature_method", "HMAC-SHA1"},
                {"oauth_timestamp", timeStamp},
                {"oauth_token", Authentication.Token},
                {"oauth_verifier", Authentication.TokenVerifier},
                {"oauth_version", "1.0"},
            };

            // put each parameter into dictionary 
            if (additionalParameters != "") {
                foreach (var kv in additionalParameters.Split('&').ToDictionary(p => p.Substring(0, p.IndexOf('=')),
                    p => p.Substring(p.IndexOf('=') + 1))) {
                    Debug.WriteLine(kv.Key + " " + kv.Value);
                    defaultParameters.Add(kv.Key, kv.Value);
                }
            }

            // The OAuth spec says to sort lexigraphically, which is the default alphabetical sort for many libraries. 
            var ret = defaultParameters
                .Select(kv => kv.Key + "=" + kv.Value)
                .Aggregate((i, j) => i + "&" + j);

            return ret;
        }

        public static async Task<string> GetAPI(string url, string parameters = null) {
            string nonce = Authentication.Utils.GetNonce();
            string timeStamp = Authentication.Utils.GetTimeStamp();

            try {
                HttpClient httpClient = new HttpClient();
                httpClient.MaxResponseContentBufferSize = int.MaxValue;
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                HttpRequestMessage requestMsg = new HttpRequestMessage();
                requestMsg.Method = new HttpMethod("GET");

                var urlWithParams = url + (!string.IsNullOrEmpty(parameters) ? "?" + parameters : "");

                // HttpClient uses full url 
                requestMsg.RequestUri = new Uri(urlWithParams);

                string signatureParameters = GenerateParameterString(string.IsNullOrEmpty(parameters) ? "" : parameters, nonce, timeStamp);

                string signatureString = "GET&" + Authentication.Utils.UrlEncode(url) + "&" + Authentication.Utils.UrlEncode(signatureParameters);

                string signature = Authentication.Utils.UrlEncode(Authentication.Utils.GenerateSignature(signatureString, Authentication.ConsumerSecretKey, APIWrapper.AuthenticationManager.Authentication.TokenSecret)).Replace("+", "%20").Replace("/", "%2F");

                var data = "oauth_consumer_key=\"" + Authentication.ConsumerKey +
                    "\", oauth_nonce=\"" + nonce +
                    "\", oauth_signature=\"" + signature +
                    "\", oauth_signature_method=\"HMAC-SHA1\", oauth_timestamp=\"" + timeStamp +
                    "\", oauth_token=\"" + Authentication.Token +
                    "\", oauth_verifier=\"" + Authentication.TokenVerifier +
                    "\", oauth_version=\"1.0\"";

                requestMsg.Headers.Authorization = new AuthenticationHeaderValue("OAuth", data);
                requestMsg.Headers.IfModifiedSince = DateTime.UtcNow;

                var response = await httpClient.SendAsync(requestMsg);
                var text = await response.Content.ReadAsStringAsync();

                Debug.WriteLine("[API REQUEST] (" + urlWithParams + "): " + text);
                return text.ToString();
            } catch (Exception ex) {
                DiagnosticsManager.LogException(ex, TAG, "Unable to make GET request.");
            }
            return "";
        }

        public static async Task<string> PostAPI(string url, string parameters) {

            string nonce = Authentication.Utils.GetNonce();
            string timeStamp = Authentication.Utils.GetTimeStamp();

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

                string paramsBaseString = GenerateParameterString(qParams, nonce, timeStamp);

                string sigBaseString = "POST&" + Authentication.Utils.UrlEncode(url) + "&" + Authentication.Utils.UrlEncode(paramsBaseString);

                string signature = Authentication.Utils.UrlEncode(Authentication.Utils.GenerateSignature(sigBaseString,
                    Authentication.ConsumerSecretKey,
                    Authentication.TokenSecret)).Replace("+", "%20").Replace("/", "%2F");

                string data = "oauth_consumer_key=\"" + Authentication.ConsumerKey +
                              "\", oauth_nonce=\"" + nonce +
                              "\", oauth_signature=\"" + signature +
                              "\", oauth_signature_method=\"HMAC-SHA1\", oauth_timestamp=\"" + timeStamp +
                              "\", oauth_token=\"" + Authentication.Token +
                              "\", oauth_verifier=\"" + Authentication.TokenVerifier +
                              "\", oauth_version=\"1.0\"";
                requestMsg.Headers.Authorization = new AuthenticationHeaderValue("OAuth", data);
                requestMsg.Content = new StringContent(parameters);
                requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var response = await httpClient.SendAsync(requestMsg);
                var text = await response.Content.ReadAsStringAsync();

                Debug.WriteLine("[API POST] (" + url + "): " + text);
                return text.ToString();
            } catch (Exception ex) {
                DiagnosticsManager.LogException(ex, TAG, "Unable to make POST request.");
            }
            return null;
        }
    }
}