using APIWrapper.Utils;
using APIWrapper.AuthenticationManager;
using APIWrapper.Client;
using Core.Common;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReblogPage : Page {

        private static string TAG = "ReblogPage";

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        string postID = "";
        string reblogKey = "";

        public ReblogPage() {
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
            var x = e.NavigationParameter.ToString().Split(',');
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

        private async void PostButton_Tapped(object sender, TappedRoutedEventArgs e) {


            var tags = Tags.Text;
            if (!string.IsNullOrEmpty(tags)) {
                tags = tags.Replace(" #", ", ");
                tags = Authentication.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length - 1) : tags.Substring(0, tags.Length - 1)));
            }
            try {
                var status = "";
                if (((Image)sender).Tag != null) {
                    if (((Image)sender).Tag.ToString() == "queue") {
                        if (string.IsNullOrWhiteSpace(PublishOn.Text)) {
                            MainPage.AlertFlyout.DisplayMessage("Please enter a time to publish the post on.");
                            return;
                        } else {
                            status = "&state=queue&publish_on=" + Authentication.Utils.UrlEncode(PublishOn.Text);
                        }
                    } else if (((Image)sender).Tag.ToString() == "draft") {
                        status = "&state=draft";
                    }
                }
                if (await CreateRequest.ReblogPost(postID, reblogKey, Authentication.Utils.UrlEncode(Caption.Text), tags, status)) {
                    MainPage.AlertFlyout.DisplayMessage("Created.");
                } else {
                    if (((Image)sender).Tag.ToString() == "queue") {
                        MainPage.AlertFlyout.DisplayMessage("Failed to add post to queue. Remember to use the same format as on the site!");
                    } else if (((Image)sender).Tag.ToString() == "draft") {
                        MainPage.AlertFlyout.DisplayMessage("Failed to add post to drafts.");
                    } else
                        MainPage.AlertFlyout.DisplayMessage("Failed to reblog post.");
                    return;
                }
                Frame.GoBack();
            } catch (Exception ex) {
                MainPage.AlertFlyout.DisplayMessage("Failed to create post");
                DiagnosticsManager.LogException(ex, TAG, "Failed to create post");
            }
        }
    }
}
