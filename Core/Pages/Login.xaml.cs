using APIWrapper.AuthenticationManager;
using Core.Common;
using System;
using System.Net.NetworkInformation;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private StatusBar sb;

        public Login() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            var applicationView = ApplicationView.GetForCurrentView();
            applicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            sb = StatusBar.GetForCurrentView();
            sb.ForegroundColor = Color.FromArgb(255, 255, 255, 255);

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e) {
            //Fix navigating
            if (Authentication.AuthenticatedTokens.Count == 0) {
                Application.Current.Exit();
            }
            e.Handled = true;
            return;
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
                sb.ProgressIndicator.Text = "Signing you in...";
                await sb.ProgressIndicator.ShowAsync();
                Email.IsEnabled = false;
                Password.IsEnabled = false;
                LoginButton.IsEnabled = false;

                string response = await APIWrapper.AuthenticationManager.Authentication.RequestAccessToken(Email.Text.ToString(), Password.Password.ToString());
                await sb.ProgressIndicator.HideAsync();

                if (response == "OK") {
                    if (Authentication.AuthenticatedTokens.Count == 1) {
                        if (!Frame.Navigate(typeof(MainPage))) {
                            throw new Exception();
                        }
                    } else {
                        if (!Frame.Navigate(typeof(AccountManager))) {
                            throw new Exception();
                        }
                    }
                } else {
                    ErrorFlyout.DisplayMessage(response);
                }
                Email.IsEnabled = true;
                Password.IsEnabled = true;
                LoginButton.IsEnabled = true;
            } else {
                ErrorFlyout.DisplayMessage("No network availiable.");
            }
        }

        private void Email_GotFocus(object sender, RoutedEventArgs e) {
            LogoImage.Height = 150;
            LoginBox.VerticalAlignment = VerticalAlignment.Top;
        }

        private void Email_LostFocus(object sender, RoutedEventArgs e) {
            //LogoImage.Visibility = Visibility.Visible;
            //LoginBox.VerticalAlignment = VerticalAlignment.Bottom;
        }

        private void Start_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
            StartBox.Visibility = Visibility.Collapsed;
            LoginBox.Visibility = Visibility.Visible;
        }
    }
}
