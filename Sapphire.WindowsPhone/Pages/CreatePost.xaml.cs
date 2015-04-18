using Core.AuthenticationManager;
using Core.Client;
using Core.Content;
using Sapphire.Utils.Misc;
using Sapphire.Shared.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Sapphire.Pages {
	public sealed partial class CreatePost : Page, IFileOpenPickerContinuable {

		private static string TAG = "CreatePost";

		private NavigationHelper navigationHelper;

		StorageFile image;
		Image PhotoView;

		public CreatePost() {
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
			string type = e.NavigationParameter.ToString();
			Type.DataContext = type;
			PageTitle.Text = PageTitle.Text.ToString() + ": " + type;
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

		private void Photo_Image_Tapped(object sender, TappedRoutedEventArgs e) {
			PhotoView = (Image)((Grid)sender).FindName("Photo_Image");

			var filePicker = new FileOpenPicker();
			filePicker.FileTypeFilter.Add(".jpg");
			filePicker.FileTypeFilter.Add(".jpeg");
			filePicker.FileTypeFilter.Add(".png");
			filePicker.ViewMode = PickerViewMode.Thumbnail;
			filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
			filePicker.SettingsIdentifier = "picker1";
			filePicker.CommitButtonText = "Open File to Process";

			filePicker.PickSingleFileAndContinue();
		}

		public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args) {
			if (args.Files.Count > 0) {
				image = args.Files[0];
				Debug.WriteLine(image.Path);
				using (IRandomAccessStream fileStream = await image.OpenAsync(FileAccessMode.Read)) {
					var photo = new BitmapImage();
					await photo.SetSourceAsync(image.OpenAsync(FileAccessMode.Read).GetResults());

					PhotoView.Source = photo;
					PhotoView.Stretch = Stretch.UniformToFill;
				}
			} else {
				App.Alert("Failed to select image.");
			}
		}

		private async void PostTextButton_Tapped(object sender, TappedRoutedEventArgs e) {
			App.DisplayStatus("Creating text post...");
			Type.IsEnabled = false;

			var g = (Grid)((StackPanel)((Image)sender).Parent).Parent;

			var parameterString = "type=text";
			var title = ((TextBox)g.FindName("Title")).Text;
			parameterString += !string.IsNullOrWhiteSpace(title) ? "&title=" + Authentication.Utils.UrlEncode(title) : "";
			var body = ((TextBox)g.FindName("Body")).Text;
			parameterString += !string.IsNullOrWhiteSpace(body) ? "&body=" + Authentication.Utils.UrlEncode(body) : "";
			var tags = ((TextBox)g.FindName("Tags")).Text;
			if (!string.IsNullOrEmpty(tags)) {
				tags = tags.Replace(" #", ", ");
				tags = Authentication.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length) : tags.Substring(0, tags.Length)));
				parameterString += "&tags=" + tags;
			}

			try {
				var status = "";
				if (((Image)sender).Tag != null) {
					if (((Image)sender).Tag.ToString() == "queue") {
						var publishOn = ((TextBox)g.FindName("PublishOn")).Text;
						if (string.IsNullOrWhiteSpace(publishOn)) {
							App.Alert("Please enter a time to publish the post on.");
							return;
						} else {
							status = "&state=queue&publish_on=" + Authentication.Utils.UrlEncode(publishOn);
						}
					} else if (((Image)sender).Tag.ToString() == "draft") {
						status = "&state=draft";
					}
				}
				parameterString += status;
				var result = await CreateRequest.CreatePost(parameterString);
				Debug.WriteLine(await result.Content.ReadAsStringAsync());
				if (result.StatusCode == HttpStatusCode.Created) {
					Type.IsEnabled = true;
					App.HideStatus();
					Frame.GoBack();
				} else
					App.Alert(result.ReasonPhrase);
			} catch (Exception ex) {
				App.Alert("Failed to create post");
			}
			Type.IsEnabled = true;
			App.HideStatus();
		}

		private async void PostQuoteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			App.DisplayStatus("Publishing quote...");
			Type.IsEnabled = false;

			var parameterString = "type=quote";
			var g = (Grid)((StackPanel)((Image)sender).Parent).Parent;
			var quote = ((TextBox)g.FindName("Quote")).Text;
			parameterString += !string.IsNullOrWhiteSpace(quote) ? "&quote=" + Authentication.Utils.UrlEncode(quote) : "";
			var source = ((TextBox)g.FindName("Source")).Text;
			parameterString += !string.IsNullOrWhiteSpace(source) ? "&source=" + Authentication.Utils.UrlEncode(source) : "";
			var tags = ((TextBox)g.FindName("Tags")).Text;
			if (!string.IsNullOrEmpty(tags)) {
				tags = tags.Replace(" #", ", ");
				tags = Authentication.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length) : tags.Substring(0, tags.Length)));
				parameterString += "&tags=" + tags;
			}
			try {
				var status = "";
				if (((Image)sender).Tag != null) {
					if (((Image)sender).Tag.ToString() == "queue") {
						var publishOn = ((TextBox)g.FindName("PublishOn")).Text;
						if (string.IsNullOrWhiteSpace(publishOn)) {
							App.Alert("Please enter a time to publish the post on.");
							return;
						} else {
							status = "&state=queue&publish_on=" + Authentication.Utils.UrlEncode(publishOn);
						}
					} else if (((Image)sender).Tag.ToString() == "draft") {
						status = "&state=draft";
					}
				}
				parameterString += status;
				var result = await CreateRequest.CreatePost(parameterString);
				Debug.WriteLine(await result.Content.ReadAsStringAsync());
				if (result.StatusCode == HttpStatusCode.Created) {
					Type.IsEnabled = true;
					App.HideStatus();
					Frame.GoBack();
				} else
					App.Alert(result.ReasonPhrase);
			} catch (Exception ex) {
				App.Alert("Failed to create post");
			}
			Type.IsEnabled = true;
			App.HideStatus();
		}

		private async void PostPhotoButton_Tapped(object sender, TappedRoutedEventArgs e) {
			App.DisplayStatus("Uploading photo...");
			Type.IsEnabled = false;
			var g = (Grid)((StackPanel)((Image)sender).Parent).Parent;

			var parameterString = "type=photo";
			var caption = ((TextBox)g.FindName("Caption")).Text;
			parameterString += !string.IsNullOrWhiteSpace(caption) ? "&caption=" + caption : "";
			var tags = ((TextBox)g.FindName("Tags")).Text;
			if (!string.IsNullOrEmpty(tags)) {
				tags = tags.Replace(" #", ", ");
				tags = tags.StartsWith(" ") ? tags.Substring(1, tags.Length) : tags.Substring(0, tags.Length);
				parameterString += "&tags=" + tags;
			}

			try {
				var status = "";
				if (((Image)sender).Tag != null) {
					if (((Image)sender).Tag.ToString() == "queue") {
						var publishOn = ((TextBox)g.FindName("PublishOn")).Text;
						if (string.IsNullOrWhiteSpace(publishOn)) {
							App.Alert("Please enter a time to publish the post on.");
							return;
						} else {
							status = "&state=queue&publish_on=" + publishOn;
						}
					} else if (((Image)sender).Tag.ToString() == "draft") {
						status = "&state=draft";
					}
				}
				parameterString += status;
				if (image != null) {
					Frame.GoBack();
					var result = await RequestService.POST("https://api.tumblr.com/v2/blog/" +
						UserPreferences.CurrentBlog.Name + ".tumblr.com/post", image, parameterString);

					if (result.StatusCode == HttpStatusCode.Created) {
						Type.IsEnabled = true;
						App.HideStatus();
					} else
						App.Alert(result.ReasonPhrase);
				}
			} catch (Exception ex) {
				App.Alert("Failed to create post");
			}
			Type.IsEnabled = true;
			App.HideStatus();
		}

		private async void PostLinkButton_Tapped(object sender, TappedRoutedEventArgs e) {
			App.DisplayStatus("Sharing link...");
			Type.IsEnabled = false;

			var parameterString = "type=quote";
			var g = (Grid)((StackPanel)((Image)sender).Parent).Parent;
			var title = ((TextBox)g.FindName("Title")).Text;
			parameterString += !string.IsNullOrWhiteSpace(title) ? "&title=" + Authentication.Utils.UrlEncode(title) : "";
			var url = ((TextBox)g.FindName("Link")).Text;
			parameterString += !string.IsNullOrWhiteSpace(url) ? "&url=" + Authentication.Utils.UrlEncode(url) : "";
			var description = ((TextBox)g.FindName("Description")).Text;
			parameterString += !string.IsNullOrWhiteSpace(description) ? "&description=" + Authentication.Utils.UrlEncode(description) : "";

			var tags = ((TextBox)g.FindName("Tags")).Text;
			if (!string.IsNullOrEmpty(tags)) {
				tags = tags.Replace(" #", ", ");
				tags = Authentication.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length) : tags.Substring(0, tags.Length)));
				parameterString += "&tags=" + tags;
			}
			try {
				var status = "";
				if (((Image)sender).Tag != null) {
					if (((Image)sender).Tag.ToString() == "queue") {
						var publishOn = ((TextBox)g.FindName("PublishOn")).Text;
						if (string.IsNullOrWhiteSpace(publishOn)) {
							App.Alert("Please enter a time to publish the post on.");
							return;
						} else {
							status = "&state=queue&publish_on=" + Authentication.Utils.UrlEncode(publishOn);
						}
					} else if (((Image)sender).Tag.ToString() == "draft") {
						status = "&state=draft";
					}
				}
				parameterString += status;
				var result = await CreateRequest.CreatePost(parameterString);
				Debug.WriteLine(await result.Content.ReadAsStringAsync());
				if (result.StatusCode == HttpStatusCode.Created) {
					Type.IsEnabled = true;
					App.HideStatus();
					Frame.GoBack();
				} else
					App.Alert(result.ReasonPhrase);
			} catch (Exception ex) {
				App.Alert("Failed to create post");
			}
			Type.IsEnabled = true;
			App.HideStatus();
		}
	}
}
