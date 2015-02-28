using Core.Client;
using Sapphire.Shared.Common;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Sapphire.Pages {
	public sealed partial class PostsPage : Page {
        private NavigationHelper navigationHelper;

        public static string TAG = "PostsPage";

        string searchTag = "";

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
            PostFeed.URL = e.NavigationParameter.ToString();

            if (PostFeed.URL.Contains("/likes")) {
                PageTitle.Text = "Likes";
            } else if (PostFeed.URL.Contains("/tagged")) {
                var x = PostFeed.URL.Split('?');
                var y = x[1].Split('&');
                searchTag = Uri.UnescapeDataString(y[0].Substring(4));
                PageTitle.Text = "Search: " + searchTag.Replace('+', ' ');
                ResultsHeader.Visibility = Visibility.Visible;
                BlogsHeader.Visibility = Visibility.Visible;
                Blogs.Visibility = Visibility.Visible;
            } else if (PostFeed.URL.Contains("/submission")) {
                PageTitle.Text = "Inbox";
            } else if (PostFeed.URL.Contains("/draft")) {
                PageTitle.Text = "Drafts";
            } else if (PostFeed.URL.Contains("/queue")) {
                PageTitle.Text = "Queue";
            }
			
            PostFeed.LoadPosts();
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


        private void GoToBlog(object sender, TappedRoutedEventArgs e) {
            if (((FrameworkElement)sender).Tag != null) {
                var frame = Window.Current.Content as Frame;
                if (!frame.Navigate(typeof(BlogDetails), ((FrameworkElement)sender).Tag.ToString().Split(' ')[0]))
                    throw new Exception("Navigation Failed");
            }
        }

        private void ToTop_Tapped(object sender, TappedRoutedEventArgs e) {
            PostFeed.ScrollToTop();
        }

        private async void FollowUnfollowButton_Tapped(object sender, TappedRoutedEventArgs e) {
            var btn = (Button)sender;
            if (btn.Content.ToString().ToLower() == "follow") {
                App.DisplayStatus("Following user...");
                if (await CreateRequest.FollowUnfollow(true, btn.Tag.ToString())) {
                    btn.Content = "UNFOLLOW";
                }
            } else if (btn.Content.ToString().ToLower() == "unfollow") {
                App.DisplayStatus("Unfollowing user...");
                if (await CreateRequest.FollowUnfollow(false, btn.Tag.ToString())) {
                    btn.Content = "FOLLOW";
                }
            }
            App.HideStatus();
        }

        private async void BlogSearch_Loaded(object sender, RoutedEventArgs e) {
            if (BlogSearch.ItemsSource == null) {
                App.DisplayStatus("Searching for blogs...");
                BlogSearch.ItemsSource = await CreateRequest.BlogSearch(searchTag);
                App.HideStatus();
            }
        }
    }
}
