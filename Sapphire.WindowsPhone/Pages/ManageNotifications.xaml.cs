﻿using API.Content;
using API.Data;
using APIWrapper.AuthenticationManager;
using Core.Common;
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
    public sealed partial class ManageNotifications : Page {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ManageNotifications() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            List.ItemsSource = UserData.UserBlogs;
            MainPage.ErrorFlyout = _ErrorFlyout;
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
                Config.OAuthToken = token;

                string secrettoken = "";
                Authentication.AuthenticatedSecretTokens.TryGetValue(Authentication.SelectedAccount, out secrettoken);
                Config.OAuthTokenSecret = secrettoken;

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
            if (!Frame.Navigate(typeof(xAuthLogin))) {
                throw new Exception();
            }
            List.ItemsSource = Authentication.AuthenticatedTokens.Keys;
        }

        private void StackPanel_Holding(object sender, HoldingRoutedEventArgs e) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e) {
            var x = ((MenuFlyoutItem)sender).Tag.ToString();

            Authentication.AuthenticatedTokens.Remove(x);
            Authentication.AuthenticatedSecretTokens.Remove(x);
            Debug.WriteLine(Authentication.AuthenticatedTokens.Count);
            if (Authentication.AuthenticatedTokens.Count != 0) {
                if (Authentication.SelectedAccount == x) {
                    Authentication.SelectedAccount = Authentication.AuthenticatedTokens.Keys.First();

                    string token = "";
                    Authentication.AuthenticatedTokens.TryGetValue(Authentication.SelectedAccount, out token);
                    Config.OAuthToken = token;

                    string secrettoken = "";
                    Authentication.AuthenticatedSecretTokens.TryGetValue(Authentication.SelectedAccount, out secrettoken);
                    Config.OAuthTokenSecret = secrettoken;

                    MainPage.SwitchedAccount = true;
                }
                Config.SaveLocalAccountStore();
            } else {
                Config.SaveLocalAccountStore();
                if (!Frame.Navigate(typeof(xAuthLogin))) {
                    throw new Exception();
                }
                return;
            }

            List.ItemsSource = Authentication.AuthenticatedTokens.Keys;
        }
    }
}
