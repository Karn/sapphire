using APIWrapper;
using APIWrapper.AuthenticationManager;
using APIWrapper.Client;
using APIWrapper.Content;
using Core.Common;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlogDetails : Page {
        private NavigationHelper navigationHelper;

        private string blogName;

        public BlogDetails() {
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
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
            blogName = e.NavigationParameter.ToString();
            LayoutRoot.DataContext = await CreateRequest.GetBlog(blogName);
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

        private void Posts_Loaded(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(Posts.LastPostID) || !Posts.URL.Contains(blogName)) {
                Posts.URL = "https://api.tumblr.com/v2/blog/" + blogName + ".tumblr.com/posts";
                Posts.LoadPosts();
            }
        }

        private async void FollowUnfollowButton_Tapped(object sender, TappedRoutedEventArgs e) {
            var x = ((Button)sender);
            if (x.Content.ToString().ToLower() == "+") {
                App.DisplayStatus("Following user...");
                if (await CreateRequest.FollowUnfollow(true, x.Tag.ToString())) {
                    x.Content = "-";
                }
            } else if (x.Content.ToString().ToLower() == "-") {
                App.DisplayStatus("Unfollowing user...");
                if (await CreateRequest.FollowUnfollow(false, x.Tag.ToString())) {
                    x.Content = "+";
                }
            }
            App.HideStatus();
        }

        private void Fav_Tapped(object sender, TappedRoutedEventArgs e) {
            if (((TextBlock)sender).Tag != null) {
                var name = ((TextBlock)sender).Tag.ToString();
                if (UserStore.FavBlogList.Any(b => b.Name == name)) {
                    UserStore.RemoveFav(name);
                    ((TextBlock)sender).Text = "add to favorites";
                } else {
                    UserStore.AddFav(name);
                    ((TextBlock)sender).Text = "remove from favorites";
                }
            }
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e) {
            if (((TextBlock)sender).Tag != null) {
                var frame = Window.Current.Content as Frame;
                if (!frame.Navigate(typeof(PostsPage), "https://api.tumblr.com/v2/blog/" + ((TextBlock)sender).Tag.ToString() + ".tumblr.com/likes"))
                    throw new Exception("Navigation Failed");
            }
        }

        private void Flyout_Loaded(object sender, RoutedEventArgs e) {
            if (((StackPanel)sender).Tag != null) {
                var name = ((StackPanel)sender).Tag.ToString();
                if (UserStore.FavBlogList.Any(b => b.Name == name)) {
                    ((TextBlock)((StackPanel)sender).FindName("fav")).Text = "remove from favorites";
                } else {
                    ((TextBlock)((StackPanel)sender).FindName("fav")).Text = "add to favorites";
                }
            }
        }
    }
}
