using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace APIWrapper.AuthenticationManager {
    public class AuthenticationUtilities {

        public bool NetworkAvailable() {
            if (NetworkInterface.GetIsNetworkAvailable()) {
                if (!string.IsNullOrEmpty(Authentication.Token) && !string.IsNullOrEmpty(Authentication.Token))
                    return true;
            }
            return false;
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

        public string UrlEncode(string value) {
            var unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
            var result = new StringBuilder();

            foreach (char symbol in value) {
                if (unreservedChars.IndexOf(symbol) != -1) {
                    result.Append(symbol);
                } else {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }

            return result.ToString();
        }

        public string GenerateSignature(string signatureBaseString, string consumerSecretKey, string tokenSecret = null) {
            IBuffer KeyMaterial = CryptographicBuffer.ConvertStringToBinary(consumerSecretKey + "&" + tokenSecret, BinaryStringEncoding.Utf8);
            MacAlgorithmProvider HmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            CryptographicKey MacKey = HmacSha1Provider.CreateKey(KeyMaterial);
            IBuffer DataToBeSigned = CryptographicBuffer.ConvertStringToBinary(signatureBaseString, BinaryStringEncoding.Utf8);
            IBuffer SignatureBuffer = CryptographicEngine.Sign(MacKey, DataToBeSigned);
            return CryptographicBuffer.EncodeToBase64String(SignatureBuffer);
        }

        public async Task<string> PostAuthenticationData(string url, string postData) {
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

            } catch (Exception PostAuthenticationException) {
                Debug.WriteLine(PostAuthenticationException.StackTrace);
                return null;
            }
        }
    }
}
