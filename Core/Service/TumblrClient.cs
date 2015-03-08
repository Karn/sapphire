using Core.Service.Exceptions;
using Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service {
	public class TumblrClient {

		private static TumblrClient _TumblrClient { get; set; }

		private string ConsumerKey { get; set; }
		private string ConsumerSecret { get; set; }

		public TumblrClient() {
			
		}

		public TumblrClient(string consumerKey, string consumerSecret) {
			Log.i("Creating client instance with keys.");
			this.ConsumerKey = consumerKey;
			this.ConsumerSecret = consumerSecret;
		}

		public static TumblrClient GetInstance() {
			if (_TumblrClient == null)
				_TumblrClient = new TumblrClient();
			return _TumblrClient;
		}
	}
}
