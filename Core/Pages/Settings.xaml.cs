using APIWrapper.Content;
using Core.Shared.Common;
using System;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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

            if (!Utils.AppLicenseHandler.IsTrial)
                UpgardePanel.Visibility = Visibility.Collapsed;
        }

        public NavigationHelper NavigationHelper {
            get { return this.navigationHelper; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
            if (UserUtils.SelectedTheme == "Dark")
                DarkTheme.IsChecked = true;
            NotificationsToggle.IsChecked = UserUtils.NotificationsEnabled;
            OneClickReblog.IsChecked = UserUtils.OneClickReblog;
            PostTags.IsChecked = UserUtils.TagsInPosts;
            UXFeedback.IsChecked = UserUtils.EnableAnalytics;
            DarkStatusbar.IsChecked = UserUtils.EnableStatusBarBG;
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

        private void SwitchTheme_Toggled(object sender, RoutedEventArgs e) {
            UserUtils.SelectedTheme = (bool)((CheckBox)sender).IsChecked ? "Dark" : "Light";
        }

        private void EnableNotifications_Toggled(object sender, RoutedEventArgs e) {
            UserUtils.NotificationsEnabled = (bool)((CheckBox)sender).IsChecked;
        }

        private void EnableOneClickReblog_Toggled(object sender, RoutedEventArgs e) {
            UserUtils.OneClickReblog = (bool)((CheckBox)sender).IsChecked;
        }

        private void DisableTagsInPosts_Toggled(object sender, RoutedEventArgs e) {
            UserUtils.TagsInPosts = (bool)((CheckBox)sender).IsChecked;
        }

        private void AnalyticsSwitch_Toggled(object sender, RoutedEventArgs e) {
            if ((bool)((CheckBox)sender).IsChecked) {
                UserUtils.EnableAnalytics = true;
                Analytics.AnalyticsManager.EnableDiagnostics();
            } else {
                UserUtils.EnableAnalytics = false;
            }
        }

        private void StatusBarBGToggle_Toggled(object sender, RoutedEventArgs e) {
            UserUtils.EnableStatusBarBG = (bool)((CheckBox)sender).IsChecked;
        }

        private async void RateReviewTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=d9b787e4-616a-40ea-bdb4-c81523cb0733"));
        }

        private async void UpgradeAppTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
            if (Utils.AppLicenseHandler.IsTrial)
                await Utils.AppLicenseHandler.RemoveAds();
        }

        private void AboutTapped(object sender, TappedRoutedEventArgs e) {
            var frame = Window.Current.Content as Frame;
            if (!frame.Navigate(typeof(Pages.About)))
                throw new Exception("NavFail");
        }
    }
}
