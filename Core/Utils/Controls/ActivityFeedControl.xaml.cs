using APIWrapper.Client;
using APIWrapper.Content.Model;
using APIWrapper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// Use setter to set item source of posts

namespace Core.Utils.Controls {
    public sealed partial class ActivityFeedControl : UserControl {

        private static string TAG = "ActivityList";

        private static HttpClient client = new HttpClient();
        public bool ContentLoaded;

        public ActivityFeedControl() {
            this.InitializeComponent();
        }

        public async void LoadPosts() {
            try {
                App.DisplayStatus("Loading activity...");
                GroupData(await CreateRequest.RetrieveActivity());
                App.HideStatus();
                ContentLoaded = true;
            } catch (Exception ex) {
                DiagnosticsManager.LogException(ex, TAG, "Error loading activity feed. ");
            }
        }

        public void ClearPosts() {
            Notifications.Visibility = Visibility.Collapsed;
        }

        private void GoToBlog(object sender, TappedRoutedEventArgs e) {
            if (((FrameworkElement)sender).Tag != null) {
                var frame = Window.Current.Content as Frame;
                if (!frame.Navigate(typeof(Pages.BlogDetails), ((FrameworkElement)sender).Tag.ToString()))
                    throw new Exception("NavFail");
            }
        }

        public void GroupData(List<Activity.Notification> items) {
            var result = from item in items group item by item.date into itemGroup orderby itemGroup.Key select itemGroup;
            csvNotifications.Source = result.Reverse();
            Notifications.Visibility = Visibility.Visible;
        }

        private void GoToPost(object sender, TappedRoutedEventArgs e) {
            var frame = Window.Current.Content as Frame;
            if (!frame.Navigate(typeof(Pages.PostDetails), ((Image)sender).Tag)) {
                throw new Exception("NavFail");
            }
        }

        private async void FollowIcon_Tapped(object sender, TappedRoutedEventArgs e) {
            var x = ((Grid)sender);
            if (await CreateRequest.FollowUnfollow(true, x.Tag.ToString())) {
                x.Visibility = Visibility.Collapsed;
            }
        }

        private void Border_Loaded(object sender, RoutedEventArgs e) {
            ((Border)sender).Width = Window.Current.Bounds.Width;
        }
    }
}
