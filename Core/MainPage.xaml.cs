using APIWrapper.AuthenticationManager;
using APIWrapper.Client;
using APIWrapper.Content;
using APIWrapper.Utils;
using Core.Shared.Common;
using Core.Utils.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Core {
    public sealed partial class MainPage : Page {

        public static string TAG = "MainPage";

        private readonly NavigationHelper navigationHelper;

        public static AlertDialog AlertFlyout;
        private static int LastIndex = -1;
        private static bool NavigatedFromToast;
        public static bool SwitchedBlog = false;
        public static bool SwitchedAccount = false;

        public static NewPostDialog NPD;

        public MainPage() {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            //Initialize
            AlertFlyout = _ErrorFlyout; //Mainpage error toast

            NPD = PostCreationView;

            HardwareButtons.BackPressed += BackButtonPressed;

            if (UserStore.NotificationsEnabled)
                RegisterBackgroundTask();

            Analytics.AnalyticsManager.RegisterView(TAG);
        }

        public async void CreateView() {
            if (UserStore.CurrentBlog == null) {
                if (await GetUserAccount() && UserStore.CurrentBlog != null) {
                    Dashboard.LoadPosts();
                    ActivityFeed.RetrieveNotifications();
                }
            }
        }

        public async Task<bool> GetUserAccount(string account = "") {
            App.DisplayStatus(App.LocaleResources.GetString("LoadingAccountDataMessage"));
            if (await CreateRequest.RetrieveAccountInformation(account)) {
                AccountPivot.DataContext = UserStore.CurrentBlog;
                return true;
            } else
                AlertFlyout.DisplayMessage(App.LocaleResources.GetString("UnableToLoadAccount"));
            App.HideStatus();
            return false;
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e) {
            HeaderAnimateIn.Begin();
            if (!UserStore.EnableStatusBarBG)
                StatusBarBG.Background = App.Current.Resources["PrimaryColor"] as SolidColorBrush;
            else
                StatusBarBG.Background = App.Current.Resources["PrimaryColorDark"] as SolidColorBrush;

            CreateView();
        }

        private void BackButtonPressed(object sender, BackPressedEventArgs e) {
            //Fix navigating
            if (PostCreationView.Visibility == Visibility.Visible) {
                CreatePostIcon.RenderTransform = new CompositeTransform() { Rotation = 0 };
                CreatePostFill.Fill = App.Current.Resources["PrimaryColor"] as SolidColorBrush;
                PostCreationView.Visibility = Visibility.Collapsed;
                e.Handled = true;
            } else if (LastIndex != -1) {
                Navigation.SelectedIndex = LastIndex;
                LastIndex = -1;
                e.Handled = true;
            } else if (NavigationPivot.SelectedIndex != 0) {
                NavigationPivot.SelectedIndex = 0;
                e.Handled = true;
            }
        }

        public NavigationHelper NavigationHelper {
            get { return this.navigationHelper; }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
            if (e.NavigationParameter != null && !string.IsNullOrEmpty(e.NavigationParameter.ToString()) && !NavigatedFromToast) {
                //Handle accounts switch to the one described in the toast
                if (e.NavigationParameter.ToString().Contains("Account:")) {
                    var s = e.NavigationParameter.ToString().Split(' ');
                    await GetUserAccount(s[1]);
                    ActivityFeed.RetrieveNotifications();
                    NavigationPivot.SelectedIndex = 1;
                    NavigatedFromToast = true;
                }
            }

            if (SwitchedAccount) {
                NavigationPivot.SelectedIndex = 0;
                CreateRequest.ReloadAccountData = true;
                await GetUserAccount(string.Empty);
                Dashboard.RefreshPosts();
                ActivityFeed.RetrieveNotifications();
                SwitchedAccount = false;
            } else if (SwitchedBlog) {
                AccountPivot.DataContext = UserStore.CurrentBlog;
                ActivityFeed.ClearPosts();
                ActivityFeed.RetrieveNotifications();
                SwitchedBlog = false;
            }

            AlertFlyout = _ErrorFlyout;

            //Remove entries before this page.
            Frame.BackStack.Clear();
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) {
            LastIndex = Navigation.SelectedIndex;
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void RegisterBackgroundTask() {
            foreach (var cur in BackgroundTaskRegistration.AllTasks) {
                if (cur.Value.Name == "PushNotificationTask") {
                    Analytics.AnalyticsManager.LogException(null, TAG, "Previous task found.");
                    return;
                }
            }
            await BackgroundExecutionManager.RequestAccessAsync();

            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder { Name = "PushNotificationTask", TaskEntryPoint = "BackgroundUtilities.NotificationHandler" };
            taskBuilder.SetTrigger(new TimeTrigger(15, false));
            taskBuilder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            BackgroundTaskRegistration myFirstTask = taskBuilder.Register();
        }

        private void NavigationIconsSelectionChanged(object sender, SelectionChangedEventArgs e) {
            HandleNav(Navigation.SelectedIndex);
        }

        private void NavigationPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            HandleNav(NavigationPivot.SelectedIndex);
        }

        private void HandleNav(int SelectedItem) {
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
                    PageTitle.Text = App.LocaleResources.GetString("Dashboard");
                    DashboardIcon.Opacity = 1.0;
                    AccountIcon.Opacity = 0.5;
                    ActivityIcon.Opacity = 0.5;
                    SearchIcon.Opacity = 0.5;
                    NavigationPivot.SelectedIndex = 0;
                    break;
                case 1:
                    PageTitle.Text = App.LocaleResources.GetString("Activity");
                    DashboardIcon.Opacity = 0.5;
                    AccountIcon.Opacity = 0.5;
                    ActivityIcon.Opacity = 1.0;
                    SearchIcon.Opacity = 0.5;
                    NavigationPivot.SelectedIndex = 1;
                    break;
                case 2:
                    PageTitle.Text = App.LocaleResources.GetString("Account");
                    DashboardIcon.Opacity = 0.5;
                    AccountIcon.Opacity = 1.0;
                    ActivityIcon.Opacity = 0.5;
                    SearchIcon.Opacity = 0.5;
                    NavigationPivot.SelectedIndex = 2;
                    break;
                case 3:
                    PageTitle.Text = App.LocaleResources.GetString("Explore");
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
            if (!Frame.Navigate(typeof(Pages.Settings)))
                Analytics.AnalyticsManager.LogException(null, TAG, "Failed to navigate to Settings.");
        }

        private void ManageAccountButton_Click(object sender, RoutedEventArgs e) {
            if (!Frame.Navigate(typeof(Pages.AccountManager)))
                Analytics.AnalyticsManager.LogException(null, TAG, "Failed to navigate to AccountManager.");
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e) {
            switch (NavigationPivot.SelectedIndex) {
                default:
                case 0:
                    break;
                case 1:
                    ActivityFeed.RetrieveNotifications(); break;
                case 2:
                    await GetUserAccount(string.Empty);
                    ActivityFeed.RetrieveNotifications(); break;
                case 3:
                    SpotlightTags.ItemsSource = await CreateRequest.RetrieveSpotlight(true); break;
            }
        }

        private void AccountDetails_Tapped(object sender, TappedRoutedEventArgs e) {
            if (UserStore.CurrentBlog != null) {
                switch (((StackPanel)sender).Tag.ToString()) {
                    case "Posts":
                        if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/posts"))
                            Analytics.AnalyticsManager.LogException(null, TAG, "Failed to navigate to current blogs posts.");
                        break;
                    case "Likes":
                        if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/user/likes"))
                            Analytics.AnalyticsManager.LogException(null, TAG, "Failed to navigate to current blogs likes.");
                        break;
                    case "Followers":
                    case "Following":
                        if (!Frame.Navigate(typeof(Pages.FollowersFollowing), ((StackPanel)sender).Tag.ToString()))
                            Analytics.AnalyticsManager.LogException(null, TAG, "Failed to navigate to Following.");
                        break;
                }
            }
        }

        public void CreatePost_Click(object sender, RoutedEventArgs e) {
            ((Button)sender).Focus(FocusState.Pointer);
            if (PostCreationView.Visibility == Visibility.Collapsed) {
                CreatePostIcon.RenderTransform = new CompositeTransform() { Rotation = 45 };
                CreatePostFill.Fill = new SolidColorBrush(Color.FromArgb(255, 207, 73, 73));
                PostCreationView.Visibility = Visibility.Visible;
                PostCreationView.AnimateIn();
            } else
                CreatePost_LostFocus(null, null);
        }

        private void CreatePost_LostFocus(object sender, RoutedEventArgs e) {
            CreatePostIcon.RenderTransform = new CompositeTransform() { Rotation = 0 };
            CreatePostFill.Fill = App.Current.Resources["PrimaryColor"] as SolidColorBrush;
            PostCreationView.AnimateOut();
        }

        private async void Spotlight_Loaded(object sender, RoutedEventArgs e) {
            if (SpotlightTags.ItemsSource == null || sender == null) {
                if (Authentication.Utils.NetworkAvailable())
                    SpotlightTags.ItemsSource = await CreateRequest.RetrieveSpotlight();
            }
        }

        private void SearchText_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == VirtualKey.Enter) {
                e.Handled = true;
                var searchTerm = SearchText.Text;
                SearchText.Text = string.Empty;
                if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/tagged?tag=" + Uri.EscapeUriString(searchTerm)))
                    Analytics.AnalyticsManager.LogException(null, TAG, "Failed to navigate to search page.");
            }
        }

        private void SpotlightItem_Tapped(object sender, TappedRoutedEventArgs e) {
            if (((Border)sender).Tag != null) {
                if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/tagged?tag=" + ((Border)sender).Tag))
                    Analytics.AnalyticsManager.LogException(null, TAG, "Failed to navigate to search page via tag.");
            }
        }

        private void ToTopButton_Click(object sender, RoutedEventArgs e) {
            Dashboard.ScrollToTop();
        }

        private void ManageBlogs_Tapped(object sender, TappedRoutedEventArgs e) {
            if (!Frame.Navigate(typeof(Pages.Blogs)))
                Analytics.AnalyticsManager.LogException(null, TAG, "Failed to navigate to blog selection.");
        }

        private void Inbox_Tapped(object sender, TappedRoutedEventArgs e) {
            if (UserStore.CurrentBlog != null) {
                if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/posts/submission")) {
                    Analytics.AnalyticsManager.LogException(null, TAG, "Failed to navigate to inbox.");
                }
            }
        }

        private void Drafts_Tapped(object sender, TappedRoutedEventArgs e) {
            if (UserStore.CurrentBlog != null) {
                if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/posts/draft")) {
                    Analytics.AnalyticsManager.LogException(null, TAG, "Failed to navigate to drafts.");
                }
            }
        }

        private void Queue_Tapped(object sender, TappedRoutedEventArgs e) {
            if (UserStore.CurrentBlog != null) {
                if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/posts/queue")) {
                    Analytics.AnalyticsManager.LogException(null, TAG, "Failed to navigate to queue.");
                }
            }
        }

        private void Favs_List_Tapped(object sender, TappedRoutedEventArgs e) {
            if (UserStore.CurrentBlog != null) {
                if (!Frame.Navigate(typeof(Pages.FavBlogs)))
                    Analytics.AnalyticsManager.LogException(null, TAG, "Failed to navigate to favorite blogs.");
            }
        }

        private void SetSpotlightItemDimensions(object sender, RoutedEventArgs e) {
            var dim = (Window.Current.Bounds.Width - 30) / 3;
            ((Border)sender).Width = dim;
            ((Border)sender).Height = dim;
        }
    }
}
