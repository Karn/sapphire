using APIWrapper.AuthenticationManager;
using APIWrapper.Client;
using APIWrapper.Content;
using APIWrapper.Utils;
using Core.Shared.Common;
using Core.Utils.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ReblogPage : Page {

		private static string TAG = "ReblogPage";

		private bool IsReply = false;
		private bool IsQuestion = false;
		private static object options;

		public ToggleControl button;

		private NavigationHelper navigationHelper;

		string postID = "";
		string reblogKey = "";

		string askTo = "";

		bool reblogged = false;

		public ReblogPage() {
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
			object[] navParams = e.NavigationParameter as object[];
			button = navParams[0] as ToggleControl;
			var parameters = navParams[1].ToString();

			if (parameters.Contains("Reply:")) {
				PageTitle.Text = "Create message";
				IsReply = true;
				AddToDraftsButton.Visibility = Visibility.Collapsed;
				AddToQueueButton.Visibility = Visibility.Collapsed;
				PublishBox.Visibility = Visibility.Collapsed;
				TagContainer.Visibility = Visibility.Collapsed;
				parameters = parameters.Substring(7);
			} else if (parameters.Contains("Message:")) {
				askTo = parameters.Substring(9);
				PageTitle.Text = "Ask: " + askTo;
				IsQuestion = true;
				AddToDraftsButton.Visibility = Visibility.Collapsed;
				AddToQueueButton.Visibility = Visibility.Collapsed;
				PublishBox.Visibility = Visibility.Collapsed;
				TagContainer.Visibility = Visibility.Collapsed;
				Caption.PlaceholderText = "Question";
				return;
			}

			var x = parameters.Split(',');
			postID = x[0].Replace(",", "");
			reblogKey = x[1].Replace(",", "");
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
			if (!IsReply && !IsQuestion)
				button.IsChecked = reblogged;
		}

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

		private void Tags_LostFocus(object sender, RoutedEventArgs e) {
			var tagBox = ((TextBox)sender);
			var tags = tagBox.Text.Split(',');
			var converted = "";
			foreach (var tag in tags) {
				converted += string.Format("#{0}, ", tag.Trim('#', ',', ' '));
			}
			tagBox.Text = converted.TrimEnd(' ');
			((TextBox)sender).Text = ((TextBox)sender).Text.TrimEnd('#', ',', ' ');
		}

		private async void PostButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var blogName = BlogName.Text;
			var tags = Tags.Text;
			if (!string.IsNullOrEmpty(tags)) {
				tags = tags.Replace(" #", ", ");
				tags = Authentication.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length) : tags.Substring(0, tags.Length)));
			}
			try {
				var status = "";
				ReplyFeilds.IsEnabled = false;
				if (((Image)sender).Tag != null) {
					if (((Image)sender).Tag.ToString() == "queue") {
						if (string.IsNullOrWhiteSpace(PublishOn.Text)) {
							App.Alert("Please enter a time to publish the post on.");
							ReplyFeilds.IsEnabled = true;
							App.HideStatus();
							return;
						} else {
							App.DisplayStatus("Adding to queue...");
							status = "&state=queue&publish_on=" + Authentication.Utils.UrlEncode(PublishOn.Text);
						}
					} else if (((Image)sender).Tag.ToString() == "draft") {
						App.DisplayStatus("Adding to drafts...");
						status = "&state=draft";
					}
				}
				if (IsReply && !string.IsNullOrWhiteSpace(Caption.Text)) {
					App.DisplayStatus("Sending reply...");
				//	var parameters = (string.Format("id={0}", postID) + (!string.IsNullOrEmpty(Caption.Text) ? "&reply_text=" + Authentication.Utils.UrlEncode(Caption.Text) : "") +
				//(!string.IsNullOrEmpty(tags) ? "&tags=" + tags : ""));
					if (await CreateRequest.CreateReply(postID, Authentication.Utils.UrlEncode(Caption.Text), false)) {
						App.Alert("Created.");
						ReplyFeilds.IsEnabled = true;
						App.HideStatus();
						Frame.GoBack();
					} else {
						App.Alert("Failed to reply to message.");
						App.HideStatus();
						button.IsChecked = false;
						return;
					}
				} else if (IsQuestion && !string.IsNullOrWhiteSpace(Caption.Text)) {
					App.DisplayStatus("Sending question...");
					if (await CreateRequest.PostQuestion(askTo, Authentication.Utils.UrlEncode(Caption.Text))) {
						App.Alert("Created.");
						ReplyFeilds.IsEnabled = true;
						App.HideStatus();
						Frame.GoBack();
					} else {
						App.Alert("Failed to send mail.");
						App.HideStatus();
						return;
					}
				} else {
					App.DisplayStatus("Reblogging post...");
					if (await CreateRequest.ReblogPost(postID, reblogKey, Authentication.Utils.UrlEncode(Caption.Text), tags, status, blogName)) {
						App.Alert("Created.");
						ReplyFeilds.IsEnabled = true;
						reblogged = true;
						App.HideStatus();
						Frame.GoBack();
					} else {
						if (((Image)sender).Tag != null) {
							if (((Image)sender).Tag.ToString() == "queue") {
								App.Alert("Failed to add post to queue. Remember to use the same format as on the site!");
							} else if (((Image)sender).Tag.ToString() == "draft") {
								App.Alert("Failed to add post to drafts.");
							} else
								App.Alert("Failed to reblog post.");
						}
						button.IsChecked = false;
					}
				}
				App.HideStatus();
				ReplyFeilds.IsEnabled = false;
			} catch (Exception ex) {
				App.HideStatus();
				App.Alert("Failed to create post");
				Analytics.AnalyticsManager.LogException(ex, TAG, "Failed to create post");
				if (button != null)
					button.IsChecked = false;
			}
		}

		private void ReblogToOptions_Loaded(object sender, RoutedEventArgs e) {
			var mainblog = UserStorageUtils.UserBlogs.First();
			ReblogToOptions.DataContext = mainblog;
		}

		private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e) {
			ReblogToOptions.DataContext = UserStorageUtils.UserBlogs.Where(b => b.Name == ((TextBlock)sender).Text).FirstOrDefault();
			Grid_Tapped(null, null);
		}

		private void ListView_Loaded(object sender, RoutedEventArgs e) {
			((ListView)sender).ItemsSource = UserStorageUtils.UserBlogs;
		}

		private void ReblogToOptions_Tapped(object sender, TappedRoutedEventArgs e) {
			options = sender;
			FrameworkElement element = sender as FrameworkElement;
			if (element == null) return;

			// If the menu was attached properly, we just need to call this handy method
			FlyoutBase.ShowAttachedFlyout(element);
		}

		private void Grid_Tapped(object sender, TappedRoutedEventArgs e) {
			FrameworkElement element = options as FrameworkElement;
			if (element == null) return;

			// If the menu was attached properly, we just need to call this handy method
			var f = FlyoutBase.GetAttachedFlyout(element);
			f.Hide();
		}
	}
}
