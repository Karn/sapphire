using APIWrapper.AuthenticationManager;
using Core.Shared.Common;
using System;
using System.Net.NetworkInformation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Shared.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page {
        private NavigationHelper navigationHelper;

        public Login() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            var applicationView = ApplicationView.GetForCurrentView();
#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif
        }
#if WINDOWS_PHONE_APP
        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e) {
            //Fix navigating
            if (Authentication.AuthenticatedTokens.Count == 0) {
                Application.Current.Exit();
            }
            e.Handled = true;
            return;
        }
#endif

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
            Frame.BackStack.Clear();
            if (e.NavigationParameter != null) {
                StartBox.Visibility = Visibility.Visible;
                LoginBox.Visibility = Visibility.Collapsed;
            } else {
                StartBox.Visibility = Visibility.Collapsed;
                LoginBox.Visibility = Visibility.Visible;
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

        private async void Button_Click(object sender, RoutedEventArgs e) {
            if (NetworkInterface.GetIsNetworkAvailable()) {
                App.DisplayStatus("Signing you in...");
                Email.IsEnabled = false;
                Password.IsEnabled = false;
                LoginButton.IsEnabled = false;

                string response = await Authentication.RequestAccessToken(Email.Text.ToString(), Password.Password.ToString());

                if (response == "OK") {
                    App.HideStatus();
                    if (Authentication.AuthenticatedTokens.Count == 1) {
                        if (!Frame.Navigate(typeof(MainPage))) {
                            throw new Exception();
                        }
                    } else {
#if WINDOWS_PHONE_APP

                        if (!Frame.Navigate(typeof(Core.Pages.AccountManager))) {
                            throw new Exception();
                        }
#endif
                    }
                } else {
                    ErrorFlyout.DisplayMessage(response);
                }
                Email.IsEnabled = true;
                Password.IsEnabled = true;
                LoginButton.IsEnabled = true;
            } else {
                ErrorFlyout.DisplayMessage(App.LocaleResources.GetString("No Network"));
            }
            App.HideStatus();
        }

        private void Email_GotFocus(object sender, RoutedEventArgs e) {
#if WINDOWS_PHONE_APP
            LogoImage.Height = 150;
            LoginBox.VerticalAlignment = VerticalAlignment.Top;
#endif
        }

        private void Start_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
            StartBox.Visibility = Visibility.Collapsed;
            LoginBox.Visibility = Visibility.Visible;
        }
    }
}
