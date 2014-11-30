using APIWrapper.Content;
using APIWrapper.Content.Model;
using APIWrapper.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace BackgroundUtilities {

    public sealed class NotificationHandler : IBackgroundTask {

        private static string TAG = "NotificationHandler";

        Dictionary<string, int> NotificationDictionary = new Dictionary<string, int>();
        Dictionary<string, List<Activity.Notification>> NotificationCounts = new Dictionary<string, List<Activity.Notification>>();

        public async void Run(IBackgroundTaskInstance taskInstance) {

            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();

            new UserStore();
            var x = UserStore.NotificationIDs;
            if (x != null)
                NotificationDictionary = x;
            if (UserStore.NotificationsEnabled)
                await RetrieveNotifications();

            _deferral.Complete();
        }

        private async Task RetrieveNotifications() {

            try {
                string Response = await APIWrapper.Client.RequestBuilder.GetAPI("https://api.tumblr.com/v2/user/notifications");

                if (Response.Contains("200")) {
                    var activity = JsonConvert.DeserializeObject<Responses.GetActivity>(Response);

                    var blogs = activity.response.blogs;
                    foreach (var b in blogs) {
                        if (!NotificationDictionary.ContainsKey(b.blog_name))
                            NotificationDictionary.Add(b.blog_name, 0);
                        if (!NotificationCounts.ContainsKey(b.blog_name))
                            NotificationCounts.Add(b.blog_name, new List<Activity.Notification>());
                        foreach (var n in b.notifications) {
                            if (n.timestamp > NotificationDictionary[b.blog_name])
                                NotificationCounts[b.blog_name].Add(n);
                        }
                        if (NotificationCounts[b.blog_name].Count > 0)
                            NotificationDictionary[b.blog_name] = NotificationCounts[b.blog_name].First().timestamp;
                    }
                }

                UserStore.NotificationIDs = NotificationDictionary;
                DisplayNotification();
            } catch (Exception ex) {
                DiagnosticsManager.LogException(ex, TAG, "Failed to serialize activity for notifications.");
            }
        }

        private void DisplayNotification() {
            Debug.WriteLine("Displaying notifications.");
            int totalCount = 0;
            foreach (var n in NotificationCounts) {
                if (n.Value.Count > 0) {
                    //Later add the avatar of the blog that recieved the notification
                    ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
                    XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
                    XmlNodeList textElements = toastXml.GetElementsByTagName("text");
                    textElements[0].AppendChild(toastXml.CreateTextNode(n.Key));
                    textElements[1].AppendChild(toastXml.CreateTextNode("You have " + n.Value.Count + " new " + (n.Value.Count == 1 ? "notification." : "notifications.")));
                    totalCount += n.Value.Count;
                    IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
                    ((XmlElement)toastNode).SetAttribute("launch", "{\"type\":\"toast\"}");

                    ToastNotification x = new ToastNotification(toastXml);
                    x.SuppressPopup = true;
                    ToastNotificationManager.CreateToastNotifier().Show(x);
                }
            }
        }
    }
}