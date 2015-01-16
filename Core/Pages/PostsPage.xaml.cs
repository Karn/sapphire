using APIWrapper.Client;
using Core.Shared.Common;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
    public sealed partial class PostsPage : Page {
        private NavigationHelper navigationHelper;

        public static ImageSource BlogsIcon = App.Current.Resources["BlogsIcon"] as BitmapImage;
        public static ImageSource TagsIcon = App.Current.Resources["TagsIcon"] as BitmapImage;

        bool loaded = false;
        string tag = "";

        public PostsPage() {
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
            PostList.URL = e.NavigationParameter.ToString();
            if (PostList.URL.Contains("/likes")) {
                PageTitle.Text = "Likes";
            } else if (PostList.URL.Contains("/tagged")) {
                var x = PostList.URL.Split('?');
                var y = x[1].Split('&');
                tag = Uri.UnescapeDataString(y[0].Substring(4));
                PageTitle.Text = "Search: " + tag.Replace('+', ' ');
                Mode.Visibility = Visibility.Visible;
            } else if (PostList.URL.Contains("/submission")) {
                PageTitle.Text = "Inbox";
            } else if (PostList.URL.Contains("/draft")) {
                PageTitle.Text = "Drafts";
            } else if (PostList.URL.Contains("/queue")) {
                PageTitle.Text = "Queue";
            }

            MainPage.AlertFlyout = _ErrorFlyout;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) { }

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

        private void PostList_Loaded(object sender, RoutedEventArgs e) {
            if (!loaded) {
                PostList.LoadPosts(true);
                loaded = true;
            }
        }

        private async void Mode_Tapped(object sender, TappedRoutedEventArgs e) {
            if (PostList.Visibility == Visibility.Visible) {
                Mode.Source = TagsIcon;
                ToTop.Visibility = Visibility.Collapsed;
                PostList.Visibility = Visibility.Collapsed;
                BlogSearch.Visibility = Visibility.Visible;
                if (BlogSearch.ItemsSource == null) {
                    if (APIWrapper.AuthenticationManager.Authentication.Utils.NetworkAvailable())
                        BlogSearch.ItemsSource = await CreateRequest.BlogSearch(tag);
                }
            } else {
                Mode.Source = BlogsIcon;
                ToTop.Visibility = Visibility.Visible;
                PostList.Visibility = Visibility.Visible;
                BlogSearch.Visibility = Visibility.Collapsed;
            }
        }

        private void GoToBlog(object sender, TappedRoutedEventArgs e) {
            if (((FrameworkElement)sender).Tag != null) {
                var frame = Window.Current.Content as Frame;
                if (!frame.Navigate(typeof(BlogDetails), ((FrameworkElement)sender).Tag.ToString().Split(' ')[0]))
                    throw new Exception("Navigation Failed");
            }
        }

        private void ToTop_Tapped(object sender, TappedRoutedEventArgs e) {
            PostList.ScrollToTop();
        }
    }
}
