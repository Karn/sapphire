using APIWrapper.AuthenticationManager;
using APIWrapper.Client;
using APIWrapper.Content;
using APIWrapper.Utils;
using Core.Common;
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

namespace Core.Pages {
    public sealed partial class CreatePost : Page, IFileOpenPickerContinuable {

        private static string TAG = "CreatePost";

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        StorageFile image;
        Image PhotoView;

        public CreatePost() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

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
            string type = e.NavigationParameter.ToString();
            Debug.WriteLine(type);
            Type.DataContext = type;
            PageTitle.Text = PageTitle.Text.ToString() + ": " + type;
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
                if (!"#, ".Contains(tagBox.Text.Last().ToString())) {
                    var tags = ((TextBox)sender).Text.Split(',');
                    var converted = "";
                    for (var i = 0; i < tags.Count() - 1; i++) {
                        converted += string.Format("#{0}, ", tags[i].Trim('#', ',', ' '));
                    }
                    tagBox.Text = converted.TrimEnd(' ');
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
                    var x = await image.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView);
                    var photo = new BitmapImage();
                    photo.SetSource(x);

                    PhotoView.Source = photo;
                    PhotoView.Stretch = Stretch.UniformToFill;
                }
            } else {
                MainPage.AlertFlyout.DisplayMessage("Failed to select image.");
            }
        }

        private async void PostTextButton_Tapped(object sender, TappedRoutedEventArgs e) {
            App.DisplayStatus("Creating text post...");
            Type.IsEnabled = false;

            var g = (Grid)((StackPanel)((Image)sender).Parent).Parent;

            var parameterString = "type=text";
            var title = ((TextBox)g.FindName("Title")).Text;
            parameterString += !string.IsNullOrWhiteSpace(title) ? "&title=" + title : "";
            var body = ((TextBox)g.FindName("Body")).Text;
            parameterString += !string.IsNullOrWhiteSpace(body) ? "&body=" + body : "";
            var tags = ((TextBox)g.FindName("Tags")).Text;
            if (!string.IsNullOrEmpty(tags)) {
                tags = tags.Replace(" #", ", ");
                tags = Authentication.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length - 1) : tags.Substring(0, tags.Length - 1)));
                parameterString += "&tags=" + tags;
            }

            try {
                var status = "";
                if (((Image)sender).Tag != null) {
                    if (((Image)sender).Tag.ToString() == "queue") {
                        var publishOn = ((TextBox)g.FindName("PublishOn")).Text;
                        if (string.IsNullOrWhiteSpace(publishOn)) {
                            MainPage.AlertFlyout.DisplayMessage("Please enter a time to publish the post on.");
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
                    MainPage.AlertFlyout.DisplayMessage(result.ReasonPhrase);
            } catch (Exception ex) {
                DiagnosticsManager.LogException(ex, TAG, "Failed to create post");
                MainPage.AlertFlyout.DisplayMessage("Failed to create post");
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
            parameterString += !string.IsNullOrWhiteSpace(quote) ? "&quote=" + quote : "";
            var source = ((TextBox)g.FindName("Source")).Text;
            parameterString += !string.IsNullOrWhiteSpace(source) ? "&source=" + source : "";
            var tags = ((TextBox)g.FindName("Tags")).Text;
            if (!string.IsNullOrEmpty(tags)) {
                tags = tags.Replace(" #", ", ");
                tags = Authentication.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length - 1) : tags.Substring(0, tags.Length - 1)));
                parameterString += "&tags=" + tags;
            }
            try {
                var status = "";
                if (((Image)sender).Tag != null) {
                    if (((Image)sender).Tag.ToString() == "queue") {
                        var publishOn = ((TextBox)g.FindName("PublishOn")).Text;
                        if (string.IsNullOrWhiteSpace(publishOn)) {
                            MainPage.AlertFlyout.DisplayMessage("Please enter a time to publish the post on.");
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
                    MainPage.AlertFlyout.DisplayMessage(result.ReasonPhrase);
            } catch (Exception ex) {
                MainPage.AlertFlyout.DisplayMessage("Failed to create post");
                DiagnosticsManager.LogException(ex, TAG, "Failed to create post");
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
                tags = tags.StartsWith(" ") ? tags.Substring(1, tags.Length - 1) : tags.Substring(0, tags.Length - 1);
                parameterString += "&tags=" + tags;
            }

            try {
                var status = "";
                if (((Image)sender).Tag != null) {
                    if (((Image)sender).Tag.ToString() == "queue") {
                        var publishOn = ((TextBox)g.FindName("PublishOn")).Text;
                        if (string.IsNullOrWhiteSpace(publishOn)) {
                            MainPage.AlertFlyout.DisplayMessage("Please enter a time to publish the post on.");
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
                    var result = await RequestHandler.POST("https://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/post", image, parameterString);
                    Debug.WriteLine(await result.Content.ReadAsStringAsync());
                    if (result.StatusCode == HttpStatusCode.Created) {
                        Type.IsEnabled = true;
                        App.HideStatus();
                        Frame.GoBack();
                    } else
                        MainPage.AlertFlyout.DisplayMessage(result.ReasonPhrase);
                }
            } catch (Exception ex) {
                MainPage.AlertFlyout.DisplayMessage("Failed to create post");
                DiagnosticsManager.LogException(ex, TAG, "Failed to create post");
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
            parameterString += !string.IsNullOrWhiteSpace(title) ? "&title=" + title : "";
            var url = ((TextBox)g.FindName("Link")).Text;
            parameterString += !string.IsNullOrWhiteSpace(url) ? "&url=" + url : "";
            var description = ((TextBox)g.FindName("Description")).Text;
            parameterString += !string.IsNullOrWhiteSpace(description) ? "&description=" + description : "";

            var tags = ((TextBox)g.FindName("Tags")).Text;
            if (!string.IsNullOrEmpty(tags)) {
                tags = tags.Replace(" #", ", ");
                tags = Authentication.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length - 1) : tags.Substring(0, tags.Length - 1)));
                parameterString += "&tags=" + tags;
            }
            try {
                var status = "";
                if (((Image)sender).Tag != null) {
                    if (((Image)sender).Tag.ToString() == "queue") {
                        var publishOn = ((TextBox)g.FindName("PublishOn")).Text;
                        if (string.IsNullOrWhiteSpace(publishOn)) {
                            MainPage.AlertFlyout.DisplayMessage("Please enter a time to publish the post on.");
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
                    MainPage.AlertFlyout.DisplayMessage(result.ReasonPhrase);
            } catch (Exception ex) {
                MainPage.AlertFlyout.DisplayMessage("Failed to create post");
                DiagnosticsManager.LogException(ex, TAG, "Failed to create post");
            }
            Type.IsEnabled = true;
            App.HideStatus();
        }
    }
}
