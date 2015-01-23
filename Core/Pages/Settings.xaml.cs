using APIWrapper.Content;
using Core.Shared.Common;
using System;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Core.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page {
        private NavigationHelper navigationHelper;

        public Settings() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            //if (!Utils.AppLicenseHandler.IsTrial) {
            //    RemoveAdsButton.Content = "Upgraded features. Thank you!";
            //    RemoveAdsButton.IsTapEnabled = false;
            //    RemoveAdsButton.Background = new SolidColorBrush(Color.FromArgb(255, 51, 63, 74));
            //}

            //AppVersion.Text = string.Format("{0}.{1}.{2}.{3}",
            //    Package.Current.Id.Version.Major.ToString(),
            //    Package.Current.Id.Version.Minor.ToString(),
            //    Package.Current.Id.Version.Build.ToString(),
            //    Package.Current.Id.Version.Revision.ToString());
        }

        public NavigationHelper NavigationHelper {
            get { return this.navigationHelper; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
            //if (UserStorageUtils.SelectedTheme == "Dark")
            //    ThemeSwitch.IsOn = true;
            //EnableNotifications.IsOn = UserStorageUtils.NotificationsEnabled;
            //EnableOneClickReblog.IsOn = UserStorageUtils.OneClickReblog;
            //DisableTagsInPosts.IsOn = UserStorageUtils.TagsInPosts;
            //AnalyticsSwitch.IsOn = UserStorageUtils.EnableAnalytics;
            //StatusBarBGToggle.IsOn = UserStorageUtils.EnableStatusBarBG;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) {

        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void RemoveAds_Click(object sender, RoutedEventArgs e) {
            if (Utils.AppLicenseHandler.IsTrial)
                await Utils.AppLicenseHandler.RemoveAds();
        }

        private async void ReviewButton_Click(object sender, RoutedEventArgs e) {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=d9b787e4-616a-40ea-bdb4-c81523cb0733"));
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e) {
            UserStorageUtils.SelectedTheme = ((ToggleSwitch)sender).IsOn ? "Dark" : "Light";
        }

        private void EnableNotifications_Toggled(object sender, RoutedEventArgs e) {
            UserStorageUtils.NotificationsEnabled = ((ToggleSwitch)sender).IsOn;
        }

        private void EnableOneClickReblog_Toggled(object sender, RoutedEventArgs e) {
            UserStorageUtils.OneClickReblog = ((ToggleSwitch)sender).IsOn;
        }

        private void DisableTagsInPosts_Toggled(object sender, RoutedEventArgs e) {
            UserStorageUtils.TagsInPosts = ((ToggleSwitch)sender).IsOn;
        }

        private void AnalyticsSwitch_Toggled(object sender, RoutedEventArgs e) {
            if (((ToggleSwitch)sender).IsOn) {
                UserStorageUtils.EnableAnalytics = true;
                Analytics.AnalyticsManager.EnableDiagnostics();
            } else {
                UserStorageUtils.EnableAnalytics = false;
            }
        }

        private void StatusBarBGToggle_Toggled(object sender, RoutedEventArgs e) {
            UserStorageUtils.EnableStatusBarBG = ((ToggleSwitch)sender).IsOn;
        }

        private async void ShareOnTwitter_Click(object sender, RoutedEventArgs e) {
            await Launcher.LaunchUriAsync(new Uri("http://twitter.com/intent/tweet?text=%23SapphireApp+is+an+awesome+%26+beautiful+Tumblr+app+for+WP8.1.+It+has+activity%2C+notifications+%2B+more%21+Download+now+at+http%3A%2F%2Ft.co%2FP4dyvc4LZ0"));
        }
    }
}
