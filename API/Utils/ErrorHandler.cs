using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Utils {
    public class DebugHandler {

        public static List<string> GeneralLog = new List<string>();

        public static List<string> ErrorLog = new List<string>();

        public static void Log(string text, string identifier = "") {
            if (!string.IsNullOrEmpty(identifier))
                text = "[" + identifier + "]" + text + ".";
            GeneralLog.Add(text);
        }
    }
}
