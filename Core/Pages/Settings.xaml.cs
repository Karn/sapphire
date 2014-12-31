using APIWrapper.Content;
using APIWrapper.Utils;
using Core.Common;
using System;
using System.IO;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//Below is how to get application package details
//http://code.msdn.microsoft.com/wpapps/Package-sample-46e239fa



// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

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

            MainPage.AlertFlyout = _ErrorFlyout;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel {
            get { return this.defaultViewModel; }
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

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) {

        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
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
            if (((ToggleSwitch)sender).IsOn) {
                UserStore.SelectedTheme = "Dark";
            } else {
                UserStore.SelectedTheme = "Light";
            }
        }

        private void EnableNotifications_Toggled(object sender, RoutedEventArgs e) {
            if (((ToggleSwitch)sender).IsOn) {
                UserStore.NotificationsEnabled = true;
            } else {
                UserStore.NotificationsEnabled = false;
            }
        }

        private void EnableOneClickReblog_Toggled(object sender, RoutedEventArgs e) {
            if (((ToggleSwitch)sender).IsOn) {
                UserStore.OneClickReblog = true;
            } else {
                UserStore.OneClickReblog = false;
            }
        }

        private void DisableTagsInPosts_Toggled(object sender, RoutedEventArgs e) {
            if (((ToggleSwitch)sender).IsOn) {
                UserStore.TagsInPosts = true;
            } else {
                UserStore.TagsInPosts = false;
            }
        }

        private void AnalyticsSwitch_Toggled(object sender, RoutedEventArgs e) {
            if (((ToggleSwitch)sender).IsOn) {
                UserStore.EnableAnalytics = true;
                DiagnosticsManager.EnableDiagnostics();
            } else {
                UserStore.EnableAnalytics = false;
            }
        }

        private void StatusBarBGToggle_Toggled(object sender, RoutedEventArgs e) {
            if (((ToggleSwitch)sender).IsOn) {
                UserStore.EnableStatusBarBG = true;
            } else {
                UserStore.EnableStatusBarBG = false;
            }
        }

        private async void ShareOnTwitter_Click(object sender, RoutedEventArgs e) {
            await Launcher.LaunchUriAsync(new Uri("http://twitter.com/intent/tweet?text=%23SapphireApp+is+an+awesome+%26+beautiful+Tumblr+app+for+WP8.1.+It+has+activity%2C+notifications+%2B+more%21+Download+now+at+http%3A%2F%2Ft.co%2FP4dyvc4LZ0"));
        }
    }
}
