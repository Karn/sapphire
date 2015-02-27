using GoogleAnalytics.Core;
using System;
using System.Diagnostics;

namespace Sapphire.Utils {
	public class Analytics {

		private static Tracker _Tracker { get; set; }

		private static Analytics _Analytics = null;

		//Turn this into a singleton

		//Log exceptions
		//Log events
		//Log infos

		public Analytics() {
			if (_Tracker == null) {
				_Tracker = GoogleAnalytics.EasyTracker.GetTracker();
				_Tracker.SendEvent("[Diagnostic]", "Start Op", "Began Google Analytics", 0);
			}
		}

		public static Analytics GetInstance() {
			if (_Analytics == null) {
				_Analytics = new Analytics();
			}
			return _Analytics;
		}

		public static void RegisterView(string pageName) {
			_Tracker.SendView(pageName);
		}

		public static void LogException(Exception ex, string TAG = "Unknown", string message = "No message.") {
			var error = "[" + TAG + "]: " + message + ((ex != null) ? " [" + ex.ToString() + "]" : "");
			Debug.WriteLine(error);
			//_Tracker.SendException(error, false);
		}

		private void Exception(Exception ex, string tAG, string v) {
			throw new NotImplementedException();
		}
	}
}
