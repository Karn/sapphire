using APIWrapper.AuthenticationManager;
using APIWrapper.Client;
using APIWrapper.Content;
using APIWrapper.Utils;
using Core.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
            //var x = new List<string>();
            //x.Add(type);
            Type.DataContext = type;
            PageTitle.Text = PageTitle.Text.ToString() + ": " + type;
            MainPage.NPD.Visibility = Visibility.Collapsed;
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
            if (e.Key == Windows.System.VirtualKey.Space) {
                var x = ((TextBox)sender).Text.Split(' ');
                var tags = string.Empty;
                var tags2 = string.Empty;
                foreach (var tag in x) {
                    if (!tag.StartsWith("#"))
                        tags += "#" + tag + " ";
                    else {
                        tags += tag + " ";
                    }
                }
                ((TextBox)sender).Text = tags;
                x = tags.Split(' ');
            } else if (e.Key == Windows.System.VirtualKey.Back) {
                var x = ((TextBox)sender).Text.Split(' ');
                var tags = string.Empty;
                for (int i = 0; i < x.Count() - 1; i++) {
                    tags += x.ElementAt(i) + " ";
                }

                ((TextBox)sender).Text = tags;
            }
            ((TextBox)sender).Select(((TextBox)sender).Text.Length, 0);
        }


        private void Photo_Image_Tapped(object sender, TappedRoutedEventArgs e) {
            PhotoView = (Image)((Grid)sender).FindName("Photo_Image");
            //PhotoView = (Image)((Grid)(((Grid)sender).Parent).FindName("Photo_Grid")).FindName("Photo_Image");

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
                    await photo.SetSourceAsync(fileStream);

                    PhotoView.Source = photo;
                    PhotoView.Stretch = Stretch.UniformToFill;
                }
            } else {
                MainPage.AlertFlyout.DisplayMessage("Failed to select image.");
            }
        }

        private void PostTextButton_Tapped(object sender, TappedRoutedEventArgs e) {
            var g = (Grid)((StackPanel)((Image)sender).Parent).Parent;
            var title = ((TextBox)g.FindName("Title")).Text;
            var body = ((TextBox)g.FindName("Body")).Text;
            var tags = ((TextBox)g.FindName("Tags")).Text;
            if (!string.IsNullOrEmpty(tags)) {
                tags = tags.Replace(" #", ", ");
                tags = Authentication.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length - 1) : tags.Substring(0, tags.Length - 1)));
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
                APIWrapper.Content.Model.CreatePost.Text(Authentication.Utils.UrlEncode(title), Authentication.Utils.UrlEncode(body), tags, status);
                Frame.GoBack();
            } catch (Exception ex) {
                DiagnosticsManager.LogException(ex, TAG, "Failed to create post");
                MainPage.AlertFlyout.DisplayMessage("Failed to create post");
            }
        }

        private void PostQuoteButton_Tapped(object sender, TappedRoutedEventArgs e) {
            var g = (Grid)((StackPanel)((Image)sender).Parent).Parent;
            var quote = ((TextBox)g.FindName("Quote")).Text;
            var source = ((TextBox)g.FindName("Source")).Text;
            var tags = ((TextBox)g.FindName("Tags")).Text;
            if (!string.IsNullOrEmpty(tags)) {
                tags = tags.Replace(" #", ", ");
                tags = Authentication.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length - 1) : tags.Substring(0, tags.Length - 1)));
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
                APIWrapper.Content.Model.CreatePost.Quote(Authentication.Utils.UrlEncode(quote), Authentication.Utils.UrlEncode(source), tags, status);
                Frame.GoBack();
            } catch (Exception ex) {
                MainPage.AlertFlyout.DisplayMessage("Failed to create post");
                DiagnosticsManager.LogException(ex, TAG, "Failed to create post");
            }
        }

        private async void PostPhotoButton_Tapped(object sender, TappedRoutedEventArgs e) {
            //var caption = ((TextBox)((StackPanel)((Button)sender).Parent).FindName("Photo_Caption")).Text;
            //var tags = ((TextBox)((StackPanel)((Button)sender).Parent).FindName("Photo_Tags")).Text;
            //if (!string.IsNullOrEmpty(tags)) {
            //    tags = tags.Replace(" #", ", ");
            //    tags = Authentication.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length - 1) : tags.Substring(0, tags.Length - 1)));
            //}

            try {
                Debug.WriteLine("Trying to post");
                if (image != null) {
                    var result = await RequestHandler.POST("http://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/post", image);
                    Debug.WriteLine(await result.Content.ReadAsStringAsync());
                }
            } catch (Exception ex) {
                MainPage.AlertFlyout.DisplayMessage("Failed to create post");
                DiagnosticsManager.LogException(ex, TAG, "Failed to create post");
            }
            Frame.GoBack();
        }

        private void PostLinkButton_Tapped(object sender, TappedRoutedEventArgs e) {
            var g = (Grid)((StackPanel)((Image)sender).Parent).Parent;
            var title = ((TextBox)g.FindName("Title")).Text;
            var url = ((TextBox)g.FindName("Link")).Text;
            var description = ((TextBox)g.FindName("Description")).Text;
            var tags = ((TextBox)g.FindName("Tags")).Text;
            if (!string.IsNullOrEmpty(tags)) {
                tags = tags.Replace(" #", ", ");
                tags = Authentication.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length - 1) : tags.Substring(0, tags.Length - 1)));
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
                APIWrapper.Content.Model.CreatePost.Link(Authentication.Utils.UrlEncode(title), Authentication.Utils.UrlEncode(url), Authentication.Utils.UrlEncode(description), tags, status);
                Frame.GoBack();
            } catch (Exception ex) {
                MainPage.AlertFlyout.DisplayMessage("Failed to create post");
                DiagnosticsManager.LogException(ex, TAG, "Failed to create post");
            }
        }
    }
}
