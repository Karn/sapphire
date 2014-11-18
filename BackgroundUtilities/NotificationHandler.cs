using APIWrapper.Content;
using APIWrapper.Content.Model;
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

            var x = UserStore.NotificationIDs;
            if (x != null) {
                NotificationDictionary = x;
                Debug.WriteLine(NotificationDictionary.Keys.Count);
            } else {
                Debug.WriteLine("nodata");
            }
            if (UserStore.NotificationsEnabled)
                await RetrieveNotifications();

            _deferral.Complete();
        }

        private async Task RetrieveNotifications() {

            try {
                string Response = await APIWrapper.Client.RequestBuilder.GetAPI("https://api.tumblr.com/v2/user/notifications");

                if (Response.Contains("200")) {
                    var activity = JsonConvert.DeserializeObject<Responses.GetActivity>(Response);

                    try {
                        var blogs = activity.response.blogs;
                        foreach (var b in blogs) {
                            Debug.WriteLine("Blog: " + b.blog_name);
                            if (!NotificationDictionary.ContainsKey(b.blog_name)) {
                                NotificationDictionary.Add(b.blog_name, 0);
                                Debug.WriteLine("Added blog to dictionary.");
                            }
                            if (!NotificationCounts.ContainsKey(b.blog_name)) {
                                NotificationCounts.Add(b.blog_name, new List<Activity.Notification>());
                                Debug.WriteLine("Added blog to counts.");
                            }
                            foreach (var n in b.notifications) {
                                if (n.timestamp > NotificationDictionary[b.blog_name]) {
                                    NotificationCounts[b.blog_name].Add(n);
                                    Debug.WriteLine("Added notification w/ ts: " + n.timestamp);
                                }
                            }
                            if (NotificationCounts[b.blog_name].Count > 0)
                                NotificationDictionary[b.blog_name] = NotificationCounts[b.blog_name].First().timestamp;
                        }

                    } catch (Exception e) {
                        Debug.WriteLine("Failed to serialize. " + e.ToString());
                    }
                }

                UserStore.NotificationIDs = NotificationDictionary;
                DisplayNotification();
            } catch (Exception e) {
                Debug.WriteLine("Error: " + e.Source);
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

            ////Update Tile
            //TileTemplateType tileTemplate = TileTemplateType.TileSquare150x150IconWithBadge;
            //XmlDocument tileXml = TileUpdateManager.GetTemplateContent(tileTemplate);
            //XmlNodeList tileAttributes = tileXml.GetElementsByTagName("binding");
            //((XmlElement)tileAttributes[0]).SetAttribute("branding", "none");
            ////Add an image for notification
            //XmlNodeList tileImageAttributes = tileXml.GetElementsByTagName("image");

            //((XmlElement)tileImageAttributes[0]).SetAttribute("src", "ms-appx:///Assets/LiveTileMed.png");
            //((XmlElement)tileImageAttributes[0]).SetAttribute("alt", "");


            //TileNotification tileNotification = new TileNotification(tileXml);
            //TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            ////TileUpdateManager.CreateTileUpdaterForApplication().Clear();


            ////Display count
            //XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            //XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            //badgeElement.SetAttribute("value", totalCount.ToString());

            //BadgeNotification badge = new BadgeNotification(badgeXml);
            //BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
        }
    }
}