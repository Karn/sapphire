using Core.AuthenticationManager;
using Core.Client;
using Sapphire.Shared.Common;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;


namespace Sapphire.Pages {
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
				tagBox.Text = Core.Utils.TagFormatter.Format(tagBox.Text);
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
				App.DisplayStatus(App.LocaleResources.GetString("Status_SendingReply"));
				if (await CreateRequest.CreateReply(postID, Authentication.Utils.UrlEncode(Caption.Text), ((FrameworkElement)sender).Tag.ToString() == "private")) {
					ReplyFeilds.IsEnabled = true;
					App.HideStatus();
					Frame.GoBack();
				} else {
					App.Alert(App.LocaleResources.GetString("Error_UnableToSendReply"));
					App.HideStatus();
				}
			}
			ReplyFeilds.IsEnabled = true;
		}

	}
}
