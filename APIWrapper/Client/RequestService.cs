using APIWrapper.AuthenticationManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace APIWrapper.Client {
	public class RequestService {

		//private static string TAG = "RequestService";

		private CancellationTokenSource CancellationToken = new CancellationTokenSource();

		public static async Task<HttpResponseMessage> GET(string URL, string additionalParameters = "", CancellationTokenSource cToken = null) {
			try {
				using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })) {
					client.MaxResponseContentBufferSize = int.MaxValue;
					client.DefaultRequestHeaders.ExpectContinue = false;

					string nonce = Authentication.Utils.GetNonce();
					string timeStamp = Authentication.Utils.GetTimeStamp();
					var requestParameters = new RequestParameters(nonce, timeStamp, additionalParameters);
					var signatureParameters = "GET&" + Authentication.Utils.UrlEncode(URL) + "&" +
						requestParameters.Encode();

					var authenticationData = RequestParameters.AuthenticationData(nonce, timeStamp,
						Authentication.Utils.GenerateSignature(signatureParameters));

					var requestMessage = new HttpRequestMessage() {
						Method = HttpMethod.Get,
						RequestUri = new Uri(URL +
						(!string.IsNullOrEmpty(additionalParameters) ? "?" + additionalParameters : ""))
					};

					requestMessage.Headers.Add("Accept-Encoding", "gzip,deflate");
					requestMessage.Headers.Add("User-Agent", "Android");
					requestMessage.Headers.Add("X-Version", "tablet/3.8.2/0/4.0.4");
					requestMessage.Headers.Add("X-YUser-Agent", "Dalvik/1.6.0 (Linux; U; Android 4.0.4; LePanII Build/IMM76D)/Tumblr/tablet/3.8.2/0/4.0.4");
					requestMessage.Headers.Add("X-Features", "AUTO_PLAY_VIDEO=B&SSL=B&TOUR_GUIDE=A&POST_ACTION_BUTTON=A");
					requestMessage.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authenticationData);
					requestMessage.Headers.IfModifiedSince = DateTime.UtcNow.Date;

					return (cToken != null) ? await client.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, cToken.Token) :
						await client.SendAsync(requestMessage);
				}
			} catch (Exception ex) {
				return new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound };
			}
		}

		public static async Task<HttpResponseMessage> POST(string URL, string parameters = "") {
			try {
				using (var client = new HttpClient() { MaxResponseContentBufferSize = int.MaxValue }) {
					client.DefaultRequestHeaders.ExpectContinue = false;

					string nonce = Authentication.Utils.GetNonce();
					string timeStamp = Authentication.Utils.GetTimeStamp();

					var requestParameters = "POST&" + Authentication.Utils.UrlEncode(URL) + "&" +
						new RequestParameters(nonce, timeStamp, parameters).Encode();

					var authenticationData = RequestParameters.AuthenticationData(nonce, timeStamp,
						Authentication.Utils.GenerateSignature(requestParameters));

					var requestMessage = new HttpRequestMessage() {
						Method = HttpMethod.Post,
						RequestUri = new Uri(URL)
					};

					requestMessage.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authenticationData);
					requestMessage.Headers.IfModifiedSince = DateTime.UtcNow;
					requestMessage.Content = new StringContent(parameters);
					requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

					return await client.SendAsync(requestMessage);
				}
			} catch (Exception ex) {
				return new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound };
			}
		}

		public static async Task<HttpResponseMessage> POST(string URL, StorageFile file, string parameters = "") {
			try {
				using (var client = new HttpClient() { MaxResponseContentBufferSize = int.MaxValue }) {
					client.DefaultRequestHeaders.ExpectContinue = false;

					string nonce = Authentication.Utils.GetNonce();
					string timeStamp = Authentication.Utils.GetTimeStamp();

					var requestParameters = "POST&" + Authentication.Utils.UrlEncode(URL) + "&" +
						new RequestParameters(nonce, timeStamp, parameters).EncodeValues();

					var authenticationData = RequestParameters.AuthenticationData(nonce, timeStamp,
						Authentication.Utils.GenerateSignature(requestParameters));

					var requestMessage = new HttpRequestMessage() {
						Method = HttpMethod.Post,
						RequestUri = new Uri(URL)
					};

					//foreach (var file in mediaFiles) {
					//    byte[] fileBytes = null;
					//    using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync()) {
					//        fileBytes = new byte[stream.Size];
					//        using (DataReader reader = new DataReader(stream)) {
					//            await reader.LoadAsync((uint)stream.Size);
					//            reader.ReadBytes(fileBytes);
					//        }
					//    }
					//}

					requestMessage.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authenticationData);
					string boundary = "---------------------------7dd36f1721dc0";
					requestMessage.Content = new ByteArrayContent(await BuildByteArray(boundary, file,
						new RequestParameters(null, null, parameters)));
					requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
					requestMessage.Content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", boundary));

					return await client.SendAsync(requestMessage);
				}
			} catch (Exception ex) {
				return new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound };
			}
		}
		private static async Task<byte[]> BuildByteArray(string boundary, StorageFile file, RequestParameters parameters) {
			var paramsToMFC = string.Join("--" + boundary + "\r\n",
				parameters.Select(p =>
				string.Format("Content-Disposition: form-data; name=\"{0}\"\r\n" +
				"\r\n" +
				"{1}\r\n"
				, p.Key, p.Value))
				);

			byte[] fileBytes = null;

			using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync()) {
				fileBytes = new byte[stream.Size];
				using (DataReader reader = new DataReader(stream)) {
					await reader.LoadAsync((uint)stream.Size);
					reader.ReadBytes(fileBytes);
				}
			}

			byte[] firstBytes = Encoding.UTF8.GetBytes(string.Format(
				"--{1}\r\n" +
				paramsToMFC +
				"--{1}\r\n" +
				"Content-Disposition: form-data; name=\"{2}\"; filename=\"{3}\"\r\n" +
				"Content-Type: {4}\r\n" +
				"\r\n",
				boundary,
				boundary,
				"data[0]",
				file.Name,
				file.ContentType));

			byte[] lastBytes = Encoding.UTF8.GetBytes(String.Format(
				"\r\n" +
				"--{0}--\r\n",
				boundary));

			int contentLength = firstBytes.Length + fileBytes.Length + lastBytes.Length;
			byte[] contentBytes = new byte[contentLength];

			// Join the 3 arrays into 1.
			Array.Copy(firstBytes, 0,
				contentBytes, 0,
				firstBytes.Length);
			Array.Copy(fileBytes, 0,
				contentBytes, firstBytes.Length,
				fileBytes.Length);
			Array.Copy(
				lastBytes, 0,
				contentBytes, firstBytes.Length + fileBytes.Length,
				lastBytes.Length);

			return contentBytes;

		}

		private static byte[] BuildByteArray(string name, string fileName, byte[] fileBytes, string boundary) {
			// Create multipart/form-data headers.
			byte[] firstBytes = Encoding.UTF8.GetBytes(String.Format(
				"--{0}\r\n" +
				"Content-Disposition: form-data; name=\"type\"\r\n" +
				"\r\n" +
				"photo\r\n" +
				"--{1}\r\n" +
				"Content-Disposition: form-data; name=\"{2}\"; filename=\"{3}\"\r\n" +
				"Content-Type: image/jpeg\r\n" +
				"\r\n",
				boundary,
				boundary,
				name,
				fileName));

			byte[] lastBytes = Encoding.UTF8.GetBytes(String.Format(
				"\r\n" +
				"--{0}--\r\n",
				boundary));

			int contentLength = firstBytes.Length + fileBytes.Length + lastBytes.Length;
			byte[] contentBytes = new byte[contentLength];

			// Join the 3 arrays into 1.
			Array.Copy(firstBytes, 0,
				contentBytes, 0,
				firstBytes.Length);
			Array.Copy(fileBytes, 0,
				contentBytes, firstBytes.Length,
				fileBytes.Length);
			Array.Copy(
				lastBytes, 0,
				contentBytes, firstBytes.Length + fileBytes.Length,
				lastBytes.Length);

			return contentBytes;
		}
	}

	internal class RequestParameters : SortedDictionary<string, string> {
		/// <summary>
		/// Generates the default request parameters for a API request.
		/// </summary>
		/// <param name="nonce">A token identifier for a request instance.</param>
		/// <param name="timestamp">A timestamp for the request instance.</param>
		/// <param name="additionalParameters">Other parameters that are to be added to the request</param>
		public RequestParameters(string nonce, string timestamp, string additionalParameters = null) {
			if (!string.IsNullOrWhiteSpace(nonce) && !string.IsNullOrWhiteSpace(timestamp))
				AddDefaults(nonce, timestamp);

			if (!string.IsNullOrWhiteSpace(additionalParameters)) {
				foreach (var kv in additionalParameters.Split('&').ToDictionary(p => p.Substring(0, p.IndexOf('=')),
				p => p.Substring(p.IndexOf('=') + 1))) {
					this.Add(kv.Key, kv.Value);
				}
			}
		}

		private void AddDefaults(string nonce, string timestamp) {
			this.Add("oauth_consumer_key", Authentication.ConsumerKey);
			this.Add("oauth_nonce", nonce);
			this.Add("oauth_signature_method", "HMAC-SHA1");
			this.Add("oauth_timestamp", timestamp);
			this.Add("oauth_token", Authentication.Token);
			this.Add("oauth_verifier", Authentication.TokenVerifier);
			this.Add("oauth_version", "1.0");
		}

		/// <summary>
		/// Encodes the API parameters into a string.
		/// </summary>
		/// <returns>The encoded parameters.</returns>
		public string Encode() {
			return Authentication.Utils.UrlEncode(string.Join("&",
				this.Select(p => string.Format("{0}={1}", p.Key, p.Value))));
		}

		public string EncodeValues() {
			return Authentication.Utils.UrlEncode(string.Join("&",
				this.Select(p => string.Format("{0}={1}", p.Key, Authentication.Utils.UrlEncode(p.Value)))));
		}

		public static string AuthenticationData(string nonce, string timestamp, string signature) {
			return "oauth_consumer_key=\"" + Authentication.ConsumerKey +
				"\", oauth_nonce=\"" + nonce +
				"\", oauth_signature=\"" + Authentication.Utils.UrlEncode(signature) +
				"\", oauth_signature_method=\"HMAC-SHA1" +
				"\", oauth_timestamp=\"" + timestamp +
				"\", oauth_token=\"" + Authentication.Token +
				"\", oauth_verifier=\"" + Authentication.TokenVerifier +
				"\", oauth_version=\"1.0\"";
		}
	};
}
