using APIWrapper.AuthenticationManager;
using APIWrapper.Client;
using APIWrapper.Content;
using APIWrapper.Utils;
using Core.Common;
using Core.Utils.Controls;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace Core {
    public sealed partial class MainPage : Page {

        public static string TAG = "MainPage";

        public static AlertDialog AlertFlyout;

        public static Storyboard RefreshButtonIntoView_;
        public static Storyboard RefreshButtonOutOfView_;

        public static Storyboard CreateButtonIntoView_;
        public static Storyboard CreateButtonOutOfView_;

        public static bool IsAnimating = false;

        private readonly NavigationHelper navigationHelper;
        public static StatusBar sb;

        public static FlyoutPresenter _LRPresenter;
        public static StackPanel MainPagePresenter;

        private static bool JustNavigatedBack;
        private static bool NavigatedFromToast;

        public static bool SwitchedBlog = false;
        public static bool SwitchedAccount = false;

        public static NewPostDialog NPD;

        public static Ellipse NewPostIndicator;

        public MainPage() {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            if (sb == null) {
                sb = StatusBar.GetForCurrentView();
                sb.ForegroundColor = Color.FromArgb(255, 255, 255, 255);
            }

            //Initialize
            AlertFlyout = _ErrorFlyout; //Mainpage error toast

            if (Authentication.AuthenticatedTokens != null && Authentication.AuthenticatedTokens.Count > 1) {
                AccountManageButton.Label = "accounts";
            }

            NPD = CreatePostControl;

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            if (UserStore.NotificationsEnabled)
                RegisterBackgroundTask();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e) {
            HeaderAnimateIn.Begin();
            if (!UserStore.EnableStatusBarBG)
                StatusBarBG.Background = App.Current.Resources["HeaderLightBlue"] as SolidColorBrush;
            else
                StatusBarBG.Background = App.Current.Resources["HeaderDarkBlue"] as SolidColorBrush;

        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e) {
            //Fix navigating
            if (CreatePostControl.Visibility == Visibility.Visible) {
                CreatePostIcon.RenderTransform = new CompositeTransform() { Rotation = 0 };
                CreatePostFill.Fill = App.Current.Resources["HeaderLightBlue"] as SolidColorBrush;
                CreatePostControl.Visibility = Visibility.Collapsed;
                e.Handled = true;
            } else if (JustNavigatedBack) {
                JustNavigatedBack = false;
                e.Handled = true;
                return;
            } else if (NavigationPivot.SelectedIndex != 0) {
                NavigationPivot.SelectedIndex = 0;
                e.Handled = true;
                return;
            } else {
                Application.Current.Exit();
            }
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
            if (e.NavigationParameter != null && !string.IsNullOrEmpty(e.NavigationParameter.ToString()) && !NavigatedFromToast) {
                //Handle accounts switch to the one described in the toast
                Posts_Loaded(null, null);
                NavigationPivot.SelectedIndex = 1;
                NavigatedFromToast = true;
            }

            AlertFlyout = _ErrorFlyout;

            if (CreatePostControl.Visibility == Visibility.Collapsed) {
                CreatePostIcon.RenderTransform = new CompositeTransform() { Rotation = 0 };
                CreatePostFill.Fill = App.Current.Resources["HeaderLightBlue"] as SolidColorBrush;
            }

            if (SwitchedAccount) {
                NavigationPivot.SelectedIndex = 0;
                CreateRequest.ReloadAccountData = true;
                SetAccountData();
                Posts.RefeshPosts();
                SwitchedAccount = false;
            } else if (SwitchedBlog) {
                AccountPivot.DataContext = UserStore.CurrentBlog;
                ActivityPosts.ClearPosts();
                ActivityPosts.LoadPosts();
                SwitchedBlog = false;
            }

            //Remove entries before this page.
            Frame.BackStack.Clear();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache. Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) {
            // TODO: Save the unique state of the page here.
            JustNavigatedBack = true;
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

        private async void RegisterBackgroundTask() {
            // Windows Phone app must call this to use trigger types (see MSDN)

            foreach (var cur in BackgroundTaskRegistration.AllTasks) {
                if (cur.Value.Name == "PushNotificationTask") {
                    DiagnosticsManager.LogException(null, TAG, "Previous task found.");
                    return;
                }
            }
            await BackgroundExecutionManager.RequestAccessAsync();

            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder { Name = "PushNotificationTask", TaskEntryPoint = "BackgroundUtilities.NotificationHandler" };
            taskBuilder.SetTrigger(new TimeTrigger(15, false));
            taskBuilder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            BackgroundTaskRegistration myFirstTask = taskBuilder.Register();
        }

        private void Navigation_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            handleNav(Navigation.SelectedIndex);
        }

        private void NavigationPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            handleNav(NavigationPivot.SelectedIndex);
        }

        private void handleNav(int SelectedItem) {
            if (NavigationPivot.SelectedIndex != 0) {
                RefreshButton.Visibility = Visibility.Visible;
                CommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
            } else if (NavigationPivot.SelectedIndex == 3) {
                RefreshButton.Visibility = Visibility.Collapsed;
            } else {
                RefreshButton.Visibility = Visibility.Collapsed;
                CommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
            }
            switch (SelectedItem) {
                case 0:
                    PageTitle.Text = "Dashboard";
                    DashboardIcon.Opacity = 1.0;
                    AccountIcon.Opacity = 0.5;
                    ActivityIcon.Opacity = 0.5;
                    SearchIcon.Opacity = 0.5;
                    NavigationPivot.SelectedIndex = 0;
                    break;
                case 1:
                    PageTitle.Text = "Activity";
                    DashboardIcon.Opacity = 0.5;
                    AccountIcon.Opacity = 0.5;
                    ActivityIcon.Opacity = 1.0;
                    SearchIcon.Opacity = 0.5;
                    NavigationPivot.SelectedIndex = 1;
                    break;
                case 2:
                    PageTitle.Text = "Account";
                    DashboardIcon.Opacity = 0.5;
                    AccountIcon.Opacity = 1.0;
                    ActivityIcon.Opacity = 0.5;
                    SearchIcon.Opacity = 0.5;
                    NavigationPivot.SelectedIndex = 2;
                    break;
                case 3:
                    PageTitle.Text = "Explore";
                    DashboardIcon.Opacity = 0.5;
                    AccountIcon.Opacity = 0.5;
                    ActivityIcon.Opacity = 0.5;
                    SearchIcon.Opacity = 1.0;
                    NavigationPivot.SelectedIndex = 3;
                    break;
                default:
                    goto case 0;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e) {
            if (!Frame.Navigate(typeof(Pages.Settings))) {
                DiagnosticsManager.LogException(null, TAG, "Failed to navigate to Settings.");
            }
        }

        private void ManageAccountButton_Click(object sender, RoutedEventArgs e) {
            if (!Frame.Navigate(typeof(Pages.AccountManager))) {
                DiagnosticsManager.LogException(null, TAG, "Failed to navigate to AccountManager.");
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e) {
            switch (NavigationPivot.SelectedIndex) {
                case 0:
                    break;
                case 1:
                    ActivityPosts.LoadPosts();
                    break;
                case 2:
                    SetAccountData();
                    break;
                case 3:
                    SpotlightTags.ItemsSource = await CreateRequest.RetrieveSpotlight(true);
                    break;
                default:
                    goto case 0;
            }
        }

        private void AccountDetails_Tapped(object sender, TappedRoutedEventArgs e) {
            if (UserStore.CurrentBlog != null) {
                switch (((StackPanel)sender).Tag.ToString()) {
                    case "Posts":
                        if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/posts")) {
                            DiagnosticsManager.LogException(null, TAG, "Failed to navigate to current blogs posts.");
                        }
                        break;
                    case "Likes":
                        if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/user/likes")) {
                            DiagnosticsManager.LogException(null, TAG, "Failed to navigate to current blogs likes.");
                        }
                        break;
                    case "Followers":
                        if (!Frame.Navigate(typeof(Pages.FollowersFollowing), "Followers")) {
                            DiagnosticsManager.LogException(null, TAG, "Failed to navigate to Followers.");
                        }
                        break;
                    case "Following":
                        if (!Frame.Navigate(typeof(Pages.FollowersFollowing), "Following")) {
                            DiagnosticsManager.LogException(null, TAG, "Failed to navigate to Following.");
                        }
                        break;
                }
            }
        }

        public void Posts_Loaded(object sender, RoutedEventArgs e) {
            if (UserStore.CurrentBlog == null)
                SetAccountData();

            Posts.LoadPosts();
        }

        private async void SetAccountData() {
            if (APIWrapper.AuthenticationManager.Authentication.Utils.NetworkAvailable()) {
                RefreshButton.IsEnabled = false;
                sb.ProgressIndicator.Text = "Loading account data...";
                await sb.ProgressIndicator.ShowAsync();
                Debug.WriteLine("Loading account data...");
                AccountPivot.DataContext = await CreateRequest.RetrieveAccountInformation() ? UserStore.CurrentBlog : null;
                if (!ActivityPosts.ContentLoaded)
                    ActivityPosts.LoadPosts();
                RefreshButton.IsEnabled = true;
                await sb.ProgressIndicator.HideAsync();
            } else {
                AlertFlyout.DisplayMessage("Unable to retrieve account details. Check your network connection.");
            }
        }

        public void CreatePost_Click(object sender, RoutedEventArgs e) {
            if (CreatePostControl.Visibility == Visibility.Collapsed) {
                CreatePostIcon.RenderTransform = new CompositeTransform() { Rotation = 45 };
                CreatePostFill.Fill = new SolidColorBrush(Color.FromArgb(255, 207, 73, 73));
                CreatePostControl.Visibility = Visibility.Visible;
                CreatePostControl.AnimateIn();

            } else {
                CreatePostIcon.RenderTransform = new CompositeTransform() { Rotation = 0 };
                CreatePostFill.Fill = App.Current.Resources["HeaderLightBlue"] as SolidColorBrush;
                CreatePostControl.Visibility = Visibility.Collapsed;
            }
        }

        private async void SpotlightTags_Loaded(object sender, RoutedEventArgs e) {
            if (SpotlightTags.ItemsSource == null || sender == null) {
                if (APIWrapper.AuthenticationManager.Authentication.Utils.NetworkAvailable())
                    SpotlightTags.ItemsSource = await CreateRequest.RetrieveSpotlight();
            }
        }

        private void SearchText_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == VirtualKey.Enter) {
                e.Handled = true;
                var x = SearchText.Text;
                SearchText.Text = "";
                if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/tagged?tag=" + Uri.EscapeUriString(x))) {
                    DiagnosticsManager.LogException(null, TAG, "Failed to navigate to search page.");
                }
            }
        }

        private void SpotlightTagItem_Tapped(object sender, TappedRoutedEventArgs e) {
            if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/tagged?tag=" + ((Border)sender).Tag)) {
                DiagnosticsManager.LogException(null, TAG, "Failed to navigate to search page via tag.");
            }
        }

        private void DashboardIcon_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) {
            Posts.ScrollToTop();
        }

        private void ManageBlogs_Tapped(object sender, TappedRoutedEventArgs e) {
            if (!Frame.Navigate(typeof(Pages.Blogs)))
                DiagnosticsManager.LogException(null, TAG, "Failed to navigate to blog selection.");
        }

        private void Inbox_Tapped(object sender, TappedRoutedEventArgs e) {
            if (UserStore.CurrentBlog != null) {
                if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/posts/submission")) {
                    DiagnosticsManager.LogException(null, TAG, "Failed to navigate to inbox.");
                }
            }
        }

        private void Drafts_Tapped(object sender, TappedRoutedEventArgs e) {
            if (UserStore.CurrentBlog != null) {
                if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/posts/draft")) {
                    DiagnosticsManager.LogException(null, TAG, "Failed to navigate to drafts.");
                }
            }
        }

        private void Queue_Tapped(object sender, TappedRoutedEventArgs e) {
            if (UserStore.CurrentBlog != null) {
                if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/posts/queue")) {
                    DiagnosticsManager.LogException(null, TAG, "Failed to navigate to queue.");
                }
            }
        }

        private void Border_Loaded(object sender, RoutedEventArgs e) {
            var dim = (Window.Current.Bounds.Width - 30) / 3;
            ((Border)sender).Width = dim;
            ((Border)sender).Height = dim;
        }
    }
}
