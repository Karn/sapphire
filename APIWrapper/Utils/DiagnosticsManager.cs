using APIWrapper.Content;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace APIWrapper.Utils {
    public class DiagnosticsManager {

        public static Application current_app;
        public static bool DiagnosticsRunning = false;

        public DiagnosticsManager(Application current) {
            if (UserStore.EnableAnalytics && current != null) {

                // Analytics.GetTracker().SetStartSession(true);
                //BugSenseHandler.Instance.InitAndStartSession(new ExceptionManager(current), "d18c8ae5");
                //////Sets user identifier 
                //if (AuthenticationManager.Authentication.AuthenticatedTokens.Count != 0)
                //    BugSenseHandler.Instance.UserIdentifier = AuthenticationManager.Authentication.AuthenticatedTokens.First().Key;
                //else
                //    BugSenseHandler.Instance.UserIdentifier = "Unknown";
                DiagnosticsRunning = true;
                //Debug.WriteLine("[DiagnosticsManager]: Initialized BugSense session. User identifier is: " + BugSenseHandler.Instance.UserIdentifier);
            }
        }

        public static void EnableDiagnostics() {
            //if (!DiagnosticsRunning) {
            //    new DiagnosticsManager(current_app);
            //}
        }

        public static void LogException(Exception ex, string TAG = "Unknown", string message = "No message.") {
            Debug.WriteLine("[" + TAG + "]: " + message + ((ex != null) ? " [" + ex.ToString() + "]" : ""));
            //if (DiagnosticsRunning && ex != null)
            //    BugSenseHandler.Instance.LogException(ex, "[" + TAG + "]", message);
        }

    }
}
