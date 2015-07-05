using Core.AuthenticationManager;
using Core.Client;
using Core.Content;
using Core.Content.Model.DatabaseHelpers;
using Core.Utils;
using Sapphire.Shared.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Sapphire {
    public sealed partial class MainView : Page {

        public static string TAG = "MainPage";

        private readonly NavigationHelper navigationHelper;

        private static int LastIndex = -1;
        public static bool SwitchedBlog = false;
        public static string SwitchedAccount;

        public MainView() {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);

            if (UserPreferences.NotificationsEnabled)
                RegisterBackgroundTask();

            HardwareButtons.BackPressed += BackButtonPressed;
        }

        public async void CreateView() {
            if (await GetUserAccount() && UserPreferences.CurrentBlog != null) {
                AccountPivot.DataContext = UserPreferences.CurrentBlog;
                for (int i = 0; i < 5; i++) {
                    Dashboard.LoadPosts();
                    if (Dashboard.FeedItemCount() > 0)
                        return;
                }
                await Activity.RetrieveNotifications();
            }
        }

        public async Task<bool> GetUserAccount(string account = "") {
            App.DisplayStatus(App.LocaleResources.GetString("LoadingAccountDataMessage"));

            if (await CreateRequest.RetrieveAccount(account)) {
                AccountPivot.DataContext = UserPreferences.CurrentBlog;
                App.HideStatus();
                return true;
            } else {
                if (!string.IsNullOrWhiteSpace(account) && account != "Sapphire.Default") {
                    var blog = DatabaseController.GetInstance().GetBlog(account);
                    if (blog != null) {
                        AccountPivot.DataContext = blog;
                    } else
                        App.Alert(App.LocaleResources.GetString("UnableToLoadAccount"));
                } else {
                    var blog = DatabaseController.GetInstance().GetBlogs().FirstOrDefault();
                    if (blog != null) {
                        AccountPivot.DataContext = blog;
                    } else
                        App.Alert(App.LocaleResources.GetString("UnableToLoadAccount"));
                }
            }
            App.HideStatus();
            return false;
        }

        private void BackButtonPressed(object sender, BackPressedEventArgs e) {
            if (PostCreationView.IsActive) {
                PostCreationView.CreatePost_LostFocus(null, null);
            } else if (LastIndex != -1) {
                NavigationPivot.SelectedIndex = LastIndex;
                LastIndex = -1;
            } else if (NavigationPivot.SelectedIndex != 0) {
                NavigationPivot.SelectedIndex = 0;
            } else {
                App.Current.Exit();
            }
            e.Handled = true;
        }

        public NavigationHelper NavigationHelper {
            get { return this.navigationHelper; }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
            if (AccountPivot.DataContext == null)
                CreateView();

            if (LastIndex != -1)
                NavigationPivot.SelectedIndex = LastIndex;

            if (!string.IsNullOrWhiteSpace(SwitchedAccount)) {
                Log.i("Refreshing due to account switch.");
                NavigationPivot.SelectedIndex = 0;
                await GetUserAccount(SwitchedAccount);
                Dashboard.RefreshPosts();
                Activity.ClearPosts();
                await Activity.RetrieveNotifications();
                SwitchedAccount = null;
            } else if (SwitchedBlog) {
                Log.i("Refreshing due to blog switch.");
                AccountPivot.DataContext = UserPreferences.CurrentBlog;
                await Activity.RetrieveNotifications();
                SwitchedBlog = false;
            } else if (e.NavigationParameter != null && !string.IsNullOrEmpty(e.NavigationParameter.ToString())) {
                //Handle accounts switch to the one described in the toast
                if (e.NavigationParameter.ToString().Contains("Account:")) {
                    var s = e.NavigationParameter.ToString().Split(' ');
                    await GetUserAccount(s[1]);
                    await Activity.RetrieveNotifications();
                    NavigationPivot.SelectedIndex = 1;
                }
            }

            //Remove entries before this page.
            Frame.BackStack.Clear();
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) {
            LastIndex = NavigationPivot.SelectedIndex;
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
                    Log.i("Previous task found.");
                    return;
                }
            }
            await BackgroundExecutionManager.RequestAccessAsync();

            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder { Name = "PushNotificationTask", TaskEntryPoint = "BackgroundUtilities.NotificationHandler" };
            taskBuilder.SetTrigger(new TimeTrigger(15, false));
            taskBuilder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            taskBuilder.Register();
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
                    PageTitle.Text = App.LocaleResources.GetString("Title_Dashboard");
                    DashboardIcon.Opacity = 1.0;
                    AccountIcon.Opacity = 0.5;
                    ActivityIcon.Opacity = 0.5;
                    SearchIcon.Opacity = 0.5;
                    NavigationPivot.SelectedIndex = 0;
                    break;
                case 1:
                    PageTitle.Text = App.LocaleResources.GetString("Title_Activity");
                    DashboardIcon.Opacity = 0.5;
                    AccountIcon.Opacity = 0.5;
                    ActivityIcon.Opacity = 1.0;
                    SearchIcon.Opacity = 0.5;
                    NavigationPivot.SelectedIndex = 1;
                    break;
                case 2:
                    PageTitle.Text = App.LocaleResources.GetString("Title_Account");
                    DashboardIcon.Opacity = 0.5;
                    AccountIcon.Opacity = 1.0;
                    ActivityIcon.Opacity = 0.5;
                    SearchIcon.Opacity = 0.5;
                    NavigationPivot.SelectedIndex = 2;
                    break;
                case 3:
                    PageTitle.Text = App.LocaleResources.GetString("Title_Explore");
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
                Log.e("Failed to navigate to Settings.");
        }

        private void ManageAccountButton_Click(object sender, RoutedEventArgs e) {
            if (!Frame.Navigate(typeof(Pages.AccountManager), "1"))
                Log.e("Failed to navigate to AccountManager.");
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e) {
            RefreshButton.IsEnabled = false;
            switch (NavigationPivot.SelectedIndex) {
                default:
                case 0:
                    break;
                case 1:
                    await Activity.RetrieveNotifications(); break;
                case 2:
                    await GetUserAccount(string.Empty); break;
                case 3:
                    SpotlightTags.ItemsSource = await CreateRequest.RetrieveSpotlight(true); break;
            }
            RefreshButton.IsEnabled = true;
        }

        private void AccountDetails_Tapped(object sender, TappedRoutedEventArgs e) {
            if (UserPreferences.CurrentBlog != null) {
                switch (((FrameworkElement)sender).Tag.ToString()) {
                    case "Posts":
                        if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserPreferences.CurrentBlog.Name + ".tumblr.com/posts"))
                            Log.e("Failed to navigate to current blogs posts.");
                        break;
                    case "Likes":
                        if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/user/likes"))
                            Log.e("Failed to navigate to current blogs likes.");
                        break;
                    case "Followers":
                    case "Following":
                        if (!Frame.Navigate(typeof(Pages.FollowersFollowing), ((FrameworkElement)sender).Tag.ToString()))
                            Log.e("Failed to navigate to Following.");
                        break;
                }
            }
        }

        private async void Spotlight_Loaded(object sender, RoutedEventArgs e) {
            if (SpotlightTags.ItemsSource == null || sender == null)
                SpotlightTags.ItemsSource = await CreateRequest.RetrieveSpotlight();
        }

        private void SearchText_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == VirtualKey.Enter) {
                e.Handled = true;
                var searchTerm = SearchText.Text;
                SearchText.Text = string.Empty;
                if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/tagged?tag=" + Uri.EscapeUriString(searchTerm)))
                    Log.e("Failed to navigate to search page.");
            }
        }

        private void SpotlightItem_Tapped(object sender, TappedRoutedEventArgs e) {
            if (((FrameworkElement)sender).Tag != null) {
                if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/tagged?tag=" + ((FrameworkElement)sender).Tag))
                    Log.e("Failed to navigate to search page via tag.");
            }
        }

        private void ToTopButton_Click(object sender, RoutedEventArgs e) {
            Dashboard.ScrollToTop();
        }

        private void ManageBlogs_Tapped(object sender, RoutedEventArgs e) {
            if (!Frame.Navigate(typeof(Pages.AccountManager)))
                Log.e("Failed to navigate to blog selection.");
        }

        private void Inbox_Tapped(object sender, RoutedEventArgs e) {
            if (UserPreferences.CurrentBlog != null) {
                if (!Frame.Navigate(typeof(Pages.Inbox))) {
                    Log.e("Failed to navigate to inbox.");
                }
            }
        }

        private void Drafts_Tapped(object sender, RoutedEventArgs e) {
            if (UserPreferences.CurrentBlog != null) {
                if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserPreferences.CurrentBlog.Name + ".tumblr.com/posts/draft")) {
                    Log.e("Failed to navigate to drafts.");
                }
            }
        }

        private void Queue_Tapped(object sender, RoutedEventArgs e) {
            if (UserPreferences.CurrentBlog != null) {
                if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserPreferences.CurrentBlog.Name + ".tumblr.com/posts/queue")) {
                    Log.e("Failed to navigate to queue.");
                }
            }
        }

        private void Favs_List_Tapped(object sender, RoutedEventArgs e) {
            if (UserPreferences.CurrentBlog != null) {
                if (!Frame.Navigate(typeof(Pages.FavBlogs)))
                    Log.e("Failed to navigate to favorite blogs.");
            }
        }

        private void SetSpotlightItemDimensions(object sender, RoutedEventArgs e) {
            var dim = (Window.Current.Bounds.Width - 25) / 3;
            ((Grid)sender).Width = dim;
            ((Grid)sender).Height = dim;
        }
    }
}
