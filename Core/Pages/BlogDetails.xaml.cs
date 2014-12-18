﻿using APIWrapper;
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
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public static ImageSource FavImage = App.Current.Resources["DefaultFavAsset"] as BitmapImage;
        public static ImageSource UnfavImage = App.Current.Resources["DefaultUnfavAsset"] as BitmapImage;

        string blogName;

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
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
            blogName = e.NavigationParameter.ToString();
            LayoutRoot.DataContext = await CreateRequest.GetBlog(blogName);
            if (!Utils.AppLicenseHandler.IsTrial) {
                Fav.Visibility = Visibility.Visible;
            }
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
            Posts.URL = "https://api.tumblr.com/v2/blog/" + blogName + ".tumblr.com/posts";
            Posts.LoadPosts();
        }

        private async void FollowUnfollowButton_Tapped(object sender, TappedRoutedEventArgs e) {
            var x = ((Button)sender);
            if (x.Content.ToString().ToLower() == "follow") {
                if (await CreateRequest.FollowUnfollow(true, x.Tag.ToString())) {
                    x.Content = "UNFOLLOW";
                }
            } else if (x.Content.ToString().ToLower() == "unfollow") {
                if (await CreateRequest.FollowUnfollow(false, x.Tag.ToString())) {
                    x.Content = "FOLLOW";
                }
            }
        }

        private void Fav_Tapped(object sender, TappedRoutedEventArgs e) {
            if (((Image)sender).Tag != null) {
                var name = ((Image)sender).Tag.ToString();
                if (UserStore.FavBlogList.Any(b => b.Name == name)) {
                    UserStore.RemoveFav(name);
                    ((Image)sender).Source = UnfavImage;
                } else {
                    UserStore.AddFav(name);
                    ((Image)sender).Source = FavImage;
                }
            }
        }

        private void Fav_Loaded(object sender, RoutedEventArgs e) {
            if (((Image)sender).Tag != null) {
                if (UserStore.FavBlogList.Any(b => b.Name == ((Image)sender).Tag.ToString()))
                    ((Image)sender).Source = FavImage;
                else
                    ((Image)sender).Source = UnfavImage;
            }
        }
    }
}
