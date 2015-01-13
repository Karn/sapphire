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

            if (!Utils.AppLicenseHandler.IsTrial) {
                RemoveAdsButton.Content = "Upgraded features. Thank you!";
                RemoveAdsButton.IsTapEnabled = false;
                RemoveAdsButton.Background = new SolidColorBrush(Color.FromArgb(255, 51, 63, 74));
            }

            AppVersion.Text = string.Format("{0}.{1}.{2}.{3}",
                Package.Current.Id.Version.Major.ToString(),
                Package.Current.Id.Version.Minor.ToString(),
                Package.Current.Id.Version.Build.ToString(),
                Package.Current.Id.Version.Revision.ToString());
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper {
            get { return this.navigationHelper; }
        }


        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
            if (UserStore.SelectedTheme == "Dark")
                ThemeSwitch.IsOn = true;
            EnableNotifications.IsOn = UserStore.NotificationsEnabled;
            EnableOneClickReblog.IsOn = UserStore.OneClickReblog;
            DisableTagsInPosts.IsOn = UserStore.TagsInPosts;
            AnalyticsSwitch.IsOn = UserStore.EnableAnalytics;
            StatusBarBGToggle.IsOn = UserStore.EnableStatusBarBG;
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
            UserStore.SelectedTheme = ((ToggleSwitch)sender).IsOn ? "Dark" : "Light";
        }

        private void EnableNotifications_Toggled(object sender, RoutedEventArgs e) {
            UserStore.NotificationsEnabled = ((ToggleSwitch)sender).IsOn;
        }

        private void EnableOneClickReblog_Toggled(object sender, RoutedEventArgs e) {
            UserStore.OneClickReblog = ((ToggleSwitch)sender).IsOn;
        }

        private void DisableTagsInPosts_Toggled(object sender, RoutedEventArgs e) {
            UserStore.TagsInPosts = ((ToggleSwitch)sender).IsOn;
        }

        private void AnalyticsSwitch_Toggled(object sender, RoutedEventArgs e) {
            if (((ToggleSwitch)sender).IsOn) {
                UserStore.EnableAnalytics = true;
                Analytics.AnalyticsManager.EnableDiagnostics();
            } else {
                UserStore.EnableAnalytics = false;
            }
        }

        private void StatusBarBGToggle_Toggled(object sender, RoutedEventArgs e) {
            UserStore.EnableStatusBarBG = ((ToggleSwitch)sender).IsOn;
        }

        private async void ShareOnTwitter_Click(object sender, RoutedEventArgs e) {
            await Launcher.LaunchUriAsync(new Uri("http://twitter.com/intent/tweet?text=%23SapphireApp+is+an+awesome+%26+beautiful+Tumblr+app+for+WP8.1.+It+has+activity%2C+notifications+%2B+more%21+Download+now+at+http%3A%2F%2Ft.co%2FP4dyvc4LZ0"));
        }
    }
}
