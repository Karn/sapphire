

using Core.Content;
using GoogleAnalytics.Core;
using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace Core.Utils {
	public class Analytics {

		private static EasClientDeviceInformation _DeviceInformation = new EasClientDeviceInformation();
		private static Analytics _Analytics { get; set; }
		private Tracker _Tracker { get; set; }



		public Analytics() {
			var trackerManager = new TrackerManager(new PlatformInfoProvider() {
				AnonymousClientId = AuthenticationManager.Authentication.SelectedAccount,
				UserAgent = string.Format("Mozilla/5.0 (Windows Phone 8.1; Android 4.2.1; {0}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.71 Mobile Safari/537.36 Edge/12.0",
				_DeviceInformation.SystemManufacturer.ToString(),
				_DeviceInformation.SystemSku.ToString().Split('_')[0])
			});

			_Tracker = trackerManager.GetTracker("UA-58464214-1");
			_Tracker.AppName = "Sapphire.";
			_Tracker.AppVersion = string.Format("{0}.{1}.{2}.{3}",
				Package.Current.Id.Version.Major.ToString(),
				Package.Current.Id.Version.Minor.ToString(),
				Package.Current.Id.Version.Build.ToString(),
				Package.Current.Id.Version.Revision.ToString());
		}

		public static Analytics GetInstance() {
			if (_Analytics == null)
				_Analytics = new Analytics();
			return _Analytics;
		}

		public void SendEvent(string str) {
			_Tracker.SendEvent("Diagnostic", str, "Debug", 0);
		}

		public void SendView(string name) {
			_Tracker.SendView(name);
		}

		public void ReportException(string message) {
			_Tracker.SendException(message, false);
		}


	}
}
