using API;
using API.Content;
using API.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using API.Data;

namespace BackgroundUtilities {

    public sealed class NotificationHandler : IBackgroundTask {

        public async void Run(IBackgroundTaskInstance taskInstance) {

            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
            UserData.LoadData();
            //if (string.IsNullOrEmpty(UserData.AreNotificationsEnabled) ? true : (UserData.AreNotificationsEnabled.Contains("T") ? true : false))
            await RetrieveNotifications();

            _deferral.Complete();
        }

        private async Task RetrieveNotifications() {

            Debug.WriteLine(Config.LastNotification);
            var LastNotification = Config.LastNotification;
            Debug.WriteLine("Last notification: " + LastNotification);

            var Notifications = new ObservableCollection<API.Content.Activity.Notification>();

            string Response = await RequestBuilder.GetAPI("https://api.tumblr.com/v2/user/notifications");

            if (Response.Contains("200")) {
                var activity = JsonConvert.DeserializeObject<Responses.GetActivity>(Response);

                try {
                    var b = activity.response.blogs.First();
                    foreach (var n in b.notifications) {
                        if (n.timestamp > LastNotification)
                            Notifications.Add(n);
                    }
                    if (Notifications.Count > 0)
                        Config.LastNotification = Notifications.First().timestamp;
                    Debug.WriteLine("Count: " + Notifications.Count);
                    Debug.WriteLine("Last notification: " + Config.LastNotification);
                } catch (Exception e) {
                    Debug.WriteLine("Failed to serialize. " + e.Message);
                }
            }

            if (Notifications.Count == 0)
                return;

            DisplayNotification(Notifications.Count);
        }

        private void DisplayNotification(int notificationCount) {
            Debug.WriteLine("Displaying " + notificationCount + " notifications.");

            //Later add the avatar of the blog that recieved the notification
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList textElements = toastXml.GetElementsByTagName("text");
            textElements[0].AppendChild(toastXml.CreateTextNode("You have " + (notificationCount > 5 ? "5+" : notificationCount.ToString()) + " new notification" + (notificationCount == 1 ? "" : "s") + "!"));
            textElements[1].AppendChild(toastXml.CreateTextNode(""));

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("launch", "{\"type\":\"toast\"}");

            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toastXml));

            //Update Tile
            TileTemplateType tileTemplate = TileTemplateType.TileSquare150x150IconWithBadge;
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(tileTemplate);
            XmlNodeList tileAttributes = tileXml.GetElementsByTagName("binding");
            ((XmlElement)tileAttributes[0]).SetAttribute("branding", "none");
            //Add an image for notification
            XmlNodeList tileImageAttributes = tileXml.GetElementsByTagName("image");

            ((XmlElement)tileImageAttributes[0]).SetAttribute("src", "ms-appx:///Assets/LiveTileMed.png");
            ((XmlElement)tileImageAttributes[0]).SetAttribute("alt", "");


            TileNotification tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            //TileUpdateManager.CreateTileUpdaterForApplication().Clear();


            //Display count
            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", notificationCount.ToString());

            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
        }

        //private static void UpdateTile() {
        //    // Create a tile update manager for the specified syndication feed.
        //    var updater = TileUpdateManager.CreateTileUpdaterForApplication();
        //    updater.EnableNotificationQueue(true);
        //    updater.Clear();

        //    // Keep track of the number feed items that get tile notifications. 
        //    int itemCount = 0;

        //    // Create a tile notification for each feed item.
        //    foreach (var item in feed.Items) {
        //        XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideText03);

        //        var title = item.Title;
        //        string titleText = title.Text == null ? String.Empty : title.Text;
        //        tileXml.GetElementsByTagName(textElementName)[0].InnerText = titleText;

        //        // Create a new tile notification. 
        //        updater.Update(new TileNotification(tileXml));

        //        // Don't create more than 5 notifications.
        //        if (itemCount++ > 5) break;
        //    }
        //}


    }
}