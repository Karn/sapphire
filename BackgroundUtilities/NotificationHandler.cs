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

        Dictionary<string, int> NotificationDictionary = new Dictionary<string, int>();
        Dictionary<string, List<Activity.Notification>> NotificationCounts = new Dictionary<string, List<Activity.Notification>>();

        public async void Run(IBackgroundTaskInstance taskInstance) {

            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();

            new UserStorageUtils();
            var x = UserStorageUtils.NotificationIDs;
            if (x != null)
                NotificationDictionary = x;
            if (UserStorageUtils.NotificationsEnabled)
                await RetrieveNotifications();

            _deferral.Complete();
        }

        private async Task RetrieveNotifications() {

            try {
                var Response = await APIWrapper.Client.RequestService.GET("https://api.tumblr.com/v2/user/notifications");

                if (Response.StatusCode == System.Net.HttpStatusCode.OK) {
                    var activity = JsonConvert.DeserializeObject<Responses.GetActivity>(await Response.Content.ReadAsStringAsync());

                    var blogs = activity.response.blogs;
                    foreach (var b in blogs) {
                        if (!NotificationDictionary.ContainsKey(b.Name))
                            NotificationDictionary.Add(b.Name, 0);
                        if (!NotificationCounts.ContainsKey(b.Name))
                            NotificationCounts.Add(b.Name, new List<Activity.Notification>());
                        foreach (var n in b.notifications) {
                            if (n.timestamp > NotificationDictionary[b.Name])
                                NotificationCounts[b.Name].Add(n);
                        }
                        if (NotificationCounts[b.Name].Count > 0)
                            NotificationDictionary[b.Name] = NotificationCounts[b.Name].First().timestamp;
                    }
                }

                UserStorageUtils.NotificationIDs = NotificationDictionary;
                DisplayNotification();
            } catch (Exception ex) {
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
                    ((XmlElement)toastNode).SetAttribute("launch", "Account: " + n.Key.ToString());

					ToastNotificationManager.History.Clear();

					ToastNotification x = new ToastNotification(toastXml);
					x.Tag = n.Value.Count.ToString();
                    x.SuppressPopup = true;
                    ToastNotificationManager.CreateToastNotifier().Show(x);
                }
            }
        }
    }
}