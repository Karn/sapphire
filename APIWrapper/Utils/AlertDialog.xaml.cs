using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace APIWrapper.Utils {
    public sealed partial class AlertDialog : UserControl {

        private static List<string> ErrorQueue = new List<string>();

        public static bool IsAnimating;

        public AlertDialog() {
            this.InitializeComponent();
        }

        public void DisplayMessage(string message) {
            if (!IsAnimating) {
                IsAnimating = true;
                Message.Text = message;
                Debug.WriteLine("[MSG]: " + message);
                Display.Begin();
            } else {
                if (ErrorQueue.Count < 5) {
                    ErrorQueue.Add(message);
                }
            }
        }

        private void Display_Completed(object sender, object e) {
            IsAnimating = false;
            if (ErrorQueue.Count != 0) {
                string message = ErrorQueue.ElementAt(0);
                ErrorQueue.RemoveAt(0);
                DisplayMessage(message);
            }
        }

        public static void AddToQueue(string message) {
            if (ErrorQueue.Count < 5) {
                ErrorQueue.Add(message);
            }
        }
    }
}
