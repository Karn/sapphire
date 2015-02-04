using APIWrapper.Client;
using APIWrapper.Content;
using Core.Shared.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlogDetails : Page {
        private NavigationHelper navigationHelper;

        static string blogName;

        static object cached_blog_data = null;
        static object cached_post_data = null;

        public BlogDetails() {
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

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
            if (cached_blog_data == null || blogName != e.NavigationParameter.ToString()) {
                blogName = e.NavigationParameter.ToString();
                LayoutRoot.DataContext = await CreateRequest.GetBlog(blogName);
                Posts.URL = "https://api.tumblr.com/v2/blog/" + blogName + ".tumblr.com/posts";
                Posts.LoadPosts();
            } else {
                Debug.WriteLine("Loading from cache.");
                LayoutRoot.DataContext = cached_blog_data;
                Posts.LoadPosts(cached_post_data);
                cached_blog_data = null;
                cached_post_data = null;
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) {
            cached_blog_data = LayoutRoot.DataContext;
            cached_post_data = Posts.GetPostSource();
        }

        #region NavigationHelper registration
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedFrom(e);
        }
        #endregion

        private void Posts_Loaded(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(Posts.LastPostID) || !Posts.URL.Contains(blogName)) {
            }
        }

        private async void FollowUnfollowButton_Tapped(object sender, TappedRoutedEventArgs e) {
            var x = ((Button)sender);
            if (x.Tag != null) {
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
            }
            App.HideStatus();
        }

        private void Fav_Tapped(object sender, TappedRoutedEventArgs e) {
            if (((TextBlock)sender).Tag != null) {
                var name = ((TextBlock)sender).Tag.ToString();
                if (UserStorageUtils.FavBlogList.Any(b => b.Name == name)) {
                    UserStorageUtils.RemoveFav(name);
                    ((TextBlock)sender).Text = "add to favorites";
                } else {
                    UserStorageUtils.AddFav(name);
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
                if (UserStorageUtils.FavBlogList.Any(b => b.Name == name)) {
                    ((TextBlock)((StackPanel)sender).FindName("fav")).Text = "remove from favorites";
                } else {
                    ((TextBlock)((StackPanel)sender).FindName("fav")).Text = "add to favorites";
                }
            }
        }
    }
}
