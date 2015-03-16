using Core.Content;
using Sapphire.Shared.Common;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Sapphire.Pages {

	public sealed partial class Settings : Page {
		private NavigationHelper navigationHelper;

		public Settings() {
			this.InitializeComponent();

			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

			if (!Core.Utils.AppLicenseHandler.IsTrial)
				UpgardePanel.Visibility = Visibility.Collapsed;
		}

		public NavigationHelper NavigationHelper {
			get { return this.navigationHelper; }
		}

		private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
			//if (UserPreferences.SelectedTheme == "Dark")
			//	DarkTheme.IsChecked = true;
			//NotificationsToggle.IsChecked = UserPreferences.NotificationsEnabled;
			//OneClickReblog.IsChecked = UserPreferences.OneClickReblog;
			//PostTags.IsChecked = UserPreferences.TagsInPosts;
			//UXFeedback.IsChecked = UserPreferences.EnableAnalytics;
			//DarkStatusbar.IsChecked = UserPreferences.EnableStatusBarBG;
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
			UserPreferences.SelectedTheme = (bool)((CheckBox)sender).IsChecked ? "Dark" : "Light";
		}

		private void EnableNotifications_Toggled(object sender, RoutedEventArgs e) {
			UserPreferences.NotificationsEnabled = (bool)((CheckBox)sender).IsChecked;
		}

		private void EnableOneClickReblog_Toggled(object sender, RoutedEventArgs e) {
			UserPreferences.OneClickReblog = (bool)((CheckBox)sender).IsChecked;
		}

		private void DisableTagsInPosts_Toggled(object sender, RoutedEventArgs e) {
			UserPreferences.TagsInPosts = (bool)((CheckBox)sender).IsChecked;
		}

		private void AnalyticsSwitch_Toggled(object sender, RoutedEventArgs e) {
			if ((bool)((CheckBox)sender).IsChecked) {
				UserPreferences.EnableAnalytics = true;
				//Analytics.AnalyticsManager.EnableDiagnostics();
			} else {
				UserPreferences.EnableAnalytics = false;
			}
		}

		private void StatusBarBGToggle_Toggled(object sender, RoutedEventArgs e) {
			UserPreferences.EnableStatusBarBG = (bool)((CheckBox)sender).IsChecked;
		}

		private async void RateReviewTapped(object sender, TappedRoutedEventArgs e) {
			await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=d9b787e4-616a-40ea-bdb4-c81523cb0733"));
		}

		private async void UpgradeAppTapped(object sender, TappedRoutedEventArgs e) {
			if (Core.Utils.AppLicenseHandler.IsTrial)
				await Core.Utils.AppLicenseHandler.RemoveAds();
		}

		private void AboutTapped(object sender, TappedRoutedEventArgs e) {
			var frame = Window.Current.Content as Frame;
			if (!frame.Navigate(typeof(About)))
				throw new Exception("NavFail");
		}

		private void AccountManage_Tapped(object sender, TappedRoutedEventArgs e) {
			var frame = Window.Current.Content as Frame;
			if (!frame.Navigate(typeof(AccountManager), "1"))
				throw new Exception("NavFail");
		}
	}
}
