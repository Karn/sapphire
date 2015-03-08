using Core.AuthenticationManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Requests {
	public class RequestParameters : SortedDictionary<string, string> {
		public override string ToString() {
			return string.Join("&",
				this.Select(p => string.Format("{0}={1}", p.Key, Authentication.Utils.UrlEncode(p.Value))));
		}

		public void AddDefaults(string nonce, string timestamp) {
			this.Add("oauth_consumer_key", Authentication.ConsumerKey);
			this.Add("oauth_nonce", nonce);
			this.Add("oauth_signature_method", "HMAC-SHA1");
			this.Add("oauth_timestamp", timestamp);
			this.Add("oauth_token", Authentication.Token);
			this.Add("oauth_verifier", Authentication.TokenVerifier);
			this.Add("oauth_version", "1.0");
		}
	}
}
