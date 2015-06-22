using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Requests {

	/// <summary>
	/// AuthenticateUser handles the process of authntication via the API.
	/// </summary>
	public class AuthenticateUser {

		/// <summary>
		/// Instance of AuthenticateUser object.
		/// </summary>
		private static AuthenticateUser _AuthenticateUser = null;

		/// <summary>
		/// Initialize an instance of AuthenticateUser.
		/// </summary>
		public AuthenticateUser() {

		}

		/// <summary>
		/// Retrieve the AuthenticateUser instance. Create if null.
		/// </summary>
		/// <returns>Singleton instance of Authenticate user</returns>
		public static AuthenticateUser GetInstance() {
			if (_AuthenticateUser == null)
				_AuthenticateUser = new AuthenticateUser();
			return _AuthenticateUser;
		}


	}
}
