using System.Diagnostics;

namespace Core.Utils {
	/// <summary>
	/// Class to provide output and diagnostic logging for application wide events.s
	/// </summary>
	public class Log {
		/// <summary>
		/// Debugging output.
		/// </summary>
		/// <param name="message">Message to describe the intended diagnostic message.</param>
		public static void d(string message) {
			Debug.WriteLine("[DIAGNOSTICS]: " + message);
		}

		/// <summary>
		/// Error output.
		/// </summary>
		/// <param name="message">Message to describe an error.</param>
		public static void e(string message) {
			Debug.WriteLine("[ERROR]: " + message);
		}

		/// <summary>
		/// Information output.
		/// </summary>
		/// <param name="message">Message to describe the given information.</param>
		public static void i(string message) {
			Debug.WriteLine("[INFO]: " + message);
		}

		/// <summary>
		/// What a terrible failiure (massive error).
		/// </summary>
		/// <param name="message">Message to describe error.</param>
		public static void wtf(string message) {
			Debug.WriteLine("[WTF]: " + message);
		}
	}
}
