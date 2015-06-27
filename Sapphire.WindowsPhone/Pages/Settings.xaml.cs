using Core.Content;
using Sapphire.Shared.Common;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
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

			((Run)AppVersion.Inlines.Last()).Text = string.Format("{0}.{1}.{2}.{3}",
				Package.Current.Id.Version.Major.ToString(),
				Package.Current.Id.Version.Minor.ToString(),
				Package.Current.Id.Version.Build.ToString(),
				Package.Current.Id.Version.Revision.ToString());

			Tags.Text = UserPreferences.DefaultTags;
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

		private void AccountManage_Tapped(object sender, RoutedEventArgs e) {
			var frame = Window.Current.Content as Frame;
			if (!frame.Navigate(typeof(AccountManager), "1"))
				throw new Exception("NavFail");
		}

		private async void RateReviewTapped(object sender, RoutedEventArgs e) {
			await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=d9b787e4-616a-40ea-bdb4-c81523cb0733"));
		}

		private async void UpgradeAppTapped(object sender, RoutedEventArgs e) {
			if (Core.Utils.AppLicenseHandler.IsTrial)
				await Core.Utils.AppLicenseHandler.RemoveAds();
		}

		private void ExpandContainer_Tapped(object sender, TappedRoutedEventArgs e) {
			var container = (FrameworkElement)((Button)sender).Content;
            container = (FrameworkElement)((StackPanel)container).Children.Last();
			container.Visibility = (container.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
		}

		private void TagBox_KeyDown(object sender, KeyRoutedEventArgs e) {
			var tagBox = ((TextBox)sender);
			if (e.Key.ToString() == "188") {
				var tags = tagBox.Text.Split(',');
				var converted = "";
				foreach (var tag in tags) {
					converted += string.Format("#{0}, ", tag.Trim('#', ',', ' '));
				}
				tagBox.Text = converted.TrimEnd(' ');
			} else if (e.Key == Windows.System.VirtualKey.Back) {
				if (!string.IsNullOrEmpty(tagBox.Text)) {
					if (!"#, ".Contains(tagBox.Text.Last().ToString())) {
						var tags = ((TextBox)sender).Text.Split(',');
						var converted = "";
						for (var i = 0; i < tags.Count() - 1; i++) {
							converted += string.Format("#{0}, ", tags[i].Trim('#', ',', ' '));
						}
						tagBox.Text = converted.TrimEnd(' ');
					}
				}
			}

			((TextBox)sender).Select(((TextBox)sender).Text.Length, 0);
		}

		private void Tags_LostFocus(object sender, RoutedEventArgs e) {
			var tagBox = ((TextBox)sender);
			var tags = tagBox.Text.Split(',');
			var converted = "";
			foreach (var tag in tags) {
				converted += string.Format("#{0}, ", tag.Trim('#', ',', ' '));
			}
			tagBox.Text = converted.TrimEnd('#', ',', ' ');

			UserPreferences.DefaultTags = converted.TrimEnd('#', ',', ' ');
        }

    }
}
