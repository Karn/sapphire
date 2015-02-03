using APIWrapper.Client;
using APIWrapper.Content.Model;
using Core.Shared.Common;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System.Linq;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
    public sealed partial class FollowersFollowing : Page {
        private NavigationHelper navigationHelper;

        private static ObservableCollection<Blog> BlogList = new ObservableCollection<Blog>();

        private int offset = 0;
        private static ScrollViewer sv;
        private static bool NewInstance;
        public bool loading = false;

        public FollowersFollowing() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        public async void SetItemSource() {
            if (!NewInstance) {
                BlogList.Clear();
                NewInstance = true;
            }
            App.DisplayStatus("Loading blogs...");
            if (PageTitle.Text.ToString() == "Followers") {
                foreach (var blog in await CreateRequest.RetrieveFollowers(offset)) {
                    //if (!BlogList.Contains(blog))
                    BlogList.Add(blog);
                }
            } else if (PageTitle.Text.ToString() == "Following") {
                foreach (var blog in await CreateRequest.RetrieveFollowing(offset)) {
                    //if (!BlogList.Contains(blog)) {
                    blog.IsFollowing = true;
                    BlogList.Add(blog);
                    //}
                }
            }
            List.ItemsSource = BlogList;
            offset += 20;
            App.HideStatus();
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
            PageTitle.Text = e.NavigationParameter.ToString();

            if (List.Items == null || List.Items.Count == 0) {
                SetItemSource();
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
            NewInstance = false;
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

        private async void FollowUnfollowButton_Tapped(object sender, TappedRoutedEventArgs e) {
            var x = ((Button)sender);
            x.IsEnabled = false;
            if (x.Content.ToString().ToLower() == "follow") {
                App.DisplayStatus("Following user...");
                if (await CreateRequest.FollowUnfollow(true, x.Tag.ToString())) {
                    x.Content = "UNFOLLOW";
                }
            } else if (x.Content.ToString().ToLower() == "unfollow") {
                App.DisplayStatus("Unfollowing user...");
                if (await CreateRequest.FollowUnfollow(false, x.Tag.ToString())) {
                    x.Content = "FOLLOW";
                }
            }
            x.IsEnabled = true;
            App.HideStatus();
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e) {
            if (sv == null)
                sv = (ScrollViewer)sender;

            if (sv.VerticalOffset + 50 > sv.ExtentHeight - sv.ActualHeight) {

                SetItemSource();
            }
        }

        private void GoToBlog(object sender, TappedRoutedEventArgs e) {
            if (((FrameworkElement)sender).Tag != null) {
                var frame = Window.Current.Content as Frame;
                if (!frame.Navigate(typeof(BlogDetails), ((FrameworkElement)sender).Tag.ToString().Split(' ')[0]))
                    throw new Exception("Navigation Failed");
            }
        }
    }
}
