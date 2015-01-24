using APIWrapper.AuthenticationManager;
using APIWrapper.Content;
using Core.Shared.Common;
using Core.Shared.Pages;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountManager : Page {
        private NavigationHelper navigationHelper;

        private static object flyoutControl;

        public AccountManager() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            List.ItemsSource = UserStorageUtils.UserBlogs;
            MainPage.AlertFlyout = _ErrorFlyout;
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e) {
            //Fix navigating
            if (!Frame.Navigate(typeof(MainPage))) {
                throw new Exception();
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
            if (Authentication.AuthenticatedTokens != null && Authentication.AuthenticatedTokens.Count > 1) {
                PageTitle.Text = "Accounts";
            }
            List.ItemsSource = Authentication.AuthenticatedTokens.Keys;
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

        private void SelectAccountButton_Tapped(object sender, TappedRoutedEventArgs e) {
            if (Authentication.SelectedAccount != ((Button)sender).Tag.ToString()) {
                Authentication.SelectedAccount = ((Button)sender).Tag.ToString();

                string token = "";
                Authentication.AuthenticatedTokens.TryGetValue(Authentication.SelectedAccount, out token);
                Authentication.Token = token;

                string secrettoken = "";
                Authentication.AuthenticatedSecretTokens.TryGetValue(Authentication.SelectedAccount, out secrettoken);
                Authentication.TokenSecret = secrettoken;

                MainPage.SwitchedAccount = true;
                if (!Frame.Navigate(typeof(MainPage))) {
                    throw new Exception();
                }
            } else {
                if (!Frame.Navigate(typeof(MainPage))) {
                    throw new Exception();
                }
            }
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e) {
            if (!Frame.Navigate(typeof(Login))) {
                throw new Exception();
            }
            List.ItemsSource = Authentication.AuthenticatedTokens.Keys;
        }

        private void Image_Holding(object sender, HoldingRoutedEventArgs e) {
            flyoutControl = sender;
            FrameworkElement element = flyoutControl as FrameworkElement;
            if (element == null) return;

            FlyoutBase.ShowAttachedFlyout(element);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e) {
            FrameworkElement element = flyoutControl as FrameworkElement;
            if (element == null) return;

            // If the menu was attached properly, we just need to call this handy method
            var f = FlyoutBase.GetAttachedFlyout(element);
            f.Hide();
        }

        private void OtherOptions_Tapped(object sender, TappedRoutedEventArgs e) {
            var selectedItem = sender as TextBlock;

            if (selectedItem != null) {
                if (selectedItem.Text.ToString() == "remove") {
                    var x = selectedItem.Tag.ToString();

                    Authentication.AuthenticatedTokens.Remove(x);
                    Authentication.AuthenticatedSecretTokens.Remove(x);
                    Debug.WriteLine(Authentication.AuthenticatedTokens.Count);
                    if (Authentication.AuthenticatedTokens.Count != 0) {
                        if (Authentication.SelectedAccount == x) {
                            Authentication.SelectedAccount = Authentication.AuthenticatedTokens.Keys.First();

                            string token = "";
                            Authentication.AuthenticatedTokens.TryGetValue(Authentication.SelectedAccount, out token);
                            Authentication.Token = token;

                            string secrettoken = "";
                            Authentication.AuthenticatedSecretTokens.TryGetValue(Authentication.SelectedAccount, out secrettoken);
                            Authentication.TokenSecret = secrettoken;

                            MainPage.SwitchedAccount = true;
                        }
                        Authentication.SetAuthenticatedTokens();
                    } else {
                        Authentication.SetAuthenticatedTokens();
                        Grid_Tapped(null, null);
                        if (!Frame.Navigate(typeof(Login))) {
                            throw new Exception();
                        }
                    }
                    List.ItemsSource = null;
                    List.ItemsSource = Authentication.AuthenticatedTokens.Keys;
                }
                Grid_Tapped(null, null);
            }
        }
    }
}
