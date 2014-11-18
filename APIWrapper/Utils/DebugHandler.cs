using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIWrapper.Utils {
    public class DebugHandler {

        static Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation deviceInfo = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
        static string firmwareVersion = deviceInfo.SystemFirmwareVersion;

        static List<string> GeneralLog = new List<string>();

        static List<string> LocalLog = new List<string>();

        static List<string> ErrorLog = new List<string>();

        public static async void Log(string text, string identifier = "") {
            text = "[Log: " + ((!string.IsNullOrEmpty(identifier)) ? identifier : "") + " (" + firmwareVersion + ")" +"] " + text;
            GeneralLog.Add(text);
            //MarkedUp.AnalyticClient.Info(text);
            Debug.WriteLine(text);
        }

        public static async void Info(string text, string identifier = "") {
            text = "[Local: " + ((!string.IsNullOrEmpty(identifier)) ? identifier : "") + " (" + firmwareVersion + ")" + "] " + text;
            LocalLog.Add(text);
            Debug.WriteLine(text);
        }

        public static async void Error(string text, string error, string identifier = "") {
            text = "[Error: " + ((!string.IsNullOrEmpty(identifier)) ? identifier : "") + " (" + firmwareVersion + ")" + "] " + text + " Stacktrace: " + error;
            ErrorLog.Add(text);
            //MarkedUp.AnalyticClient.Error(text);
            Debug.WriteLine(text);
        }
    }
}
