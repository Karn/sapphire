using Core.Client;
using Core.Content.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// Use setter to set item source of posts

namespace Sapphire.Utils.Controls {
    public sealed partial class ActivityView : UserControl {

        private static string TAG = "ActivityList";

        public ActivityView() {
            this.InitializeComponent();
        }

        public async Task RetrieveNotifications() {
            try {
                App.DisplayStatus(App.LocaleResources.GetString("LoadingActivity"));
                var activity = await CreateRequest.RetrieveActivity();
                if (activity.Count > 0)
                    GroupData(activity);
                App.HideStatus();
            } catch (Exception ex) {
                //TODO: Dispaly error message
            }
        }

        public void ClearPosts() {
            Activity.Visibility = Visibility.Collapsed;
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
            Activity.Visibility = Visibility.Visible;
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
            ((Grid)sender).Width = Window.Current.Bounds.Width;
        }

        public void ScrollToTop() {
            try {
                Activity.ScrollIntoView(Activity.Items.First());
            } catch { }
        }
    }
}
