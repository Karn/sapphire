using API;
using API.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// Use setter to set item source of posts

namespace Core.Utils.Controls {
    public sealed partial class ActivityList : UserControl {

        private static HttpClient client = new HttpClient();
        public bool ContentLoaded;
        private static ScrollViewer sv;

        public ActivityList() {
            this.InitializeComponent();
        }

        public async void LoadPosts() {
            try {
                MainPage.sb = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                MainPage.sb.ForegroundColor = Color.FromArgb(255, 255, 255, 255);
                MainPage.sb.ProgressIndicator.Text = "Loading activity...";
                await MainPage.sb.ProgressIndicator.ShowAsync();
                GroupData(await RequestHandler.RetrieveActivity());
                Debug.WriteLine(Config.LastNotification);
                await MainPage.sb.ProgressIndicator.HideAsync();
                ContentLoaded = true;
            } catch (Exception e) {
                API.Utils.DebugHandler.ErrorLog.Add("Error loading activity feed. " + e.Source);
            }
        }

        public void ClearPosts()
        {
            //GroupData(new ObservableCollection<APIContent.Content.Activity.Notification>());
            Notifications.Visibility = Visibility.Collapsed;
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e) {
            if (sv == null)
                sv = (ScrollViewer)sender;

            if (sv.VerticalOffset + 25 > sv.ExtentHeight - sv.ActualHeight) {
                //loadingMorePosts();
            }
        }

        private void GoToBlog(object sender, TappedRoutedEventArgs e) {
            var frame = Window.Current.Content as Frame;
            if (!frame.Navigate(typeof(Pages.BlogDetails), ((Image)sender).Tag)) {
                throw new Exception("NavFail");
            }
        }

        public void GroupData(List<API.Content.Activity.Notification> items) {
            var result = from item in items group item by item.date into itemGroup orderby itemGroup.Key select itemGroup;
            csvNotifications.Source = result.Reverse();
            Notifications.Visibility = Visibility.Visible;
        }

        private void Notifications_Loaded(object sender, RoutedEventArgs e) {
            //if (Notifications.Items.Count != 0)
                //Config.LastNotification = (Notifications.Items.First() as APIContent.Content.Activity.Notification).timestamp;
        }

        private void GoToPost(object sender, TappedRoutedEventArgs e) {
            var frame = Window.Current.Content as Frame;
            if (!frame.Navigate(typeof(Pages.PostDetails), ((Image)sender).Tag)) {
                throw new Exception("NavFail");
            }
        }
    }
}
