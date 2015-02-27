using APIWrapper.AuthenticationManager;
using APIWrapper.Client;
using APIWrapper.Content;
using APIWrapper.Utils;
using Sapphire.Shared.Common;
using Sapphire.Utils.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Sapphire.Pages {
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ReplyPage : Page {

		private NavigationHelper navigationHelper;

		string postID = "";

		public ReplyPage() {
			this.InitializeComponent();

			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
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
			postID = e.NavigationParameter.ToString();
		}

		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) { }

		#region NavigationHelper registration

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			this.navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e) {
			this.navigationHelper.OnNavigatedFrom(e);
		}

		#endregion

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

		private async void PostButton_Tapped(object sender, TappedRoutedEventArgs e) {
			ReplyFeilds.IsEnabled = false;
			if (!string.IsNullOrWhiteSpace(Caption.Text)) {
				App.DisplayStatus("Sending reply...");
				if (await CreateRequest.CreateReply(postID, Authentication.Utils.UrlEncode(Caption.Text), ((FrameworkElement)sender).Tag.ToString() == "private")) {
					ReplyFeilds.IsEnabled = true;
					App.HideStatus();
					Frame.GoBack();
				} else {
					App.Alert("Failed to reply to message.");
					App.HideStatus();
				}
			}
			ReplyFeilds.IsEnabled = true;
		}

	}
}
