using System;
using System.Diagnostics;

namespace Core.Analytics {
    public class AnalyticsManager {

        public AnalyticsManager() {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("[Diagnostic]", "Start Op", "Began Google Analytics", 0);
        }

        public static void RegisterView(string pageName) {
            GoogleAnalytics.EasyTracker.GetTracker().SendView(pageName);
        }

        public static void LogException(Exception ex, string TAG = "Unknown", string message = "No message.") {
            var error = "[" + TAG + "]: " + message + ((ex != null) ? " [" + ex.ToString() + "]" : "");
            GoogleAnalytics.EasyTracker.GetTracker().SendException(error, false);
            //if (DiagnosticsRunning && ex != null)
            //    BugSenseHandler.Instance.LogException(ex, "[" + TAG + "]", message);
        }

        public static void EnableDiagnostics() {
            //if (!DiagnosticsRunning) {
            //    new DiagnosticsManager(current_app);
            //}
        }

    }
}
