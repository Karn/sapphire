using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Exceptions {
	public class TumblrExeception : Exception {
		public TumblrExeception() {
		}

		public TumblrExeception(string message) : base(message) {
		}

		public TumblrExeception(string message, Exception innerException) : base(message, innerException) {
		}
	}

	public class AuthorizationFailedException : TumblrExeception {
		public AuthorizationFailedException() {
		}

		public AuthorizationFailedException(string message) : base(message) {
		}

		public AuthorizationFailedException(string message, Exception innerException) : base(message, innerException) {
		}
	}

	public class NetworkFailureException : TumblrExeception {
		public NetworkFailureException() {
		}

		public NetworkFailureException(string message) : base(message) {
		}

		public NetworkFailureException(string message, Exception innerException) : base(message, innerException) {
		}
	}
}
