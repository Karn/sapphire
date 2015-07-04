using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace Core.AuthenticationManager {
    public class AuthenticationUtilities {

        public string GetNonce() {
			return (new Random()).Next(123400, 9999999).ToString();
		}

        public string GetTimeStamp() {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        public string UrlEncode(string str) {
            str = WebUtility.UrlEncode(str);

            str = str.Replace("'", "%27").Replace("(", "%28").Replace(")", "%29").Replace("*", "%2A").Replace("!", "%21").Replace("%7e", "~").Replace("+", "%20");

            StringBuilder sbuilder = new StringBuilder(str);
            for (int i = 0; i < sbuilder.Length; i++) {
                if (sbuilder[i] == '%') {
                    if (Char.IsLetter(sbuilder[i + 1]) || Char.IsLetter(sbuilder[i + 2])) {
                        sbuilder[i + 1] = Char.ToUpper(sbuilder[i + 1]);
                        sbuilder[i + 2] = Char.ToUpper(sbuilder[i + 2]);
                    }
                }
            }

            return sbuilder.ToString();
        }

        public string GenerateSignature(string signatureBaseString, string tokenSecret = "") {
            IBuffer KeyMaterial = CryptographicBuffer.ConvertStringToBinary(Authentication.ConsumerSecretKey + "&" + (string.IsNullOrWhiteSpace(tokenSecret) ? Authentication.TokenSecret : ""), BinaryStringEncoding.Utf8);
            MacAlgorithmProvider HmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            CryptographicKey MacKey = HmacSha1Provider.CreateKey(KeyMaterial);
            IBuffer DataToBeSigned = CryptographicBuffer.ConvertStringToBinary(signatureBaseString, BinaryStringEncoding.Utf8);
            IBuffer SignatureBuffer = CryptographicEngine.Sign(MacKey, DataToBeSigned);
            return CryptographicBuffer.EncodeToBase64String(SignatureBuffer);
        }

        public async Task<string> PostAuthenticationData(string url, string postData) {
            try {
                using (var httpClient = new HttpClient()) {
                    httpClient.MaxResponseContentBufferSize = int.MaxValue;
                    httpClient.DefaultRequestHeaders.ExpectContinue = false;
                    HttpRequestMessage requestMsg = new HttpRequestMessage();

                    requestMsg.Content = new StringContent(postData);
                    requestMsg.Method = new HttpMethod("POST");
                    requestMsg.RequestUri = new Uri(url, UriKind.Absolute);
                    requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    var response = await httpClient.SendAsync(requestMsg);
                    return await response.Content.ReadAsStringAsync();
                }
            } catch (Exception ex) {
                return null;
            }
        }
    }
}
